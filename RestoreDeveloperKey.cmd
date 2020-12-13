@setlocal EnableDelayedExpansion
@echo off

set SCRIPT_DIR=%~dp0
set SRC_DIR=%SCRIPT_DIR%src
set ASSEMBLYINFO_TOOL=%SCRIPT_DIR%tools\BuildTool.exe
set DEVELOPER_KEY=%SCRIPT_DIR%sig\DeveloperKey.snk
set CURRENT_KEY=%SCRIPT_DIR%sig\Key.snk
set "TAILVIEWER_ASSEMBLIES=Installer Tailviewer Tailviewer.AcceptanceTests Tailviewer.Api archiver Tailviewer.Archiver.Test Tailviewer.Core Tailviewer.PluginRepository Tailviewer.PluginRepository.Service Tailviewer.PluginRepository.Test Tailviewer.SystemTests Tailviewer.Test"

echo Restoring developer key (strong name key file)...

if not exist "%DEVELOPER_KEY%" goto :DEVELOPER_KEY_MISSING

fc /B "%DEVELOPER_KEY%" "%CURRENT_KEY%">nul
if %errorlevel% == 0 goto :NO_RESTORE_NECESSARY

echo Modifying AssemblyInfo.cs files of the entire solution...
for /r "%SRC_DIR%" %%i in (*.*) do call :CHANGE_ASSEMBLYINFO %%i || goto :ASSEMBLYINFO_ERROR

echo Modifying assembly redirections in app.config files...
call :GENERATE_REDIRECTS "src\Tailviewer\App.config"
call :GENERATE_REDIRECTS "src\Tailviewer.PluginRepository\App.config"
call :GENERATE_REDIRECTS "src\Tailviewer.PluginRepository.Service\App.config"

echo Copying developer key to current key file...
COPY "%DEVELOPER_KEY%" "%CURRENT_KEY%" || goto :COPY_ERROR

echo SUCESS: Tailviewer now compiles with DeveloperKey.snk
exit /b 0

:DEVELOPER_KEY_MISSING
echo ERROR: Developer key %DEVELOPER_KEY% is missing or cannot be accessed.
exit /b -1

:NO_RESTORE_NECESSARY
echo Developer key has already been restored, nothing needs to be done
exit /b 0

:COPY_ERROR
echo ERROR: There was an error copying the developer key: copy returned %errorlevel%
exit /b -3

:CHANGE_ASSEMBLYINFO
set ASSEMBLY_INFO_FULL_PATH=%~1
set ASSEMBLY_INFO_FILE_NAME=%~n1%~x1
if not %ASSEMBLY_INFO_FILE_NAME% == AssemblyInfo.cs exit /b 0
echo Patching %ASSEMBLY_INFO_FULL_PATH%...
"%ASSEMBLYINFO_TOOL%" patch-internals-visible-to --assembly-info "%ASSEMBLY_INFO_FULL_PATH%" --key-path "%DEVELOPER_KEY%" --assemblies %TAILVIEWER_ASSEMBLIES%
exit /b %errorlevel%

:GENERATE_REDIRECTS
set APP_CONFIG_PATH=%~1
tools\GenerateRedirects.exe --appconfig %APP_CONFIG_PATH% --assemblyname "Tailviewer.Api" --assemblyinfo "src\GlobalAssemblyInfo.cs" || exit /b %errorlevel%
tools\GenerateRedirects.exe --appconfig %APP_CONFIG_PATH% --assemblyname "Tailviewer.Core" --assemblyinfo "src\GlobalAssemblyInfo.cs" || exit /b %errorlevel%
tools\GenerateRedirects.exe --appconfig %APP_CONFIG_PATH% --assemblyname "archive" --assemblyinfo "src\GlobalAssemblyInfo.cs" || exit /b %errorlevel%
exit /b 0

:ASSEMBLYINFO_ERROR
echo ERROR: There was an error modifying AssemblyInfo.cs: AssemblyInfo.exe returned %errorlevel%
exit /b -4

endlocal