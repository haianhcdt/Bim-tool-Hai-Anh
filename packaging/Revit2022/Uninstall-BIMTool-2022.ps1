# Script gá»¡ cÃ i Ä‘áº·t add-in BIM TOOL cho Autodesk Revit 2022
# TÃ¡c giáº£: Nguyá»…n TÃ´n Háº£i Anh - BIM Manager (BVN)

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

Write-Host "==========================================================" -ForegroundColor Yellow
Write-Host "  Gá»  CÃ€I Äáº¶T ADD-IN BIM TOOL - REVIT 2022                  " -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Yellow

$revitProcesses = Get-Process -Name Revit -ErrorAction SilentlyContinue
if ($revitProcesses) {
    Write-Warning "PhÃ¡t hiá»‡n Autodesk Revit Ä‘ang má»Ÿ. Vui lÃ²ng Ä‘Ã³ng Revit trÆ°á»›c khi gá»¡ cÃ i Ä‘áº·t."
    exit 1
}

$destAddinsRoot = Join-Path $env:APPDATA "Autodesk\Revit\Addins\2022"
$manifestTarget = Join-Path $destAddinsRoot "BIMTool.Revit2022.addin"
$destPluginDir = Join-Path $destAddinsRoot "BIMTool"

if (Test-Path -LiteralPath $manifestTarget) {
    Remove-Item -LiteralPath $manifestTarget -Force
    Write-Host "ÄÃ£ xÃ³a file manifest: $manifestTarget" -ForegroundColor Green
}
if (Test-Path -LiteralPath $destPluginDir) {
    Remove-Item -LiteralPath $destPluginDir -Recurse -Force
    Write-Host "ÄÃ£ xÃ³a thÆ° má»¥c plugin: $destPluginDir" -ForegroundColor Green
}

Write-Host "Gá»¡ cÃ i Ä‘áº·t add-in BIM TOOL cho Revit 2022 hoÃ n táº¥t." -ForegroundColor Green
