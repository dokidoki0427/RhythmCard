<#
.SYNOPSIS
    One-shot project gate: resolve packages, compile, run ProjectAutomation.Validate.
    Use this after every code change. Exit code 0 means the project is healthy.
#>
param(
    [switch]$Quiet
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot\common.ps1"

$root = Get-ProjectRoot
$log = Join-Path $root "Logs\verify.log"

& "$PSScriptRoot\u.ps1" -Method "CardRhythm.EditorAutomation.ProjectAutomation.Validate" -LogFile $log
$code = $LASTEXITCODE

$report = Join-Path $root "Logs\automation-report.json"
if (Test-Path $report) {
    Write-Section "automation-report.json"
    Get-Content $report -Encoding UTF8 | ForEach-Object { Write-Host $_ }
}

exit $code
