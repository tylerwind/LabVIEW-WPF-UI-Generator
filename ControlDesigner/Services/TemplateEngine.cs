using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;
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
                case ControlType.ChartDisplay: templateFileName = "ChartControl.xaml.template"; break;
                case ControlType.PieDisplay: templateFileName = "PieControl.xaml.template"; break;
                case ControlType.GaugeDisplay: templateFileName = "GaugeControl.xaml.template"; break;
                case ControlType.DataGridDisplay: templateFileName = "DataGridControl.xaml.template"; break;
                case ControlType.TreeDisplay: templateFileName = "TreeControl.xaml.template"; break;
                case ControlType.SidebarNav: templateFileName = "SidebarControl.xaml.template"; break;
                case ControlType.TextInput:
                default: templateFileName = "TextInputControl.xaml.template"; break;
            }

            string templatePath = Path.Combine(_templateDir, templateFileName);
            string template = File.ReadAllText(templatePath);
            string xaml = ApplyStyle(template, style);
            // 替换所有可能的占位命名空间
            xaml = ReplaceNamespace(xaml, controlName);
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
                case ControlType.ChartDisplay:
                    xamlFileName = "ChartControl.xaml";
                    fixedFiles = new string[] { "ChartControl.xaml.cs", "ChartPanel.cs" };
                    break;
                case ControlType.PieDisplay:
                    xamlFileName = "PieControl.xaml";
                    fixedFiles = new string[] { "PieControl.xaml.cs", "PiePanel.cs" };
                    break;
                case ControlType.GaugeDisplay:
                    xamlFileName = "GaugeControl.xaml";
                    fixedFiles = new string[] { "GaugeControl.xaml.cs", "GaugePanel.cs" };
                    break;
                case ControlType.DataGridDisplay:
                    xamlFileName = "DataGridControl.xaml";
                    fixedFiles = new string[] { "DataGridControl.xaml.cs", "DataGridPanel.cs" };
                    break;
                case ControlType.TreeDisplay:
                    xamlFileName = "TreeControl.xaml";
                    fixedFiles = new string[] { "TreeControl.xaml.cs", "TreePanel.cs" };
                    break;
                case ControlType.SidebarNav:
                    xamlFileName = "SidebarControl.xaml";
                    fixedFiles = new string[] { "SidebarControl.xaml.cs", "SidebarPanel.cs" };
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

            File.WriteAllText(Path.Combine(outputDir, xamlFileName), xaml, Encoding.UTF8);

            foreach (var file in fixedFiles)
            {
                string src = Path.Combine(_templateDir, file);
                if (File.Exists(src))
                {
                    string content = File.ReadAllText(src);
                    content = ReplaceNamespace(content, controlName);
                    
                    // 对 .cs 文件也执行 ApplyStyle 以替换可能的动态属性（如 Chart 和 DataGrid 的配置）
                    if (file.EndsWith(".cs"))
                    {
                        content = ApplyStyle(content, style);
                    }
                    
                    File.WriteAllText(Path.Combine(outputDir, file), content, Encoding.UTF8);
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
                else if (type == ControlType.ChartDisplay)
                {
                    csproj = csproj.Replace("TextInputControl.xaml.cs", "ChartControl.xaml.cs");
                    csproj = csproj.Replace("TextInputControl.xaml", "ChartControl.xaml");
                    csproj = csproj.Replace("<Compile Include=\"TextInputHost.cs\" />", "");
                    csproj = csproj.Replace("TextInputPanel.cs", "ChartPanel.cs");
                    csproj = csproj.Replace("<Compile Include=\"ValueChangedEventArgs.cs\" />", "");
                }
                else if (type == ControlType.PieDisplay)
                {
                    csproj = csproj.Replace("TextInputControl.xaml.cs", "PieControl.xaml.cs");
                    csproj = csproj.Replace("TextInputControl.xaml", "PieControl.xaml");
                    csproj = csproj.Replace("<Compile Include=\"TextInputHost.cs\" />", "");
                    csproj = csproj.Replace("TextInputPanel.cs", "PiePanel.cs");
                    csproj = csproj.Replace("<Compile Include=\"ValueChangedEventArgs.cs\" />", "");
                }
                else if (type == ControlType.GaugeDisplay)
                {
                    csproj = csproj.Replace("TextInputControl.xaml.cs", "GaugeControl.xaml.cs");
                    csproj = csproj.Replace("TextInputControl.xaml", "GaugeControl.xaml");
                    csproj = csproj.Replace("<Compile Include=\"TextInputHost.cs\" />", "");
                    csproj = csproj.Replace("TextInputPanel.cs", "GaugePanel.cs");
                    csproj = csproj.Replace("<Compile Include=\"ValueChangedEventArgs.cs\" />", "");
                }
                else if (type == ControlType.DataGridDisplay)
                {
                    csproj = csproj.Replace("TextInputControl.xaml.cs", "DataGridControl.xaml.cs");
                    csproj = csproj.Replace("TextInputControl.xaml", "DataGridControl.xaml");
                    csproj = csproj.Replace("<Compile Include=\"TextInputHost.cs\" />", "");
                    csproj = csproj.Replace("TextInputPanel.cs", "DataGridPanel.cs");
                    csproj = csproj.Replace("<Compile Include=\"ValueChangedEventArgs.cs\" />", "");
                }
                else if (type == ControlType.TreeDisplay)
                {
                    csproj = csproj.Replace("TextInputControl.xaml.cs", "TreeControl.xaml.cs");
                    csproj = csproj.Replace("TextInputControl.xaml", "TreeControl.xaml");
                    csproj = csproj.Replace("<Compile Include=\"TextInputHost.cs\" />", "");
                    csproj = csproj.Replace("TextInputPanel.cs", "TreePanel.cs");
                    csproj = csproj.Replace("<Compile Include=\"ValueChangedEventArgs.cs\" />", "");
                }
                else if (type == ControlType.SidebarNav)
                {
                    csproj = csproj.Replace("TextInputControl.xaml.cs", "SidebarControl.xaml.cs");
                    csproj = csproj.Replace("TextInputControl.xaml", "SidebarControl.xaml");
                    csproj = csproj.Replace("<Compile Include=\"TextInputHost.cs\" />", "");
                    csproj = csproj.Replace("TextInputPanel.cs", "SidebarPanel.cs");
                    csproj = csproj.Replace("<Compile Include=\"ValueChangedEventArgs.cs\" />", "");
                }

                csproj = csproj.Replace("<RootNamespace>WpfTextInput</RootNamespace>",
                                       "<RootNamespace>" + controlName + "</RootNamespace>");
                csproj = csproj.Replace("<AssemblyName>WpfTextInput</AssemblyName>",
                                       "<AssemblyName>" + controlName + "</AssemblyName>");
                // 生成唯一 GUID 避免冲突
                csproj = csproj.Replace("{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}",
                                       "{" + Guid.NewGuid().ToString().ToUpper() + "}");
                File.WriteAllText(Path.Combine(outputDir, controlName + ".csproj"), csproj, Encoding.UTF8);
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
                new { Type = ControlType.ChartDisplay,      XamlTemplate = "ChartControl.xaml.template",             XamlOut = "ChartControl.xaml",             Files = new[] { "ChartControl.xaml.cs", "ChartPanel.cs" } },
                new { Type = ControlType.PieDisplay,        XamlTemplate = "PieControl.xaml.template",               XamlOut = "PieControl.xaml",               Files = new[] { "PieControl.xaml.cs", "PiePanel.cs" } },
                new { Type = ControlType.GaugeDisplay,      XamlTemplate = "GaugeControl.xaml.template",             XamlOut = "GaugeControl.xaml",             Files = new[] { "GaugeControl.xaml.cs", "GaugePanel.cs" } },
                new { Type = ControlType.DataGridDisplay,   XamlTemplate = "DataGridControl.xaml.template",          XamlOut = "DataGridControl.xaml",          Files = new[] { "DataGridControl.xaml.cs", "DataGridPanel.cs" } },
                new { Type = ControlType.TreeDisplay,       XamlTemplate = "TreeControl.xaml.template",              XamlOut = "TreeControl.xaml",              Files = new[] { "TreeControl.xaml.cs", "TreePanel.cs" } },
                new { Type = ControlType.SidebarNav,        XamlTemplate = "SidebarControl.xaml.template",           XamlOut = "SidebarControl.xaml",           Files = new[] { "SidebarControl.xaml.cs", "SidebarPanel.cs" } },
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
                    xaml = ReplaceNamespace(xaml, assemblyName);
                    File.WriteAllText(Path.Combine(outputDir, ctrl.XamlOut), xaml, Encoding.UTF8);
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
                        content = ReplaceNamespace(content, assemblyName);
                        content = ApplyStyle(content, style);
                        File.WriteAllText(Path.Combine(outputDir, file), content, Encoding.UTF8);
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
                File.WriteAllText(Path.Combine(outputDir, assemblyName + ".csproj"), csproj, Encoding.UTF8);
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
                { "{{BorderThickness}}", FormatNumber(style.BorderThickness) },
                { "{{CornerRadius}}", FormatNumber(style.CornerRadius) },
                { "{{ShadowBlur}}", FormatNumber(style.ShadowBlur) },
                { "{{ShadowDepth}}", FormatNumber(style.ShadowDepth) },
                { "{{ShadowColor}}", CleanColor(style.ShadowColor) },
                { "{{ShadowOpacity}}", FormatNumber(style.ShadowOpacity) },
                { "{{ShadowMargin}}", CalcShadowMargin(style.ShadowBlur, style.ShadowDepth) },
                { "{{HighlightColor}}", CleanColor(style.HighlightColor) },
                { "{{HighlightOpacity}}", FormatNumber(style.HighlightOpacity) },
                { "{{FontFamily}}", style.FontFamily },
                { "{{FontSize}}", FormatNumber(style.FontSize) },
                { "{{FontColor}}", CleanColor(style.FontColor) },
                { "{{CaretColor}}", CleanColor(style.CaretColor) },
                { "{{LabelColor}}", CleanColor(style.LabelColor) },
                { "{{LabelFontSize}}", FormatNumber(style.LabelFontSize) },
                { "{{FocusBorderColor}}", CleanColor(style.FocusBorderColor) },
                { "{{AccentColor}}", CleanColor(style.AccentColor) },
                { "{{LedActiveColor}}", CleanColor(style.LedOnColor) },
                { "{{LedOffColor}}", CleanColor(style.LedOffColor) },
                { "{{CardPadding}}", style.CardPadding },
                { "{{DataGridRowHeight}}", FormatNumber(style.DataGridRowHeight) },
                { "{{DataGridHeaderColor}}", CleanColor(style.DataGridHeaderBackground) },
                { "{{DataGridBackground}}", CleanColor(style.DataGridBackground) },
                { "{{DataGridAlternatingOpacity}}", FormatNumber(style.DataGridAlternatingOpacity) },
                { "{{DataGridGridLines}}", style.DataGridGridLinesVisible ? "Horizontal" : "None" },
                { "{{DataGridLabelText}}", style.DataGridLabelText },
                { "{{DataGridHeaderVisibility}}", style.DataGridShowHeader ? "Column" : "None" },

                // Chart 专属
                { "{{ChartTitle}}", style.ChartTitle },
                { "{{ChartSubtitle}}", style.ChartSubtitle },
                { "{{ChartLineWeight}}", FormatNumber(style.ChartLineWeight) },
                { "{{ChartFillOpacity}}", FormatNumber(style.ChartFillOpacity) },
                { "{{ChartColor1}}", CleanColor(style.ChartColor1) },
                { "{{ChartColor2}}", CleanColor(style.ChartColor2) },
                { "{{ChartColor3}}", CleanColor(style.ChartColor3) },
                { "{{ChartShowGridLines}}", style.ChartShowGridLines.ToString().ToLower() },
                { "{{ChartPlotBackground}}", CleanColor(style.ChartPlotBackground) },
                { "{{ChartShowSeriesCards}}", style.ChartShowSeriesCards.ToString().ToLower() },
                { "{{ChartShowSeriesCardsVisibility}}", style.ChartShowSeriesCards ? "Visible" : "Collapsed" },

                // Gauge 专属
                { "{{GaugeStartColor}}", CleanColor(style.GaugeColor1) },
                { "{{GaugeEndColor}}", CleanColor(style.GaugeColor2) },

                // Slider 专属
                { "{{SliderColor1}}", CleanColor(style.SliderColor1) },
                { "{{SliderColor2}}", CleanColor(style.SliderColor2) },

                // ProgressBar 专属
                { "{{ProgressColor1}}", CleanColor(style.ProgressColor1) },
                { "{{ProgressColor2}}", CleanColor(style.ProgressColor2) },

                // Toggle 专属
                { "{{ToggleActiveColor}}", CleanColor(style.ToggleColorOn) },
                { "{{ToggleInactiveColor}}", CleanColor(style.ToggleColorOff) },
                { "{{ComboBoxArrowColor}}", CleanColor(style.ComboBoxArrowColor) },

                // Tree 专属
                { "{{TreeItemHeight}}", FormatNumber(style.TreeItemHeight) },
                { "{{TreeIndentSize}}", FormatNumber(style.TreeIndentSize) },
                { "{{TreeLabelText}}", style.TreeLabelText },
                { "{{TreeBackground}}", CleanColor(style.TreeBackground) },
                { "{{NodeCheckBoxVisibility}}", style.TreeShowCheckBox ? "Visibility=\"{Binding ShowCheckBox, Converter={StaticResource BooleanToVisibilityConverter}}\"" : "Visibility=\"Collapsed\"" },
                { "{{SidebarLogoText}}", style.SidebarLogoText },
                { "{{SidebarLogoIconText}}", string.IsNullOrWhiteSpace(style.SidebarLogoIconText) ? "🚀" : style.SidebarLogoIconText },
                { "{{SidebarLogoImagePath}}", string.IsNullOrWhiteSpace(style.SidebarLogoImagePath) ? string.Empty : style.SidebarLogoImagePath },
                { "{{SidebarLogoUseImage}}", style.SidebarLogoUseImage ? "True" : "False" },
                { "{{SidebarLogoMargin}}", style.SidebarLogoMargin },
                { "{{SidebarBackground}}", CleanColor(style.SidebarBackground) },
                { "{{SidebarItemHeight}}", FormatNumber(style.SidebarItemHeight) },
                { "{{SidebarItemSpacing}}", FormatNumber(style.SidebarItemSpacing) },
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

        private static string FormatNumber(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
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

            return string.Format("{0},{1},{2},{3}", left, top, right, bottom);

        }

        private string ReplaceNamespace(string content, string newNamespace)
        {
            if (string.IsNullOrEmpty(content)) return content;
            return content.Replace("{{Namespace}}", newNamespace)
                          .Replace("WpfTextInput", newNamespace)
                          .Replace("WpfNumericDisplay", newNamespace)
                          .Replace("WpfComboBox", newNamespace)
                          .Replace("WpfSlider", newNamespace)
                          .Replace("WpfButton", newNamespace)
                          .Replace("WpfLedIndicator", newNamespace)
                          .Replace("WpfToggleSwitch", newNamespace)
                          .Replace("WpfProgressBar", newNamespace)
                          .Replace("WpfChart", newNamespace)
                          .Replace("WpfPie", newNamespace)
                          .Replace("WpfGauge", newNamespace)
                          .Replace("WpfDataGrid", newNamespace)
                          .Replace("WpfTree", newNamespace)
                          .Replace("WpfSidebar", newNamespace);
        }
    }
}
