@echo off
setlocal

set SCRIPT_DIR=%~dp0

sc start "Tailviewer.PluginRepository"
exit /b %errorlevel%

endlocal
