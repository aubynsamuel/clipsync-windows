; Script for ClipSync Installer
#define MyAppName "ClipSync"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Samuel Aubyn"
#define MyAppExeName "ClipSyncWindows.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application
AppId={{156DCBB3-4DFE-44ED-AA91-736BABE00F9D}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
; Architecture settings
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; Output settings
OutputDir=.
OutputBaseFilename=ClipSync_Setup
SetupIconFile=publish\app.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
; Add Windows startup option
DisableProgramGroupPage=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startupicon"; Description: "Start ClipSync when Windows starts"; GroupDescription: "Startup options:"; Flags: unchecked

[Files]
; Include ALL files from the publish directory
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon;IconFilename: "{app}\app.ico"
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startupicon; IconFilename: "{app}\app.ico" 

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
