# ==============================================================================
# SCRIPT GỠ CÀI ĐẶT TOÀN DIỆN CHO BỘ CÔNG CỤ BIM TOOL (REVIT 2019 - 2024)
# Tác giả: Nguyễn Tôn Hải Anh - BIM Manager (BVN)
# ==============================================================================

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

Write-Host "======================================================================" -ForegroundColor Yellow
Write-Host "     GỠ CÀI ĐẶT BỘ CÔNG CỤ BIM TOOL (ĐA PHIÊN BẢN)                   " -ForegroundColor Yellow
Write-Host "======================================================================" -ForegroundColor Yellow
Write-Host ""

$revitProcesses = Get-Process -Name Revit -ErrorAction SilentlyContinue
if ($revitProcesses) {
    Write-Warning "Phát hiện Autodesk Revit đang mở. Vui lòng đóng Revit trước khi gỡ cài đặt."
    exit 1
}

$supportedVersions = @('2019', '2021', '2022', '2023', '2024')
$removedCount = 0

foreach ($v in $supportedVersions) {
    $addinsBase = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$v"
    $manifestTarget = Join-Path $addinsBase "BIMTool.Revit$v.addin"
    $destPluginDir = Join-Path $addinsBase "BIMTool"

    $found = $false
    if (Test-Path -LiteralPath $manifestTarget) {
        Remove-Item -LiteralPath $manifestTarget -Force
        Write-Host "-> Revit $v: Đã xóa file manifest: $manifestTarget" -ForegroundColor Green
        $found = $true
    }
    if (Test-Path -LiteralPath $destPluginDir) {
        Remove-Item -LiteralPath $destPluginDir -Recurse -Force
        Write-Host "-> Revit $v: Đã xóa thư mục plugin: $destPluginDir" -ForegroundColor Green
        $found = $true
    }

    if ($found) {
        $removedCount++
    }
}

Write-Host ""
Write-Host "======================================================================" -ForegroundColor Yellow
Write-Host "  Đã gỡ cài đặt sạch sẽ add-in BIM TOOL khỏi $removedCount phiên bản Revit. " -ForegroundColor Green
Write-Host "======================================================================" -ForegroundColor Yellow
Write-Host ""
