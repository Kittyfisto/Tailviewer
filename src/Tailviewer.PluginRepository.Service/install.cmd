@echo off
setlocal

call set_environment.cmd
call require_admin.cmd || exit /b 1

set INSTALL_PATH=%ProgramFiles%\Tailviewer.PluginRepository
set OLD_REPO=%INSTALL_PATH%\repository.exe
set DATABASE_FOLDER=%ALLUSERSPROFILE%\Tailviewer.PluginRepository
set DATABASE_PATH=%DATABASE_FOLDER%\Plugins.isdb
set DATABASE_EXPORT_PATH=%temp%\tailviewer.pluginrepository\upgrade
set DATABASE_IMPORT_SCRIPT=%DATABASE_EXPORT_PATH%\import.cmd

if exist "%OLD_REPO%" (
    pushd "%INSTALL_PATH%"
    call "delete_service.cmd" || goto :ERROR
    popd

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
xcopy "%ScriptDir%*" "%INSTALL_PATH%" /i /s || goto :ERROR

if exist "%DATABASE_IMPORT_SCRIPT%" (
    pushd "%INSTALL_PATH%
    echo Import database...
    call "%DATABASE_IMPORT_SCRIPT%" || goto :ERROR
    popd
)

echo Starting service...
pushd %INSTALL_PATH%
call "create_service.cmd" || goto :ERROR
call "start_service.cmd" || goto :ERROR
popd

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
