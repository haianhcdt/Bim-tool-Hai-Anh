; Inno Setup Script cho BIM-Tool-WorkingView
; Tac gia: Nguyen Tan Hai Anh (BVN)
; Phien ban: 1.0.0

#define MyAppName "BIM Tool - Hai Anh"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Nguyen Tan Hai Anh - BVN"
#define MyAppURL "https://github.com/haianhcdt/Bim-tool-Hai-Anh"

[Setup]
AppId={{A24532C5-ACDE-4946-8414-D8EE406FA589}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={commonpf}\DSCons\BIM-Tool-WorkingView
DefaultGroupName={#MyAppName}
DisableDirPage=yes
DisableProgramGroupPage=yes
OutputDir=..\artifacts\installer
OutputBaseFilename=BIM-Tool-WorkingView-Setup-1.0.0
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
CreateUninstallRegKey=yes
UninstallDisplayName={#MyAppName} - Revit MEP Tools (Uninstall)

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; --- Revit 2019 ---
Source: "..\artifacts\Revit2019\DSCons.Revit.Starter.addin"; DestDir: "{commonappdata}\Autodesk\Revit\Addins\2019"; Flags: ignoreversion; Check: HasRevit2019
Source: "..\artifacts\Revit2019\DSCons.Revit.Starter.dll"; DestDir: "{commonappdata}\Autodesk\Revit\Addins\2019\BIM-Tool-WorkingView"; Flags: ignoreversion; Check: HasRevit2019
Source: "..\artifacts\Revit2019\DSCons.Revit.Starter.Core.dll"; DestDir: "{commonappdata}\Autodesk\Revit\Addins\2019\BIM-Tool-WorkingView"; Flags: ignoreversion; Check: HasRevit2019

; --- Revit 2021 ---
Source: "..\artifacts\Revit2021\DSCons.Revit.Starter.addin"; DestDir: "{commonappdata}\Autodesk\Revit\Addins\2021"; Flags: ignoreversion; Check: HasRevit2021
Source: "..\artifacts\Revit2021\DSCons.Revit.Starter.dll"; DestDir: "{commonappdata}\Autodesk\Revit\Addins\2021\BIM-Tool-WorkingView"; Flags: ignoreversion; Check: HasRevit2021
Source: "..\artifacts\Revit2021\DSCons.Revit.Starter.Core.dll"; DestDir: "{commonappdata}\Autodesk\Revit\Addins\2021\BIM-Tool-WorkingView"; Flags: ignoreversion; Check: HasRevit2021

; --- Revit 2022 ---
Source: "..\artifacts\Revit2022\DSCons.Revit.Starter.addin"; DestDir: "{commonappdata}\Autodesk\Revit\Addins\2022"; Flags: ignoreversion; Check: HasRevit2022
Source: "..\artifacts\Revit2022\DSCons.Revit.Starter.dll"; DestDir: "{commonappdata}\Autodesk\Revit\Addins\2022\BIM-Tool-WorkingView"; Flags: ignoreversion; Check: HasRevit2022
Source: "..\artifacts\Revit2022\DSCons.Revit.Starter.Core.dll"; DestDir: "{commonappdata}\Autodesk\Revit\Addins\2022\BIM-Tool-WorkingView"; Flags: ignoreversion; Check: HasRevit2022

[UninstallDelete]
Type: files; Name: "{commonappdata}\Autodesk\Revit\Addins\2019\DSCons.Revit.Starter.addin"
Type: filesandordirs; Name: "{commonappdata}\Autodesk\Revit\Addins\2019\BIM-Tool-WorkingView"
Type: files; Name: "{commonappdata}\Autodesk\Revit\Addins\2021\DSCons.Revit.Starter.addin"
Type: filesandordirs; Name: "{commonappdata}\Autodesk\Revit\Addins\2021\BIM-Tool-WorkingView"
Type: files; Name: "{commonappdata}\Autodesk\Revit\Addins\2022\DSCons.Revit.Starter.addin"
Type: filesandordirs; Name: "{commonappdata}\Autodesk\Revit\Addins\2022\BIM-Tool-WorkingView"

[Code]
var
  Found2019: Boolean;
  Found2021: Boolean;
  Found2022: Boolean;

function CheckRevitInstalled(VersionYear: String): Boolean;
var
  ProgramFilesPath: String;
  AddinsPath: String;
begin
  ProgramFilesPath := ExpandConstant('{commonpf}\Autodesk\Revit ' + VersionYear);
  AddinsPath := ExpandConstant('{commonappdata}\Autodesk\Revit\Addins\' + VersionYear);
  Result := DirExists(ProgramFilesPath) or DirExists(AddinsPath) or RegKeyExists(HKLM, 'SOFTWARE\Autodesk\Revit\' + VersionYear);
end;

procedure InitializeWizard;
begin
  Found2019 := CheckRevitInstalled('2019');
  Found2021 := CheckRevitInstalled('2021');
  Found2022 := CheckRevitInstalled('2022');
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

function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo, MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
var
  S: String;
begin
  S := 'DANH SACH PHIEN BAN REVIT DA DO TIM THAY TREN MAY:' + NewLine;
  if Found2019 then
    S := S + '  [x] Autodesk Revit 2019: DA TIM THAY -> Se cai add-in vao Addins\2019' + NewLine
  else
    S := S + '  [ ] Autodesk Revit 2019: Khong tim thay' + NewLine;

  if Found2021 then
    S := S + '  [x] Autodesk Revit 2021: DA TIM THAY -> Se cai add-in vao Addins\2021' + NewLine
  else
    S := S + '  [ ] Autodesk Revit 2021: Khong tim thay' + NewLine;

  if Found2022 then
    S := S + '  [x] Autodesk Revit 2022: DA TIM THAY -> Se cai add-in vao Addins\2022' + NewLine
  else
    S := S + '  [ ] Autodesk Revit 2022: Khong tim thay' + NewLine;

  if (not Found2019) and (not Found2021) and (not Found2022) then
    S := S + NewLine + 'CANH BAO: Khong tim thay phien ban Revit nao phu hop tren may nay!' + NewLine;

  Result := S + NewLine + 'MUC GO CAI DAT: Se duoc tao trong Windows Settings > Apps & Features.';
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if CurPageID = wpReady then
  begin
    if (not Found2019) and (not Found2021) and (not Found2022) then
    begin
      if MsgBox('Bo cai chua phat hien phien ban Revit (2019, 2021, 2022) nao tren may tinh. Ban co chac chan muon tiep tuc khong?', mbConfirmation, MB_YESNO) = IDNO then
        Result := False;
    end;
  end;
end;