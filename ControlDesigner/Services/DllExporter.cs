using System;
using System.Diagnostics;
using System.IO;
using ControlDesigner.Models;

namespace ControlDesigner.Services
{
    /// <summary>
    /// DLL 导出服务 — 调用 MSBuild 编译并输出 DLL
    /// </summary>
    public class DllExporter
    {
        private readonly TemplateEngine _templateEngine;
        private readonly string _msBuildPath;

        public DllExporter(TemplateEngine templateEngine)
        {
            _templateEngine = templateEngine;
            _msBuildPath = FindMsBuild();
        }

        /// <summary>
        /// 导出 DLL 到指定路径
        /// </summary>
        /// <param name="style">控件样式</param>
        /// <param name="outputDllPath">输出 DLL 路径</param>
        /// <param name="controlName">控件名称（作为命名空间和程序集名）</param>
        /// <param name="type">控件类型</param>
        public ExportResult Export(ControlStyle style, string outputDllPath, string controlName, ControlType type)
        {
            var result = new ExportResult();

            try
            {
                // 1. 创建临时编译目录
                string tempDir = Path.Combine(Path.GetTempPath(),
                    "LvControlExport_" + Guid.NewGuid().ToString("N").Substring(0, 8));
                _templateEngine.GenerateProject(style, tempDir, controlName, type);

                // 2. 调用 MSBuild（使用控件名称作为项目文件名）
                string csproj = Path.Combine(tempDir, controlName + ".csproj");
                var psi = new ProcessStartInfo
                {
                    FileName = _msBuildPath,
                    Arguments = $"\"{csproj}\" /p:Configuration=Release /verbosity:minimal /t:Rebuild",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using (var proc = Process.Start(psi))
                {
                    result.BuildOutput = proc.StandardOutput.ReadToEnd();
                    result.BuildErrors = proc.StandardError.ReadToEnd();
                    proc.WaitForExit();
                    result.ExitCode = proc.ExitCode;
                }

                if (result.ExitCode == 0)
                {
                    // 3. 复制 DLL 到目标路径（输出文件名和程序集名一致）
                    string builtDll = Path.Combine(tempDir, "bin", "Release", controlName + ".dll");
                    if (File.Exists(builtDll))
                    {
                        string outDir = Path.GetDirectoryName(outputDllPath);
                        if (!string.IsNullOrEmpty(outDir))
                            Directory.CreateDirectory(outDir);
                        File.Copy(builtDll, outputDllPath, true);
                        result.Success = true;
                        result.DllPath = outputDllPath;
                        result.ControlName = controlName;
                    }
                    else
                    {
                        result.ErrorMessage = "编译成功但未找到输出 DLL: " + builtDll;
                    }
                }
                else
                {
                    string errStr = result.BuildErrors;
                    if (string.IsNullOrWhiteSpace(errStr))
                    {
                        errStr = result.BuildOutput;
                        if (errStr.Length > 500)
                        {
                            // 尝试找到 error 的位置
                            int errIdx = errStr.IndexOf("error ");
                            if (errIdx >= 0)
                            {
                                errStr = errStr.Substring(errIdx, Math.Min(500, errStr.Length - errIdx));
                            }
                        }
                    }
                    result.ErrorMessage = "编译失败: \n" + errStr;
                }

                // 4. 清理临时目录
                if (result.Success)
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
                else
                {
                    result.ErrorMessage += $"\n[调试] 模板代码暂存于: {tempDir}";
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        private string FindMsBuild()
        {
            string[] candidates = {
                @"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe",
                @"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
                @"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
                @"C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
                @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
                @"C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
            };

            foreach (var path in candidates)
            {
                if (File.Exists(path)) return path;
            }

            return "MSBuild.exe";
        }
    }

    /// <summary>
    /// 导出结果
    /// </summary>
    public class ExportResult
    {
        public bool Success { get; set; }
        public string DllPath { get; set; }
        public string ControlName { get; set; }
        public string BuildOutput { get; set; }
        public string BuildErrors { get; set; }
        public string ErrorMessage { get; set; }
        public int ExitCode { get; set; }
    }
}
