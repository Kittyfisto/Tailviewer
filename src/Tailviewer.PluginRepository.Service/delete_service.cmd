@echo off
setlocal

set SCRIPT_DIR=%~dp0

sc delete "Tailviewer.PluginRepository"
set RET=%ERRORLEVEL%

rem The service doesn't exist
if %RET% == 1060 exit /b 0

exit /b  %RET%

endlocal
