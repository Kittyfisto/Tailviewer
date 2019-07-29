@setlocal
@echo off

call set_environment.cmd
call require_admin.cmd || exit /b 1

call stop_service.cmd

sc delete "%ServiceName%" > nul 2>&1
set ret=%errorlevel%
if %ret% == 1060 goto :NOTCREATED
if %ret% == 0 goto :SUCCESS
goto :ERROR

:NOTCREATED
echo Service "%ServiceName%" doesn't exist, nothing needs to be done
exit /b 0

:ERROR
echo Error: Unable to delete service "%ServiceName%"
echo sc returned %ret%
net helpmsg %ret%
exit /b %ret%

:SUCCESS
echo Service "%servicename%" deleted
exit /b 0

endlocal
