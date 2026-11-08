<#
.SYNOPSIS
    Runs Unity in batch mode and turns the log into a usable exit code.

.DESCRIPTION
    -batchmode -nographics -quit, optional -executeMethod.
    Exit codes: 0 ok, 1 Unity/runtime failure, 2 C# compile errors, 3 editor not found.

.EXAMPLE
    .\tools\u.ps1
    .\tools\u.ps1 -Method CardRhythm.EditorAutomation.ProjectAutomation.Validate
#>
param(
    [string]$Method = "",
    [string]$LogFile = "",
    [int]$TailLines = 60
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot\common.ps1"

$root = Get-ProjectRoot
$editor = Get-UnityEditor
Assert-ProjectUnlocked -ProjectRoot $root

if ([string]::IsNullOrWhiteSpace($LogFile)) {
    $LogFile = Join-Path $root "Logs\batch.log"
}
$logDir = Split-Path -Parent $LogFile
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir -Force | Out-Null }
if (Test-Path $LogFile) { Remove-Item $LogFile -Force }

$argLine = "-batchmode -nographics -quit -projectPath `"$root`" -logFile `"$LogFile`""
if (-not [string]::IsNullOrWhiteSpace($Method)) {
    $argLine += " -executeMethod `"$Method`""
}

Write-Section "Unity batch"
Write-Host "editor : $editor"
Write-Host "method : $(if ($Method) { $Method } else { '(compile / import only)' })"
Write-Host "log    : $LogFile"

$started = Get-Date
$process = Start-Process -FilePath $editor -ArgumentList $argLine -Wait -PassThru
$seconds = [math]::Round(((Get-Date) - $started).TotalSeconds, 1)

$compileErrors = @()
$failures = @()
if (Test-Path $LogFile) {
    $compileErrors = @(Select-String -Path $LogFile -Pattern "error CS\d+" | Select-Object -First 20)
    $failures = @(Select-String -Path $LogFile -Pattern "Aborting batchmode due to failure|executeMethod (class|method) .* not found|Unhandled exception" | Select-Object -First 10)
}

Write-Host "exit   : $($process.ExitCode)  time: ${seconds}s"

if ($compileErrors.Count -gt 0) {
    Write-Host ""
    Write-Host "C# COMPILE ERRORS:" -ForegroundColor Red
    $compileErrors | ForEach-Object { Write-Host $_.Line }
    exit 2
}

if ($process.ExitCode -ne 0 -or $failures.Count -gt 0) {
    Write-Host ""
    Write-Host "FAILURES:" -ForegroundColor Red
    $failures | ForEach-Object { Write-Host $_.Line }
    Write-Host ""
    Write-Host "---- log tail ----" -ForegroundColor DarkGray
    Get-LogTail $LogFile $TailLines | ForEach-Object { Write-Host $_ }
    exit 1
}

Write-Host "OK" -ForegroundColor Green
exit 0
