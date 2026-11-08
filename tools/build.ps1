<#
.SYNOPSIS
    Builds a Windows standalone player headless.

.EXAMPLE
    .\tools\build.ps1
#>
param()

$ErrorActionPreference = "Stop"
. "$PSScriptRoot\common.ps1"

$root = Get-ProjectRoot
$log = Join-Path $root "Logs\build.log"

& "$PSScriptRoot\u.ps1" -Method "CardRhythm.EditorAutomation.ProjectAutomation.BuildWindows" -LogFile $log
$code = $LASTEXITCODE

$output = Join-Path $root "Builds\Windows\CardRhythm.exe"
if (Test-Path $output) {
    $size = [math]::Round((Get-Item $output).Length / 1MB, 1)
    Write-Host "player: $output ($size MB)"
}

exit $code
