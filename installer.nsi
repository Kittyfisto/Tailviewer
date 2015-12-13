# define name of installer
OutFile "Tailviewer-setup.exe" 

InstallDir "$PROGRAMFILES\Tailviewer ALPHA"
 
; Request application privileges for Windows Vista
RequestExecutionLevel admin
 
# start default section
Section
 
    # set the installation directory as the destination for the following actions
    SetOutPath $INSTDIR
		File "bin\Release\Tailviewer.exe"
 
    # create the uninstaller
    WriteUninstaller "$INSTDIR\uninstall.exe"
 
	CreateShortCut "$DESKTOP\Tailviewer ALPHA.lnk" "$INSTDIR\Tailviewer.exe"
 
    # create a shortcut named "new shortcut" in the start menu programs directory
    # point the new shortcut at the program uninstaller
    CreateShortCut "$SMPROGRAMS\new shortcut.lnk" "$INSTDIR\uninstall.exe"
SectionEnd
 
# uninstaller section start
Section "uninstall"
 
    # first, delete the uninstaller
    Delete "$INSTDIR\uninstall.exe"
 
    # second, remove the link from the start menu
    Delete "$SMPROGRAMS\new shortcut.lnk"
 
# uninstaller section end
SectionEnd