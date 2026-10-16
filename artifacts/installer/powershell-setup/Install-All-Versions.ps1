# ==============================================================================
# SCRIPT CAI DAT TU DONG DA PHIEN BAN - BIM TOOL (REVIT 2019 - 2024)
# Tac gia: Nguyen Ton Hai Anh - BIM Manager (BVN)
# Phien ban: 1.2.0
# ==============================================================================

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "     BO CAI DAT TU DONG DA PHIEN BAN - BIM TOOL (v1.2.0)              " -ForegroundColor Cyan
Write-Host "     Tac gia: Nguyen Ton Hai Anh - BIM Manager (BVN)                  " -ForegroundColor DarkGray
Write-Host "     Ho tro: Autodesk Revit 2019, 2021, 2022, 2023, 2024              " -ForegroundColor DarkCyan
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host ""

# 1. Kiem tra tien trinh Revit
$revitProcesses = Get-Process -Name Revit -ErrorAction SilentlyContinue
if ($revitProcesses) {
    Write-Warning "Phat hien Autodesk Revit dang mo tren may tinh."
    Write-Host "Vui long luu file va DONG HOAN TOAN REVIT truoc khi cai dat de tranh xung dot file DLL." -ForegroundColor Yellow
    Write-Host "Nhan phim bat ky sau khi da dong Revit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    $revitProcesses = Get-Process -Name Revit -ErrorAction SilentlyContinue
    if ($revitProcesses) {
        Write-Error "Revit van dang chay. Qua trinh cai dat bi huy."
        exit 1
    }
}

$supportedVersions = @('2019', '2021', '2022', '2023', '2024')
$artifactsRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$installedCount = 0

Write-Host "Dang tu dong quet va cai dat cho cac phien ban Revit phat hien duoc..." -ForegroundColor White
Write-Host ""

foreach ($v in $supportedVersions) {
    $versionDir = Join-Path $artifactsRoot "Revit$v"
    $manifestSource = Join-Path $versionDir "DSCons.Revit.Starter.addin"
    $starterDllSource = Join-Path $versionDir "DSCons.Revit.Starter.dll"
    $coreDllSource = Join-Path $versionDir "DSCons.Revit.Starter.Core.dll"

    if (-not (Test-Path $manifestSource) -or -not (Test-Path $starterDllSource)) {
        continue
    }

    $addinsBase = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$v"
    $hasRevitInstall = (Test-Path "C:\Program Files\Autodesk\Revit $v") -or (Test-Path $addinsBase)

    if ($hasRevitInstall) {
        Write-Host "------------------------------------------------------" -ForegroundColor DarkGray
        Write-Host "-> Phat hien Autodesk Revit $v: Tien hanh cai dat..." -ForegroundColor Green
        
        $destPluginDir = Join-Path $addinsBase "BIM-Tool-WorkingView"

        if (-not (Test-Path $addinsBase)) {
            New-Item -ItemType Directory -Path $addinsBase -Force | Out-Null
        }
        if (-not (Test-Path $destPluginDir)) {
            New-Item -ItemType Directory -Path $destPluginDir -Force | Out-Null
        }

        # Sao chep Manifest
        Copy-Item -LiteralPath $manifestSource -Destination $addinsBase -Force
        Write-Host "   + Da nap file Manifest: DSCons.Revit.Starter.addin" -ForegroundColor Gray

        # Sao chep DLLs
        Copy-Item -LiteralPath $starterDllSource -Destination $destPluginDir -Force
        if (Test-Path $coreDllSource) {
            Copy-Item -LiteralPath $coreDllSource -Destination $destPluginDir -Force
        }
        Write-Host "   + Da sao chep thu vien DLL vao: $destPluginDir" -ForegroundColor Gray
        
        Write-Host "   ? CAI DAT THANH CONG CHO REVIT $v!" -ForegroundColor Green
        $installedCount++
    }
}

Write-Host ""
Write-Host "======================================================================" -ForegroundColor Cyan
if ($installedCount -gt 0) {
    Write-Host "  TONG KET: Da cai dat thanh cong add-in BIM TOOL cho $installedCount phien ban Revit!  " -ForegroundColor Green
    Write-Host "======================================================================" -ForegroundColor Cyan
    Write-Host "Cac buoc tiep theo:" -ForegroundColor White
    Write-Host "1. Khoi dong Autodesk Revit tuong ung."
    Write-Host "2. Nhin len thanh Ribbon tren cung, ban se thay tab moi: 'BIM TOOL'."
    Write-Host "3. Su dung tron bo 8 tinh nang toi uu hoa mo hinh."
} else {
    Write-Host "  Khong tim thay phien ban Revit tuong thich (2019, 2021 - 2024) nao duoc cai dat." -ForegroundColor Yellow
    Write-Host "======================================================================" -ForegroundColor Cyan
}
Write-Host ""