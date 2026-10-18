# Script cÃ i Ä‘áº·t add-in BIM TOOL cho Autodesk Revit 2022
# TÃ¡c giáº£: Nguyá»…n TÃ´n Háº£i Anh - BIM Manager (BVN)

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  CÃ€I Äáº¶T ADD-IN BIM TOOL CHO AUTODESK REVIT 2022           " -ForegroundColor Cyan
Write-Host "  TÃ¡c giáº£: Nguyá»…n TÃ´n Háº£i Anh - BIM Manager (BVN)         " -ForegroundColor DarkGray
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Kiá»ƒm tra tiáº¿n trÃ¬nh Revit
$revitProcesses = Get-Process -Name Revit -ErrorAction SilentlyContinue
if ($revitProcesses) {
    Write-Warning "PhÃ¡t hiá»‡n Autodesk Revit Ä‘ang má»Ÿ."
    Write-Host "Vui lÃ²ng lÆ°u file vÃ  ÄÃ“NG hoÃ n toÃ n Revit trÆ°á»›c khi cÃ i Ä‘áº·t Ä‘á»ƒ trÃ¡nh khÃ³a file DLL." -ForegroundColor Yellow
    exit 1
}

# 2. XÃ¡c Ä‘á»‹nh thÆ° má»¥c nguá»“n vÃ  thÆ° má»¥c Ä‘Ã­ch
$sourceDir = $PSScriptRoot
$manifestSource = Join-Path $sourceDir "BIMTool.Revit2022.addin"
$binariesSource = Join-Path $sourceDir "BIMTool"

if (-not (Test-Path -LiteralPath $manifestSource)) {
    Write-Error "KhÃ´ng tÃ¬m tháº¥y file manifest: $manifestSource"
    exit 1
}

$destAddinsRoot = Join-Path $env:APPDATA "Autodesk\Revit\Addins\2022"
$destPluginDir = Join-Path $destAddinsRoot "BIMTool"

# 3. Táº¡o thÆ° má»¥c náº¿u chÆ°a tá»“n táº¡i
if (-not (Test-Path -LiteralPath $destAddinsRoot)) {
    New-Item -ItemType Directory -Path $destAddinsRoot -Force | Out-Null
}
if (-not (Test-Path -LiteralPath $destPluginDir)) {
    New-Item -ItemType Directory -Path $destPluginDir -Force | Out-Null
}

# 4. Sao chÃ©p file manifest vÃ  binaries
Write-Host "Äang sao chÃ©p file manifest vÃ o: $destAddinsRoot" -ForegroundColor Green
Copy-Item -LiteralPath $manifestSource -Destination $destAddinsRoot -Force

if (Test-Path -LiteralPath $binariesSource) {
    Write-Host "Äang sao chÃ©p cÃ¡c file DLL vÃ o: $destPluginDir" -ForegroundColor Green
    Copy-Item -Path (Join-Path $binariesSource "*") -Destination $destPluginDir -Recurse -Force
} else {
    Write-Warning "ThÆ° má»¥c binaries 'BIMTool' chÆ°a cÃ³ sáºµn trong gÃ³i."
}

Write-Host ""
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "  CÃ€I Äáº¶T THÃ€NH CÃ”NG CHO REVIT 2022!                        " -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "CÃ¡c bÆ°á»›c kiá»ƒm tra tiáº¿p theo:" -ForegroundColor White
Write-Host "1. Má»Ÿ Autodesk Revit 2022."
Write-Host "2. TÃ¬m tab ribbon 'BIM TOOL' trÃªn thanh cÃ´ng cá»¥."
Write-Host "3. Báº¥m vÃ o nÃºt 'Dá»n Dáº¹p File' Ä‘á»ƒ sá»­ dá»¥ng."
Write-Host ""
