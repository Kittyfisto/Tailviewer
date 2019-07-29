@setlocal
@echo off

net session >nul 2>&1
if not %errorLevel% == 0 goto :NOADMIN
goto :SUCCESS

:NOADMIN
echo This script requires administrator privileges to run!
echo Please run your shell (cmd, powershell, etc..) as administrator and retry.
exit /b -1

:SUCCESS
exit /b

endlocal
