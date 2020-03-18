@setlocal
@echo off
setlocal ENABLEDELAYEDEXPANSION

echo Uninstalling Tailviewer...

set INSTALLATION_PATH=%~dp0

if %INSTALLATION_PATH:~-1%==\ set INSTALLATION_PATH=%INSTALLATION_PATH:~0,-1%

set TAILVIEWER_SETUP=tailviewer-setup.exe
set SRC_INSTALLATION_EXECUTABLE=%INSTALLATION_PATH%\%TAILVIEWER_SETUP%

set DEST_INSTALLATION_EXECUTABLE=%TEMP%\%TAILVIEWER_SETUP%
set DE_INTALLER_BATCH=%TEMP%\remove_tailviewer.cmd
echo @echo off>"%DE_INTALLER_BATCH%"
echo %INSTALLATION_PATH%

echo Copying installer %SRC_INSTALLATION_EXECUTABLE% to temporary location %DEST_INSTALLATION_EXECUTABLE%
copy /Y "%SRC_INSTALLATION_EXECUTABLE%" "%DEST_INSTALLATION_EXECUTABLE%" || goto :UNINSTALL_ERROR

echo Deleting installer %SRC_INSTALLATION_EXECUTABLE%
del /Q "%SRC_INSTALLATION_EXECUTABLE%" || goto :UNINSTALL_ERROR

echo create unistallation batch to continue work in a safe place
echo create unistallation batch to continue work in a safe place
echo echo ================================================================>>"%DE_INTALLER_BATCH%"
echo echo Uninstalling tailviewer>>"%DE_INTALLER_BATCH%"
echo.>>"%DE_INTALLER_BATCH%"
echo "%DEST_INSTALLATION_EXECUTABLE%" uninstall "%INSTALLATION_PATH%" ^|^| goto :UNINSTALL_ERROR>>"%DE_INTALLER_BATCH%"
echo.>>"%DE_INTALLER_BATCH%"
echo echo Deleting temporary installer %DEST_INSTALLATION_EXECUTABLE%>>"%DE_INTALLER_BATCH%"
echo del /Q "%DEST_INSTALLATION_EXECUTABLE%" ^|^| goto :UNINSTALL_ERROR>>"%DE_INTALLER_BATCH%"
echo.>>"%DE_INTALLER_BATCH%"
echo echo Safely ignore the following error message; it can not be avoided>>"%DE_INTALLER_BATCH%"
echo del /Q "%DE_INTALLER_BATCH%">>"%DE_INTALLER_BATCH%"

:: the end
echo :UNINSTALL_ERROR>>"%DE_INTALLER_BATCH%"
echo echo An error occured during uninstallation>>"%DE_INTALLER_BATCH%"
echo exit /b -1>>"%DE_INTALLER_BATCH%"
:: call the safe unistaller which terminates this job
echo run the rest in a safe place
"%DE_INTALLER_BATCH%"

goto :UNINSTALL_SUCCESS

:UNINSTALL_SUCCESS
echo Uninstalling tailviewer succeeded
exit /b 0

:UNINSTALL_ERROR
echo An error occured during uninstallation
exit /b -1

endlocal
