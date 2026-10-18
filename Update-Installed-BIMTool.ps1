# ==============================================================================
# SCRIPT CẬP NHẬT TÍNH NĂNG MỚI VÀO BỘ TOOL ĐANG CÀI ĐẶT TRÊN REVIT
# Tác giả: Nguyễn Tôn Hải Anh - BIM Manager (BVN)
# ==============================================================================

[CmdletBinding()]
param(
    [switch]$ForceCloseRevit
)

$ErrorActionPreference = 'Stop'

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  CẬP NHẬT TÍNH NĂNG MỚI VÀO BỘ TOOL BIM TOOL ĐANG CÀI     " -ForegroundColor Cyan
Write-Host "  Tác giả: Nguyễn Tôn Hải Anh - BIM Manager (BVN)         " -ForegroundColor DarkGray
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Kiểm tra tiến trình Revit
$revitProcesses = Get-Process -Name Revit -ErrorAction SilentlyContinue
if ($revitProcesses) {
    if ($ForceCloseRevit) {
        Write-Warning "Đang tự động đóng tiến trình Revit theo yêu cầu (-ForceCloseRevit)..."
        $revitProcesses | Stop-Process -Force
        Start-Sleep -Seconds 2
    } else {
        Write-Warning "Phát hiện Autodesk Revit đang mở trên máy tính."
        Write-Host "Vui lòng lưu mô hình và ĐÓNG REVIT để mở khóa file DLL trước khi cập nhật." -ForegroundColor Yellow
        Write-Host "Nhấn phím bất kỳ sau khi đã đóng Revit..."
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
        $revitProcesses = Get-Process -Name Revit -ErrorAction SilentlyContinue
        if ($revitProcesses) {
            Write-Error "Revit vẫn đang chạy. Quá trình cập nhật bị hủy để tránh xung đột file."
            exit 1
        }
    }
}

# 2. Thư mục nguồn (artifacts/Revit2024 hoặc packaging/Revit2024/BIMTool)
$sourceDir = Join-Path $PSScriptRoot "Revit2024\BIMTool"
if (-not (Test-Path -LiteralPath $sourceDir)) {
    # Thử tìm theo đường dẫn dự án
    $sourceDir = "C:\Users\DELL\OneDrive\02.STUDY\12.REVIT AI\Projects\BIM-Tool-WorkingView\artifacts\Revit2024"
}

if (-not (Test-Path -LiteralPath $sourceDir)) {
    Write-Error "Không tìm thấy thư mục binaries nguồn: $sourceDir"
    exit 1
}

# 3. Thư mục đích đang cài đặt trên Revit 2024
$destDir = "$env:APPDATA\Autodesk\Revit\Addins\2024\BIM-Tool-WorkingView"
if (-not (Test-Path -LiteralPath $destDir)) {
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
}

Write-Host "Đang sao chép các DLL cập nhật mới nhất vào: $destDir" -ForegroundColor Green

Copy-Item -Path (Join-Path $sourceDir "DSCons.Revit.Starter.dll") -Destination $destDir -Force
Copy-Item -Path (Join-Path $sourceDir "DSCons.Revit.Starter.Core.dll") -Destination $destDir -Force
if (Test-Path (Join-Path $sourceDir "DSCons.Revit.Starter.pdb")) {
    Copy-Item -Path (Join-Path $sourceDir "DSCons.Revit.Starter.pdb") -Destination $destDir -Force
}

Write-Host ""
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "  CẬP NHẬT THÀNH CÔNG VÀO BỘ TOOL REVIT 2024!            " -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "Các tính năng mới đã sẵn sàng:" -ForegroundColor White
Write-Host "  • Nút mới: 'Mặt Bằng MEP' (Batch Floor Plans) trên tab BIM TOOL" -ForegroundColor Cyan
Write-Host "  • Tự động tạo 13 hệ thống MEP: CB, EL, EG, EP, EU, FA, ET, FI, HS, HD, MV, MP, ME" -ForegroundColor White
Write-Host "  • Cài đặt View Range chuẩn: Top 0, Cut 1200, Bottom 0, Depth 0" -ForegroundColor White
Write-Host "  • Hỗ trợ 5 Users: User1..User5 và tự động map mã Tầng (LBx, Lxx, LRF, LRO)" -ForegroundColor White
Write-Host ""
Write-Host "Hãy mở lại Revit 2024 để bắt đầu sử dụng!" -ForegroundColor Yellow
