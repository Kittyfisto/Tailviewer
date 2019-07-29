@setlocal
@echo off

call set_environment.cmd

sc stop "%ServiceName%" > nul 2>&1
set ret=%errorlevel%

rem The service isn't installed
if %ret% == 1060 goto :NOTINSTALLED
if %ret% == 1062 goto :NOTRUNNING
if %ret% == 0 goto :SUCCESS
goto :ERROR

:NOTINSTALLED
echo Service "%ServiceName%" isn't installed, nothing needs to be done
exit /b 0

:NOTRUNNING
echo Service "%ServiceName%" isn't running, nothing needs to be done
exit /b 0

:ERROR
echo Error: Unable to stop service "%ServiceName%"
echo sc returned %ret%
net helpmsg %ret%
exit /b %ret%

:SUCCESS
echo Service "%servicename%" stopped
exit /b 0

endlocal
