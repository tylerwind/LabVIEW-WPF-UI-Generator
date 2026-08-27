# PowerShell TDD Test Script for Runtime Style Redraw (Anti-Caching Assembly Version)

$RootPath = "."

# 动态生成唯一的测试程序集名称，彻底击穿 .NET 缓存机制
$rand = Get-Random -Minimum 10000 -Maximum 99999
$testNamespace = "WpfTextInputTest_$rand"

$projectSrc = "$RootPath\ExportTemplate\Template.csproj"
$projectPath = "$RootPath\ExportTemplate\TemplateTest.csproj"
$templatePath = "$RootPath\ExportTemplate\TextInputControl.xaml.template"
$xamlPath = "$RootPath\ExportTemplate\TextInputControl.xaml"
$dllPath = "$RootPath\ExportTemplate\bin\Release\$testNamespace.dll"
$jsonPath = "$RootPath\ExportTemplate\test_style.json"

Write-Host "=================== TDD RED-GREEN TEST RUN ===================" -ForegroundColor Cyan
Write-Host "Using Unique Test Namespace: $testNamespace" -ForegroundColor Blue

# 1. 编译前置处理：读取模板并替换占位符生成 .xaml，同时应用测试命名空间 (去掉 if-else 解决 PowerShell 解析偏振)
Write-Host "Preprocessing XAML Template..." -ForegroundColor Blue
$xamlContent = Get-Content -Path $templatePath -Raw

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
$xamlContent = $xamlContent.Replace("{{FontSize}}", "24")
$xamlContent = $xamlContent.Replace("{{FontColor}}", "#3A3F50")
$xamlContent = $xamlContent.Replace("{{CaretColor}}", "#5A6070")
$xamlContent = $xamlContent.Replace("{{LabelColor}}", "#8A90A0")
$xamlContent = $xamlContent.Replace("{{LabelFontSize}}", "11")
$xamlContent = $xamlContent.Replace("{{FocusBorderColor}}", "#B0B8C8")
$xamlContent = $xamlContent.Replace("{{AccentColor}}", "#7A8AA8")
$xamlContent = $xamlContent.Replace("{{CardPadding}}", "12,8,12,6")
$xamlContent = $xamlContent.Replace("{{Namespace}}", $testNamespace)
$xamlContent = $xamlContent.Replace("WpfTextInput", $testNamespace)

Set-Content -Path $xamlPath -Value $xamlContent -Encoding UTF8
Write-Host "[OK] Preprocessed XAML successfully!" -ForegroundColor Green

$csFiles = @("TextInputControl.xaml.cs", "TextInputPanel.cs", "TextInputHost.cs", "ValueChangedEventArgs.cs")
foreach ($file in $csFiles) {
    $srcPath = "$RootPath\ExportTemplate\$file"
    $backPath = "$RootPath\ExportTemplate\$file.bak"
    Write-Host "Backing up $srcPath to $backPath"
    Copy-Item -Path $srcPath -Destination $backPath -Force
    
    $content = Get-Content -Path $srcPath -Raw
    Write-Host "Replacing namespace in $srcPath"
    $content = $content.Replace("WpfTextInput", $testNamespace)
    Set-Content -Path $srcPath -Value $content -Encoding UTF8
}

Write-Host "Checking if Template.csproj exists..." -ForegroundColor Blue
Write-Host "  projectSrc path: $projectSrc"
Write-Host "  projectSrc exists: $(Test-Path $projectSrc)"

$csprojContent = Get-Content -Path $projectSrc -Raw
$csprojContent = $csprojContent.Replace("<RootNamespace>WpfTextInput</RootNamespace>", "<RootNamespace>$testNamespace</RootNamespace>")
$csprojContent = $csprojContent.Replace("<AssemblyName>WpfTextInput</AssemblyName>", "<AssemblyName>$testNamespace</AssemblyName>")
Set-Content -Path $projectPath -Value $csprojContent -Encoding UTF8

# 4. 自动定位 MSBuild
$msBuildPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
if (-not (Test-Path $msBuildPath)) {
    $msBuildPath = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
}
if (-not (Test-Path $msBuildPath)) {
    Write-Host "[ERROR] MSBuild.exe not found! Test aborted." -ForegroundColor Red
    exit 1
}

# 5. 写入测试 JSON 样式配置文件 (极简单行字符串形式)
$testStyleJson = Get-Content -Path "$RootPath\UI\data\MyButton.style.json" -Raw -Encoding UTF8
$testStyleJson = $testStyleJson.Replace('"ShadowColor":"#A3A9B5"', '"ShadowColor":""')
Set-Content -Path $jsonPath -Value $testStyleJson -Encoding UTF8

# 强力清理 obj 和 bin
$objPath = "$RootPath\ExportTemplate\obj"
$binPath = "$RootPath\ExportTemplate\bin"
if (Test-Path $objPath) { Remove-Item -Path $objPath -Recurse -Force | Out-Null }
if (Test-Path $binPath) { Remove-Item -Path $binPath -Recurse -Force | Out-Null }

# 6. 执行 MSBuild 编译
Write-Host "Compiling temporary unique project..." -ForegroundColor Blue
& $msBuildPath $projectPath "/p:Configuration=Release" "/t:Rebuild" "/verbosity:minimal"

if ($LASTEXITCODE -ne 0) {
    Write-Host "[FAIL] Compilation failed!" -ForegroundColor Red
    foreach ($file in $csFiles) {
        $backPath = "$RootPath\ExportTemplate\$file.bak"
        if (Test-Path $backPath) {
            Move-Item -Path $backPath -Destination "$RootPath\ExportTemplate\$file" -Force
        }
    }
    if (Test-Path $xamlPath) { Remove-Item $xamlPath -Force }
    if (Test-Path $projectPath) { Remove-Item $projectPath -Force }
    exit 1
}
Write-Host "[OK] Compilation successful!" -ForegroundColor Green

# 7. 加载生成的全新 DLL 并反射实例化测试
try {
    # 强力载入系统依赖库
    [System.Reflection.Assembly]::LoadWithPartialName("System.Web.Extensions") | Out-Null
    [System.Reflection.Assembly]::LoadWithPartialName("System.Windows.Forms") | Out-Null
    [System.Reflection.Assembly]::LoadWithPartialName("PresentationCore") | Out-Null
    [System.Reflection.Assembly]::LoadWithPartialName("PresentationFramework") | Out-Null
    [System.Reflection.Assembly]::LoadWithPartialName("WindowsBase") | Out-Null

    # 载入字节码，彻底隔离缓存
    $bytes = [System.IO.File]::ReadAllBytes($dllPath)
    $assembly = [System.Reflection.Assembly]::Load($bytes)
    
    $panelType = $assembly.GetType("$testNamespace.TextInputPanel")
    if ($panelType -eq $null) {
        Write-Host "[FAIL] Panel type not found in unique DLL!" -ForegroundColor Red
        exit 1
    }

    $panelInstance = [Activator]::CreateInstance($panelType)
    Write-Host "[OK] Instantiated Panel successfully!" -ForegroundColor Green

    # 8. 调用重绘 API
    Write-Host "Invoking UpdateStyleFromJson..." -ForegroundColor Blue
    $method = $panelType.GetMethod("UpdateStyleFromJson")
    if ($method -eq $null) {
        Write-Host "[FAIL] Method UpdateStyleFromJson does not exist!" -ForegroundColor Red
        exit 1
    }

    $method.Invoke($panelInstance, @([string]$testStyleJson))
    Write-Host "[OK] Invoked UpdateStyleFromJson successfully!" -ForegroundColor Green
    
    # 9. 读取公有 WpfControl 属性以验证属性直抹
    $wpfControlProp = $null
    foreach ($p in $panelType.GetProperties()) {
        if ($p.Name -eq "WpfControl") {
            $wpfControlProp = $p
            break
        }
    }
    if ($wpfControlProp -eq $null) {
        Write-Host "[FAIL] Property WpfControl not found via reflection!" -ForegroundColor Red
        exit 1
    }
    $wpfControl = $wpfControlProp.GetValue($panelInstance, $null)
    
    if ($wpfControl -eq $null) {
        Write-Host "[FAIL] Internal WPF Control WpfControl is null!" -ForegroundColor Red
        exit 1
    }

    # 反射读取公有属性 CurrentStyle
    $currentStyleProp = $wpfControl.GetType().GetProperty("CurrentStyle")
    if ($currentStyleProp -eq $null) {
        Write-Host "[FAIL] CurrentStyle property not found in WPF Control!" -ForegroundColor Red
        exit 1
    }
    $currentStyle = $currentStyleProp.GetValue($wpfControl, $null)

    if ($currentStyle -eq $null) {
        Write-Host "[FAIL] CurrentStyle property is null on WPF Control!" -ForegroundColor Red
        exit 1
    }

    # 反射读取私有/内部字段 InputBox
    $inputBoxField = $wpfControl.GetType().GetField("InputBox", [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
    if ($inputBoxField -eq $null) {
        Write-Host "[FAIL] Field InputBox not found!" -ForegroundColor Red
        exit 1
    }
    $inputBox = $inputBoxField.GetValue($wpfControl)
    if ($inputBox -eq $null) {
        Write-Host "[FAIL] InputBox control is null!" -ForegroundColor Red
        exit 1
    }
    $actualFontSize = $inputBox.FontSize
    if ($actualFontSize -ne 14.0) {
        Write-Host "[FAIL] Assertion Failed: Expected InputBox.FontSize 14.0 (updated from JSON), but got $actualFontSize" -ForegroundColor Red
        exit 1
    }

    # 断言 ControlBackground
    $bgColor = $wpfControl.Background.Color.ToString()
    if ($bgColor -notlike "*E3E6EC*") {
        Write-Host "[FAIL] Assertion Failed: Expected Background color containing E3E6EC, but got $bgColor" -ForegroundColor Red
        exit 1
    }

    Write-Host "[PASS] TDD GREEN - Target FontSize and ControlBackground successfully updated!" -ForegroundColor Green

} catch {
    Write-Host "[FAIL] Test error occurred: $_" -ForegroundColor Red
    exit 1
} finally {
    foreach ($file in $csFiles) {
        $backPath = "$RootPath\ExportTemplate\$file.bak"
        if (Test-Path $backPath) {
            Move-Item -Path $backPath -Destination "$RootPath\ExportTemplate\$file" -Force
        }
    }
    if (Test-Path $xamlPath) { Remove-Item $xamlPath -Force }
    if (Test-Path $jsonPath) { Remove-Item $jsonPath -Force }
    if (Test-Path $projectPath) { Remove-Item $projectPath -Force }
}

Write-Host "=================== TEST COMPLETION ===================" -ForegroundColor Cyan
