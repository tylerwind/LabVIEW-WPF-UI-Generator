# PowerShell 脚本：自动获取 ILRepack.exe 并进行程序集打包与私有化隔离

param (
    [string]$InputDll = "UI\data\MyControlAll.dll",
    [string]$OutputDll = "UI\data\MyControlAll_Isolated.dll",
    [string[]]$DependencyDlls = @()
)

[System.Reflection.Assembly]::LoadWithPartialName("System.IO.Compression.FileSystem") | Out-Null

$toolsDir = Join-Path $PSScriptRoot "tools"
if (-not (Test-Path $toolsDir)) {
    New-Item -ItemType Directory -Force -Path $toolsDir | Out-Null
}

$ilRepackPath = Join-Path $toolsDir "ILRepack.exe"

# 1. 检查是否存在 ILRepack.exe
if (-not (Test-Path $ilRepackPath)) {
    Write-Host "[1/3] Downloading ILRepack..." -ForegroundColor Cyan
    $nupkgPath = Join-Path $toolsDir "ilrepack.zip"
    $extractPath = Join-Path $toolsDir "ilrepack_tmp"
    
    try {
        $url = "https://www.nuget.org/api/v2/package/ILRepack/2.0.18"
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
        Invoke-WebRequest -Uri $url -OutFile $nupkgPath -UseBasicParsing
        
        [System.IO.Compression.ZipFile]::ExtractToDirectory($nupkgPath, $extractPath)
        $extractedExe = Join-Path $extractPath "tools\ILRepack.exe"
        if (Test-Path $extractedExe) {
            Copy-Item $extractedExe $ilRepackPath -Force
            Write-Host "[SUCCESS] ILRepack.exe downloaded!" -ForegroundColor Green
        }
    } catch {
        Write-Host "[WARN] Download error: $_" -ForegroundColor Yellow
    } finally {
        if (Test-Path $nupkgPath) { Remove-Item $nupkgPath -Force -ErrorAction SilentlyContinue }
        if (Test-Path $extractPath) { Remove-Item $extractPath -Recurse -Force -ErrorAction SilentlyContinue }
    }
}

if (-not (Test-Path $ilRepackPath)) {
    Write-Host "[ERROR] Cannot find ILRepack.exe" -ForegroundColor Red
    exit 1
}

# 2. 准备物理输入路径
$fullInput = (Resolve-Path $InputDll -ErrorAction SilentlyContinue).Path
if (-not $fullInput -or -not (Test-Path $fullInput)) {
    Write-Host "[ERROR] Input DLL not found: $InputDll" -ForegroundColor Red
    exit 1
}

$inputDir = Split-Path $fullInput -Parent
$fullOutput = [System.IO.Path]::Combine($inputDir, [System.IO.Path]::GetFileName($OutputDll))

# 检查是否有需要一起打包的第三方依赖项 DLL（例如 Newtonsoft.Json.dll）
$depsList = @()
foreach ($dep in $DependencyDlls) {
    if (Test-Path $dep) {
        $depsList += (Resolve-Path $dep).Path
    }
}

Write-Host "[2/3] Executing ILRepack internalization..." -ForegroundColor Cyan
Write-Host "Input DLL: $fullInput"
Write-Host "Output DLL: $fullOutput"

# 构造 ILRepack 参数
$argsList = @("/internalize", "/out:`"$fullOutput`"", "`"$fullInput`"")
foreach ($d in $depsList) {
    $argsList += "`"$d`""
}

$pinfo = New-Object System.Diagnostics.ProcessStartInfo
$pinfo.FileName = $ilRepackPath
$pinfo.Arguments = ($argsList -join " ")
$pinfo.UseShellExecute = $false
$pinfo.RedirectStandardOutput = $true
$pinfo.RedirectStandardError = $true
$pinfo.CreateNoWindow = $true

$process = [System.Diagnostics.Process]::Start($pinfo)
$stdout = $process.StandardOutput.ReadToEnd()
$stderr = $process.StandardError.ReadToEnd()
$process.WaitForExit()

if ($process.ExitCode -eq 0 -and (Test-Path $fullOutput)) {
    Write-Host "[3/3] [SUCCESS] Packaged isolated DLL: $fullOutput" -ForegroundColor Green
} else {
    Write-Host "[ERROR] ILRepack failed:" -ForegroundColor Red
    Write-Host $stdout -ForegroundColor Yellow
    Write-Host $stderr -ForegroundColor Red
    exit 1
}
