[Setup]
AppName=System&&Game Manager
AppVersion=0.1.0
DefaultDirName={autopf}\SystemGameManager
DefaultGroupName=SystemGameManager
OutputDir=Output
OutputBaseFilename=setup
Compression=lzma
SolidCompression=yes

[Files]
Source: "bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\SystemGame Manager"; Filename: "{app}\SystemGameManager.exe"
Name: "{commondesktop}\SystemGame Manager"; Filename: "{app}\SystemGameManager.exe"

[Run]
Filename: "{app}\SystemGameManager.exe"; Description: "Programm starten"; Flags: nowait postinstall skipifsilent