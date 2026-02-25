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
                    content = content.Replace("{{ControlBackground}}", style.ControlBackground);
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

        private string ApplyStyle(string template, ControlStyle style)
        {
            var replacements = new Dictionary<string, string>
            {
                { "{{ControlBackground}}", style.ControlBackground },
                { "{{GradientStart}}", style.GradientStart },
                { "{{GradientMid}}", style.GradientMid },
                { "{{GradientEnd}}", style.GradientEnd },
                { "{{BorderColor}}", style.BorderColor },
                { "{{BorderThickness}}", style.BorderThickness.ToString() },
                { "{{CornerRadius}}", style.CornerRadius.ToString() },
                { "{{ShadowBlur}}", style.ShadowBlur.ToString() },
                { "{{ShadowDepth}}", style.ShadowDepth.ToString() },
                { "{{ShadowColor}}", style.ShadowColor },
                { "{{ShadowOpacity}}", style.ShadowOpacity.ToString("F2") },
                { "{{ShadowMargin}}", CalcShadowMargin(style.ShadowBlur, style.ShadowDepth) },
                { "{{HighlightColor}}", style.HighlightColor },
                { "{{HighlightOpacity}}", style.HighlightOpacity.ToString("F2") },
                { "{{FontFamily}}", style.FontFamily },
                { "{{FontSize}}", style.FontSize.ToString() },
                { "{{FontColor}}", style.FontColor },
                { "{{CaretColor}}", style.CaretColor },
                { "{{LabelColor}}", style.LabelColor },
                { "{{LabelFontSize}}", style.LabelFontSize.ToString() },
                { "{{FocusBorderColor}}", style.FocusBorderColor },
                { "{{AccentColor}}", style.AccentColor },
                { "{{CardPadding}}", style.CardPadding },
            };

            string result = template;
            foreach (var kvp in replacements)
            {
                result = result.Replace(kvp.Key, kvp.Value);
            }
            return result;
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
