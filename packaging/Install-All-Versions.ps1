# ==============================================================================
# SCRIPT CÀI ĐẶT ĐA PHIÊN BẢN CHO BỘ CÔNG CỤ BIM TOOL (REVIT 2019 - 2024)
# Tác giả: Nguyễn Tôn Hải Anh - BIM Manager (BVN)
# ==============================================================================

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "     BỘ CÀI ĐẶT TỰ ĐỘNG ĐA PHIÊN BẢN - BIM TOOL                       " -ForegroundColor Cyan
Write-Host "     Tác giả: Nguyễn Tôn Hải Anh - BIM Manager (BVN)                  " -ForegroundColor DarkGray
Write-Host "     Hỗ trợ: Autodesk Revit 2019, 2021, 2022, 2023, 2024              " -ForegroundColor DarkCyan
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host ""

# 1. Kiểm tra tiến trình Revit
$revitProcesses = Get-Process -Name Revit -ErrorAction SilentlyContinue
if ($revitProcesses) {
    Write-Warning "Phát hiện Autodesk Revit đang mở trên máy tính."
    Write-Host "Vui lòng lưu file và ĐÓNG HOÀN TOÀN REVIT trước khi cài đặt để tránh xung đột file DLL." -ForegroundColor Yellow
    Write-Host "Nhấn phím bất kỳ sau khi đã đóng Revit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    $revitProcesses = Get-Process -Name Revit -ErrorAction SilentlyContinue
    if ($revitProcesses) {
        Write-Error "Revit vẫn đang chạy. Quá trình cài đặt bị hủy."
        exit 1
    }
}

$supportedVersions = @('2019', '2021', '2022', '2023', '2024')
$rootPackaging = $PSScriptRoot
$installedCount = 0

Write-Host "Đang tự động quét và cài đặt cho các phiên bản Revit phát hiện được..." -ForegroundColor White
Write-Host ""

foreach ($v in $supportedVersions) {
    $versionDir = Join-Path $rootPackaging "Revit$v"
    $manifestSource = Join-Path $versionDir "BIMTool.Revit$v.addin"
    $binariesSource = Join-Path $versionDir "BIMTool"

    if (-not (Test-Path $manifestSource) -or -not (Test-Path $binariesSource)) {
        continue
    }

    $addinsBase = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$v"
    
    # Kiểm tra xem máy tính có thư mục Addins của năm đó hay không (hoặc có cài đặt Revit năm đó)
    $hasRevitInstall = (Test-Path "C:\Program Files\Autodesk\Revit $v") -or (Test-Path $addinsBase)

    if ($hasRevitInstall) {
        Write-Host "------------------------------------------------------" -ForegroundColor DarkGray
        Write-Host "-> Phát hiện Autodesk Revit $v: Tiến hành cài đặt..." -ForegroundColor Green
        
        $destPluginDir = Join-Path $addinsBase "BIMTool"

        if (-not (Test-Path $addinsBase)) {
            New-Item -ItemType Directory -Path $addinsBase -Force | Out-Null
        }
        if (-not (Test-Path $destPluginDir)) {
            New-Item -ItemType Directory -Path $destPluginDir -Force | Out-Null
        }

        # Sao chép Manifest
        Copy-Item -LiteralPath $manifestSource -Destination $addinsBase -Force
        Write-Host "   + Đã nạp file Manifest: BIMTool.Revit$v.addin" -ForegroundColor Gray

        # Sao chép DLLs
        Copy-Item -Path (Join-Path $binariesSource "*") -Destination $destPluginDir -Recurse -Force
        Write-Host "   + Đã sao chép toàn bộ thư viện DLL vào: $destPluginDir" -ForegroundColor Gray
        
        Write-Host "   ✔ CÀI ĐẶT THÀNH CÔNG CHO REVIT $v!" -ForegroundColor Green
        $installedCount++
    }
}

Write-Host ""
Write-Host "======================================================================" -ForegroundColor Cyan
if ($installedCount -gt 0) {
    Write-Host "  TỔNG KẾT: Đã cài đặt thành công add-in BIM TOOL cho $installedCount phiên bản Revit!  " -ForegroundColor Green
    Write-Host "======================================================================" -ForegroundColor Cyan
    Write-Host "Các bước tiếp theo:" -ForegroundColor White
    Write-Host "1. Khởi động Autodesk Revit tương ứng."
    Write-Host "2. Nhìn lên thanh Ribbon trên cùng, bạn sẽ thấy tab mới: 'BIM TOOL'."
    Write-Host "3. Bấm vào nút 'Dọn Dẹp File' để sử dụng bộ công cụ dọn dẹp mô hình bàn giao."
} else {
    Write-Host "  Không tìm thấy phiên bản Revit tương thích (2019, 2021 - 2024) nào được cài đặt." -ForegroundColor Yellow
    Write-Host "======================================================================" -ForegroundColor Cyan
}
Write-Host ""
