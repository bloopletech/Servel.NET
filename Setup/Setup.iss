#define MyAppName "Servel.NET"
#define MyAppPublisher "Servel.NET"
#define MyAppURL "https://www.github.com/bloopletech/Servel.NET"
#define MyAppExeName "Servel.NET.exe"
#define MyAppInputPath "..\" + MyAppName + "\bin\x64\Release\win-x64\publish\" + MyAppExeName
#define _MyAppVersion GetStringFileInfo(MyAppInputPath, "ProductVersion")
#define MyAppVersion Copy(_MyAppVersion, 1, Pos("+", _MyAppVersion) - 1)

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{DDA150A5-254B-4CB2-8E0E-C28B7648F8BF}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir=bin
OutputBaseFilename={#MyAppName}.Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: {#MyAppInputPath}; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Run]
Filename: {sys}\netsh.exe; Parameters: "advfirewall firewall add rule name={#MyAppName} dir=in action=allow program=""{app}\{#MyAppExeName}"" profile=Private enable=yes" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "create {#MyAppName} binPath=""{app}\{#MyAppExeName}"" start=auto" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "description {#MyAppName} ""Serves directories on your computer to your local network over HTTP/HTTPS.""" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "start {#MyAppName}" ; Flags: runhidden

Filename: "http://localhost:9292/"; Flags: shellexec postinstall skipifsilent; Description: "Connect to {#MyAppName}"
Filename: {sys}\notepad.exe; Parameters: "{app}\Configuration.json"; WorkingDir: "{app}"; Flags: runascurrentuser nowait postinstall skipifsilent; Description: "Edit Configuration File"


[UninstallRun]
Filename: {sys}\sc.exe; Parameters: "stop {#MyAppName}" ; Flags: runhidden ; RunOnceId: "StopService"
Filename: {sys}\sc.exe; Parameters: "delete {#MyAppName}" ; Flags: runhidden ; RunOnceId: "DeleteService"
Filename: {sys}\netsh.exe; Parameters: "advfirewall firewall delete rule name={#MyAppName} program=""{app}\{#MyAppExeName}""" ; Flags: runhidden ; RunOnceId: "DeleteFirewallRule"

[Code]
function PrepareToInstall(var NeedsRestart: Boolean): String;
var ResultCode: Integer;
begin  
  Exec(ExpandConstant('{sys}\sc.exe'), 'stop {#MyAppName}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Exec(ExpandConstant('{sys}\sc.exe'), 'delete {#MyAppName}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Exec(ExpandConstant('{sys}\netsh.exe'), ExpandConstant('advfirewall firewall delete rule name={#MyAppName} program="{app}\{#MyAppExeName}"'), '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := '';
end;

