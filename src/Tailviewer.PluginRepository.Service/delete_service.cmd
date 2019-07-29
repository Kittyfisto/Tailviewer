@echo off
setlocal

set SCRIPT_DIR=%~dp0

sc delete "Tailviewer.PluginRepository"
exit /b %errorlevel%

endlocal
