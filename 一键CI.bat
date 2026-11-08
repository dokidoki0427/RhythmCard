@echo off
rem Double-click launcher: full gate (verify + tests + build).
rem ASCII only: cmd.exe parses .bat with the OEM code page, Chinese would garble.
cd /d "%~dp0"

powershell -NoProfile -ExecutionPolicy Bypass -File ".\tools\ci.ps1"
set CODE=%errorlevel%

echo.
if "%CODE%"=="4" echo [X] Unity Editor is open. Close it, then double-click again.
if "%CODE%"=="2" echo [X] C# compile errors. Open Logs\verify.log and search for "error CS".
if "%CODE%"=="0" echo [OK] All green.
if not "%CODE%"=="0" if not "%CODE%"=="2" if not "%CODE%"=="4" echo [X] Failed with exit code %CODE%. See Logs\.
echo.
pause
