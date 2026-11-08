<#
.SYNOPSIS
    One-time bootstrap: creates Assets/Scenes/Boot.unity with the GameFramework prefab
    and registers it in Build Settings. Idempotent, safe to re-run.

.NOTES
    Close the Unity Editor first (batch mode needs exclusive project access).
#>
param()

$ErrorActionPreference = "Stop"
. "$PSScriptRoot\common.ps1"

$root = Get-ProjectRoot
$log = Join-Path $root "Logs\init.log"

& "$PSScriptRoot\u.ps1" -Method "CardRhythm.EditorAutomation.ProjectAutomation.CreateBootScene" -LogFile $log
exit $LASTEXITCODE
