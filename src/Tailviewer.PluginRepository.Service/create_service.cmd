@echo off
setlocal

set SCRIPT_DIR=%~dp0
set SERVICE_PATH=%SCRIPT_DIR%repository-svc.exe

if not exist "%SERVICE_PATH%" goto :NOSERVICE

sc create "Tailviewer.PluginRepository" binPath= "%SERVICE_PATH%" start= auto
set RET=%errorlevel%

rem The service already exists => Not a problem
if %RET% == 1073 exit /b 0

exit /b %RET%

:NOSERVICE
echo Error: Unable to find %SERVICE_PATH%
exit /b -1

endlocal
