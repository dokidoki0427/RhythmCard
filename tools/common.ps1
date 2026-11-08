# Shared helpers for the automation scripts. ASCII only on purpose:
# Windows PowerShell 5.1 mis-reads UTF-8 files without BOM.

$script:ProjectRoot = Split-Path -Parent $PSScriptRoot

function Get-UnityEditor {
    if ($env:UNITY_EDITOR_PATH -and (Test-Path $env:UNITY_EDITOR_PATH)) {
        return $env:UNITY_EDITOR_PATH
    }

    $candidates = @(
        "E:\Unity\Editor\Unity.exe",
        "C:\Program Files\Unity\Hub\Editor\6000.0.61f1\Editor\Unity.exe"
    )
    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) { return $candidate }
    }

    $cli = Get-Command unity -ErrorAction SilentlyContinue
    if ($cli) {
        $line = & $cli.Source editors -i 2>$null | Select-Object -Skip 1 | Select-Object -First 1
        if ($line) {
            $version = ($line -split "`t")[0]
            $path = Join-Path "C:\Program Files\Unity\Hub\Editor" "$version\Editor\Unity.exe"
            if (Test-Path $path) { return $path }
        }
    }

    throw "Unity Editor not found. Set UNITY_EDITOR_PATH to Unity.exe."
}

function Get-ProjectRoot {
    return $script:ProjectRoot
}

function Write-Section([string]$text) {
    Write-Host ""
    Write-Host "==== $text ====" -ForegroundColor Cyan
}

function Get-LogTail([string]$path, [int]$lines = 60) {
    if (-not (Test-Path $path)) { return @() }
    return Get-Content $path -Tail $lines
}

function Test-ProjectLocked {
    param([string]$ProjectRoot)

    $lock = Join-Path $ProjectRoot "Temp\UnityLockfile"
    if (-not (Test-Path $lock)) { return $false }

    try {
        $stream = [System.IO.File]::Open($lock, "Open", "ReadWrite", "None")
        $stream.Close()
        return $false
    } catch {
        return $true
    }
}

function Assert-ProjectUnlocked {
    param([string]$ProjectRoot)

    if (Test-ProjectLocked -ProjectRoot $ProjectRoot) {
        Write-Host ""
        Write-Host "Project is open in the Unity Editor." -ForegroundColor Yellow
        Write-Host "Batch mode needs exclusive access: close the Editor (or a copy), then retry." -ForegroundColor Yellow
        exit 4
    }
}

