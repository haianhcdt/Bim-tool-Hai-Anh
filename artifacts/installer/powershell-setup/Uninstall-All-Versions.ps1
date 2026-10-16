# ==============================================================================
# SCRIPT GO CAI DAT TOAN DIEN CHO BO CONG CU BIM TOOL (REVIT 2019 - 2024)
# Tac gia: Nguyen Ton Hai Anh - BIM Manager (BVN)
# ==============================================================================

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

Write-Host "======================================================================" -ForegroundColor Yellow
Write-Host "     GO CAI DAT BO CONG CU BIM TOOL (DA PHIEN BAN)                   " -ForegroundColor Yellow
Write-Host "======================================================================" -ForegroundColor Yellow
Write-Host ""

$revitProcesses = Get-Process -Name Revit -ErrorAction SilentlyContinue
if ($revitProcesses) {
    Write-Warning "Phat hien Autodesk Revit dang mo. Vui long dong Revit truoc khi go cai dat."
    exit 1
}

$supportedVersions = @('2019', '2021', '2022', '2023', '2024')
$removedCount = 0

foreach ($v in $supportedVersions) {
    $addinsBase = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$v"
    $manifestTarget = Join-Path $addinsBase "DSCons.Revit.Starter.addin"
    $destPluginDir = Join-Path $addinsBase "BIM-Tool-WorkingView"

    $found = $false
    if (Test-Path -LiteralPath $manifestTarget) {
        Remove-Item -LiteralPath $manifestTarget -Force
        Write-Host "-> Revit $v: Da xoa file manifest: $manifestTarget" -ForegroundColor Green
        $found = $true
    }
    if (Test-Path -LiteralPath $destPluginDir) {
        Remove-Item -LiteralPath $destPluginDir -Recurse -Force
        Write-Host "-> Revit $v: Da xoa thu muc plugin: $destPluginDir" -ForegroundColor Green
        $found = $true
    }

    if ($found) {
        $removedCount++
    }
}

Write-Host ""
Write-Host "======================================================================" -ForegroundColor Yellow
Write-Host "  Da go cai dat sach se add-in BIM TOOL khoi $removedCount phien ban Revit. " -ForegroundColor Green
Write-Host "======================================================================" -ForegroundColor Yellow
Write-Host ""