; ==============================================================================
; Script Inno Setup tạo bộ cài đặt đa phiên bản BIM TOOL (Revit 2019 - 2024)
; Tác giả: Nguyễn Tôn Hải Anh - BIM Manager (BVN)
; ==============================================================================

#define MyAppName "BIM TOOL Multi-Version Suite"
#define MyAppVersion "1.5.2"
#define MyAppPublisher "Nguyễn Tôn Hải Anh (BVN)"
#define MyAppId "{{A24532C5-ACDE-4946-8414-D8EE406FA589}"

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={userappdata}\Autodesk\Revit\Addins
DisableDirPage=yes
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=Setup_BIMTool_MultiVersion_2019_2024
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
CreateUninstallRegKey=yes
UninstallDisplayName={#MyAppName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Types]
Name: "full"; Description: "Cài đặt toàn bộ phiên bản phát hiện được"
Name: "custom"; Description: "Tùy chỉnh chọn phiên bản Revit"; Flags: iscustom

[Components]
Name: "revit2019"; Description: "Autodesk Revit 2019"; Types: full custom
Name: "revit2021"; Description: "Autodesk Revit 2021"; Types: full custom
Name: "revit2022"; Description: "Autodesk Revit 2022"; Types: full custom
Name: "revit2023"; Description: "Autodesk Revit 2023"; Types: full custom
Name: "revit2024"; Description: "Autodesk Revit 2024"; Types: full custom

[Files]
; Revit 2019
Source: "Revit2019\BIMTool.Revit2019.addin"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2019"; Components: revit2019; Flags: ignoreversion
Source: "Revit2019\BIMTool\*"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2019\BIMTool"; Components: revit2019; Flags: ignoreversion recursesubdirs createallsubdirs

; Revit 2021
Source: "Revit2021\BIMTool.Revit2021.addin"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2021"; Components: revit2021; Flags: ignoreversion
Source: "Revit2021\BIMTool\*"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2021\BIMTool"; Components: revit2021; Flags: ignoreversion recursesubdirs createallsubdirs

; Revit 2022
Source: "Revit2022\BIMTool.Revit2022.addin"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2022"; Components: revit2022; Flags: ignoreversion
Source: "Revit2022\BIMTool\*"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2022\BIMTool"; Components: revit2022; Flags: ignoreversion recursesubdirs createallsubdirs

; Revit 2023
Source: "Revit2023\BIMTool.Revit2023.addin"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2023"; Components: revit2023; Flags: ignoreversion
Source: "Revit2023\BIMTool\*"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2023\BIMTool"; Components: revit2023; Flags: ignoreversion recursesubdirs createallsubdirs

; Revit 2024
Source: "Revit2024\BIMTool.Revit2024.addin"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2024"; Components: revit2024; Flags: ignoreversion
Source: "Revit2024\BIMTool\*"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2024\BIMTool"; Components: revit2024; Flags: ignoreversion recursesubdirs createallsubdirs

; Hướng dẫn sử dụng
Source: "HUONG_DAN_CAI_DAT_TOAN_DIEN.md"; DestDir: "{userappdata}\Autodesk\Revit\Addins"; Flags: ignoreversion isreadme

[UninstallDelete]
Type: files; Name: "{userappdata}\Autodesk\Revit\Addins\2019\BIMTool.Revit2019.addin"
Type: filesandordirs; Name: "{userappdata}\Autodesk\Revit\Addins\2019\BIMTool"
Type: files; Name: "{userappdata}\Autodesk\Revit\Addins\2021\BIMTool.Revit2021.addin"
Type: filesandordirs; Name: "{userappdata}\Autodesk\Revit\Addins\2021\BIMTool"
Type: files; Name: "{userappdata}\Autodesk\Revit\Addins\2022\BIMTool.Revit2022.addin"
Type: filesandordirs; Name: "{userappdata}\Autodesk\Revit\Addins\2022\BIMTool"
Type: files; Name: "{userappdata}\Autodesk\Revit\Addins\2023\BIMTool.Revit2023.addin"
Type: filesandordirs; Name: "{userappdata}\Autodesk\Revit\Addins\2023\BIMTool"
Type: files; Name: "{userappdata}\Autodesk\Revit\Addins\2024\BIMTool.Revit2024.addin"
Type: filesandordirs; Name: "{userappdata}\Autodesk\Revit\Addins\2024\BIMTool"

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
    if MsgBox('Phát hiện Autodesk Revit đang mở.' #13#10 #13#10 +
              'Vui lòng lưu file và đóng toàn bộ Revit trước khi tiếp tục cài đặt.',
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
    if MsgBox('Phát hiện Autodesk Revit đang mở.' #13#10 #13#10 +
              'Vui lòng lưu file và đóng Revit trước khi gỡ cài đặt add-in.',
              mbConfirmation, MB_RETRYCANCEL) = IDCANCEL then
    begin
      Result := False;
      Exit;
    end;
  end;
end;
