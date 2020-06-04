@setlocal EnableDelayedExpansion
@echo off

set SCRIPT_DIR=%~dp0
set SRC_DIR=%SCRIPT_DIR%src
set ASSEMBLYINFO_TOOL=%SCRIPT_DIR%tools\BuildTool.exe
set CRYPT_TOOL=%SCRIPT_DIR%tools\Crypt.exe
set OFFICIAL_KEY=%SCRIPT_DIR%sig\sns
set DEVELOPER_KEY=%SCRIPT_DIR%sig\DeveloperKey.snk
set CURRENT_KEY=%SCRIPT_DIR%sig\Key.snk
set "TAILVIEWER_ASSEMBLIES=Installer Tailviewer Tailviewer.AcceptanceTests Tailviewer.Api archiver Tailviewer.Archiver.Test Tailviewer.Core Tailviewer.PluginRepository Tailviewer.PluginRepository.Service Tailviewer.PluginRepository.Test Tailviewer.SystemTests Tailviewer.Test"

echo Restoring official key (strong name key file)...

if not exist "%OFFICIAL_KEY%" goto :OFFICIAL_KEY_MISSING
if not exist "%CURRENT_KEY%" goto :START

rem We assume that if the current key and the official key are different, then we have restored the official key.
rem The reason being is that we do not want to CONSTANTLY decrypt the official key to the harddisk (for obvious reasons).
fc /B "%DEVELOPER_KEY%" "%CURRENT_KEY%">nul
if %errorlevel% == 1 goto :NO_RESTORE_NECESSARY

:START

echo Decrypting official key...
"%CRYPT_TOOL%" decrypt "%OFFICIAL_KEY%" "%CURRENT_KEY%" || goto :DECRYPT_ERROR

echo Modifying AssemblyInfo.cs files of the entire solution...
for /r "%SRC_DIR%" %%i in (*.*) do call :CHANGE_ASSEMBLYINFO %%i || goto :ASSEMBLYINFO_ERROR

echo Modifying assembly redirections in app.config files...
call :GENERATE_REDIRECTS "src\Tailviewer\App.config"
call :GENERATE_REDIRECTS "src\Tailviewer.PluginRepository\App.config"
call :GENERATE_REDIRECTS "src\Tailviewer.PluginRepository.Service\App.config"

echo SUCESS: Tailviewer now compiles with the official strong name key
exit /b 0

:OFFICIAL_KEY_MISSING
echo ERROR: Official key %OFFICIAL_KEY% is missing or cannot be accessed.
exit /b -1

:NO_RESTORE_NECESSARY
echo Official key has already been restored, nothing needs to be done
exit /b 0

:DECRYPT_ERROR
echo ERROR: Unable to decrypt official key
exit /b -2

:ASSEMBLYINFO_ERROR
echo ERROR: There was an error modifying AssemblyInfo.cs: AssemblyInfo.exe returned %errorlevel%
exit /b -3

:COPY_ERROR
echo ERROR: There was an error copying the developer key: copy returned %errorlevel%
exit /b -4

:CHANGE_ASSEMBLYINFO
set ASSEMBLY_INFO_FULL_PATH=%~1
set ASSEMBLY_INFO_FILE_NAME=%~n1%~x1
if not %ASSEMBLY_INFO_FILE_NAME% == AssemblyInfo.cs exit /b 0
echo Patching %ASSEMBLY_INFO_FULL_PATH%...
"%ASSEMBLYINFO_TOOL%" patch-internals-visible-to --assembly-info "%ASSEMBLY_INFO_FULL_PATH%" --key-path "%CURRENT_KEY%" --assemblies %TAILVIEWER_ASSEMBLIES%
exit /b %errorlevel%

:GENERATE_REDIRECTS
set APP_CONFIG_PATH=%~1
tools\GenerateRedirects.exe --appconfig %APP_CONFIG_PATH% --assemblyname "Tailviewer.Api" --assemblyinfo "src\GlobalAssemblyInfo.cs" || exit /b %errorlevel%
tools\GenerateRedirects.exe --appconfig %APP_CONFIG_PATH% --assemblyname "Tailviewer.Core" --assemblyinfo "src\GlobalAssemblyInfo.cs" || exit /b %errorlevel%
tools\GenerateRedirects.exe --appconfig %APP_CONFIG_PATH% --assemblyname "archive" --assemblyinfo "src\GlobalAssemblyInfo.cs" || exit /b %errorlevel%
exit /b 0

endlocal