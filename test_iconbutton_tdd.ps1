# PowerShell TDD Test Script for IconButton

$RootPath = "."
$rand = Get-Random -Minimum 10000 -Maximum 99999
$testNamespace = "WpfIconButtonTest_$rand"

$projectSrc = "$RootPath\ExportTemplate\Template.csproj"
$projectPath = "$RootPath\ExportTemplate\TemplateTest.csproj"
$templatePath = "$RootPath\ExportTemplate\IconButtonControl.xaml.template"
$xamlPath = "$RootPath\ExportTemplate\IconButtonControl.xaml"
$dllPath = "$RootPath\ExportTemplate\bin\Release\$testNamespace.dll"
$jsonPath = "$RootPath\ExportTemplate\test_icon_style.json"

Write-Host "=================== TDD ICONBUTTON TEST RUN ===================" -ForegroundColor Cyan
Write-Host "Using Unique Test Namespace: $testNamespace" -ForegroundColor Blue

# 1. 编译前置处理
Write-Host "Preprocessing XAML Template..." -ForegroundColor Blue
$xamlContent = Get-Content -Path $templatePath -Raw -Encoding UTF8

# 极简级联替换
$xamlContent = $xamlContent.Replace("{{ControlBackground}}", "#E3E6EC")
$xamlContent = $xamlContent.Replace("{{GradientStart}}", "#EAEDF2")
$xamlContent = $xamlContent.Replace("{{GradientMid}}", "#E0E3E9")
$xamlContent = $xamlContent.Replace("{{GradientEnd}}", "#D8DCE3")
$xamlContent = $xamlContent.Replace("{{BorderColor}}", "#DDE0E6")
$xamlContent = $xamlContent.Replace("{{BorderThickness}}", "1")
$xamlContent = $xamlContent.Replace("{{CornerRadius}}", "12")
$xamlContent = $xamlContent.Replace("{{ShadowBlur}}", "10")
$xamlContent = $xamlContent.Replace("{{ShadowDepth}}", "4")
$xamlContent = $xamlContent.Replace("{{ShadowColor}}", "#A3A9B5")
$xamlContent = $xamlContent.Replace("{{ShadowOpacity}}", "0.5")
$xamlContent = $xamlContent.Replace("{{ShadowMargin}}", "8,8,8,8")
$xamlContent = $xamlContent.Replace("{{FontFamily}}", "Segoe UI")
$xamlContent = $xamlContent.Replace("{{FontSize}}", "14")
$xamlContent = $xamlContent.Replace("{{FontColor}}", "#3A3F50")
$xamlContent = $xamlContent.Replace("{{HighlightColor}}", "#FFFFFF")
$xamlContent = $xamlContent.Replace("{{HighlightOpacity}}", "0.65")
$xamlContent = $xamlContent.Replace("{{AccentColor}}", "#7A8AA8")
$xamlContent = $xamlContent.Replace("{{CardPadding}}", "12,8,12,6")
$xamlContent = $xamlContent.Replace("WpfIconButton", $testNamespace)

Set-Content -Path $xamlPath -Value $xamlContent -Encoding UTF8
Write-Host "[OK] Preprocessed XAML successfully!" -ForegroundColor Green

$csFiles = @("IconButtonControl.xaml.cs", "IconButtonPanel.cs")
foreach ($file in $csFiles) {
    $srcPath = "$RootPath\ExportTemplate\$file"
    $backPath = "$RootPath\ExportTemplate\$file.bak"
    Write-Host "Backing up $srcPath to $backPath"
    Copy-Item -Path $srcPath -Destination $backPath -Force
    
    $content = Get-Content -Path $srcPath -Raw -Encoding UTF8
    Write-Host "Replacing namespace in $srcPath"
    $content = $content.Replace("WpfIconButton", $testNamespace)
    Set-Content -Path $srcPath -Value $content -Encoding UTF8
}

$csprojContent = Get-Content -Path $projectSrc -Raw -Encoding UTF8
$csprojContent = $csprojContent.Replace("<RootNamespace>WpfTextInput</RootNamespace>", "<RootNamespace>$testNamespace</RootNamespace>")
$csprojContent = $csprojContent.Replace("<AssemblyName>WpfTextInput</AssemblyName>", "<AssemblyName>$testNamespace</AssemblyName>")
# Ensure compiled files match IconButton only
$csprojContent = $csprojContent.Replace('<Compile Include="TextInputControl.xaml.cs">', "<Compile Include=""IconButtonControl.xaml.cs"">")
$csprojContent = $csprojContent.Replace('<Compile Include="TextInputPanel.cs" />', "<Compile Include=""IconButtonPanel.cs"" />")
$csprojContent = $csprojContent.Replace('<Compile Include="TextInputHost.cs" />', "")
$csprojContent = $csprojContent.Replace('<DependentUpon>TextInputControl.xaml</DependentUpon>', "<DependentUpon>IconButtonControl.xaml</DependentUpon>")
$csprojContent = $csprojContent.Replace('<Page Include="TextInputControl.xaml">', "<Page Include=""IconButtonControl.xaml"">")

Set-Content -Path $projectPath -Value $csprojContent -Encoding UTF8

# 2. 定位 MSBuild
$msBuildPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
if (-not (Test-Path $msBuildPath)) {
    $msBuildPath = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
}
if (-not (Test-Path $msBuildPath)) {
    Write-Host "[ERROR] MSBuild.exe not found! Test aborted." -ForegroundColor Red
    exit 1
}

# 3. 写入测试 JSON 样式配置文件
$testStyleJson = @"
{
  "ControlBackground":"#E3E6EC",
  "FontFamily":"Segoe UI",
  "FontSize":16,
  "FontColor":"#1E90FF",
  "FontWeight":"Bold",
  "AccentColor":"#FF00FF"
}
"@
Set-Content -Path $jsonPath -Value $testStyleJson -Encoding UTF8

# 强力清理 obj 和 bin
$objPath = "$RootPath\ExportTemplate\obj"
$binPath = "$RootPath\ExportTemplate\bin"
if (Test-Path $objPath) { Remove-Item -Path $objPath -Recurse -Force | Out-Null }
if (Test-Path $binPath) { Remove-Item -Path $binPath -Recurse -Force | Out-Null }

# 4. 编译
Write-Host "Compiling temporary unique project..." -ForegroundColor Blue
& $msBuildPath $projectPath "/p:Configuration=Release" "/t:Rebuild" "/verbosity:minimal"

if ($LASTEXITCODE -ne 0) {
    Write-Host "[FAIL] Compilation failed!" -ForegroundColor Red
    exit 1
}
Write-Host "[OK] Compilation successful!" -ForegroundColor Green

# 5. 反射执行并验证属性和方法
try {
    [System.Reflection.Assembly]::LoadWithPartialName("PresentationCore") | Out-Null
    [System.Reflection.Assembly]::LoadWithPartialName("PresentationFramework") | Out-Null
    [System.Reflection.Assembly]::LoadWithPartialName("WindowsBase") | Out-Null

    $bytes = [System.IO.File]::ReadAllBytes($dllPath)
    $assembly = [System.Reflection.Assembly]::Load($bytes)
    
    $panelType = $assembly.GetType("$testNamespace.IconButtonPanel")
    $panelInstance = [Activator]::CreateInstance($panelType)
    Write-Host "[OK] Instantiated IconButtonPanel successfully!" -ForegroundColor Green

    # 5.1 验证 LabelText
    $labelProp = $panelType.GetProperty("LabelText")
    $labelProp.SetValue($panelInstance, "测试文字", $null)
    $actLabel = $labelProp.GetValue($panelInstance, $null)
    if ($actLabel -ne "测试文字") {
        Write-Host "[FAIL] LabelText property failed! Got: $actLabel" -ForegroundColor Red
        exit 1
    }
    Write-Host "[PASS] LabelText checked successfully." -ForegroundColor Green

    # 5.2 验证 Value 属性
    $props = $panelType.GetProperties() | Select-Object -ExpandProperty Name
    Write-Host "Available properties on IconButtonPanel: ($($props -join ', '))" -ForegroundColor Gray
    $valProp = $panelType.GetProperty("Value")
    if ($valProp -eq $null) {
        Write-Host "[FAIL] Value property not found!" -ForegroundColor Red
        exit 1
    }
    $valProp.SetValue($panelInstance, $true, $null)
    $actVal = $valProp.GetValue($panelInstance, $null)
    if ($actVal -ne $true) {
        Write-Host "[FAIL] Value property failed! Got: $actVal" -ForegroundColor Red
        exit 1
    }
    Write-Host "[PASS] Value property checked successfully." -ForegroundColor Green

    # 5.3 验证 SetLabelVisible
    $setVisibleMethod = $panelType.GetMethod("SetLabelVisible")
    if ($setVisibleMethod -eq $null) {
        Write-Host "[FAIL] SetLabelVisible method not found!" -ForegroundColor Red
        exit 1
    }
    $setVisibleMethod.Invoke($panelInstance, @($false))
    Write-Host "[PASS] SetLabelVisible invoked successfully." -ForegroundColor Green

    # 5.4 验证 UpdateStyleFromJson 与 ActiveColor / ActiveColorValue
    $updateStyleMethod = $panelType.GetMethod("UpdateStyleFromJson")
    if ($updateStyleMethod -eq $null) {
        Write-Host "[FAIL] UpdateStyleFromJson method not found!" -ForegroundColor Red
        exit 1
    }
    $updateStyleMethod.Invoke($panelInstance, @($jsonPath))
    
    $activeColorProp = $panelType.GetProperty("ActiveColor")
    $actColor = $activeColorProp.GetValue($panelInstance, $null)
    if ($actColor -ne "#FF00FF") {
        Write-Host "[FAIL] Dynamic style update failed! Expected ActiveColor #FF00FF, got: $actColor" -ForegroundColor Red
        exit 1
    }
    
    $activeColorValueProp = $panelType.GetProperty("ActiveColorValue")
    $actColorVal = $activeColorValueProp.GetValue($panelInstance, $null)
    # #FF00FF is R=255, G=0, B=255 -> (255<<16) | 255 = 16711935
    if ($actColorVal -ne 16711935) {
        Write-Host "[FAIL] ActiveColorValue failed! Expected 16711935, got: $actColorVal" -ForegroundColor Red
        exit 1
    }
    Write-Host "[PASS] Style updates, ActiveColor, and ActiveColorValue checked successfully." -ForegroundColor Green

    # 5.5 Check the actual internal TextBlock is Bold (FontWeight check)
    $wpfControlProp = $panelType.GetProperty("WpfControl")
    $wpfControl = $wpfControlProp.GetValue($panelInstance, $null)
    $labelBlockField = $wpfControl.GetType().GetField("LabelBlock", [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    $labelBlock = $labelBlockField.GetValue($wpfControl)
    $labelWeight = $labelBlock.FontWeight.ToString()
    if ($labelWeight -ne "Bold") {
        Write-Host "[FAIL] Dynamic FontWeight failed! Expected Bold, got: $labelWeight" -ForegroundColor Red
        exit 1
    }
    Write-Host "[PASS] Dynamic FontWeight check passed." -ForegroundColor Green

    Write-Host "=================== TDD ICONBUTTON GREEN LIGHT! ===================" -ForegroundColor Green

} catch {
    Write-Host "[FAIL] Test error occurred: $_" -ForegroundColor Red
    if ($_.Exception) {
        Write-Host "Exception details: $($_.Exception.ToString())" -ForegroundColor Red
        if ($_.Exception.InnerException) {
            Write-Host "Inner Exception: $($_.Exception.InnerException.ToString())" -ForegroundColor Red
        }
    }
    exit 1
} finally {
    foreach ($file in $csFiles) {
        $backPath = "$RootPath\ExportTemplate\$file.bak"
        if (Test-Path $backPath) {
            Move-Item -Path $backPath -Destination "$RootPath\ExportTemplate\$file" -Force
            Remove-Item $backPath -Force -ErrorAction SilentlyContinue
        }
    }
    if (Test-Path $xamlPath) { Remove-Item $xamlPath -Force }
    if (Test-Path $jsonPath) { Remove-Item $jsonPath -Force }
    if (Test-Path $projectPath) { Remove-Item $projectPath -Force }
}
