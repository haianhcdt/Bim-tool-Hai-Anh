# ==============================================================================
# SCRIPT CAP NHAT TINH NANG MOI VAO BO TOOL DANG CAI DAT TREN REVIT
# Tac gia: Nguyen Ton Hai Anh - BIM Manager (BVN)
# ==============================================================================

[CmdletBinding()]
param(
    [switch]$ForceCloseRevit
)

$ErrorActionPreference = 'Stop'

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  CAP NHAT TINH NANG MOI VAO BO TOOL BIM TOOL DANG CAI     " -ForegroundColor Cyan
Write-Host "  Tac gia: Nguyen Ton Hai Anh - BIM Manager (BVN)         " -ForegroundColor DarkGray
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Kiem tra tien trinh Revit
$revitProcesses = Get-Process -Name Revit -ErrorAction SilentlyContinue
if ($revitProcesses) {
    if ($ForceCloseRevit) {
        Write-Warning "Dang tu dong dong tien trinh Revit theo yeu cau..."
        $revitProcesses | Stop-Process -Force
        Start-Sleep -Seconds 2
    } else {
        Write-Error "Revit dang chay. Vui long luu mo hinh va dong Revit truoc khi cap nhat."
        exit 1
    }
}

# 2. Thu muc nguon
$sourceDir = Join-Path $PSScriptRoot "Revit2024\BIMTool"
if (-not (Test-Path -LiteralPath $sourceDir)) {
    $sourceDir = "C:\Users\DELL\OneDrive\02.STUDY\12.REVIT AI\Projects\BIM-Tool-WorkingView\artifacts\Revit2024"
}

if (-not (Test-Path -LiteralPath $sourceDir)) {
    Write-Error "Khong tim thay thu muc binaries nguon: $sourceDir"
    exit 1
}

# 3. Thu muc dich dang cai dat tren Revit 2024
$destDir = "$env:APPDATA\Autodesk\Revit\Addins\2024\BIM-Tool-WorkingView"
if (-not (Test-Path -LiteralPath $destDir)) {
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
}

Write-Host "Dang sao chep cac DLL cap nhat moi nhat vao: $destDir" -ForegroundColor Green

Copy-Item -Path (Join-Path $sourceDir "DSCons.Revit.Starter.dll") -Destination $destDir -Force
Copy-Item -Path (Join-Path $sourceDir "DSCons.Revit.Starter.Core.dll") -Destination $destDir -Force
if (Test-Path (Join-Path $sourceDir "DSCons.Revit.Starter.pdb")) {
    Copy-Item -Path (Join-Path $sourceDir "DSCons.Revit.Starter.pdb") -Destination $destDir -Force
}
if (Test-Path (Join-Path $sourceDir "DSCons.Revit.Starter.Core.pdb")) {
    Copy-Item -Path (Join-Path $sourceDir "DSCons.Revit.Starter.Core.pdb") -Destination $destDir -Force
}

Write-Host ""
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "  CAP NHAT THANH CONG VAO BO TOOL REVIT 2024!            " -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "Cac tinh nang moi da san sang tren tab BIM TOOL:" -ForegroundColor White
Write-Host "  * Nut: 'Creat view Working' (3-trong-1):" -ForegroundColor Cyan
Write-Host "    - Tab 1: Mat Bang MEP (13 he MEP, View Range chuan)" -ForegroundColor DarkGray
Write-Host "    - Tab 2: Mat Cat MEP (Ver/Hoz ne Grid, keo qua tat ca Level, Far Clip 1000mm)" -ForegroundColor DarkGray
Write-Host "    - Tab 3: 3D Tang MEP (Dong hop Section Box theo mat bang, tich chon 1 lan)" -ForegroundColor DarkGray
Write-Host ""
Write-Host "Hay mo lai Revit 2024 de bat dau su dung!" -ForegroundColor Yellow