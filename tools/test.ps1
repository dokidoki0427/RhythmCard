<#
.SYNOPSIS
    Runs Unity Test Framework tests headless and reports pass/fail.

.EXAMPLE
    .\tools\test.ps1
    .\tools\test.ps1 -Platform PlayMode
#>
param(
    [ValidateSet("EditMode", "PlayMode")]
    [string]$Platform = "EditMode",
    [string]$Filter = ""
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot\common.ps1"

$root = Get-ProjectRoot
$editor = Get-UnityEditor
Assert-ProjectUnlocked -ProjectRoot $root
$results = Join-Path $root "Logs\test-$Platform.xml"
$log = Join-Path $root "Logs\test-$Platform.log"

New-Item -ItemType Directory -Path (Join-Path $root "Logs") -Force | Out-Null
if (Test-Path $results) { Remove-Item $results -Force }

$argLine = "-batchmode -nographics -projectPath `"$root`" -runTests -testPlatform $Platform -testResults `"$results`" -logFile `"$log`""
if (-not [string]::IsNullOrWhiteSpace($Filter)) {
    $argLine += " -testFilter `"$Filter`""
}

Write-Section "Unity tests ($Platform)"
Write-Host "editor  : $editor"
Write-Host "results : $results"

$started = Get-Date
$process = Start-Process -FilePath $editor -ArgumentList $argLine -Wait -PassThru
$seconds = [math]::Round(((Get-Date) - $started).TotalSeconds, 1)

if (-not (Test-Path $results)) {
    Write-Host "No test results produced (exit $($process.ExitCode)). Log tail:" -ForegroundColor Red
    Get-LogTail $log 60 | ForEach-Object { Write-Host $_ }
    exit 1
}

[xml]$doc = Get-Content $results
$run = $doc.'test-run'
$total = [int]$run.total
$failed = [int]$run.failed
$passed = [int]$run.passed
$skipped = [int]$run.skipped

Write-Host ("total={0} passed={1} failed={2} skipped={3} time={4}s" -f $total, $passed, $failed, $skipped, $seconds)

if ($failed -gt 0) {
    Write-Host ""
    Write-Host "FAILED TESTS:" -ForegroundColor Red
    $doc.SelectNodes("//test-case[@result='Failed']") | ForEach-Object {
        Write-Host ("  - " + $_.fullname) -ForegroundColor Red
        if ($_.failure -and $_.failure.message) {
            Write-Host ("    " + $_.failure.message.Trim()) -ForegroundColor DarkRed
        }
    }
    exit 1
}

Write-Host "OK" -ForegroundColor Green
exit 0
