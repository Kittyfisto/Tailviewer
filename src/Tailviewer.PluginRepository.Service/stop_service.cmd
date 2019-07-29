@echo off
setlocal

set SCRIPT_DIR=%~dp0

sc stop "Tailviewer.PluginRepository"
exit /b %errorlevel%

endlocal
