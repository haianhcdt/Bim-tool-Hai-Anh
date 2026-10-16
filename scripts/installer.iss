; Inno Setup Script cho BIM-Tool-WorkingView (Tích hợp Bộ Dọn Dẹp Mô Hình)
; Tác giả: Nguyễn Tôn Hải Anh (BVN)
; Phiên bản: 1.1.0 (Bao gồm Working 3D View, Batch Tag, Align Tags, Dọn Dẹp File, Cảnh Báo Audit)

#define MyAppName "BIM Tool - Hai Anh"
#define MyAppVersion "1.2.0"
#define MyAppPublisher "Nguyen Ton Hai Anh - BVN"
#define MyAppURL "https://github.com/haianhcdt/Bim-tool-Hai-Anh"

[Setup]
AppId={{A24532C5-ACDE-4946-8414-D8EE406FA589}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={userappdata}\Autodesk\Revit\Addins
DisableDirPage=yes
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\artifacts\installer
OutputBaseFilename=BIM-Tool-WorkingView-Setup-1.2.0
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
CreateUninstallRegKey=yes
UninstallDisplayName={#MyAppName} - Revit MEP Tools (Uninstall)
UninstallFilesDir={userappdata}\Autodesk\Revit\Addins\BIM-Tool-WorkingView\Uninstall

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; --- Revit 2019 ---
Source: "..\artifacts\Revit2019\DSCons.Revit.Starter.addin"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2019"; Flags: ignoreversion; Check: HasRevit2019
Source: "..\artifacts\Revit2019\DSCons.Revit.Starter.dll"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2019\BIM-Tool-WorkingView"; Flags: ignoreversion; Check: HasRevit2019
Source: "..\artifacts\Revit2019\DSCons.Revit.Starter.Core.dll"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2019\BIM-Tool-WorkingView"; Flags: ignoreversion; Check: HasRevit2019

; --- Revit 2021 ---
Source: "..\artifacts\Revit2021\DSCons.Revit.Starter.addin"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2021"; Flags: ignoreversion; Check: HasRevit2021
Source: "..\artifacts\Revit2021\DSCons.Revit.Starter.dll"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2021\BIM-Tool-WorkingView"; Flags: ignoreversion; Check: HasRevit2021
Source: "..\artifacts\Revit2021\DSCons.Revit.Starter.Core.dll"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2021\BIM-Tool-WorkingView"; Flags: ignoreversion; Check: HasRevit2021

; --- Revit 2022 ---
Source: "..\artifacts\Revit2022\DSCons.Revit.Starter.addin"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2022"; Flags: ignoreversion; Check: HasRevit2022
Source: "..\artifacts\Revit2022\DSCons.Revit.Starter.dll"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2022\BIM-Tool-WorkingView"; Flags: ignoreversion; Check: HasRevit2022
Source: "..\artifacts\Revit2022\DSCons.Revit.Starter.Core.dll"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2022\BIM-Tool-WorkingView"; Flags: ignoreversion; Check: HasRevit2022

; --- Revit 2023 ---
Source: "..\artifacts\Revit2023\DSCons.Revit.Starter.addin"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2023"; Flags: ignoreversion; Check: HasRevit2023
Source: "..\artifacts\Revit2023\DSCons.Revit.Starter.dll"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2023\BIM-Tool-WorkingView"; Flags: ignoreversion; Check: HasRevit2023
Source: "..\artifacts\Revit2023\DSCons.Revit.Starter.Core.dll"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2023\BIM-Tool-WorkingView"; Flags: ignoreversion; Check: HasRevit2023

; --- Revit 2024 ---
Source: "..\artifacts\Revit2024\DSCons.Revit.Starter.addin"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2024"; Flags: ignoreversion; Check: HasRevit2024
Source: "..\artifacts\Revit2024\DSCons.Revit.Starter.dll"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2024\BIM-Tool-WorkingView"; Flags: ignoreversion; Check: HasRevit2024
Source: "..\artifacts\Revit2024\DSCons.Revit.Starter.Core.dll"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2024\BIM-Tool-WorkingView"; Flags: ignoreversion; Check: HasRevit2024

[UninstallDelete]
Type: files; Name: "{userappdata}\Autodesk\Revit\Addins\2019\DSCons.Revit.Starter.addin"
Type: filesandordirs; Name: "{userappdata}\Autodesk\Revit\Addins\2019\BIM-Tool-WorkingView"
Type: files; Name: "{userappdata}\Autodesk\Revit\Addins\2021\DSCons.Revit.Starter.addin"
Type: filesandordirs; Name: "{userappdata}\Autodesk\Revit\Addins\2021\BIM-Tool-WorkingView"
Type: files; Name: "{userappdata}\Autodesk\Revit\Addins\2022\DSCons.Revit.Starter.addin"
Type: filesandordirs; Name: "{userappdata}\Autodesk\Revit\Addins\2022\BIM-Tool-WorkingView"
Type: files; Name: "{userappdata}\Autodesk\Revit\Addins\2023\DSCons.Revit.Starter.addin"
Type: filesandordirs; Name: "{userappdata}\Autodesk\Revit\Addins\2023\BIM-Tool-WorkingView"
Type: files; Name: "{userappdata}\Autodesk\Revit\Addins\2024\DSCons.Revit.Starter.addin"
Type: filesandordirs; Name: "{userappdata}\Autodesk\Revit\Addins\2024\BIM-Tool-WorkingView"

[Code]
var
  Found2019: Boolean;
  Found2021: Boolean;
  Found2022: Boolean;
  Found2023: Boolean;
  Found2024: Boolean;

function CheckRevitInstalled(VersionYear: String): Boolean;
var
  ProgramFilesPath: String;
  UserAddinsPath: String;
  CommonAddinsPath: String;
begin
  ProgramFilesPath := ExpandConstant('{commonpf}\Autodesk\Revit ' + VersionYear);
  UserAddinsPath := ExpandConstant('{userappdata}\Autodesk\Revit\Addins\' + VersionYear);
  CommonAddinsPath := ExpandConstant('{commonappdata}\Autodesk\Revit\Addins\' + VersionYear);
  Result := DirExists(ProgramFilesPath) or DirExists(UserAddinsPath) or DirExists(CommonAddinsPath) or RegKeyExists(HKLM, 'SOFTWARE\Autodesk\Revit\' + VersionYear);
end;

procedure InitializeWizard;
begin
  Found2019 := CheckRevitInstalled('2019');
  Found2021 := CheckRevitInstalled('2021');
  Found2022 := CheckRevitInstalled('2022');
  Found2023 := CheckRevitInstalled('2023');
  Found2024 := CheckRevitInstalled('2024');
end;

function HasRevit2019: Boolean;
begin
  Result := Found2019;
end;

function HasRevit2021: Boolean;
begin
  Result := Found2021;
end;

function HasRevit2022: Boolean;
begin
  Result := Found2022;
end;

function HasRevit2023: Boolean;
begin
  Result := Found2023;
end;

function HasRevit2024: Boolean;
begin
  Result := Found2024;
end;

function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo, MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
var
  S: String;
begin
  S := 'DANH SACH PHIEN BAN REVIT DA DO TIM THAY TREN MAY:' + NewLine;
  if Found2019 then
    S := S + '  [x] Autodesk Revit 2019: DA TIM THAY -> Se cai vao Addins\2019' + NewLine
  else
    S := S + '  [ ] Autodesk Revit 2019: Khong tim thay' + NewLine;

  if Found2021 then
    S := S + '  [x] Autodesk Revit 2021: DA TIM THAY -> Se cai vao Addins\2021' + NewLine
  else
    S := S + '  [ ] Autodesk Revit 2021: Khong tim thay' + NewLine;

  if Found2022 then
    S := S + '  [x] Autodesk Revit 2022: DA TIM THAY -> Se cai vao Addins\2022' + NewLine
  else
    S := S + '  [ ] Autodesk Revit 2022: Khong tim thay' + NewLine;

  if Found2023 then
    S := S + '  [x] Autodesk Revit 2023: DA TIM THAY -> Se cai vao Addins\2023' + NewLine
  else
    S := S + '  [ ] Autodesk Revit 2023: Khong tim thay' + NewLine;

  if Found2024 then
    S := S + '  [x] Autodesk Revit 2024: DA TIM THAY -> Se cai vao Addins\2024' + NewLine
  else
    S := S + '  [ ] Autodesk Revit 2024: Khong tim thay' + NewLine;

  if (not Found2019) and (not Found2021) and (not Found2022) and (not Found2023) and (not Found2024) then
    S := S + NewLine + 'CANH BAO: Khong tim thay phien ban Revit (2019-2024) nao phu hop tren may nay!' + NewLine;

  Result := S + NewLine + 'TICH HOP TRON BO CONG CU TREN TAB RIBBON "BIM TOOL":' + NewLine +
           '  1. Working Views (Quan ly & Tao Mat bang, 3D View lam viec ca nhan)' + NewLine +
           '  2. Batch Tag (Ghi chu tag ong gio hang loat)' + NewLine +
           '  3. Align Tags (Can giong the ghi chu thang hang)' + NewLine +
           '  4. Clean File (Don dep file ban giao, Warnings Audit, Duplicate Mark & Purge Unused)' + NewLine +
           '  5. About Me (Thong tin ca nhan tac gia)' + NewLine +
           NewLine + 'MUC GO CAI DAT: Se duoc tao trong Windows Settings > Apps & Features.';
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if CurPageID = wpReady then
  begin
    if (not Found2019) and (not Found2021) and (not Found2022) and (not Found2023) and (not Found2024) then
    begin
      if MsgBox('Bo cai chua phat hien phien ban Revit (2019-2024) nao tren may tinh. Ban co chac chan muon tiep tuc khong?', mbConfirmation, MB_YESNO) = IDNO then
        Result := False;
    end;
  end;
end;
