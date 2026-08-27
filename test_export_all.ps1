# PowerShell Script to test ExportAll DLL Collection programmatically

# Load necessary assemblies
[System.Reflection.Assembly]::LoadWithPartialName("System.Web.Extensions") | Out-Null
[System.Reflection.Assembly]::LoadWithPartialName("PresentationCore") | Out-Null
[System.Reflection.Assembly]::LoadWithPartialName("PresentationFramework") | Out-Null
[System.Reflection.Assembly]::LoadWithPartialName("WindowsBase") | Out-Null
[System.Reflection.Assembly]::LoadWithPartialName("System.Xaml") | Out-Null

$exePath = Get-ChildItem ".\ControlDesigner\bin\Release\WPF*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -ExpandProperty FullName -First 1
if (-not $exePath -or -not (Test-Path $exePath)) {
    Write-Host "Error: WPF designer executable not found. Build it first." -ForegroundColor Red
    exit 1
}

Write-Host "Using executable: $exePath"

$bytes = [System.IO.File]::ReadAllBytes($exePath)
$assembly = [System.Reflection.Assembly]::Load($bytes)

# Create ControlStyle
$styleType = $assembly.GetType("ControlDesigner.Models.ControlStyle")
$style = [Activator]::CreateInstance($styleType)

# Populate style fields
$style.ControlBackground = "#E3E6EC"
$style.GradientStart = "#EAEDF2"
$style.GradientMid = "#E0E3E9"
$style.GradientEnd = "#D8DCE3"
$style.BorderColor = "#DDE0E6"
$style.FontFamily = "Segoe UI"
$style.FontColor = "#FF0000"

# Instantiate TemplateEngine and DllExporter
$templateEngineType = $assembly.GetType("ControlDesigner.Services.TemplateEngine")
$templateDir = Resolve-Path "ExportTemplate"
$templateEngine = [Activator]::CreateInstance($templateEngineType, $templateDir.Path)

$dllExporterType = $assembly.GetType("ControlDesigner.Services.DllExporter")
$dllExporter = [Activator]::CreateInstance($dllExporterType, $templateEngine)

# Run ExportAll
$outputPath = Join-Path $pwd "UI\Ui Xcontrol\Xcontrol\MyControlAll_Test.dll"
if (Test-Path $outputPath) { Remove-Item $outputPath -Force }

Write-Host "Running ExportAll..."
$result = $dllExporter.ExportAll($style, $outputPath, "MyControlAll_Test")

if ($result.Success) {
    Write-Host "[PASS] ExportAll Succeeded! DLL exported to: $outputPath" -ForegroundColor Green
    if (Test-Path $outputPath) { Remove-Item $outputPath -Force }
} else {
    Write-Host "[FAIL] ExportAll Failed!" -ForegroundColor Red
    Write-Host "Error Message:" -ForegroundColor Red
    Write-Host $result.ErrorMessage -ForegroundColor Yellow
    Write-Host "Build Output:" -ForegroundColor Red
    Write-Host $result.BuildOutput -ForegroundColor Gray
    Write-Host "Build Errors:" -ForegroundColor Red
    Write-Host $result.BuildErrors -ForegroundColor Red
    exit 1
}
