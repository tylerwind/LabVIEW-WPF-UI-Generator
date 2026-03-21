# PowerShell Script to verify generator build and export logs

$generatorPath = Get-ChildItem "$PSScriptRoot\ControlDesigner\bin\Release\WPF*.exe" | Select-Object -ExpandProperty FullName
$errorLogPath = "$PSScriptRoot\UI\MyChart_ErrorLog.txt"

Write-Host "--- Automated Generator Verification ---" -ForegroundColor Cyan

# 1. Check if EXE exists
if (Test-Path $generatorPath) {
    $lastWrite = (Get-Item $generatorPath).LastWriteTime
    Write-Host "[OK] Generator found: $generatorPath" -ForegroundColor Green
    Write-Host "     Last Build Time: $lastWrite"
} else {
    Write-Host "[ERROR] Generator not found at $generatorPath" -ForegroundColor Red
    exit 1
}

# 2. Check for recent errors in MyChart_ErrorLog.txt
if (Test-Path $errorLogPath) {
    $content = Get-Content $errorLogPath -Tail 5
    if ($content -match "失败" -or $content -match "error") {
        Write-Host "[WARNING] Recent export failure detected in $errorLogPath" -ForegroundColor Yellow
        $content | ForEach-Object { Write-Host "     $_" -ForegroundColor Gray }
    } else {
        Write-Host "[OK] No recent errors detected in $errorLogPath" -ForegroundColor Green
    }
} else {
    Write-Host "[INFO] No error log found at $errorLogPath. Assuming fresh state." -ForegroundColor Blue
}

Write-Host "---------------------------------------"
Write-Host "Verification complete. Please perform a manual export in the GUI to confirm the template fix." -ForegroundColor Cyan
