using System;
using System.Collections.Generic;
using System.IO;
using ControlDesigner.Models;

namespace ControlDesigner.Services
{
    /// <summary>
    /// XAML 模板引擎 — 将样式配置填充到模板中
    /// </summary>
    public class TemplateEngine
    {
        private readonly string _templateDir;

        public TemplateEngine(string templateDir)
        {
            _templateDir = templateDir;
        }

        /// <summary>
        /// 根据样式生成 XAML 内容
        /// </summary>
        public string GenerateXaml(ControlStyle style, string controlName, ControlType type)
        {
            string templateFileName;
            switch (type)
            {
                case ControlType.NumericDisplay: templateFileName = "NumericDisplayControl.xaml.template"; break;
                case ControlType.ComboBoxInput: templateFileName = "ComboBoxControl.xaml.template"; break;
                case ControlType.SliderInput: templateFileName = "SliderControl.xaml.template"; break;
                case ControlType.ButtonInput: templateFileName = "ButtonControl.xaml.template"; break;
                case ControlType.LedIndicator: templateFileName = "LedControl.xaml.template"; break;
                case ControlType.ToggleSwitch: templateFileName = "ToggleSwitchControl.xaml.template"; break;
                case ControlType.ProgressBarInput: templateFileName = "ProgressBarControl.xaml.template"; break;
                case ControlType.TextInput:
                default: templateFileName = "TextInputControl.xaml.template"; break;
            }

            string templatePath = Path.Combine(_templateDir, templateFileName);
            string template = File.ReadAllText(templatePath);
            string xaml = ApplyStyle(template, style);
            // 替换命名空间
            xaml = xaml.Replace("WpfTextInput", controlName);
            return xaml;
        }

        /// <summary>
        /// 将所有源文件输出到指定目录，使用自定义控件名称
        /// </summary>
        public void GenerateProject(ControlStyle style, string outputDir, string controlName, ControlType type)
        {
            Directory.CreateDirectory(outputDir);

            // 生成 XAML（从模板替换 + 命名空间替换）
            string xaml = GenerateXaml(style, controlName, type);
            string xamlFileName;
            string[] fixedFiles;

            switch (type)
            {
                case ControlType.NumericDisplay:
                    xamlFileName = "NumericDisplayControl.xaml";
                    fixedFiles = new string[] { "NumericDisplayControl.xaml.cs", "NumericDisplayPanel.cs" };
                    break;
                case ControlType.ComboBoxInput:
                    xamlFileName = "ComboBoxControl.xaml";
                    fixedFiles = new string[] { "ComboBoxControl.xaml.cs", "ComboBoxPanel.cs" };
                    break;
                case ControlType.SliderInput:
                    xamlFileName = "SliderControl.xaml";
                    fixedFiles = new string[] { "SliderControl.xaml.cs", "SliderPanel.cs" };
                    break;
                case ControlType.ButtonInput:
                    xamlFileName = "ButtonControl.xaml";
                    fixedFiles = new string[] { "ButtonControl.xaml.cs", "ButtonPanel.cs" };
                    break;
                case ControlType.LedIndicator:
                    xamlFileName = "LedControl.xaml";
                    fixedFiles = new string[] { "LedControl.xaml.cs", "LedPanel.cs" };
                    break;
                case ControlType.ToggleSwitch:
                    xamlFileName = "ToggleSwitchControl.xaml";
                    fixedFiles = new string[] { "ToggleSwitchControl.xaml.cs", "ToggleSwitchPanel.cs" };
                    break;
                case ControlType.ProgressBarInput:
                    xamlFileName = "ProgressBarControl.xaml";
                    fixedFiles = new string[] { "ProgressBarControl.xaml.cs", "ProgressBarPanel.cs" };
                    break;

                case ControlType.TextInput:
                default:
                    xamlFileName = "TextInputControl.xaml";
                    fixedFiles = new string[] {
                        "TextInputControl.xaml.cs",
                        "TextInputPanel.cs",
                        "TextInputHost.cs",
                        "ValueChangedEventArgs.cs"
                    };
                    break;
            }

            File.WriteAllText(Path.Combine(outputDir, xamlFileName), xaml);

            foreach (var file in fixedFiles)
            {
                string src = Path.Combine(_templateDir, file);
                if (File.Exists(src))
                {
                    string content = File.ReadAllText(src);
                    content = content.Replace("WpfTextInput", controlName);
                    content = ApplyStyle(content, style);
                    File.WriteAllText(Path.Combine(outputDir, file), content);
                }
            }

            // 生成 .csproj —— 替换 AssemblyName 和 RootNamespace
            string csprojSrc = Path.Combine(_templateDir, "Template.csproj");
            if (File.Exists(csprojSrc))
            {
                string csproj = File.ReadAllText(csprojSrc);

                if (type == ControlType.NumericDisplay)
                {
                    csproj = csproj.Replace("TextInputControl.xaml.cs", "NumericDisplayControl.xaml.cs");
                    csproj = csproj.Replace("TextInputControl.xaml", "NumericDisplayControl.xaml");
                    csproj = csproj.Replace("<Compile Include=\"TextInputHost.cs\" />", "");
                    csproj = csproj.Replace("TextInputPanel.cs", "NumericDisplayPanel.cs");
                    csproj = csproj.Replace("<Compile Include=\"ValueChangedEventArgs.cs\" />", "");
                }
                else if (type == ControlType.ComboBoxInput)
                {
                    csproj = csproj.Replace("TextInputControl.xaml.cs", "ComboBoxControl.xaml.cs");
                    csproj = csproj.Replace("TextInputControl.xaml", "ComboBoxControl.xaml");
                    csproj = csproj.Replace("<Compile Include=\"TextInputHost.cs\" />", "");
                    csproj = csproj.Replace("TextInputPanel.cs", "ComboBoxPanel.cs");
                    csproj = csproj.Replace("<Compile Include=\"ValueChangedEventArgs.cs\" />", "");
                }
                else if (type == ControlType.SliderInput)
                {
                    csproj = csproj.Replace("TextInputControl.xaml.cs", "SliderControl.xaml.cs");
                    csproj = csproj.Replace("TextInputControl.xaml", "SliderControl.xaml");
                    csproj = csproj.Replace("<Compile Include=\"TextInputHost.cs\" />", "");
                    csproj = csproj.Replace("TextInputPanel.cs", "SliderPanel.cs");
                    csproj = csproj.Replace("<Compile Include=\"ValueChangedEventArgs.cs\" />", "");
                }
                else if (type == ControlType.ButtonInput)
                {
                    csproj = csproj.Replace("TextInputControl.xaml.cs", "ButtonControl.xaml.cs");
                    csproj = csproj.Replace("TextInputControl.xaml", "ButtonControl.xaml");
                    csproj = csproj.Replace("<Compile Include=\"TextInputHost.cs\" />", "");
                    csproj = csproj.Replace("TextInputPanel.cs", "ButtonPanel.cs");
                    csproj = csproj.Replace("<Compile Include=\"ValueChangedEventArgs.cs\" />", "");
                }
                else if (type == ControlType.LedIndicator)
                {
                    csproj = csproj.Replace("TextInputControl.xaml.cs", "LedControl.xaml.cs");
                    csproj = csproj.Replace("TextInputControl.xaml", "LedControl.xaml");
                    csproj = csproj.Replace("<Compile Include=\"TextInputHost.cs\" />", "");
                    csproj = csproj.Replace("TextInputPanel.cs", "LedPanel.cs");
                    csproj = csproj.Replace("<Compile Include=\"ValueChangedEventArgs.cs\" />", "");
                }
                else if (type == ControlType.ToggleSwitch)
                {
                    csproj = csproj.Replace("TextInputControl.xaml.cs", "ToggleSwitchControl.xaml.cs");
                    csproj = csproj.Replace("TextInputControl.xaml", "ToggleSwitchControl.xaml");
                    csproj = csproj.Replace("<Compile Include=\"TextInputHost.cs\" />", "");
                    csproj = csproj.Replace("TextInputPanel.cs", "ToggleSwitchPanel.cs");
                    csproj = csproj.Replace("<Compile Include=\"ValueChangedEventArgs.cs\" />", "");
                }
                else if (type == ControlType.ProgressBarInput)
                {
                    csproj = csproj.Replace("TextInputControl.xaml.cs", "ProgressBarControl.xaml.cs");
                    csproj = csproj.Replace("TextInputControl.xaml", "ProgressBarControl.xaml");
                    csproj = csproj.Replace("<Compile Include=\"TextInputHost.cs\" />", "");
                    csproj = csproj.Replace("TextInputPanel.cs", "ProgressBarPanel.cs");
                    csproj = csproj.Replace("<Compile Include=\"ValueChangedEventArgs.cs\" />", "");
                }

                csproj = csproj.Replace("<RootNamespace>WpfTextInput</RootNamespace>",
                                       "<RootNamespace>" + controlName + "</RootNamespace>");
                csproj = csproj.Replace("<AssemblyName>WpfTextInput</AssemblyName>",
                                       "<AssemblyName>" + controlName + "</AssemblyName>");
                // 生成唯一 GUID 避免冲突
                csproj = csproj.Replace("{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}",
                                       "{" + Guid.NewGuid().ToString().ToUpper() + "}");
                File.WriteAllText(Path.Combine(outputDir, controlName + ".csproj"), csproj);
            }
        }

        /// <summary>
        /// 将所有控件的源文件输出到同一目录，编译为统一 DLL
        /// </summary>
        public void GenerateAllProject(ControlStyle style, string outputDir, string assemblyName)
        {
            Directory.CreateDirectory(outputDir);

            // 定义所有控件类型及其文件
            var allControls = new[]
            {
                new { Type = ControlType.TextInput,        XamlTemplate = "TextInputControl.xaml.template",        XamlOut = "TextInputControl.xaml",        Files = new[] { "TextInputControl.xaml.cs", "TextInputPanel.cs", "TextInputHost.cs", "ValueChangedEventArgs.cs" } },
                new { Type = ControlType.NumericDisplay,    XamlTemplate = "NumericDisplayControl.xaml.template",    XamlOut = "NumericDisplayControl.xaml",    Files = new[] { "NumericDisplayControl.xaml.cs", "NumericDisplayPanel.cs" } },
                new { Type = ControlType.ComboBoxInput,     XamlTemplate = "ComboBoxControl.xaml.template",          XamlOut = "ComboBoxControl.xaml",          Files = new[] { "ComboBoxControl.xaml.cs", "ComboBoxPanel.cs" } },
                new { Type = ControlType.SliderInput,       XamlTemplate = "SliderControl.xaml.template",            XamlOut = "SliderControl.xaml",            Files = new[] { "SliderControl.xaml.cs", "SliderPanel.cs" } },
                new { Type = ControlType.ButtonInput,       XamlTemplate = "ButtonControl.xaml.template",            XamlOut = "ButtonControl.xaml",            Files = new[] { "ButtonControl.xaml.cs", "ButtonPanel.cs" } },
                new { Type = ControlType.LedIndicator,      XamlTemplate = "LedControl.xaml.template",              XamlOut = "LedControl.xaml",              Files = new[] { "LedControl.xaml.cs", "LedPanel.cs" } },
                new { Type = ControlType.ToggleSwitch,      XamlTemplate = "ToggleSwitchControl.xaml.template",      XamlOut = "ToggleSwitchControl.xaml",      Files = new[] { "ToggleSwitchControl.xaml.cs", "ToggleSwitchPanel.cs" } },
                new { Type = ControlType.ProgressBarInput,  XamlTemplate = "ProgressBarControl.xaml.template",       XamlOut = "ProgressBarControl.xaml",       Files = new[] { "ProgressBarControl.xaml.cs", "ProgressBarPanel.cs" } },
            };

            var writtenFiles = new HashSet<string>();

            foreach (var ctrl in allControls)
            {
                // 生成 XAML
                string templatePath = Path.Combine(_templateDir, ctrl.XamlTemplate);
                if (File.Exists(templatePath))
                {
                    string template = File.ReadAllText(templatePath);
                    string xaml = ApplyStyle(template, style);
                    xaml = xaml.Replace("WpfTextInput", assemblyName)
                               .Replace("WpfSlider", assemblyName)
                               .Replace("WpfButton", assemblyName)
                               .Replace("WpfComboBox", assemblyName);
                    File.WriteAllText(Path.Combine(outputDir, ctrl.XamlOut), xaml);
                }

                // 复制 code-behind 和 Panel
                foreach (var file in ctrl.Files)
                {
                    if (writtenFiles.Contains(file)) continue;
                    writtenFiles.Add(file);

                    string src = Path.Combine(_templateDir, file);
                    if (File.Exists(src))
                {
                    string content = File.ReadAllText(src);
                    content = content.Replace("WpfTextInput", assemblyName)
                                       .Replace("WpfNumericDisplay", assemblyName)
                                       .Replace("WpfSlider", assemblyName)
                                       .Replace("WpfButton", assemblyName)
                                       .Replace("WpfLedIndicator", assemblyName)
                                       .Replace("WpfToggleSwitch", assemblyName)
                                       .Replace("WpfProgressBar", assemblyName)
                                       .Replace("WpfComboBox", assemblyName);
                    content = ApplyStyle(content, style);
                    File.WriteAllText(Path.Combine(outputDir, file), content);
                }
                }
            }

            // 生成 .csproj
            string csprojSrc = Path.Combine(_templateDir, "AllTemplate.csproj");
            if (File.Exists(csprojSrc))
            {
                string csproj = File.ReadAllText(csprojSrc);
                csproj = csproj.Replace("<RootNamespace>WpfTextInput</RootNamespace>",
                                       "<RootNamespace>" + assemblyName + "</RootNamespace>");
                csproj = csproj.Replace("<AssemblyName>WpfTextInput</AssemblyName>",
                                       "<AssemblyName>" + assemblyName + "</AssemblyName>");
                csproj = csproj.Replace("{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}",
                                       "{" + Guid.NewGuid().ToString().ToUpper() + "}");
                File.WriteAllText(Path.Combine(outputDir, assemblyName + ".csproj"), csproj);
            }
        }

        private string ApplyStyle(string template, ControlStyle style)
        {
            var replacements = new Dictionary<string, string>
            {
                { "{{ControlBackground}}", CleanColor(style.ControlBackground) },
                { "{{GradientStart}}", CleanColor(style.GradientStart) },
                { "{{GradientMid}}", CleanColor(style.GradientMid) },
                { "{{GradientEnd}}", CleanColor(style.GradientEnd) },
                { "{{BorderColor}}", CleanColor(style.BorderColor) },
                { "{{BorderThickness}}", style.BorderThickness.ToString() },
                { "{{CornerRadius}}", style.CornerRadius.ToString() },
                { "{{ShadowBlur}}", style.ShadowBlur.ToString() },
                { "{{ShadowDepth}}", style.ShadowDepth.ToString() },
                { "{{ShadowColor}}", CleanColor(style.ShadowColor) },
                { "{{ShadowOpacity}}", style.ShadowOpacity.ToString("F2") },
                { "{{ShadowMargin}}", CalcShadowMargin(style.ShadowBlur, style.ShadowDepth) },
                { "{{HighlightColor}}", CleanColor(style.HighlightColor) },
                { "{{HighlightOpacity}}", style.HighlightOpacity.ToString("F2") },
                { "{{FontFamily}}", style.FontFamily },
                { "{{FontSize}}", style.FontSize.ToString() },
                { "{{FontColor}}", CleanColor(style.FontColor) },
                { "{{CaretColor}}", CleanColor(style.CaretColor) },
                { "{{LabelColor}}", CleanColor(style.LabelColor) },
                { "{{LabelFontSize}}", style.LabelFontSize.ToString() },
                { "{{FocusBorderColor}}", CleanColor(style.FocusBorderColor) },
                { "{{AccentColor}}", CleanColor(style.AccentColor) },
                { "{{LedOnColor}}", CleanColor(style.LedOnColor) },
                { "{{LedOffColor}}", CleanColor(style.LedOffColor) },
                { "{{CardPadding}}", style.CardPadding },
            };

            string result = template;
            foreach (var kvp in replacements)
            {
                result = result.Replace(kvp.Key, kvp.Value);
            }
            return result;
        }

        private static string CleanColor(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return "#FFFFFF";
            string s = hex.Trim();
            if (s.Equals("Transparent", StringComparison.OrdinalIgnoreCase)) return "Transparent";
            s = s.TrimStart('#');
            if (string.IsNullOrEmpty(s)) return "#FFFFFF";
            return "#" + s;
        }

        /// <summary>
        /// 根据阴影参数计算 Margin，让阴影有足够空间自然消散
        /// 阴影方向 315°（右下方），所以右和下需要更多空间
        /// </summary>
        private string CalcShadowMargin(double blur, double depth)
        {
            // 阴影扩散半径 ≈ blur * 0.7
            double spread = blur * 0.7;
            // 315° 方向：dx ≈ depth * cos(45°)，dy ≈ depth * sin(45°)
            double offset = depth * 0.71;

            int left   = (int)Math.Ceiling(Math.Max(spread - offset, 2));
            int top    = (int)Math.Ceiling(Math.Max(spread - offset, 2));
            int right  = (int)Math.Ceiling(spread + offset);
            int bottom = (int)Math.Ceiling(spread + offset);

            return $"{left},{top},{right},{bottom}";
        }
    }
}
