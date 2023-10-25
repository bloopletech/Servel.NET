#define MyAppName "Servel.NET"
#define MyAppVersion "1.7.0"
#define MyAppPublisher "Servel.NET"
#define MyAppURL "https://www.github.com/bloopletech/Servel.NET"
#define MyAppExeName "Servel.NET.exe"

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
OutputBaseFilename={#MyAppName} Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\{#MyAppName}\bin\x64\Release\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Run]
Filename: {sys}\netsh.exe; Parameters: "advfirewall firewall add rule name={#MyAppName} dir=in action=allow program=""{app}\{#MyAppExeName}"" profile=Private enable=yes" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "create {#MyAppName} binPath=""{app}\{#MyAppExeName}"" start=auto" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "description {#MyAppName} ""Serves directories on your computer to your local network over HTTP/HTTPS.""" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "start {#MyAppName}" ; Flags: runhidden

[UninstallRun]
Filename: {sys}\sc.exe; Parameters: "stop {#MyAppName}" ; Flags: runhidden ; RunOnceId: "StopService"
Filename: {sys}\sc.exe; Parameters: "delete {#MyAppName}" ; Flags: runhidden ; RunOnceId: "DeleteService"
Filename: {sys}\netsh.exe; Parameters: "advfirewall firewall delete rule name={#MyAppName} program=""{app}\{#MyAppExeName}""" ; Flags: runhidden ; RunOnceId: "DeleteFirewallRule"

[Code]
// From https://stackoverflow.com/a/72526428
function IsDotNetInstalled(DotNetName: string): Boolean;
var
  Cmd, Args: string;
  FileName: string;
  Output: AnsiString;
  Command: string;
  ResultCode: Integer;
begin
  FileName := ExpandConstant('{tmp}\dotnet.txt');
  Cmd := ExpandConstant('{cmd}');
  Command := 'dotnet --list-runtimes';
  Args := '/C ' + Command + ' > "' + FileName + '" 2>&1';
  if Exec(Cmd, Args, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and
     (ResultCode = 0) then
  begin
    if LoadStringFromFile(FileName, Output) then
    begin
      if Pos(DotNetName, Output) > 0 then
      begin
        Log('"' + DotNetName + '" found in output of "' + Command + '"');
        Result := True;
      end
        else
      begin
        Log('"' + DotNetName + '" not found in output of "' + Command + '"');
        Result := False;
      end;
    end
      else
    begin
      Log('Failed to read output of "' + Command + '"');
    end;
  end
    else
  begin
    Log('Failed to execute "' + Command + '"');
    Result := False;
  end;
  DeleteFile(FileName);
end;

function InitializeSetup: Boolean;
var
  dotNet7Installed: Boolean;
  ErrorCode: Integer;
begin
  Result := True;

  dotNet7Installed := IsDotNetInstalled('Microsoft.AspNetCore.App 7.');

  if not dotNet7Installed then
  begin
    MsgBox('The .ASP.NET Core Runtime 7 is required.' + #13#10 + 'Please download and install it, and then re-run this setup program.'
      + #13#10 + 'When you click OK the download page will be opened.', mbCriticalError, MB_OK);
    ShellExecAsOriginalUser('', 'https://dotnet.microsoft.com/en-us/download/dotnet/7.0', '', '', SW_SHOW, ewNoWait, ErrorCode)
    Result := False;
  end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var ResultCode: Integer;
begin  
  Exec(ExpandConstant('{sys}\sc.exe'), 'stop {#MyAppName}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Exec(ExpandConstant('{sys}\sc.exe'), 'delete {#MyAppName}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Exec(ExpandConstant('{sys}\netsh.exe'), ExpandConstant('advfirewall firewall delete rule name={#MyAppName} program="{app}\{#MyAppExeName}"'), '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := '';
end;

