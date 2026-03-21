$designerPath = "d:\Tyler\公众号\LabVIEW-WPF-UI-Generator-main\ControlDesigner\bin\Release\WPF控件.exe"

if (-Not (Test-Path $designerPath)) {
    Write-Host "Designer Error: executable not found."
    exit 1
}

Write-Host "Manual Testing Needed:"
Write-Host "The UI Generator depends on user interactions to export DLLs, since it's a visual designer tool."
Write-Host "To verify the integration, please run: $designerPath"
Write-Host "1. Click on the Chart tab."
Write-Host "2. Try changing Theme to Dark/Light."
Write-Host "3. Click Export and verify the output DLL."
Write-Host "4. Repeat for Dashboard Panel."
