@setlocal
@echo off

call set_environment.cmd
call require_admin.cmd || exit /b 1

if not exist "%ServicePath%" goto :NOSERVICE

echo %ScriptDir%
echo %ServicePath%

sc create "%ServiceName%" binPath= "%ServicePath%" start= auto > nul 2>&1
set ret=%errorlevel%

if %ret% == 1073 goto :ALREADYCREATED
if %ret% == 0 goto :SUCCESS
goto :ERROR

:NOSERVICE
echo Error: Unable to find %ServicePath%
exit /b -1

:ALREADYCREATED
echo Service "%ServiceName%" is already created, nothing needs to be done
exit /b 0

:ERROR
echo Error: Unable to create service "%ServiceName%"
echo sc returned %ret%
net helpmsg %ret%
exit /b %ret%

:SUCCESS
echo Service "%ServiceName%" created
exit /b 0

endlocal
