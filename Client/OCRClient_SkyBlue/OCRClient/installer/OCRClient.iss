; Inno Setup 6.3+ (miễn phí: https://jrsoftware.org/isinfo.php)
; Cách dùng: build Release trước, rồi mở file này bằng Inno Setup và bấm Compile.
; Kết quả: installer\Output\OCRClient_Setup.exe

#define MyAppName "OCR Client"
#define MyAppVersion "1.0.0"
#define MyAppExe "OCRClient.exe"

[Setup]
AppId={{B7A2E4C1-5D3F-4E8A-9C10-2F6A7B8D9E01}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\OCRClient
DefaultGroupName={#MyAppName}
OutputDir=Output
OutputBaseFilename=OCRClient_Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequiredOverridesAllowed=dialog
UninstallDisplayIcon={app}\{#MyAppExe}
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Tạo biểu tượng ngoài Desktop"; GroupDescription: "Biểu tượng:"

[Files]
Source: "..\bin\Release\net48\*"; DestDir: "{app}"; Excludes: "*.pdb,*.xml"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExe}"; Description: "Chạy {#MyAppName}"; Flags: nowait postinstall skipifsilent
