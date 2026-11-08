@echo off
rem Double-click launcher: compile + project validation only (fast, ~8s).
cd /d "%~dp0"

powershell -NoProfile -ExecutionPolicy Bypass -File ".\tools\verify.ps1"
set CODE=%errorlevel%

echo.
if "%CODE%"=="4" echo [X] Unity Editor is open. Close it, then double-click again.
if "%CODE%"=="2" echo [X] C# compile errors. Open Logs\verify.log and search for "error CS".
if "%CODE%"=="0" echo [OK] Project is healthy.
echo.
pause
