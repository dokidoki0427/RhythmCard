<#
.SYNOPSIS
    Full local pipeline: verify -> editmode tests -> Windows build.
    Mirrors what CI does, so a green run here means a green CI run.
#>
param(
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot\common.ps1"

$steps = @()
$failed = $false

Write-Section "STEP 1/3 verify"
& "$PSScriptRoot\verify.ps1"
if ($LASTEXITCODE -ne 0) { $steps += "verify:FAIL"; $failed = $true } else { $steps += "verify:PASS" }

if (-not $failed) {
    Write-Section "STEP 2/3 tests"
    & "$PSScriptRoot\test.ps1" -Platform EditMode
    if ($LASTEXITCODE -ne 0) { $steps += "tests:FAIL"; $failed = $true } else { $steps += "tests:PASS" }
}

if (-not $failed -and -not $SkipBuild) {
    Write-Section "STEP 3/3 build"
    & "$PSScriptRoot\build.ps1"
    if ($LASTEXITCODE -ne 0) { $steps += "build:FAIL"; $failed = $true } else { $steps += "build:PASS" }
}

Write-Section "SUMMARY"
$steps | ForEach-Object { Write-Host "  $_" }

if ($failed) {
    Write-Host "CI FAILED" -ForegroundColor Red
    exit 1
}

Write-Host "CI PASSED" -ForegroundColor Green
exit 0
