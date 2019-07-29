@echo off
setlocal

set SCRIPT_DIR=%~dp0
set ZIP=%SCRIPT_DIR%..\..\tools\zip.exe
set BIN_DIR=%SCRIPT_DIR%..\..\bin
set PLUGIN_REPO_SRC_DIR=%SCRIPT_DIR%
set ZIP_FILE=%BIN_DIR%\repository.zip

if not exist "%ZIP%" goto :NOZIP

rem We need to create an example configuration that we can put into the archive...
"%BIN_DIR%\repository.exe" write-configuration "%BIN_DIR%\example-configuration.xml"

if exist "%ZIP_FILE%" del "%ZIP_FILE%"
"%ZIP%" --output "%ZIP_FILE%" --append --input "%BIN_DIR%\repository.exe" ^
 "%BIN_DIR%\repository.exe.config" ^
 "%BIN_DIR%\example-configuration.xml" ^
 "%BIN_DIR%\repository-svc.exe" ^
 "%BIN_DIR%\repository-svc.exe.config" ^
 "%BIN_DIR%\set_environment.cmd" ^
 "%BIN_DIR%\require_admin.cmd" ^
 "%BIN_DIR%\install.cmd" ^
 "%BIN_DIR%\create_service.cmd" ^
 "%BIN_DIR%\delete_service.cmd" ^
 "%BIN_DIR%\start_service.cmd" ^
 "%BIN_DIR%\stop_service.cmd" ^
 "%PLUGIN_REPO_SRC_DIR%\README.md" ^
 "%BIN_DIR%\Tailviewer.Api.dll" ^
 "%BIN_DIR%\Tailviewer.Core.dll" ^
 "%BIN_DIR%\archive.exe" ^
 "%BIN_DIR%\System.Extensions.dll" ^
 "%BIN_DIR%\SharpRemote.dll" ^
 "%BIN_DIR%\Metrolib.dll" ^
 "%BIN_DIR%\IsabelDb.dll" ^
 "%BIN_DIR%\PE.dll" ^
 "%BIN_DIR%\protobuf-net.dll" ^
 "%BIN_DIR%\log4net.dll" ^
 "%BIN_DIR%\CommandLine.dll" ^
 "%BIN_DIR%\System.Data.SQLite.dll"

"%ZIP%" --output "%ZIP_FILE%" --input "%BIN_DIR%\x86\SQLite.Interop.dll" --folder x86 --append
"%ZIP%" --output "%ZIP_FILE%" --input "%BIN_DIR%\x64\SQLite.Interop.dll" --folder x64 --append

rem zip -r -p "%ZIP_FILE%" "%BIN_DIR%\repository.exe"
rem zip -r -p "%ZIP_FILE%" "%BIN_DIR%\repository.exe.config"
goto :DONE

:NOZIP
echo Could not find %ZIP%
exit /b -1

:DONE
echo Created zip file %ZIP_FILE%

endlocal
