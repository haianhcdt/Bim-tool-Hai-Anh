; Script Inno Setup táº¡o bá»™ cÃ i Ä‘áº·t add-in BIM TOOL cho Autodesk Revit 2023
; TÃ¡c giáº£: Nguyá»…n TÃ´n Háº£i Anh - BIM Manager (BVN)

#define MyAppName "BIM TOOL for Revit 2023"
#define MyAppVersion "1.5.1"
#define MyAppPublisher "Nguyá»…n TÃ´n Háº£i Anh (BVN)"
#define MyAppId "{{A24532C5-ACDE-4946-8414-D8EE406FA589}"
#define AddinDir "{userappdata}\Autodesk\Revit\Addins\2023"

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={#AddinDir}
DisableDirPage=yes
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=Setup_BIMTool_Revit2023
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
CreateUninstallRegKey=yes
UninstallDisplayName={#MyAppName}
UninstallFilesDir={#AddinDir}\BIMTool\Uninstall

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "BIMTool.Revit2023.addin"; DestDir: "{#AddinDir}"; Flags: ignoreversion
Source: "BIMTool\DSCons.Revit.Starter.dll"; DestDir: "{#AddinDir}\BIMTool"; Flags: ignoreversion
Source: "BIMTool\DSCons.Revit.Starter.Core.dll"; DestDir: "{#AddinDir}\BIMTool"; Flags: ignoreversion
Source: "HUONG_DAN_CAI_DAT_REVIT_2023.md"; DestDir: "{#AddinDir}\BIMTool"; Flags: ignoreversion isreadme

[UninstallDelete]
Type: files; Name: "{#AddinDir}\BIMTool.Revit2023.addin"
Type: filesandordirs; Name: "{#AddinDir}\BIMTool"

[Code]
function IsRevitRunning(): Boolean;
var
  ResultCode: Integer;
begin
  Result := False;
  Exec('cmd.exe', '/c tasklist /FI "IMAGENAME eq Revit.exe" | findstr /I "Revit.exe"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  if ResultCode = 0 then
    Result := True;
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  while IsRevitRunning() do
  begin
    if MsgBox('PhÃ¡t hiá»‡n Autodesk Revit Ä‘ang má»Ÿ.' #13#10 #13#10 +
              'Vui lÃ²ng lÆ°u file vÃ  Ä‘Ã³ng Revit trÆ°á»›c khi tiáº¿p tá»¥c cÃ i Ä‘áº·t add-in.',
              mbConfirmation, MB_RETRYCANCEL) = IDCANCEL then
    begin
      Result := False;
      Exit;
    end;
  end;
end;

function InitializeUninstall(): Boolean;
begin
  Result := True;
  while IsRevitRunning() do
  begin
    if MsgBox('PhÃ¡t hiá»‡n Autodesk Revit Ä‘ang má»Ÿ.' #13#10 #13#10 +
              'Vui lÃ²ng lÆ°u file vÃ  Ä‘Ã³ng Revit trÆ°á»›c khi gá»¡ cÃ i Ä‘áº·t add-in.',
              mbConfirmation, MB_RETRYCANCEL) = IDCANCEL then
    begin
      Result := False;
      Exit;
    end;
  end;
end;
