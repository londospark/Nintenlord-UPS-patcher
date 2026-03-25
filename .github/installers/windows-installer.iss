; Inno Setup Script for Nintenlord UPS Patcher
#define MyAppName "Nintenlord UPS Patcher"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "londospark"
#define MyAppURL "https://github.com/londospark/Nintenlord-UPS-patcher"
#define MyAppExeName "Nintenlord UPS patcher.Avalonia.exe"

[Setup]
AppId={{B8E9D9F0-8F4E-4E8E-9E8E-8E8E8E8E8E8E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
OutputDir=.
OutputBaseFilename=Nintenlord-UPS-Patcher-Setup-{#RUNTIME}
SetupIconFile=..\..\Nintenlord UPS patcher\NUPS icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed={#ARCH_ALLOWED}
ArchitecturesInstallIn64BitMode={#ARCH_64BIT}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\..\publish\{#RUNTIME}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
