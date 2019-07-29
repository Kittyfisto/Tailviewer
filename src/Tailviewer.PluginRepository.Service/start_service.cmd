@echo off
setlocal

call set_environment.cmd

sc start "%ServiceName%" > nul 2>&1
set ret=%errorlevel%

rem Service not installed so let's do that...
if %ret% == 1060 (
	echo Service "%ServiceName%" does not exist, creating it...
	call create_service.cmd
	sc start "%ServiceName%" > nul 2>&1
	set ret=%errorlevel%	
)
if %ret% == 1056 goto :RUNNING
if not %ret% == 0 goto :ERROR
goto :SUCCESS

:ERROR
echo Error: Unable to start service "%ServiceName%"
echo sc returned %ret%
net helpmsg %ret%
exit /b %ret%

:RUNNING
echo Service "%ServiceName%" is already running, nothing needs to be done
exit /b 0

:SUCCESS
echo Service "%ServiceName%" started
exit /b 0

endlocal
