@echo off
setlocal

net session >nul 2>&1
if not %errorLevel% == 0 goto :NOADMIN

set SCRIPT_DIR=%~dp0
set INSTALL_PATH=%ProgramFiles%\Tailviewer.PluginRepository
set OLD_REPO=%INSTALL_PATH%\repository.exe
set DATABASE_FOLDER=%ALLUSERSPROFILE%\Tailviewer.PluginRepository
set DATABASE_PATH=%DATABASE_FOLDER%\Plugins.isdb
set DATABASE_EXPORT_PATH=%temp%\tailviewer.pluginrepository\upgrade
set DATABASE_IMPORT_SCRIPT=%DATABASE_EXPORT_PATH%\import.cmd

if exist "%OLD_REPO%" (
    call "%INSTALL_PATH%\stop_service.cmd" || goto :ERROR

    echo Exporting database...
    "%OLD_REPO%" export "%DATABASE_EXPORT_PATH%" || goto :ERROR

    echo Removing previous installation...
    rmdir /s /q "%INSTALL_PATH%"
)

if exist "%DATABASE_PATH%" (
    echo Renaming old database...
    if exist "%DATABASE_FOLDER%\Plugins.isdb.old" del "%DATABASE_FOLDER%\Plugins.isdb.old"

    ren "%DATABASE_PATH%" "Plugins.isdb.old"
    timeout /t 1 /nobreak > NUL
)

echo Installing...
if not exist "%INSTALL_PATH%" mkdir "%INSTALL_PATH%"
xcopy "%SCRIPT_DIR%*" "%INSTALL_PATH%" /i /s || goto :ERROR

if exist "%DATABASE_IMPORT_SCRIPT%" (
    echo Import database...
    call "%DATABASE_IMPORT_SCRIPT%" || goto :ERROR
)

echo Starting service...
call "%INSTALL_PATH%\create_service.cmd" || goto :ERROR
call "%INSTALL_PATH%\start_service.cmd" || goto :ERROR

goto :SUCCESS

:NOADMIN
echo NO administrative permissions found.
exit /b -1

:ERROR
echo Installation failed!
exit /b %errorlevel%

:SUCCESS
echo Installation successful!
exit /b 0

endlocal
