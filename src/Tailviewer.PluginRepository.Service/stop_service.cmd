@echo off
setlocal

set SCRIPT_DIR=%~dp0

sc stop "Tailviewer.PluginRepository"
set RET=%ERRORLEVEL%

rem The service isn't installed
if %RET% == 1060 exit /b 0

rem The service isn't running
if %RET% == 1062 exit /b 0

exit /b %RET%

endlocal
