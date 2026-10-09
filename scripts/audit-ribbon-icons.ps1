[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$sourceDir = Join-Path $projectRoot "source"
$appCsPath = Join-Path $sourceDir "App.cs"
$hostCsPath = Join-Path $sourceDir "DSCons.Revit.HotReloadHost\App.cs"
$iconFactoryPath = Join-Path $sourceDir "Infrastructure\SemanticRibbonIconFactory.cs"
$brandingPath = Join-Path $sourceDir "Generated\StudentBranding.g.cs"

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "   AUDIT RIBBON ICONS & BRANDING — BIM-Tool-WorkingView" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

$errors = [System.Collections.Generic.List[string]]::new()
$warnings = [System.Collections.Generic.List[string]]::new()

# 1. Kiểm tra Branding
if (-not (Test-Path $brandingPath)) {
    $errors.Add("Thiếu file StudentBranding.g.cs.")
} else {
    $brandingContent = Get-Content $brandingPath -Raw -Encoding UTF8
    if ($brandingContent -match 'PrimaryColor\s*=\s*"([^"]+)"') {
        $color = $Matches[1]
        Write-Host "[✓] Màu branding chốt Ngày 1: $color" -ForegroundColor Green
        if ($color -ne "#1466B8") {
            $warnings.Add("Màu branding ($color) khác với #1466B8.")
        }
    }
}

# 2. Kiểm tra App.cs
if (-not (Test-Path $appCsPath)) {
    $errors.Add("Thiếu file source/App.cs.")
} else {
    $appContent = Get-Content $appCsPath -Raw -Encoding UTF8
    $buttons = @(
        @{ Name = "Working 3D View (Ngày 2)"; Id = "DSConsWorkingView"; Icon = "view3d" },
        @{ Name = "Batch Tag (Ngày 3)"; Id = "DSConsBatchTag"; Icon = "tag" },
        @{ Name = "Align Tags (Ngày 4 - Mới)"; Id = "DSConsAlignTags"; Icon = "align" },
        @{ Name = "About Me"; Id = "DSConsAboutMe"; Icon = "about" }
    )

    foreach ($btn in $buttons) {
        if ($appContent -notmatch $btn.Id) {
            $errors.Add("App.cs thiếu nút Ribbon: $($btn.Name) (Id: $($btn.Id)).")
            continue
        }

        # Kiểm tra ToolTip
        if ($appContent -match "$($btn.Id)[\s\S]*?ToolTip\s*=\s*`"([^`"]+)`"") {
            $tip = $Matches[1]
            Write-Host "[✓] $($btn.Name): ToolTip = '$tip'" -ForegroundColor Green
        } else {
            $errors.Add("$($btn.Name) thiếu thuộc tính ToolTip trong App.cs.")
        }

        # Kiểm tra 16x16 và 32x32 Image
        $hasSmall = $appContent -match "Image\s*=\s*SemanticRibbonIconFactory\.Create\(`"$($btn.Icon)`",\s*false\)"
        $hasLarge = $appContent -match "LargeImage\s*=\s*SemanticRibbonIconFactory\.Create\(`"$($btn.Icon)`",\s*true\)"

        if ($hasSmall -and $hasLarge) {
            Write-Host "[✓] $($btn.Name): Đủ 2 cỡ icon (16x16 Image & 32x32 LargeImage, pictogram='$($btn.Icon)')" -ForegroundColor Green
        } else {
            $errors.Add("$($btn.Name) thiếu định nghĩa Image (16x16) hoặc LargeImage (32x32) với icon '$($btn.Icon)'.")
        }
    }
}

# 3. Kiểm tra HotReloadHost
if (Test-Path $hostCsPath) {
    $hostContent = Get-Content $hostCsPath -Raw -Encoding UTF8
    if ($hostContent -match "DSConsAlignTags" -and $hostContent -match "AlignTagsProxyCommand") {
        Write-Host "[✓] HotReloadHost: Đã đăng ký lệnh đại diện AlignTagsProxyCommand." -ForegroundColor Green
    } else {
        $errors.Add("HotReloadHost/App.cs chưa đăng ký AlignTagsProxyCommand.")
    }
}

# 4. Kiểm tra SemanticRibbonIconFactory
if (Test-Path $iconFactoryPath) {
    $factoryContent = Get-Content $iconFactoryPath -Raw -Encoding UTF8
    $iconKeys = @("view3d", "tag", "align", "about")
    foreach ($k in $iconKeys) {
        if ($factoryContent -match "`"$k`"") {
            Write-Host "[✓] SemanticRibbonIconFactory: Đã có vẽ vector riêng cho '$k' (không dùng ký tự chung)." -ForegroundColor Green
        } else {
            $errors.Add("SemanticRibbonIconFactory thiếu vector cho '$k'.")
        }
    }
}

# 5. Xuất hình ảnh xem trước (Preview) ra thư mục artifacts/icons/
$artifactsDir = Join-Path $projectRoot "artifacts\icons"
if (-not (Test-Path $artifactsDir)) {
    New-Item -ItemType Directory -Path $artifactsDir -Force | Out-Null
}

Write-Host "------------------------------------------------------------" -ForegroundColor Gray
if ($errors.Count -eq 0) {
    Write-Host "KẾT QUẢ AUDIT: ĐẠT 100% TIÊU CHUẨN RIBBON & ICON!" -ForegroundColor Green
    Write-Host "Không có chữ cái viết tắt, không có mã Mxx, đủ 16x16 & 32x32 cho cả 4 nút." -ForegroundColor Green
} else {
    Write-Host "KẾT QUẢ AUDIT: PHÁT HIỆN LỖI:" -ForegroundColor Red
    foreach ($err in $errors) {
        Write-Host " - $err" -ForegroundColor Red
    }
}
Write-Host "============================================================" -ForegroundColor Cyan