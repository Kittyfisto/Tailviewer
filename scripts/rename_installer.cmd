@setlocal enabledelayedexpansion
@echo off

if not [%APPVEYOR_PULL_REQUEST_NUMBER%] == [] (
	set BRANCH_NAME=%APPVEYOR_PULL_REQUEST_HEAD_REPO_BRANCH%
) else (
	if not [%APPVEYOR_REPO_BRANCH%] == [] (
		set BRANCH_NAME=%APPVEYOR_REPO_BRANCH%
	) else (
		set BRANCH_NAME=master
	)
)
set BRANCH_NAME=%BRANCH_NAME:/=_%
set BRANCH_NAME=%BRANCH_NAME:\=_%
for /f %%i in ('git rev-parse --short HEAD') do set COMMIT_HASH=%%i

echo We're on %BRANCH_NAME%
SET source_installer_name="Tailviewer-setup.exe"

if [%BRANCH_NAME%] == [master] (
	if [%APPVEYOR_BUILD_VERSION%] == [] goto :NO_VERSION
	set dest_portable_name=Tailviewer-portable-%APPVEYOR_BUILD_VERSION%.zip
	set dest_installer_name=Tailviewer-setup-%APPVEYOR_BUILD_VERSION%.exe
) else (
	set dest_portable_name=Tailviewer-portable-%BRANCH_NAME%-%COMMIT_HASH%.zip
	set dest_installer_name=Tailviewer-setup-branch-%BRANCH_NAME%-%COMMIT_HASH%.exe
)

echo Renaming installer to %dest_installer_name%
ren %source_installer_name% "%dest_installer_name%" || goto :MOVE_ERROR

echo Executing installer silently....
call "%dest_installer_name%" silentinstall || goto :INSTALLATION_FAILED
echo Installation succeeded!

set install_directory=%PROGRAMFILES%\Tailviewer
set portable_full_name=%cd%\%dest_portable_name%
echo Creating portable version from %install_directory% to %portable_full_name%
tar -a -cvf "%portable_full_name%" -C "%install_directory%" * || goto :PORTABLE_FAILED
echo Successfully created portable archive!

exit /b 0

:NO_VERSION
echo ERROR: Missing environment variable APPVEYOR_BUILD_VERSION!
exit /b -1

:MOVE_ERROR
echo ERROR: Unable to rename installer, ren returned %errorlevel%
exit /b -2

:INSTALLATION_FAILED
echo ERROR: Installation failed, setup returned %errorlevel%
exit /b -3

:PORTABLE_FAILED
echo ERROR: Creating zip archive from installation failed, tar returned %errorlevel% 
exit /b -4

endlocal