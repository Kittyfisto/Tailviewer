# define name of installer
OutFile "SharpTail-setup.exe" 

InstallDir "$PROGRAMFILES\SharpTail ALPHA"
 
# For removing Start Menu shortcut in Windows 7
RequestExecutionLevel user
 
# start default section
Section
 
    # set the installation directory as the destination for the following actions
    SetOutPath $INSTDIR
		File "bin\Release\SharpTail.exe"
 
    # create the uninstaller
    WriteUninstaller "$INSTDIR\uninstall.exe"
 
	CreateShortCut "$DESKTOP\SharpTail ALPHA.lnk" "$INSTDIR\SharpTail.exe"
 
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