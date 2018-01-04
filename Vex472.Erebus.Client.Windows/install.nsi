Name "Erebus Client 1.0"
OutFile "out\erebus_1.0.exe"
InstallDir $PROGRAMFILES\Erebus\Client
InstallDirRegKey HKLM "SOFTWARE\472\Erebus\Client" "install_dir"
RequestExecutionLevel admin
Icon icon.ico

Page components
Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

Section "Erebus Client (required)"
SectionIn RO
SetOutPath $INSTDIR
File /r /x *.xml /x *.vshost.exe /x *.config /x *.pdb /x *.log /x *.dat /x *.vshost.exe.manifest bin\Release\*.*
Rename bin\Release _bin
RMDir /r bin
Rename _bin bin
WriteRegStr HKLM "SOFTWARE\472\Erebus\Client" "install_dir" $INSTDIR
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Erebus Client" "DisplayName" "Erebus Client"
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Erebus Client" "UninstallString" '"$INSTDIR\uninstall.exe"'
WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Erebus Client" "NoModify" 1
WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Erebus Client" "NoRepair" 1
WriteUninstaller "uninstall.exe"
SectionEnd

Section "Start Menu Shortcuts"
CreateDirectory "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Erebus"
CreateShortCut "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Erebus\Uninstall Erebus.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
CreateShortCut "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Erebus\Erebus Client.lnk" "$INSTDIR\Vex472.Erebus.Client.Windows.exe" "" "$INSTDIR\Vex472.Erebus.Client.Windows.exe" 0
SectionEnd

Section "Uninstall"
RMDir /r /REBOOTOK $INSTDIR
ReadRegStr $0 HKCU "SOFTWARE\472\Erebus\Client" "logfile"
ReadRegStr $1 HKCU "SOFTWARE\472\Erebus\Client" "keyfile"
Delete /REBOOTOK $0
Delete /REBOOTOK $1
DeleteRegKey HKLM "SOFTWARE\472\Erebus\Client"
DeleteRegKey HKCU "SOFTWARE\472\Erebus\Client"
DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Erebus Client"
RMDir /r "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Erebus"
SectionEnd