using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using System.Web.Script.Serialization;
using ControlDesigner.Models;
using ControlDesigner.Services;

namespace ControlDesigner
{
    public partial class MainWindow : Window
    {
        private ControlStyle _style;
        private TemplateEngine _templateEngine;
        private DllExporter _exporter;
        private bool _suppressUpdate;
        private List<PresetTheme> _presets;
        private ControlType _currentControlType = ControlType.TextInput;
        private Random _rnd = new Random();


        public MainWindow()
        {
            _suppressUpdate = true;
            InitializeComponent();
            InitializeServices();
            LoadPresets();
            _style = new ControlStyle();
            SyncUIFromStyle();
            UpdatePreview();
            UpdateColorSwatches();
            _suppressUpdate = false;
        }

        #region 初始化

        private void InitializeServices()
        {
            string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // 模板目录：与设计器同级的 ExportTemplate
            string templateDir = Path.Combine(exeDir, "..", "..", "..", "ExportTemplate");
            if (!Directory.Exists(templateDir))
                templateDir = Path.Combine(exeDir, "ExportTemplate");

            _templateEngine = new TemplateEngine(templateDir);
            _exporter = new DllExporter(_templateEngine);
        }

        private void LoadPresets()
        {
            _presets = new List<PresetTheme>();
            try
            {
                string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string jsonPath = Path.Combine(exeDir, "Themes", "Presets.json");
                if (!File.Exists(jsonPath))
                    jsonPath = Path.Combine(exeDir, "..", "..", "Themes", "Presets.json");

                if (File.Exists(jsonPath))
                {
                    string json = File.ReadAllText(jsonPath);
                    _presets = SimpleJsonParser.ParsePresets(json);
                }
            }
            catch { }

            // 渲染预设按钮
            PresetPanel.Children.Clear();
            foreach (var preset in _presets)
            {
                var btn = new Button
                {
                    Content = preset.Name,
                    Tag = preset,
                    Style = (Style)FindResource("PresetBtn"),
                };
                btn.Click += PresetBtn_Click;
                PresetPanel.Children.Add(btn);
            }
        }

        #endregion

        #region UI ↔ Style 同步

        private void SyncUIFromStyle()
        {
            _suppressUpdate = true;

            TxtControlBg.Text = _style.ControlBackground;
            TxtGradStart.Text = _style.GradientStart;
            TxtGradMid.Text = _style.GradientMid;
            TxtGradEnd.Text = _style.GradientEnd;

            TxtBorderColor.Text = _style.BorderColor;
            SliderBorderW.Value = _style.BorderThickness;
            SliderCorner.Value = _style.CornerRadius;

            TxtShadowColor.Text = _style.ShadowColor;
            SliderShadowBlur.Value = _style.ShadowBlur;
            SliderShadowDepth.Value = _style.ShadowDepth;
            SliderShadowOp.Value = _style.ShadowOpacity;

            TxtFontFamily.Text = _style.FontFamily;
            SliderFontSize.Value = _style.FontSize;
            TxtFontColor.Text = _style.FontColor;
            TxtLabelColor.Text = _style.LabelColor;
            SliderLabelSize.Value = _style.LabelFontSize;

            TxtFocusBorder.Text = _style.FocusBorderColor;
            TxtAccentColor.Text = _style.AccentColor;
            if (TxtComboBoxArrowColor != null) TxtComboBoxArrowColor.Text = _style.ComboBoxArrowColor;

            if (TxtLedOnColor != null) TxtLedOnColor.Text = _style.LedOnColor;
            if (TxtLedOffColor != null) TxtLedOffColor.Text = _style.LedOffColor;

            if (TxtChartTitle != null) TxtChartTitle.Text = _style.ChartTitle;
            if (TxtChartSubtitle != null) TxtChartSubtitle.Text = _style.ChartSubtitle;
            if (SliderChartLineW != null) SliderChartLineW.Value = _style.ChartLineWeight;
            if (SliderChartFill != null) SliderChartFill.Value = _style.ChartFillOpacity;
            if (TxtChartColor1 != null) TxtChartColor1.Text = _style.ChartColor1;
            if (TxtChartColor2 != null) TxtChartColor2.Text = _style.ChartColor2;
            if (TxtChartColor3 != null) TxtChartColor3.Text = _style.ChartColor3;
            if (TxtChartPlotBg != null) TxtChartPlotBg.Text = _style.ChartPlotBackground;
            if (ChkChartGridLines != null) ChkChartGridLines.IsChecked = _style.ChartShowGridLines;
            if (ChkChartShowSeriesCards != null) ChkChartShowSeriesCards.IsChecked = _style.ChartShowSeriesCards;

            if (SliderTableRowH != null) SliderTableRowH.Value = _style.DataGridRowHeight;
            if (TxtTableHeader != null) TxtTableHeader.Text = _style.DataGridHeaderBackground;
            if (TxtTableBg != null) TxtTableBg.Text = _style.DataGridBackground;
            if (SliderTableAlt != null) SliderTableAlt.Value = _style.DataGridAlternatingOpacity;
            if (ChkTableGridLines != null) ChkTableGridLines.IsChecked = _style.DataGridGridLinesVisible;
            if (TxtTableLabel != null) TxtTableLabel.Text = _style.DataGridLabelText;
            if (ChkTableHeader != null) ChkTableHeader.IsChecked = _style.DataGridShowHeader;

            if (TxtGaugeColor1 != null) TxtGaugeColor1.Text = _style.GaugeColor1;
            if (TxtGaugeColor2 != null) TxtGaugeColor2.Text = _style.GaugeColor2;

            if (TxtSliderColor1 != null) TxtSliderColor1.Text = _style.SliderColor1;
            if (TxtSliderColor2 != null) TxtSliderColor2.Text = _style.SliderColor2;
            if (TxtProgressColor1 != null) TxtProgressColor1.Text = _style.ProgressColor1;
            if (TxtProgressColor2 != null) TxtProgressColor2.Text = _style.ProgressColor2;

            if (TxtToggleColorOn != null) TxtToggleColorOn.Text = _style.ToggleColorOn;
            if (TxtToggleColorOff != null) TxtToggleColorOff.Text = _style.ToggleColorOff;

            UpdateSliderLabels();
            _suppressUpdate = false;
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "导入样式配置",
                Filter = "样式配置文件|*.style.json|JSON 文件|*.json|所有文件|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    string json = File.ReadAllText(dlg.FileName);
                    var serializer = new JavaScriptSerializer();
                    var importedStyle = serializer.Deserialize<ControlStyle>(json);
                    
                    if (importedStyle != null)
                    {
                        _style = importedStyle;
                        SyncUIFromStyle();
                        UpdateColorSwatches();
                        UpdatePreview();
                        
                        string controlName = Path.GetFileNameWithoutExtension(dlg.FileName).Replace(".style", "");
                        TxtControlName.Text = controlName;

                        MessageBox.Show("样式已成功导入！", "导入成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导入样式配置文件失败:\n" + ex.Message, "导入失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SyncStyleFromUI()
        {
            _style.ControlBackground = CleanColorString(TxtControlBg.Text);
            _style.GradientStart = CleanColorString(TxtGradStart.Text);
            _style.GradientMid = CleanColorString(TxtGradMid.Text);
            _style.GradientEnd = CleanColorString(TxtGradEnd.Text);

            _style.BorderColor = CleanColorString(TxtBorderColor.Text);
            _style.BorderThickness = SliderBorderW.Value;
            _style.CornerRadius = SliderCorner.Value;

            _style.ShadowColor = CleanColorString(TxtShadowColor.Text);
            _style.ShadowBlur = SliderShadowBlur.Value;
            _style.ShadowDepth = SliderShadowDepth.Value;
            _style.ShadowOpacity = SliderShadowOp.Value;

            _style.FontFamily = TxtFontFamily.Text.Trim();
            _style.FontSize = SliderFontSize.Value;
            _style.FontColor = CleanColorString(TxtFontColor.Text);
            _style.LabelColor = CleanColorString(TxtLabelColor.Text);
            _style.LabelFontSize = SliderLabelSize.Value;

            _style.FocusBorderColor = CleanColorString(TxtFocusBorder.Text);
            _style.AccentColor = CleanColorString(TxtAccentColor.Text);
            if (TxtComboBoxArrowColor != null) _style.ComboBoxArrowColor = CleanColorString(TxtComboBoxArrowColor.Text);

            if (TxtLedOnColor != null) _style.LedOnColor = CleanColorString(TxtLedOnColor.Text);
            if (TxtLedOffColor != null) _style.LedOffColor = CleanColorString(TxtLedOffColor.Text);

            if (TxtChartTitle != null) _style.ChartTitle = TxtChartTitle.Text;
            if (TxtChartSubtitle != null) _style.ChartSubtitle = TxtChartSubtitle.Text;
            if (SliderChartLineW != null) _style.ChartLineWeight = SliderChartLineW.Value;
            if (SliderChartFill != null) _style.ChartFillOpacity = SliderChartFill.Value;
            if (TxtChartColor1 != null) _style.ChartColor1 = CleanColorString(TxtChartColor1.Text);
            if (TxtChartColor2 != null) _style.ChartColor2 = CleanColorString(TxtChartColor2.Text);
            if (TxtChartColor3 != null) _style.ChartColor3 = CleanColorString(TxtChartColor3.Text);
            if (TxtChartPlotBg != null) _style.ChartPlotBackground = CleanColorString(TxtChartPlotBg.Text);
            if (ChkChartGridLines != null) _style.ChartShowGridLines = ChkChartGridLines.IsChecked == true;
            if (ChkChartShowSeriesCards != null) _style.ChartShowSeriesCards = ChkChartShowSeriesCards.IsChecked == true;

            if (SliderTableRowH != null) _style.DataGridRowHeight = SliderTableRowH.Value;
            if (TxtTableHeader != null) _style.DataGridHeaderBackground = CleanColorString(TxtTableHeader.Text);
            if (TxtTableBg != null) _style.DataGridBackground = CleanColorString(TxtTableBg.Text);
            if (SliderTableAlt != null) _style.DataGridAlternatingOpacity = SliderTableAlt.Value;
            if (ChkTableGridLines != null) _style.DataGridGridLinesVisible = ChkTableGridLines.IsChecked == true;
            if (TxtTableLabel != null) _style.DataGridLabelText = TxtTableLabel.Text;
            if (ChkTableHeader != null) _style.DataGridShowHeader = ChkTableHeader.IsChecked == true;

            if (TxtGaugeColor1 != null) _style.GaugeColor1 = CleanColorString(TxtGaugeColor1.Text);
            if (TxtGaugeColor2 != null) _style.GaugeColor2 = CleanColorString(TxtGaugeColor2.Text);

            if (TxtSliderColor1 != null) _style.SliderColor1 = CleanColorString(TxtSliderColor1.Text);
            if (TxtSliderColor2 != null) _style.SliderColor2 = CleanColorString(TxtSliderColor2.Text);
            if (TxtProgressColor1 != null) _style.ProgressColor1 = CleanColorString(TxtProgressColor1.Text);
            if (TxtProgressColor2 != null) _style.ProgressColor2 = CleanColorString(TxtProgressColor2.Text);

            if (TxtToggleColorOn != null) _style.ToggleColorOn = CleanColorString(TxtToggleColorOn.Text);
            if (TxtToggleColorOff != null) _style.ToggleColorOff = CleanColorString(TxtToggleColorOff.Text);
        }

        private void UpdateSliderLabels()
        {
            if (LblBorderW == null) return;
            LblBorderW.Text = SliderBorderW.Value.ToString("F1");
            LblCorner.Text = SliderCorner.Value.ToString("F0");
            LblShadowBlur.Text = SliderShadowBlur.Value.ToString("F0");
            LblShadowDepth.Text = SliderShadowDepth.Value.ToString("F0");
            LblShadowOp.Text = SliderShadowOp.Value.ToString("F2");
            LblFontSize.Text = SliderFontSize.Value.ToString("F0");
            LblLabelSize.Text = SliderLabelSize.Value.ToString("F0");
            if (LblChartLineW != null) LblChartLineW.Text = SliderChartLineW.Value.ToString("F1");
            if (LblChartFill != null) LblChartFill.Text = SliderChartFill.Value.ToString("F2");
            if (LblTableRowH != null) LblTableRowH.Text = SliderTableRowH.Value.ToString("F0");
            if (LblTableAlt != null) LblTableAlt.Text = SliderTableAlt.Value.ToString("F2");
        }

        private void UpdateColorSwatches()
        {
            if (SwatchControlBg == null) return;
            SetSwatchColor(SwatchControlBg, TxtControlBg.Text);
            SetSwatchColor(SwatchGradStart, TxtGradStart.Text);
            SetSwatchColor(SwatchGradMid, TxtGradMid.Text);
            SetSwatchColor(SwatchGradEnd, TxtGradEnd.Text);
            SetSwatchColor(SwatchBorderColor, TxtBorderColor.Text);
            SetSwatchColor(SwatchShadowColor, TxtShadowColor.Text);
            SetSwatchColor(SwatchFontColor, TxtFontColor.Text);
            SetSwatchColor(SwatchLabelColor, TxtLabelColor.Text);
            SetSwatchColor(SwatchFocusBorder, TxtFocusBorder.Text);
            SetSwatchColor(SwatchAccentColor, TxtAccentColor.Text);
            if (SwatchComboBoxArrowColor != null) SetSwatchColor(SwatchComboBoxArrowColor, TxtComboBoxArrowColor.Text);
            if (SwatchLedOn != null) SetSwatchColor(SwatchLedOn, TxtLedOnColor.Text);
            if (SwatchLedOff != null) SetSwatchColor(SwatchLedOff, TxtLedOffColor.Text);
            if (SwatchChartColor1 != null) SetSwatchColor(SwatchChartColor1, TxtChartColor1.Text);
            if (SwatchChartColor2 != null) SetSwatchColor(SwatchChartColor2, TxtChartColor2.Text);
            if (SwatchChartColor3 != null) SetSwatchColor(SwatchChartColor3, TxtChartColor3.Text);
            if (SwatchChartPlotBg != null) SetSwatchColor(SwatchChartPlotBg, TxtChartPlotBg.Text);
            if (SwatchTableHeader != null) SetSwatchColor(SwatchTableHeader, TxtTableHeader.Text);
            if (SwatchTableBg != null) SetSwatchColor(SwatchTableBg, TxtTableBg.Text);
            if (SwatchGaugeColor1 != null) SetSwatchColor(SwatchGaugeColor1, TxtGaugeColor1.Text);
            if (SwatchGaugeColor2 != null) SetSwatchColor(SwatchGaugeColor2, TxtGaugeColor2.Text);

            if (SwatchSliderColor1 != null) SetSwatchColor(SwatchSliderColor1, TxtSliderColor1.Text);
            if (SwatchSliderColor2 != null) SetSwatchColor(SwatchSliderColor2, TxtSliderColor2.Text);
            if (SwatchProgressColor1 != null) SetSwatchColor(SwatchProgressColor1, TxtProgressColor1.Text);
            if (SwatchProgressColor2 != null) SetSwatchColor(SwatchProgressColor2, TxtProgressColor2.Text);

            if (SwatchToggleColorOn != null) SetSwatchColor(SwatchToggleColorOn, TxtToggleColorOn.Text);
            if (SwatchToggleColorOff != null) SetSwatchColor(SwatchToggleColorOff, TxtToggleColorOff.Text);
        }

        private void SetSwatchColor(Border swatch, string hex)
        {
            swatch.Background = TryParseBrush(hex, Brushes.Transparent);
        }

        /// <summary>
        /// 点击色块弹出颜色选取器
        /// </summary>
        private void Swatch_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var swatch = (Border)sender;
            string swatchName = swatch.Name;

            // 根据色块名称找到对应的 TextBox
            TextBox targetTextBox = FindTextBoxForSwatch(swatchName);
            if (targetTextBox == null) return;

            string currentColor = targetTextBox.Text.Trim();
            var picker = new ColorPickerWindow(currentColor);
            picker.Owner = this;

            if (picker.ShowDialog() == true)
            {
                targetTextBox.Text = picker.SelectedColor;
            }
        }

        private TextBox FindTextBoxForSwatch(string swatchName)
        {
            // Swatch名称 → TextBox 映射
            switch (swatchName)
            {
                case "SwatchControlBg": return TxtControlBg;
                case "SwatchGradStart": return TxtGradStart;
                case "SwatchGradMid": return TxtGradMid;
                case "SwatchGradEnd": return TxtGradEnd;
                case "SwatchBorderColor": return TxtBorderColor;
                case "SwatchShadowColor": return TxtShadowColor;
                case "SwatchFontColor": return TxtFontColor;
                case "SwatchLabelColor": return TxtLabelColor;
                case "SwatchFocusBorder": return TxtFocusBorder;
                case "SwatchAccentColor": return TxtAccentColor;
                case "SwatchComboBoxArrowColor": return TxtComboBoxArrowColor;
                case "SwatchLedOn": return TxtLedOnColor;
                case "SwatchLedOff": return TxtLedOffColor;
                case "SwatchChartColor1": return TxtChartColor1;
                case "SwatchChartColor2": return TxtChartColor2;
                case "SwatchChartColor3": return TxtChartColor3;
                case "SwatchChartPlotBg": return TxtChartPlotBg;
                case "SwatchTableHeader": return TxtTableHeader;
                case "SwatchTableBg": return TxtTableBg;
                case "SwatchGaugeColor1": return TxtGaugeColor1;
                case "SwatchGaugeColor2": return TxtGaugeColor2;
                case "SwatchSliderColor1": return TxtSliderColor1;
                case "SwatchSliderColor2": return TxtSliderColor2;
                case "SwatchProgressColor1": return TxtProgressColor1;
                case "SwatchProgressColor2": return TxtProgressColor2;
                case "SwatchToggleColorOn": return TxtToggleColorOn;
                case "SwatchToggleColorOff": return TxtToggleColorOff;
                default: return null;
            }
        }

        #endregion

        #region 实时预览

        private void UpdatePreview()
        {
            try
            {
                // 预览区背景应保持中性，不随控件背景色改变，以展示控件边界
                PreviewContainer.Background = new SolidColorBrush(Color.FromRgb(240, 240, 243));

                var preview = BuildPreviewControl();

                // 所有控件现在都直接填充，不走 Viewbox 整体缩放，以保持文字清晰度
                PreviewContainer.Child = preview;
            }
            catch { }
        }

        private UIElement BuildPreviewControl()
        {
            double cr = _style.CornerRadius;

            // 外层容器
            var outerGrid = new Grid { 
                Margin = new Thickness(20),
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };

            // 高光层已移除

            // 最终修正：仅基础输入控件需要全局 Card 包装以提供基础容器感
            bool shouldWrap = _currentControlType == ControlType.TextInput || 
                            _currentControlType == ControlType.NumericDisplay ||
                            _currentControlType == ControlType.ComboBoxInput;

            bool isLarge = _currentControlType == ControlType.ChartDisplay || 
                           _currentControlType == ControlType.DataGridDisplay ||
                           _currentControlType == ControlType.PieDisplay ||
                           _currentControlType == ControlType.GaugeDisplay;

            // 核心包装容器
            var shadowLayer = new Border
            {
                CornerRadius = new CornerRadius(cr),
                Background = TryParseBrush(_style.ControlBackground, Brushes.LightGray),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Effect = new DropShadowEffect
                {
                    BlurRadius = _style.ShadowBlur,
                    ShadowDepth = _style.ShadowDepth,
                    Direction = 315,
                    Color = TryParseColor(_style.ShadowColor, Colors.Gray),
                    Opacity = _style.ShadowOpacity,
                },
            };

            var card = new Border
            {
                CornerRadius = new CornerRadius(cr),
                Padding = ParseThickness(_style.CardPadding),
                BorderThickness = new Thickness(_style.BorderThickness),
                BorderBrush = TryParseBrush(_style.BorderColor, Brushes.Gray),
            };

            var dock = new DockPanel { LastChildFill = true };

            if (shouldWrap)
            {
                shadowLayer.MaxWidth = 520; 
                shadowLayer.MaxHeight = 280;
                shadowLayer.MinWidth = 280;
                card.MinHeight = 120; 
            }
            else if (isLarge)
            {
                // 大型组件使用响应式宽度，避免在窄窗口下被裁剪
                dock.MaxWidth = 720; 
                dock.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
            else
            {
                // 按钮、LED、开关、滑动条、进度条等直接居中
                dock.HorizontalAlignment = HorizontalAlignment.Center;
                dock.VerticalAlignment = VerticalAlignment.Center;
            }

            // 渐变背景
            var grad = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
            };
            grad.GradientStops.Add(new GradientStop(TryParseColor(_style.GradientStart, Colors.White), 0));
            grad.GradientStops.Add(new GradientStop(TryParseColor(_style.GradientMid, Colors.WhiteSmoke), 0.5));
            grad.GradientStops.Add(new GradientStop(TryParseColor(_style.GradientEnd, Colors.Gainsboro), 1));
            card.Background = grad;

            // card 现在只需填充 shadowLayer 即可
            card.HorizontalAlignment = HorizontalAlignment.Stretch;
            card.VerticalAlignment = VerticalAlignment.Stretch;

            // 标签
            var label = new TextBlock
            {
                Text = "标签名称",
                FontFamily = new FontFamily(_style.FontFamily),
                FontSize = _style.LabelFontSize,
                FontWeight = FontWeights.SemiBold,
                Foreground = TryParseBrush(_style.LabelColor, Brushes.Gray),
                Margin = new Thickness(4, 0, 0, 4),
            };
            DockPanel.SetDock(label, Dock.Top);

            // 聚焦指示线
            var accentLine = new Border
            {
                Height = 2,
                CornerRadius = new CornerRadius(1),
                Margin = new Thickness(8, 2, 8, 0),
                Background = TryParseBrush(_style.AccentColor, Brushes.Transparent),
            };
            DockPanel.SetDock(accentLine, Dock.Bottom);

            if (_currentControlType == ControlType.TextInput)
            {
                var textBox = new TextBox
                {
                    Text = "在此输入文字预览效果...",
                    FontFamily = new FontFamily(_style.FontFamily),
                    FontSize = _style.FontSize,
                    Foreground = TryParseBrush(_style.FontColor, Brushes.Black),
                    CaretBrush = TryParseBrush(_style.CaretColor, Brushes.Gray),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(10, 4, 10, 4),
                    AcceptsReturn = true,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };
                dock.Children.Add(label);
                dock.Children.Add(accentLine);
                dock.Children.Add(textBox);
            }
            else if (_currentControlType == ControlType.NumericDisplay)
            {
                var valBlock = new TextBlock
                {
                    Text = "123.45",
                    FontFamily = new FontFamily(_style.FontFamily),
                    FontSize = _style.FontSize,
                    FontWeight = FontWeights.Bold,
                    Foreground = TryParseBrush(_style.FontColor, Brushes.Black),
                    VerticalAlignment = VerticalAlignment.Center,
                };
                var unitBlock = new TextBlock
                {
                    Text = "V",
                    FontFamily = new FontFamily(_style.FontFamily),
                    FontSize = Math.Max(10, _style.FontSize * 0.7),
                    Foreground = TryParseBrush(_style.LabelColor, Brushes.Gray),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(4, 3, 0, 0)
                };
                var sp = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Margin = new Thickness(10, 4, 10, 4)
                };
                sp.Children.Add(valBlock);
                sp.Children.Add(unitBlock);

                dock.Children.Add(label);
                dock.Children.Add(accentLine);
                dock.Children.Add(sp);
            }
            else if (_currentControlType == ControlType.ComboBoxInput)
            {
                var comboBox = new ComboBox
                {
                    FontFamily = new FontFamily(_style.FontFamily),
                    FontSize = _style.FontSize,
                    Foreground = TryParseBrush(_style.FontColor, Brushes.Black),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(10, 4, 10, 4),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    IsEditable = false
                };
                
                var customStyle = GetPreviewComboBoxStyle();
                if (customStyle != null)
                {
                    comboBox.Style = customStyle;
                }

                comboBox.Items.Add("番茄炒鸡蛋");
                comboBox.Items.Add("紫菜汤");
                comboBox.Items.Add("凉拌粉丝");
                comboBox.Items.Add("红烧排骨");
                comboBox.SelectedIndex = 1;

                dock.Children.Add(label);
                dock.Children.Add(accentLine);
                dock.Children.Add(comboBox);
            }
            else if (_currentControlType == ControlType.SliderInput)
            {
                // 滑动杆预览: 进度条风格填充+拖动把手+实时数值
                var sliderStack = new StackPanel { Margin = new Thickness(4) };

                // 标签行
                var headerGrid = new Grid { Margin = new Thickness(2, 0, 2, 4) };
                var lbl = new TextBlock
                {
                    Text = "标签",
                    FontFamily = new FontFamily(_style.FontFamily),
                    FontSize = _style.LabelFontSize,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = TryParseBrush(_style.LabelColor, Brushes.Gray),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                var valBlock = new TextBlock
                {
                    Text = "50.00",
                    FontFamily = new FontFamily(_style.FontFamily),
                    FontSize = _style.LabelFontSize,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = TryParseBrush(_style.LabelColor, Brushes.Gray),
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                headerGrid.Children.Add(lbl);
                headerGrid.Children.Add(valBlock);

                // 轨道容器
                Color accentCol = TryParseColor(_style.AccentColor, Color.FromRgb(122, 138, 168));
                var trackContainer = new Grid { Height = 20, Margin = new Thickness(2, 0, 2, 0), Cursor = System.Windows.Input.Cursors.Hand };

                // 背景轨道
                var trackBg = new Border
                {
                    Height = 6, CornerRadius = new CornerRadius(3),
                    VerticalAlignment = VerticalAlignment.Center,
                    Background = new SolidColorBrush(Color.FromArgb(50, 180, 180, 180))
                };

                // 填充轨道
                var fillBar = new Border
                {
                    Height = 6, CornerRadius = new CornerRadius(3),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Background = new LinearGradientBrush(TryParseColor(_style.SliderColor1, accentCol), TryParseColor(_style.SliderColor2, Colors.SteelBlue), 0)
                };

                // 圆形把手
                var thumbTranslate = new TranslateTransform(0, 0);
                var thumbEl = new Border
                {
                    Width = 16, Height = 16, CornerRadius = new CornerRadius(8),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    RenderTransform = thumbTranslate,
                    Background = Brushes.White,
                    Effect = new DropShadowEffect { BlurRadius = 4, ShadowDepth = 1, Direction = 270, Color = Colors.Gray, Opacity = 0.4 }
                };

                trackContainer.Children.Add(trackBg);
                trackContainer.Children.Add(fillBar);
                trackContainer.Children.Add(thumbEl);

                double currentRatio = 0.5;
                Action<double> updateVisual = null;
                updateVisual = (ratio) =>
                {
                    currentRatio = Math.Max(0, Math.Min(1, ratio));
                    double trackW = trackContainer.ActualWidth;
                    if (trackW <= 0) return;
                    fillBar.Width = Math.Max(0, trackW * currentRatio);
                    thumbTranslate.X = Math.Max(0, trackW * currentRatio - 8);
                    valBlock.Text = (currentRatio * 100).ToString("F2");
                };

                trackContainer.SizeChanged += (s, ev) => updateVisual(currentRatio);
                trackContainer.MouseLeftButtonDown += (s, ev) =>
                {
                    updateVisual(ev.GetPosition(trackContainer).X / trackContainer.ActualWidth);
                    trackContainer.CaptureMouse();
                };
                trackContainer.MouseMove += (s, ev) =>
                {
                    if (ev.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                        updateVisual(ev.GetPosition(trackContainer).X / trackContainer.ActualWidth);
                };
                trackContainer.MouseLeftButtonUp += (s, ev) => trackContainer.ReleaseMouseCapture();

                sliderStack.Children.Add(headerGrid);
                sliderStack.Children.Add(trackContainer);

                // 为各种输入类控件统一预览宽度，保证美观
                sliderStack.Width = 360; 
                sliderStack.HorizontalAlignment = HorizontalAlignment.Center;
                sliderStack.VerticalAlignment = VerticalAlignment.Center;

                outerGrid.Children.Clear();
                outerGrid.Children.Add(sliderStack);
                return outerGrid;
            }
            else if (_currentControlType == ControlType.ButtonInput)
            {
                var button = new Button
                {
                    Content = "Test",
                    Cursor = System.Windows.Input.Cursors.Hand,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    MinWidth = 200,
                    Padding = new Thickness(40, 15, 40, 15)
                };

                var customStyle = GetPreviewButtonStyle();
                if (customStyle != null)
                {
                    button.Style = customStyle;
                }

                dock.Children.Add(button);
            }
            else if (_currentControlType == ControlType.LedIndicator)
            {
                // LED 预览: 无卡片，可点击切换亮灭
                var ledGrid = new Grid { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                ledGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                ledGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var lbl = new TextBlock
                {
                    Text = "指示灯",
                    FontFamily = new FontFamily(_style.FontFamily),
                    FontSize = _style.FontSize,
                    Foreground = TryParseBrush(_style.FontColor, Brushes.Black),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 12, 0)
                };
                Grid.SetColumn(lbl, 0);

                Color onCol = TryParseColor(_style.LedOnColor, Color.FromRgb(76, 175, 80));
                Color offCol = TryParseColor(_style.LedOffColor, Colors.Gray);

                var ledContainer = new Grid { Width = 40, Height = 40, Cursor = System.Windows.Input.Cursors.Hand };
                // 凹槽底座
                var ledBase = new System.Windows.Shapes.Ellipse
                {
                    Margin = new Thickness(-2),
                    Fill = new RadialGradientBrush(
                        new GradientStopCollection { new GradientStop(Color.FromArgb(80, 200, 200, 200), 0.7), new GradientStop(Colors.Transparent, 1.0) })
                };
                // 灯体
                bool isOn = true;
                var ledBulb = new System.Windows.Shapes.Ellipse
                {
                    Fill = new RadialGradientBrush(onCol, Color.FromArgb(180, onCol.R, onCol.G, onCol.B))
                };
                // 高光
                var ledHL = new System.Windows.Shapes.Ellipse
                {
                    Margin = new Thickness(4, 3, 8, 12), Opacity = 0.45,
                    Fill = new RadialGradientBrush(
                        new GradientStopCollection { new GradientStop(Colors.White, 0), new GradientStop(Colors.Transparent, 1) })
                    { Center = new Point(0.5, 0.3), GradientOrigin = new Point(0.5, 0.2) }
                };
                // 外发光
                var halo = new System.Windows.Shapes.Ellipse
                {
                    Margin = new Thickness(-4), Opacity = _style.ShadowOpacity, IsHitTestVisible = false,
                    Fill = new SolidColorBrush(onCol),
                    Effect = new System.Windows.Media.Effects.BlurEffect { Radius = _style.ShadowBlur * 0.5 }
                };

                ledContainer.Children.Add(ledBase);
                ledContainer.Children.Add(ledBulb);
                ledContainer.Children.Add(ledHL);
                ledContainer.Children.Add(halo);

                // 点击切换
                ledContainer.MouseLeftButtonDown += (s, ev) => {
                    isOn = !isOn;
                    if (isOn)
                    {
                        ledBulb.Fill = new RadialGradientBrush(onCol, Color.FromArgb(180, onCol.R, onCol.G, onCol.B));
                        halo.Opacity = _style.ShadowOpacity;
                    }
                    else
                    {
                        ledBulb.Fill = new SolidColorBrush(offCol);
                        halo.Opacity = 0;
                    }
                };

                Grid.SetColumn(ledContainer, 1);
                ledGrid.Children.Add(lbl);
                ledGrid.Children.Add(ledContainer);

                dock.Children.Add(ledGrid);
            }
            else if (_currentControlType == ControlType.ToggleSwitch)
            {
                // Toggle 预览: 无卡片，可点击切换
                var toggleGrid = new Grid { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Cursor = System.Windows.Input.Cursors.Hand };
                toggleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                toggleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var lbl = new TextBlock
                {
                    Text = "开关",
                    FontFamily = new FontFamily(_style.FontFamily),
                    FontSize = _style.FontSize,
                    Foreground = TryParseBrush(_style.FontColor, Brushes.Black),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 12, 0)
                };
                Grid.SetColumn(lbl, 0);

                var trackGrid = new Grid { Width = 64, Height = 34, VerticalAlignment = VerticalAlignment.Center };
                var trackBrush = new SolidColorBrush(TryParseColor(_style.ToggleColorOff, Color.FromRgb(200, 204, 208)));
                var track = new Border { CornerRadius = new CornerRadius(17), Background = trackBrush };
                var thumbTranslate = new TranslateTransform(0, 0);
                var thumb = new Border
                {
                    Width = 30, Height = 30, CornerRadius = new CornerRadius(15),
                    HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(2, 0, 0, 0),
                    Background = Brushes.White,
                    RenderTransform = thumbTranslate,
                    Effect = new DropShadowEffect { BlurRadius = 4, ShadowDepth = 1, Direction = 315, Color = Colors.Gray, Opacity = 0.3 }
                };
                trackGrid.Children.Add(track);
                trackGrid.Children.Add(thumb);

                bool isOn = false;
                Color accentCol = TryParseColor(_style.AccentColor, Color.FromRgb(74, 144, 226));

                trackGrid.MouseLeftButtonDown += (s, ev) => {
                    isOn = !isOn;
                    var dur = TimeSpan.FromSeconds(0.2);
                    thumbTranslate.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(isOn ? 30 : 0, dur));
                    trackBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(isOn ? TryParseColor(_style.ToggleColorOn, accentCol) : TryParseColor(_style.ToggleColorOff, Color.FromRgb(200, 204, 208)), dur));
                };

                Grid.SetColumn(trackGrid, 1);
                toggleGrid.Children.Add(lbl);
                toggleGrid.Children.Add(trackGrid);

                dock.Children.Add(toggleGrid);
            }
            else if (_currentControlType == ControlType.ProgressBarInput)
            {
                // 进度条预览: 无卡片，点击轨道设置进度
                var progStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center, Width = 280 };

                var headerGrid = new Grid { Margin = new Thickness(2, 0, 2, 4) };
                var pctBlock = new TextBlock
                {
                    Text = "65%",
                    FontFamily = new FontFamily(_style.FontFamily),
                    FontSize = _style.LabelFontSize, FontWeight = FontWeights.SemiBold,
                    Foreground = TryParseBrush(_style.FontColor, Brushes.Black),
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                var lblBlock = new TextBlock
                {
                    Text = "进度",
                    FontFamily = new FontFamily(_style.FontFamily),
                    FontSize = _style.LabelFontSize, FontWeight = FontWeights.SemiBold,
                    Foreground = TryParseBrush(_style.LabelColor, Brushes.Gray),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                headerGrid.Children.Add(lblBlock);
                headerGrid.Children.Add(pctBlock);

                Color accentCol = TryParseColor(_style.AccentColor, Color.FromRgb(74, 144, 226));
                var trackBorder = new Border
                {
                    Height = 10, CornerRadius = new CornerRadius(5), Margin = new Thickness(2, 0, 2, 0),
                    Background = new SolidColorBrush(Color.FromArgb(50, 180, 180, 180)),
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                var fillBar = new Border
                {
                    CornerRadius = new CornerRadius(5), HorizontalAlignment = HorizontalAlignment.Left,
                    Background = new LinearGradientBrush(TryParseColor(_style.ProgressColor1, accentCol), TryParseColor(_style.ProgressColor2, Colors.SteelBlue), 0)
                };
                var trackContent = new Grid();
                trackContent.Children.Add(fillBar);
                trackBorder.Child = trackContent;

                // 点击轨道设置进度
                trackBorder.MouseLeftButtonDown += (s, ev) => {
                    double x = ev.GetPosition(trackBorder).X;
                    double ratio = Math.Max(0, Math.Min(1, x / trackBorder.ActualWidth));
                    fillBar.Width = Math.Max(0, trackBorder.ActualWidth * ratio);
                    pctBlock.Text = string.Format("{0:F0}%", ratio * 100);
                };
                // 初始化填充 (65%)
                trackBorder.SizeChanged += (s, ev) => {
                    fillBar.Width = Math.Max(0, ev.NewSize.Width * 0.65);
                };

                progStack.Children.Add(headerGrid);
                progStack.Children.Add(trackBorder);

                progStack.Width = 320;
                progStack.HorizontalAlignment = HorizontalAlignment.Center;
                progStack.VerticalAlignment = VerticalAlignment.Center;

                dock.Children.Add(progStack);
            }
            else if (_currentControlType == ControlType.ChartDisplay)
            {
                string[] modeNames = { "Smooth", "Linear", "Step" };
                int curMode = _style.ChartLineMode;

                // Header
                var headerGrid = new Grid { Margin = new Thickness(0,0,0,6) };
                DockPanel.SetDock(headerGrid, Dock.Top);
                var titleStack = new StackPanel();
                titleStack.Children.Add(new TextBlock { Text = _style.ChartTitle, FontFamily = new FontFamily(_style.FontFamily), FontSize = 16, FontWeight = FontWeights.Bold, Foreground = TryParseBrush(_style.FontColor, Brushes.Black) });
                titleStack.Children.Add(new TextBlock { Text = _style.ChartSubtitle, FontFamily = new FontFamily(_style.FontFamily), FontSize = 10, Foreground = Brushes.Gray });
                headerGrid.Children.Add(titleStack);
                var modeBtn = new Border { 
                    CornerRadius = new CornerRadius(14), 
                    Background = new SolidColorBrush(Color.FromArgb(30, 0, 0, 0)), 
                    BorderBrush = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(14,5,14,5), 
                    HorizontalAlignment = HorizontalAlignment.Right, 
                    VerticalAlignment = VerticalAlignment.Top,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                modeBtn.Child = new TextBlock { Text = modeNames[curMode], FontSize = 11, FontWeight = FontWeights.SemiBold, Foreground = TryParseBrush(_style.FontColor, Brushes.Black) };
                modeBtn.MouseLeftButtonDown += (s2, e2) => { _style.ChartLineMode = (_style.ChartLineMode + 1) % 3; UpdatePreview(); };
                headerGrid.Children.Add(modeBtn);
                // dock.Children.Add(headerGrid); // 挪到单独 card 容器中

                Color cl1 = TryParseColor(_style.ChartColor1, Colors.DodgerBlue);
                Color cl2 = TryParseColor(_style.ChartColor2, Colors.MediumSpringGreen);
                Color cl3 = TryParseColor(_style.ChartColor3, Colors.OrangeRed);

                // Legend
                var legendPanel = new WrapPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(30,6,0,0) };
                DockPanel.SetDock(legendPanel, Dock.Bottom);

                Action<Color, string, string> addLeg = (c, t, prop) => {
                    var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,0,18,0) };
                    var legSwatch = new Border { 
                        Width=14, Height=8, CornerRadius=new CornerRadius(2), 
                        Background=new SolidColorBrush(c), 
                        Margin=new Thickness(0,0,6,0), 
                        VerticalAlignment=VerticalAlignment.Center,
                        Cursor = System.Windows.Input.Cursors.Hand,
                        ToolTip = "点击修改此曲线颜色"
                    };
                    legSwatch.MouseLeftButtonDown += (s3, e3) => {
                        string curHex = (string)_style.GetType().GetProperty(prop).GetValue(_style, null);
                        var picker = new ColorPickerWindow(curHex);
                        picker.Owner = this;
                        if (picker.ShowDialog() == true) {
                            _style.GetType().GetProperty(prop).SetValue(_style, picker.SelectedColor, null);
                            SyncUIFromStyle();
                            UpdatePreview();
                        }
                    };
                    sp.Children.Add(legSwatch);
                    sp.Children.Add(new TextBlock { Text=t, FontSize=10, Foreground=TryParseBrush(_style.FontColor, Brushes.Black), VerticalAlignment=VerticalAlignment.Center});
                    legendPanel.Children.Add(sp);
                };
                addLeg(cl1, "System A", "ChartColor1");
                addLeg(cl2, "System B", "ChartColor2");
                addLeg(cl3, "System C", "ChartColor3");
                // dock.Children.Add(legendPanel); // 挪到单独 card 容器中

                // 图表主体
                var chartGrid = new Grid();
                chartGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
                chartGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                chartGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                chartGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(18) });

                var yLabelGrid = new Grid();
                Grid.SetRow(yLabelGrid, 0); Grid.SetColumn(yLabelGrid, 0);
                string[] yLabels = { "100", "80", "60", "40", "20", "0" };
                for (int yi = 0; yi < 6; yi++) {
                    yLabelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    yLabelGrid.Children.Add(new TextBlock { Text = yLabels[yi], FontSize = 9, Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0,0,4,0), VerticalAlignment = VerticalAlignment.Top });
                    Grid.SetRow(yLabelGrid.Children[yLabelGrid.Children.Count-1] as UIElement, yi);
                }
                chartGrid.Children.Add(yLabelGrid);

                var xLabelGrid = new Grid();
                Grid.SetRow(xLabelGrid, 1); Grid.SetColumn(xLabelGrid, 1);
                string[] xLabels = { "0s", "10s", "20s", "30s", "40s", "50s", "60s" };
                for (int xi = 0; xi < xLabels.Length; xi++) {
                    xLabelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    var xTxt = new TextBlock { Text = xLabels[xi], FontSize = 9, Foreground = Brushes.Gray, HorizontalAlignment = (xi == 0 ? HorizontalAlignment.Left : (xi == xLabels.Length-1 ? HorizontalAlignment.Right : HorizontalAlignment.Center)) };
                    Grid.SetColumn(xTxt, xi);
                    xLabelGrid.Children.Add(xTxt);
                }
                chartGrid.Children.Add(xLabelGrid);

                // 绘图区
                var plotBorder = new Border { Background = TryParseBrush(_style.ChartPlotBackground, new SolidColorBrush(Color.FromArgb(8, 0, 0, 0))), CornerRadius = new CornerRadius(4), ClipToBounds = true };
                Grid.SetRow(plotBorder, 0); Grid.SetColumn(plotBorder, 1);
                var vb = new Viewbox { Stretch = Stretch.Fill };
                double cW = 500, cH = 250;
                var cv = new Canvas { Width = cW, Height = cH };
                vb.Child = cv; plotBorder.Child = vb;

                if (_style.ChartShowGridLines)
                {
                    for (int gi = 1; gi <= 5; gi++)
                        cv.Children.Add(new System.Windows.Shapes.Line { X1=0, Y1=cH*gi/6.0, X2=cW, Y2=cH*gi/6.0, Stroke=new SolidColorBrush(Color.FromArgb(25,0,0,0)), StrokeThickness=0.8 });
                    for (int gi = 1; gi <= 6; gi++)
                        cv.Children.Add(new System.Windows.Shapes.Line { X1=cW*gi/7.0, Y1=0, X2=cW*gi/7.0, Y2=cH, Stroke=new SolidColorBrush(Color.FromArgb(15,0,0,0)), StrokeThickness=0.8 });
                }

                // 生成 40 个关键数据点以消除混叠现象并统一数据源
                int kc = 40;
                double[][] rawY = new double[3][];
                for (int si = 0; si < 3; si++) {
                    rawY[si] = new double[kc];
                    for (int k = 0; k < kc; k++) {
                        // 使用固定的频率逻辑，确保所有模式一致
                        double phase = 60.0 * k / (kc - 1.0);
                        rawY[si][k] = cH * 0.4 + si * cH * 0.12 + Math.Sin(phase * (0.15 + si * 0.08)) * cH * 0.2;
                        rawY[si][k] = Math.Max(cH * 0.05, Math.Min(cH * 0.95, rawY[si][k]));
                    }
                }

                double lineW = _style.ChartLineWeight;
                double fillOp = _style.ChartFillOpacity;

                for (int si = 0; si < 3; si++) {
                    Color sc = (si == 0) ? cl1 : (si == 1 ? cl2 : cl3);
                    var lp = new PointCollection();
                    var fp = new PointCollection();
                    fp.Add(new Point(0, cH));

                    if (curMode == 0) { // Smooth
                        int n = 200; // 渲染依然用高分辨率，但数学函数与 rawY 完全一致
                        for (int i = 0; i < n; i++) {
                            double x = cW * i / (n - 1.0);
                            double phase = 60.0 * i / (n - 1.0);
                            double y = cH * 0.4 + si * cH * 0.12 + Math.Sin(phase * (0.15 + si * 0.08)) * cH * 0.2;
                            y = Math.Max(cH * 0.05, Math.Min(cH * 0.95, y));
                            lp.Add(new Point(x, y)); fp.Add(new Point(x, y));
                        }
                    } else if (curMode == 1) { // Linear
                        for (int k = 0; k < kc; k++) {
                            double x = cW * k / (kc - 1.0);
                            lp.Add(new Point(x, rawY[si][k])); fp.Add(new Point(x, rawY[si][k]));
                        }
                    } else { // Step
                        for (int k = 0; k < kc; k++) {
                            double x = cW * k / (kc - 1.0);
                            if (k > 0) { lp.Add(new Point(x, rawY[si][k-1])); fp.Add(new Point(x, rawY[si][k-1])); }
                            lp.Add(new Point(x, rawY[si][k])); fp.Add(new Point(x, rawY[si][k]));
                        }
                    }

                    fp.Add(new Point(cW, cH));
                    cv.Children.Add(new System.Windows.Shapes.Polygon { Points = fp, Fill = new LinearGradientBrush(sc, Colors.Transparent, 90) { Opacity = fillOp } });
                    cv.Children.Add(new System.Windows.Shapes.Polyline { Points = lp, Stroke = new SolidColorBrush(sc), StrokeThickness = lineW, StrokeLineJoin = PenLineJoin.Round });
                }

                chartGrid.Children.Add(plotBorder);
                
                // --- 组合最终布局 ---
                // --- 组合最终布局 ---
                // 禁用外层 Card 的默认背景与投影
                card.Background = Brushes.Transparent;
                card.BorderThickness = new Thickness(0);
                if (shadowLayer != null) shadowLayer.Background = Brushes.Transparent;

                var chartBacking = new Border {
                    Background = grad,
                    CornerRadius = new CornerRadius(cr),
                    Padding = ParseThickness(_style.CardPadding),
                    BorderThickness = new Thickness(_style.BorderThickness),
                    BorderBrush = TryParseBrush(_style.BorderColor, Brushes.Gray),
                    Effect = new DropShadowEffect { BlurRadius = _style.ShadowBlur, ShadowDepth = _style.ShadowDepth, Direction = 315, Color = TryParseColor(_style.ShadowColor, Colors.Gray), Opacity = _style.ShadowOpacity },
                    MaxWidth = 550, MaxHeight = 380,
                    UseLayoutRounding = true, SnapsToDevicePixels = true
                };
                TextOptions.SetTextFormattingMode(chartBacking, TextFormattingMode.Display);
                TextOptions.SetTextRenderingMode(chartBacking, TextRenderingMode.ClearType);

                var backingStack = new DockPanel { LastChildFill = true };
                backingStack.Children.Add(headerGrid); // 挂在上面
                backingStack.Children.Add(legendPanel); // 挂在下面
                backingStack.Children.Add(chartGrid); // 填充中间
                chartBacking.Child = backingStack;

                var chartWrapper = new Grid { VerticalAlignment = VerticalAlignment.Center };
                chartWrapper.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                chartWrapper.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                if (_style.ChartShowSeriesCards)
                {
                    var cardsPanel = new Border 
                    {
                        CornerRadius = new CornerRadius(Math.Max(8, _style.CornerRadius * 0.8)),
                        Padding = new Thickness(14, 12, 14, 12),
                        Margin = new Thickness(0, 0, 24, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        Effect = new DropShadowEffect
                        {
                            BlurRadius = _style.ShadowBlur,
                            ShadowDepth = _style.ShadowDepth,
                            Direction = 315,
                            Color = TryParseColor(_style.ShadowColor, Colors.Gray),
                            Opacity = _style.ShadowOpacity
                        }
                    };

                    var cardGradient = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 1) };
                    cardGradient.GradientStops.Add(new GradientStop(TryParseColor(_style.GradientStart, Colors.White), 0));
                    cardGradient.GradientStops.Add(new GradientStop(TryParseColor(_style.GradientMid, Colors.WhiteSmoke), 0.5));
                    cardGradient.GradientStops.Add(new GradientStop(TryParseColor(_style.GradientEnd, Colors.Gainsboro), 1));
                    cardsPanel.Background = cardGradient;

                    var innerStack = new StackPanel { Orientation = Orientation.Vertical };
                    Grid.SetIsSharedSizeScope(innerStack, true);

                    string[] titles = { "System A", "System B", "System C" };
                    string[] vals = { "85.2", "64.8", "42.5" };
                    Color[] colors = { cl1, cl2, cl3 };
                    string[] props = { "ChartColor1", "ChartColor2", "ChartColor3" };

                    for (int i = 0; i < 3; i++) {
                        var rowGrid = new Grid { Margin = new Thickness(0, 4, 0, 4) };
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, SharedSizeGroup="ColorCol" });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, SharedSizeGroup="ValueCol" });

                        var nameBlock = new TextBlock { Text = titles[i], FontSize = 12, Foreground = Brushes.DimGray, VerticalAlignment = VerticalAlignment.Center };
                        Grid.SetColumn(nameBlock, 0);

                        var colorDot = new Border { Width = 10, Height = 10, CornerRadius = new CornerRadius(5), Background = new SolidColorBrush(colors[i]), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 8, 0), Cursor = System.Windows.Input.Cursors.Hand, ToolTip = "点击修改颜色" };
                        string propName = props[i];
                        colorDot.MouseLeftButtonDown += (sd, ed) => {
                            string curHex = (string)_style.GetType().GetProperty(propName).GetValue(_style, null);
                            var cp = new ColorPickerWindow(curHex);
                            cp.Owner = this;
                            if (cp.ShowDialog() == true) {
                                _style.GetType().GetProperty(propName).SetValue(_style, cp.SelectedColor, null);
                                SyncUIFromStyle(); UpdatePreview();
                            }
                        };
                        Grid.SetColumn(colorDot, 1);

                        var valBlock = new TextBlock { Text = vals[i], FontSize = 13, FontWeight = FontWeights.SemiBold, Foreground = TryParseBrush(_style.FontColor, Brushes.Black), VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Right, MinWidth = 26 };
                        Grid.SetColumn(valBlock, 2);

                        rowGrid.Children.Add(nameBlock); rowGrid.Children.Add(colorDot); rowGrid.Children.Add(valBlock);
                        innerStack.Children.Add(rowGrid);
                    }
                    cardsPanel.Child = innerStack;

                    Grid.SetColumn(cardsPanel, 0);
                    chartWrapper.Children.Add(cardsPanel);
                }

                Grid.SetColumn(chartBacking, 1);
                chartWrapper.Children.Add(chartBacking);
                
                dock.Children.Add(chartWrapper);

            }
            else if (_currentControlType == ControlType.DataGridDisplay)
            {
                // 数据表格预览：回归标准 dock 管线
                label.Text = _style.DataGridLabelText;
                dock.Children.Add(label);
                
                var previewWrapper = new Grid { Margin = new Thickness(0,12,0,0), MaxWidth = 720, MinWidth = 450, HorizontalAlignment = HorizontalAlignment.Center };
                
                var shadowFrame = new Border {
                    Background = TryParseBrush(_style.DataGridBackground, Brushes.White),
                    CornerRadius = new CornerRadius(8),
                    Effect = new DropShadowEffect { 
                        BlurRadius = _style.ShadowBlur, 
                        ShadowDepth = _style.ShadowDepth, 
                        Opacity = _style.ShadowOpacity, 
                        Color = TryParseColor(_style.ShadowColor, Colors.Gray),
                        Direction = 315
                    }
                };
                
                var dgBorder = new Border { 
                    BorderBrush = TryParseBrush(_style.BorderColor, Brushes.LightGray), 
                    BorderThickness = new Thickness(1), 
                    CornerRadius = new CornerRadius(8), 
                    Background = Brushes.Transparent,
                    ClipToBounds = true
                };
                
                var contentStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Stretch };
                
                // 表头：5 列模式
                var headerBorder = new Border {
                    Background = TryParseBrush(_style.DataGridHeaderBackground, new SolidColorBrush(Color.FromArgb(20, 0, 0, 0))),
                    Height = Math.Max(25, _style.DataGridRowHeight * 0.8),
                    CornerRadius = new CornerRadius(8, 8, 0, 0),
                    ClipToBounds = true,
                    Visibility = _style.DataGridShowHeader ? Visibility.Visible : Visibility.Collapsed
                };

                var tHeaderGrid = new Grid { Background = Brushes.Transparent };
                tHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
                tHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
                tHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                tHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });
                tHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });
                
                string[] hText = { "√", "项目名称", "采集时间", "数据值", "状态" };
                for(int hi=0; hi<hText.Length; hi++) {
                    var txt = new TextBlock { Text = hText[hi], FontWeight = FontWeights.Bold, VerticalAlignment=VerticalAlignment.Center, HorizontalAlignment=HorizontalAlignment.Center, FontSize=11, Foreground=TryParseBrush(_style.FontColor, Brushes.Black) };
                    if(hi==1) txt.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetColumn(txt, hi); tHeaderGrid.Children.Add(txt);
                }
                headerBorder.Child = tHeaderGrid;
                contentStack.Children.Add(headerBorder);

                double rowH = _style.DataGridRowHeight;
                double altOp = _style.DataGridAlternatingOpacity;
                bool showLines = _style.DataGridGridLinesVisible;

                for (int ri=0; ri<8; ri++) {
                    var rGrid = new Grid { Height = rowH, HorizontalAlignment = HorizontalAlignment.Stretch };
                    var rBorder = new Border { 
                        Background = (ri % 2 == 1) ? new SolidColorBrush(Color.FromArgb((byte)(altOp * 255), 0,0,0)) : Brushes.Transparent,
                        BorderBrush = showLines ? TryParseBrush(_style.BorderColor, Brushes.LightGray) : Brushes.Transparent,
                        BorderThickness = showLines ? new Thickness(0,0,0,0.5) : new Thickness(0)
                    };
                    
                    rGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
                    rGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
                    rGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    rGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });
                    rGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });
                    
                    var chk = new CheckBox { IsChecked = ri%3!=0, HorizontalAlignment=HorizontalAlignment.Center, VerticalAlignment=VerticalAlignment.Center };
                    Grid.SetColumn(chk, 0);

                    var tNam = new TextBlock { Text = "Sensor_" + (ri+101), Margin = new Thickness(12, 0, 0, 0), VerticalAlignment=VerticalAlignment.Center, FontSize=11, Foreground=TryParseBrush(_style.FontColor, Brushes.Black) };
                    Grid.SetColumn(tNam, 1);

                    var tTim = new TextBlock { Text = DateTime.Now.AddSeconds(-ri*30).ToString("HH:mm:ss"), VerticalAlignment=VerticalAlignment.Center, HorizontalAlignment=HorizontalAlignment.Center, FontSize=10, Foreground=Brushes.Gray };
                    Grid.SetColumn(tTim, 2);

                    var tVal = new TextBlock { Text = (24.5 + ri*1.2).ToString("F1"), VerticalAlignment=VerticalAlignment.Center, HorizontalAlignment=HorizontalAlignment.Center, FontSize=11, FontWeight=FontWeights.SemiBold, Foreground=TryParseBrush(_style.FontColor, Brushes.Black) };
                    Grid.SetColumn(tVal, 3);

                    var tSta = new Border { Width=42, Height=Math.Max(16, rowH * 0.5), CornerRadius=new CornerRadius(9), Background = (ri%4==0 ? Brushes.Coral : Brushes.MediumSeaGreen), HorizontalAlignment=HorizontalAlignment.Center, VerticalAlignment=VerticalAlignment.Center };
                    tSta.Child = new TextBlock { Text = (ri%4==0 ? "Error" : "OK"), Foreground=Brushes.White, FontSize=9, FontWeight=FontWeights.Bold, HorizontalAlignment=HorizontalAlignment.Center, VerticalAlignment=VerticalAlignment.Center };
                    Grid.SetColumn(tSta, 4);

                    Grid.SetColumnSpan(rBorder, 5); 
                    rGrid.Children.Add(rBorder);
                    rGrid.Children.Add(chk);
                    rGrid.Children.Add(tNam);
                    rGrid.Children.Add(tTim);
                    rGrid.Children.Add(tVal);
                    rGrid.Children.Add(tSta);
                    contentStack.Children.Add(rGrid);
                }
                dgBorder.Child = contentStack;
                previewWrapper.Children.Add(shadowFrame);
                previewWrapper.Children.Add(dgBorder);
                dock.Children.Add(previewWrapper);
            }
            else if (_currentControlType == ControlType.PieDisplay)
            {
                // Pie Placeholder
                var headerGrid = new Grid { Margin = new Thickness(0,0,0,12) };
                var titleStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
                titleStack.Children.Add(new TextBlock { Text = _style.ChartTitle, FontFamily = new FontFamily(_style.FontFamily), FontSize = 16, FontWeight = FontWeights.Bold, Foreground = TryParseBrush(_style.FontColor, Brushes.Black), HorizontalAlignment = HorizontalAlignment.Center });
                // titleStack.Children.Add(new TextBlock { Text = _style.ChartSubtitle, FontFamily = new FontFamily(_style.FontFamily), FontSize = 10, Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Center });
                headerGrid.Children.Add(titleStack);
                // 不在这里加到 dock 了，稍后加入到 pieWrapper 内部

                Color cl1 = TryParseColor(_style.ChartColor1, Colors.DodgerBlue);
                Color cl2 = TryParseColor(_style.ChartColor2, Colors.MediumSpringGreen);
                Color cl3 = TryParseColor(_style.ChartColor3, Colors.OrangeRed);

                var mainContainer = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                
                Border cardsPanel = null;
                if (_style.ChartShowSeriesCards) {
                    cardsPanel = new Border {
                        CornerRadius = new CornerRadius(Math.Max(8, _style.CornerRadius * 0.8)),
                        Padding = new Thickness(14, 12, 14, 12),
                        Margin = new Thickness(0, 0, 24, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        Effect = new DropShadowEffect { BlurRadius = _style.ShadowBlur, ShadowDepth = _style.ShadowDepth, Direction = 315, Color = TryParseColor(_style.ShadowColor, Colors.Gray), Opacity = _style.ShadowOpacity }
                    };
                    var cardGradient = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 1) };
                    cardGradient.GradientStops.Add(new GradientStop(TryParseColor(_style.GradientStart, Colors.White), 0));
                    cardGradient.GradientStops.Add(new GradientStop(TryParseColor(_style.GradientMid, Colors.WhiteSmoke), 0.5));
                    cardGradient.GradientStops.Add(new GradientStop(TryParseColor(_style.GradientEnd, Colors.Gainsboro), 1));
                    cardsPanel.Background = cardGradient;
                    
                    var innerStack = new StackPanel { Orientation = Orientation.Vertical, VerticalAlignment=VerticalAlignment.Center };
                    Grid.SetIsSharedSizeScope(innerStack, true);

                    string[] titles = { "A 部件", "B 部件", "C 部件" };
                    string[] vals = { "45.0 (45%)", "35.0 (35%)", "20.0 (20%)" };
                    Color[] colors = { cl1, cl2, cl3 };

                    for (int i=0; i<3; i++) {
                        var rowGrid = new Grid { Margin = new Thickness(0, 6, 0, 6) };
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, SharedSizeGroup="ColorCol" });
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, SharedSizeGroup="ValueCol" });

                        var nameBlock = new TextBlock { Text = titles[i], FontSize = 10, Foreground = Brushes.Gray, VerticalAlignment = VerticalAlignment.Center };
                        Grid.SetColumn(nameBlock, 0);

                        var colorDot = new Border { Width = 10, Height = 10, CornerRadius = new CornerRadius(5), Background = new SolidColorBrush(colors[i]), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 8, 0) };
                        Grid.SetColumn(colorDot, 1);

                        var valBlock = new TextBlock { Text = vals[i], FontSize = 12, FontWeight = FontWeights.Bold, Foreground = TryParseBrush(_style.FontColor, Brushes.Black), VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Right, MinWidth = 26 };
                        Grid.SetColumn(valBlock, 2);

                        rowGrid.Children.Add(nameBlock); rowGrid.Children.Add(colorDot); rowGrid.Children.Add(valBlock);
                        innerStack.Children.Add(rowGrid);
                    }
                    cardsPanel.Child = innerStack;
                    // mainContainer.Children.Add(cardsPanel); // 挪到下面 pieWrapper 内部进行排版
                }

                // 外部红底圆环框架 (由于层次感)
                var pieWrapper = new Grid { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                pieWrapper.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header Row
                pieWrapper.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Pie Row
                pieWrapper.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 0: 卡片列
                pieWrapper.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 1: 饼图列

                if (_style.ChartShowSeriesCards) {
                    Grid.SetRow(cardsPanel, 1);
                    Grid.SetColumn(cardsPanel, 0);
                    pieWrapper.Children.Add(cardsPanel);
                }

                // 将本来在顶部的 Header 附着在饼图正上方
                Grid.SetRow(headerGrid, 0);
                Grid.SetColumn(headerGrid, _style.ChartShowSeriesCards ? 1 : 0);
                pieWrapper.Children.Add(headerGrid);

                var bgGradient = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 1) };
                bgGradient.GradientStops.Add(new GradientStop(TryParseColor(_style.GradientStart, Colors.White), 0));
                bgGradient.GradientStops.Add(new GradientStop(TryParseColor(_style.GradientMid, Colors.WhiteSmoke), 0.5));
                bgGradient.GradientStops.Add(new GradientStop(TryParseColor(_style.GradientEnd, Colors.Gainsboro), 1));

                var outerFrame = new System.Windows.Shapes.Ellipse { 
                    Width = 176, Height = 176, 
                    Fill = bgGradient,
                    Stroke = new SolidColorBrush(Color.FromArgb(15, 0,0,0)), // 微弱边框
                    StrokeThickness = 1,
                    Effect = new DropShadowEffect { 
                        BlurRadius = _style.ShadowBlur, 
                        ShadowDepth = _style.ShadowDepth, 
                        Opacity = _style.ShadowOpacity, 
                        Color = TryParseColor(_style.ShadowColor, Colors.Gray),
                        Direction = 315
                    } 
                };
                Grid.SetRow(outerFrame, 1);
                Grid.SetColumn(outerFrame, _style.ChartShowSeriesCards ? 1 : 0);
                pieWrapper.Children.Add(outerFrame);

                var pieCanvas = new Canvas { Width = 160, Height = 160, HorizontalAlignment=HorizontalAlignment.Center, VerticalAlignment=VerticalAlignment.Center };
                var path1 = new System.Windows.Shapes.Path { Fill=new SolidColorBrush(cl1), Data=Geometry.Parse("M 80 80 L 80 0 A 80 80 0 0 1 156 55 Z") };
                var path2 = new System.Windows.Shapes.Path { Fill=new SolidColorBrush(cl2), Data=Geometry.Parse("M 80 80 L 156 55 A 80 80 0 0 1 15 127 Z") };
                var path3 = new System.Windows.Shapes.Path { Fill=new SolidColorBrush(cl3), Data=Geometry.Parse("M 80 80 L 15 127 A 80 80 0 0 1 80 0 Z") };
                pieCanvas.Children.Add(path1); pieCanvas.Children.Add(path2); pieCanvas.Children.Add(path3);
                
                var innerCircle = new System.Windows.Shapes.Ellipse { 
                    Width = 100, Height = 100, 
                    Fill = TryParseBrush(_style.ControlBackground, Brushes.WhiteSmoke), 
                    HorizontalAlignment = HorizontalAlignment.Center, 
                    VerticalAlignment = VerticalAlignment.Center 
                };
                
                // 将画布与内部遮罩组合
                var drawingGrid = new Grid { Width = 160, Height = 160 };
                drawingGrid.Children.Add(pieCanvas);
                drawingGrid.Children.Add(innerCircle);

                Grid.SetRow(drawingGrid, 1);
                Grid.SetColumn(drawingGrid, _style.ChartShowSeriesCards ? 1 : 0);
                pieWrapper.Children.Add(drawingGrid);
                
                dock.Children.Add(pieWrapper);
            }
            else if (_currentControlType == ControlType.GaugeDisplay)
            {
                // Gauge Placeholder
                var gaugeGrid = new Grid { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 20, 0, 0) };
                gaugeGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                gaugeGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var headerGrid = new StackPanel { Margin = new Thickness(0,0,0,20), HorizontalAlignment = HorizontalAlignment.Center };
                headerGrid.Children.Add(new TextBlock { Text = _style.ChartTitle, FontFamily = new FontFamily(_style.FontFamily), FontSize = 16, FontWeight = FontWeights.Bold, Foreground = TryParseBrush(_style.FontColor, Brushes.Black), HorizontalAlignment = HorizontalAlignment.Center });
                // headerGrid.Children.Add(new TextBlock { Text = _style.ChartSubtitle, FontFamily = new FontFamily(_style.FontFamily), FontSize = 12, Foreground = TryParseBrush(_style.LabelColor, Brushes.Gray), HorizontalAlignment = HorizontalAlignment.Center });
                Grid.SetRow(headerGrid, 0);
                gaugeGrid.Children.Add(headerGrid);

                Color accent = TryParseColor(_style.AccentColor, Colors.DeepSkyBlue);
                // Gauge specific elements
                var gaugeCanvas = new Canvas { Width = 240, Height = 240, HorizontalAlignment = HorizontalAlignment.Center, Background = Brushes.Transparent };
                
                var bgGradient = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 1) };
                bgGradient.GradientStops.Add(new GradientStop(TryParseColor(_style.GradientStart, Colors.White), 0));
                bgGradient.GradientStops.Add(new GradientStop(TryParseColor(_style.GradientMid, Colors.WhiteSmoke), 0.5));
                bgGradient.GradientStops.Add(new GradientStop(TryParseColor(_style.GradientEnd, Colors.Gainsboro), 1));

                var baseCirc = new System.Windows.Shapes.Ellipse { Width=240, Height=240, Fill=bgGradient, Effect = new DropShadowEffect { BlurRadius=_style.ShadowBlur, ShadowDepth=_style.ShadowDepth, Direction=315, Color=TryParseColor(_style.ShadowColor, Colors.Gray), Opacity=_style.ShadowOpacity } };
                gaugeCanvas.Children.Add(baseCirc);
                
                var trackArc = new System.Windows.Shapes.Ellipse { Width=180, Height=180, Stroke = new SolidColorBrush(Color.FromArgb(15, 0,0,0)), StrokeThickness = 30 };
                Canvas.SetLeft(trackArc, 30); Canvas.SetTop(trackArc, 30);
                gaugeCanvas.Children.Add(trackArc);

                double currentValue = 65;
                Brush arcGradient = new LinearGradientBrush(
                    TryParseColor(_style.GaugeColor1, Colors.Blue),
                    TryParseColor(_style.GaugeColor2, Colors.Green),
                    new Point(0.5, 0), new Point(0.5, 1));
                
                double initialSweep = (currentValue / 100.0) * 360.0;
                var valueArc = BuildArc(120, 120, 75, -90, initialSweep, 30, arcGradient);
                gaugeCanvas.Children.Add(valueArc);

                var valueText = new TextBlock { Text = string.Format("{0}%", currentValue), FontSize = 32, FontWeight = FontWeights.Bold, Foreground = TryParseBrush(_style.FontColor, Brushes.Black) };
                // Center text roughly
                valueText.Loaded += (s, e) => {
                    Canvas.SetLeft(valueText, 120 - valueText.ActualWidth / 2);
                    Canvas.SetTop(valueText, 120 - valueText.ActualHeight / 2);
                };
                gaugeCanvas.Children.Add(valueText);

                gaugeCanvas.MouseLeftButtonDown += (s, e) => {
                    var pos = e.GetPosition(gaugeCanvas);
                    double dx = pos.X - 120;
                    double dy = pos.Y - 120;
                    double angle = Math.Atan2(dy, dx) * 180 / Math.PI;
                    angle += 90; 
                    if (angle < 0) angle += 360;
                    currentValue = Math.Round((angle / 360.0) * 100);
                    
                    gaugeCanvas.Children.Remove(valueArc);
                    double sweep = (currentValue / 100.0) * 360.0;
                    valueArc = BuildArc(120, 120, 75, -90, sweep, 30, arcGradient);
                    gaugeCanvas.Children.Insert(2, valueArc); 
                    valueText.Text = string.Format("{0}%", currentValue);
                    Canvas.SetLeft(valueText, 120 - valueText.ActualWidth / 2);
                    Canvas.SetTop(valueText, 120 - valueText.ActualHeight / 2);
                };

                Grid.SetRow(gaugeCanvas, 1);
                gaugeGrid.Children.Add(gaugeCanvas);

                dock.Children.Add(gaugeGrid);
            }

            if (shouldWrap) {
                card.Child = dock;
                shadowLayer.Child = card;
                outerGrid.Children.Add(shadowLayer);
            } else {
                // 自含控件 (按扭、图表等) 直接居中放置，避免双重边框
                dock.HorizontalAlignment = HorizontalAlignment.Center;
                dock.VerticalAlignment = VerticalAlignment.Center;
                outerGrid.Children.Add(dock);
            }

            return outerGrid;
        }

        private System.Windows.Shapes.Path BuildArc(double cx, double cy, double radius, double startAngle, double sweepAngle, double thickness, Brush strokeBrush)
        {
            double startRad = startAngle * Math.PI / 180.0;
            double endRad = (startAngle + sweepAngle) * Math.PI / 180.0;

            Point startPoint = new Point(cx + radius * Math.Cos(startRad), cy + radius * Math.Sin(startRad));
            Point endPoint = new Point(cx + radius * Math.Cos(endRad), cy + radius * Math.Sin(endRad));

            bool isLargeArc = sweepAngle > 180;

            var arcSegment = new System.Windows.Media.ArcSegment
            {
                Point = endPoint,
                Size = new Size(radius, radius),
                IsLargeArc = isLargeArc,
                SweepDirection = System.Windows.Media.SweepDirection.Clockwise
            };

            var pathFigure = new System.Windows.Media.PathFigure
            {
                StartPoint = startPoint,
                Segments = new System.Windows.Media.PathSegmentCollection { arcSegment },
                IsClosed = false
            };

            var pathGeometry = new System.Windows.Media.PathGeometry
            {
                Figures = new System.Windows.Media.PathFigureCollection { pathFigure }
            };

            return new System.Windows.Shapes.Path
            {
                Stroke = strokeBrush,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Data = pathGeometry
            };
        }

        private Style GetPreviewComboBoxStyle()
        {
            string xaml = @"<Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='ComboBox'>
    <Style.Resources>
        <Thickness x:Key='ShadowMarginValue'>{{ShadowMargin}}</Thickness>
        <DropShadowEffect x:Key='CardShadow' BlurRadius='{{ShadowBlur}}' ShadowDepth='{{ShadowDepth}}' Direction='315' Color='{{ShadowColor}}' Opacity='{{ShadowOpacity}}'/>
        <ControlTemplate x:Key='ComboBoxToggleButton' TargetType='ToggleButton'>
            <Grid Background='Transparent'>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition />
                    <ColumnDefinition Width='32' />
                </Grid.ColumnDefinitions>
                <Path x:Name='Arrow' Grid.Column='1' HorizontalAlignment='Center' VerticalAlignment='Center' Data='M 0 0 L 5 5 L 10 0' Stroke='{{ComboBoxArrowColor}}' StrokeThickness='2' Fill='Transparent' />
            </Grid>
            <ControlTemplate.Triggers>
                <Trigger Property='IsChecked' Value='True'>
                    <Setter TargetName='Arrow' Property='Data' Value='M 0 5 L 5 0 L 10 5' />
                </Trigger>
            </ControlTemplate.Triggers>
        </ControlTemplate>
        <Style x:Key='FloatingComboBoxItemStyle' TargetType='ComboBoxItem'>
            <Setter Property='SnapsToDevicePixels' Value='True'/>
            <Setter Property='Padding' Value='16,10'/>
            <Setter Property='HorizontalContentAlignment' Value='Center'/>
            <Setter Property='VerticalContentAlignment' Value='Center'/>
            <Setter Property='Foreground' Value='{{FontColor}}'/>
            <Setter Property='Background' Value='Transparent'/>
            <Setter Property='Template'>
                <Setter.Value>
                    <ControlTemplate TargetType='ComboBoxItem'>
                        <Border x:Name='OuterBorder' Background='Transparent' Margin='4,2'>
                            <Border x:Name='InnerBorder' Background='{TemplateBinding Background}' CornerRadius='6' Padding='{TemplateBinding Padding}'>
                                <ContentPresenter HorizontalAlignment='{TemplateBinding HorizontalContentAlignment}' VerticalAlignment='{TemplateBinding VerticalContentAlignment}'/>
                            </Border>
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property='IsHighlighted' Value='True'>
                                <Setter TargetName='InnerBorder' Property='Background' Value='{{HighlightColor}}'/>
                            </Trigger>
                            <Trigger Property='IsSelected' Value='True'>
                                <Setter TargetName='InnerBorder' Property='Background' Value='{{HighlightColor}}'/>
                                <Setter Property='FontWeight' Value='SemiBold'/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    </Style.Resources>
    <Setter Property='SnapsToDevicePixels' Value='True'/>
    <Setter Property='OverridesDefaultStyle' Value='True'/>
    <Setter Property='ScrollViewer.HorizontalScrollBarVisibility' Value='Auto'/>
    <Setter Property='ScrollViewer.VerticalScrollBarVisibility' Value='Auto'/>
    <Setter Property='ScrollViewer.CanContentScroll' Value='True'/>
    <Setter Property='Background' Value='Transparent'/>
    <Setter Property='BorderThickness' Value='0'/>
    <Setter Property='ItemContainerStyle' Value='{StaticResource FloatingComboBoxItemStyle}'/>
    <Setter Property='Template'>
        <Setter.Value>
            <ControlTemplate TargetType='ComboBox'>
                <Grid>
                    <ToggleButton x:Name='ToggleButton' Template='{StaticResource ComboBoxToggleButton}' Focusable='False' IsChecked='{Binding IsDropDownOpen, Mode=TwoWay, RelativeSource={RelativeSource TemplatedParent}}' ClickMode='Press'/>
                    <ContentPresenter x:Name='ContentSite' IsHitTestVisible='False' Content='{TemplateBinding SelectionBoxItem}' ContentTemplate='{TemplateBinding SelectionBoxItemTemplate}' ContentTemplateSelector='{TemplateBinding ItemTemplateSelector}' Margin='16,0,32,0' VerticalAlignment='Center' HorizontalAlignment='Left'/>
                    <Popup x:Name='Popup' Placement='Bottom' IsOpen='{TemplateBinding IsDropDownOpen}' AllowsTransparency='True' Focusable='False' PopupAnimation='Fade' VerticalOffset='8'>
                        <Grid x:Name='DropDown' SnapsToDevicePixels='True' MinWidth='{TemplateBinding ActualWidth}' MaxHeight='{TemplateBinding MaxDropDownHeight}' Margin='{StaticResource ShadowMarginValue}'>
                            <Border x:Name='DropDownBorder' BorderBrush='Transparent' BorderThickness='0' CornerRadius='{{CornerRadius}}' Effect='{StaticResource CardShadow}'>
                                <Border.Background>
                                    <LinearGradientBrush StartPoint='0,0' EndPoint='1,1'>
                                        <GradientStop Color='{{GradientStart}}' Offset='0'/>
                                        <GradientStop Color='{{GradientMid}}' Offset='0.5'/>
                                        <GradientStop Color='{{GradientEnd}}' Offset='1'/>
                                    </LinearGradientBrush>
                                </Border.Background>
                                <ScrollViewer Margin='0,6' SnapsToDevicePixels='True'>
                                    <StackPanel IsItemsHost='True' KeyboardNavigation.DirectionalNavigation='Contained'/>
                                </ScrollViewer>
                            </Border>
                        </Grid>
                    </Popup>
                </Grid>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>";

            // 替换所有占位符 (注入前清洗)
            xaml = xaml.Replace("{{FontColor}}", CleanColorString(_style.FontColor));
            xaml = xaml.Replace("{{HighlightColor}}", CleanColorString(_style.HighlightColor));
            xaml = xaml.Replace("{{AccentColor}}", CleanColorString(_style.AccentColor));
            xaml = xaml.Replace("{{ComboBoxArrowColor}}", CleanColorString(_style.ComboBoxArrowColor));
            xaml = xaml.Replace("{{ControlBackground}}", CleanColorString(_style.ControlBackground));
            xaml = xaml.Replace("{{BorderColor}}", CleanColorString(_style.BorderColor));
            xaml = xaml.Replace("{{CornerRadius}}", _style.CornerRadius.ToString());
            xaml = xaml.Replace("{{ShadowBlur}}", _style.ShadowBlur.ToString());
            xaml = xaml.Replace("{{ShadowDepth}}", _style.ShadowDepth.ToString());
            xaml = xaml.Replace("{{ShadowColor}}", CleanColorString(_style.ShadowColor));
            xaml = xaml.Replace("{{ShadowOpacity}}", _style.ShadowOpacity.ToString("F2"));
            xaml = xaml.Replace("{{ShadowMargin}}", CalcShadowMargin(_style.ShadowBlur, _style.ShadowDepth));
            xaml = xaml.Replace("{{GradientStart}}", CleanColorString(_style.GradientStart));
            xaml = xaml.Replace("{{GradientMid}}", CleanColorString(_style.GradientMid));
            xaml = xaml.Replace("{{GradientEnd}}", CleanColorString(_style.GradientEnd));

            try
            {
                return (Style)System.Windows.Markup.XamlReader.Parse(xaml);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }

        private string CalcShadowMargin(double blur, double depth)
        {
            double margin = blur + Math.Abs(depth) + 5;
            return string.Format("{0},{1},{2},{3}", margin, margin, margin, margin);
        }

        private Style GetPreviewButtonStyle()
        {
            string xaml = @"<Style TargetType=""Button"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <Setter Property=""Template"">
        <Setter.Value>
            <ControlTemplate TargetType=""Button"">
                <Grid Margin=""{{ShadowMargin}}"">
                    <!-- 外阴影层1: 左上白色高光 -->
                    <Border x:Name=""LightShadowBorder"" CornerRadius=""{{CornerRadius}}"">
                        <Border.Effect>
                            <DropShadowEffect x:Name=""LightShadow"" BlurRadius=""8"" Direction=""135"" ShadowDepth=""3"" Color=""#FFFFFF"" Opacity=""1.0"" />
                        </Border.Effect>
                        <Border.Background>
                            <SolidColorBrush Color=""Transparent"" />
                        </Border.Background>
                    </Border>
                    <!-- 外阴影层2: 右下暗色阴影 + 主体 -->
                    <Border x:Name=""MainBorder"" CornerRadius=""{{CornerRadius}}"" RenderTransformOrigin=""0.5,0.5"" Cursor=""Hand"">
                        <Border.Effect>
                            <DropShadowEffect x:Name=""PartShadow"" BlurRadius=""{{ShadowBlur}}"" Direction=""315"" ShadowDepth=""{{ShadowDepth}}"" Color=""{{ShadowColor}}"" Opacity=""{{ShadowOpacity}}"" />
                        </Border.Effect>
                        <Border.RenderTransform>
                            <TransformGroup>
                                <ScaleTransform x:Name=""ScaleTrans"" ScaleX=""1.0"" ScaleY=""1.0"" />
                                <TranslateTransform x:Name=""PosTrans"" X=""0"" Y=""0"" />
                            </TransformGroup>
                        </Border.RenderTransform>
                        <Border.Background>
                            <LinearGradientBrush StartPoint=""0,0"" EndPoint=""1,1"">
                                <GradientStop Color=""{{GradientStart}}"" Offset=""0.0""/>
                                <GradientStop Color=""{{GradientMid}}"" Offset=""0.5""/>
                                <GradientStop Color=""{{GradientEnd}}"" Offset=""1.0""/>
                            </LinearGradientBrush>
                        </Border.Background>

                        <Grid Background=""Transparent"">
                            <!-- Hover Overlay -->
                            <Border x:Name=""HoverOverlay"" CornerRadius=""{{CornerRadius}}"" Opacity=""0"">
                                <Border.Background>
                                    <SolidColorBrush Color=""{{HighlightColor}}"" />
                                </Border.Background>
                            </Border>
                            <Border CornerRadius=""{{CornerRadius}}"" BorderThickness=""{{BorderThickness}}"">
                                <Border.BorderBrush>
                                    <SolidColorBrush Color=""{{BorderColor}}""/>
                                </Border.BorderBrush>
                                <Grid Background=""Transparent"">
                                    <TextBlock x:Name=""LabelBlock"" Margin=""{{CardPadding}}"" Text=""{TemplateBinding Content}"" FontFamily=""{{FontFamily}}"" FontSize=""{{FontSize}}"" Foreground=""{{FontColor}}"" HorizontalAlignment=""Center"" VerticalAlignment=""Center"" TextAlignment=""Center"" TextOptions.TextFormattingMode=""Display"" />
                                </Grid>
                            </Border>
                        </Grid>
                    </Border>
                </Grid>
                <ControlTemplate.Triggers>
                    <Trigger Property=""IsMouseOver"" Value=""True"">
                        <Trigger.EnterActions>
                            <BeginStoryboard> <Storyboard> <DoubleAnimation Storyboard.TargetName=""HoverOverlay"" Storyboard.TargetProperty=""Opacity"" To=""0.5"" Duration=""0:0:0.2""/> </Storyboard> </BeginStoryboard>
                        </Trigger.EnterActions>
                        <Trigger.ExitActions>
                            <BeginStoryboard> <Storyboard> <DoubleAnimation Storyboard.TargetName=""HoverOverlay"" Storyboard.TargetProperty=""Opacity"" To=""0"" Duration=""0:0:0.3""/> </Storyboard> </BeginStoryboard>
                        </Trigger.ExitActions>
                    </Trigger>
                    <Trigger Property=""IsPressed"" Value=""True"">
                        <Trigger.EnterActions>
                            <BeginStoryboard> <Storyboard> 
                                <DoubleAnimation Storyboard.TargetName=""PosTrans"" Storyboard.TargetProperty=""X"" To=""1.5"" Duration=""0:0:0.1""/> 
                                <DoubleAnimation Storyboard.TargetName=""PosTrans"" Storyboard.TargetProperty=""Y"" To=""1.5"" Duration=""0:0:0.1""/> 
                                <DoubleAnimation Storyboard.TargetName=""PartShadow"" Storyboard.TargetProperty=""ShadowDepth"" To=""0.0"" Duration=""0:0:0.15""/> 
                                <DoubleAnimation Storyboard.TargetName=""PartShadow"" Storyboard.TargetProperty=""BlurRadius"" To=""0.0"" Duration=""0:0:0.15""/> 
                                <DoubleAnimation Storyboard.TargetName=""PartShadow"" Storyboard.TargetProperty=""Opacity"" To=""0.0"" Duration=""0:0:0.15""/> 
                                <DoubleAnimation Storyboard.TargetName=""LightShadow"" Storyboard.TargetProperty=""Opacity"" To=""0.0"" Duration=""0:0:0.15""/> 
                                <DoubleAnimation Storyboard.TargetName=""LightShadow"" Storyboard.TargetProperty=""BlurRadius"" To=""0.0"" Duration=""0:0:0.15""/> 
                            </Storyboard> </BeginStoryboard>
                        </Trigger.EnterActions>
                        <Trigger.ExitActions>
                            <BeginStoryboard> <Storyboard> 
                                <DoubleAnimation Storyboard.TargetName=""PosTrans"" Storyboard.TargetProperty=""X"" To=""0.0"" Duration=""0:0:0.15""/> 
                                <DoubleAnimation Storyboard.TargetName=""PosTrans"" Storyboard.TargetProperty=""Y"" To=""0.0"" Duration=""0:0:0.15""/> 
                                <DoubleAnimation Storyboard.TargetName=""PartShadow"" Storyboard.TargetProperty=""ShadowDepth"" To=""{{ShadowDepth}}"" Duration=""0:0:0.2""/> 
                                <DoubleAnimation Storyboard.TargetName=""PartShadow"" Storyboard.TargetProperty=""BlurRadius"" To=""{{ShadowBlur}}"" Duration=""0:0:0.2""/> 
                                <DoubleAnimation Storyboard.TargetName=""PartShadow"" Storyboard.TargetProperty=""Opacity"" To=""{{ShadowOpacity}}"" Duration=""0:0:0.2""/> 
                                <DoubleAnimation Storyboard.TargetName=""LightShadow"" Storyboard.TargetProperty=""Opacity"" To=""1.0"" Duration=""0:0:0.2""/> 
                                <DoubleAnimation Storyboard.TargetName=""LightShadow"" Storyboard.TargetProperty=""BlurRadius"" To=""8.0"" Duration=""0:0:0.2""/> 
                            </Storyboard> </BeginStoryboard>
                        </Trigger.ExitActions>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>";

            xaml = xaml.Replace("{{FontColor}}", CleanColorString(_style.FontColor));
            xaml = xaml.Replace("{{FontFamily}}", _style.FontFamily);
            xaml = xaml.Replace("{{FontSize}}", _style.FontSize.ToString());
            xaml = xaml.Replace("{{HighlightColor}}", CleanColorString(_style.HighlightColor));
            xaml = xaml.Replace("{{AccentColor}}", CleanColorString(_style.AccentColor));
            xaml = xaml.Replace("{{ComboBoxArrowColor}}", CleanColorString(_style.ComboBoxArrowColor));
            xaml = xaml.Replace("{{ControlBackground}}", CleanColorString(_style.ControlBackground));
            xaml = xaml.Replace("{{BorderColor}}", CleanColorString(_style.BorderColor));
            xaml = xaml.Replace("{{BorderThickness}}", _style.BorderThickness.ToString());
            xaml = xaml.Replace("{{CardPadding}}", _style.CardPadding);
            xaml = xaml.Replace("{{CornerRadius}}", _style.CornerRadius.ToString());
            xaml = xaml.Replace("{{ShadowBlur}}", _style.ShadowBlur.ToString());
            xaml = xaml.Replace("{{ShadowDepth}}", _style.ShadowDepth.ToString());
            xaml = xaml.Replace("{{ShadowColor}}", CleanColorString(_style.ShadowColor));
            xaml = xaml.Replace("{{ShadowOpacity}}", _style.ShadowOpacity.ToString("F2"));
            xaml = xaml.Replace("{{ShadowMargin}}", CalcShadowMargin(_style.ShadowBlur, _style.ShadowDepth));
            xaml = xaml.Replace("{{GradientStart}}", CleanColorString(_style.GradientStart));
            xaml = xaml.Replace("{{GradientMid}}", CleanColorString(_style.GradientMid));
            xaml = xaml.Replace("{{GradientEnd}}", CleanColorString(_style.GradientEnd));

            try
            {
                return (Style)System.Windows.Markup.XamlReader.Parse(xaml);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("XAML Parse Error: {0}\n{1}", ex.Message, ex.StackTrace));
                return null;
            }
        }

        #endregion

        #region 事件处理

        private void Navigation_Checked(object sender, RoutedEventArgs e)
        {
            if (_suppressUpdate) return;
            
            string currentName = TxtControlName.Text.Trim();
            string currentTitle = TxtChartTitle != null ? TxtChartTitle.Text.Trim() : "";
            bool isDefaultTitle = string.IsNullOrEmpty(currentTitle) || currentTitle == "标题" || currentTitle == "实时曲线监控" || currentTitle == "折线图" || currentTitle == "饼图" || currentTitle == "仪表";
            // 如果文本为空，或者以"My"开头（例如用户未删干净默认字距），或者正好是几个默认的控件名称，都允许自动更新名称
            bool isDefaultText = string.IsNullOrEmpty(currentName) || 
                                 currentName.StartsWith("My") || 
                                 currentName == "TextInput" || 
                                 currentName == "NumericDisplay" || 
                                 currentName == "ComboBox" || 
                                 currentName == "Slider" ||
                                 currentName == "Chart" ||
                                 currentName == "Pie" ||
                                 currentName == "Gauge";

            if (sender == TabTextInput)
            {
                _currentControlType = ControlType.TextInput;
                if (isDefaultText) TxtControlName.Text = "MyTextInput";
            }
            else if (sender == TabNumeric)
            {
                _currentControlType = ControlType.NumericDisplay;
                if (isDefaultText) TxtControlName.Text = "MyNumericDisplay";
            }
            else if (sender == TabComboBox)
            {
                _currentControlType = ControlType.ComboBoxInput;
                if (isDefaultText) TxtControlName.Text = "MyComboBox";
            }
            else if (sender == TabSlider)
            {
                _currentControlType = ControlType.SliderInput;
                if (isDefaultText) TxtControlName.Text = "MySlider";
            }
            else if (sender == TabButton)
            {
                _currentControlType = ControlType.ButtonInput;
                if (isDefaultText || currentName == "Button") TxtControlName.Text = "MyButton";
            }
            else if (sender == TabLed)
            {
                _currentControlType = ControlType.LedIndicator;
                if (isDefaultText) TxtControlName.Text = "MyLed";
            }
            else if (sender == TabToggle)
            {
                _currentControlType = ControlType.ToggleSwitch;
                if (isDefaultText) TxtControlName.Text = "MyToggle";
            }
            else if (sender == TabProgress)
            {
                _currentControlType = ControlType.ProgressBarInput;
                if (isDefaultText) TxtControlName.Text = "MyProgressBar";
            }
            else if (sender == TabChart)
            {
                _currentControlType = ControlType.ChartDisplay;
                if (isDefaultText) TxtControlName.Text = "MyChart";
                if (isDefaultTitle && TxtChartTitle != null) TxtChartTitle.Text = "折线图";
            }
            else if (sender == TabPie)
            {
                _currentControlType = ControlType.PieDisplay;
                if (isDefaultText) TxtControlName.Text = "MyPie";
                if (isDefaultTitle && TxtChartTitle != null) TxtChartTitle.Text = "饼图";
            }
            else if (sender == TabGauge)
            {
                _currentControlType = ControlType.GaugeDisplay;
                if (isDefaultText) TxtControlName.Text = "MyGauge";
                if (isDefaultTitle) _style.ChartTitle = "仪表";
            }
            else if (sender == TabTable)
            {
                _currentControlType = ControlType.DataGridDisplay;
                if (isDefaultText) TxtControlName.Text = "MyDataGrid";
            }

            UpdateSidebarVisibility();
            UpdatePreview();
        }

        private void UpdateSidebarVisibility()
        {
            if (GroupBackground == null) return;
            bool isSimple = _currentControlType == ControlType.LedIndicator
                         || _currentControlType == ControlType.ToggleSwitch
                         || _currentControlType == ControlType.ProgressBarInput;

            GroupBackground.Visibility = isSimple ? Visibility.Collapsed : Visibility.Visible;
            GroupBorder.Visibility = isSimple ? Visibility.Collapsed : Visibility.Visible;
            // LED 显示阴影组（用于控制发光晕效果）
            GroupShadow.Visibility = (isSimple && _currentControlType != ControlType.LedIndicator) ? Visibility.Collapsed : Visibility.Visible;
            GroupFocus.Visibility = (_currentControlType == ControlType.LedIndicator) ? Visibility.Collapsed : Visibility.Visible;
            GroupFont.Visibility = Visibility.Visible;
            GroupLedColors.Visibility = _currentControlType == ControlType.LedIndicator ? Visibility.Visible : Visibility.Collapsed;
            if (GroupComboBoxConfig != null) GroupComboBoxConfig.Visibility = _currentControlType == ControlType.ComboBoxInput ? Visibility.Visible : Visibility.Collapsed;
            GroupChartConfig.Visibility = (_currentControlType == ControlType.ChartDisplay || _currentControlType == ControlType.PieDisplay) ? Visibility.Visible : Visibility.Collapsed;
            if (GroupChartConfig.Visibility == Visibility.Visible)
            {
                bool isPie = _currentControlType == ControlType.PieDisplay;
                var grid = GroupChartConfig.Children[1] as Grid;
                if (grid != null)
                {
                    foreach (UIElement child in grid.Children)
                    {
                        int r = Grid.GetRow(child);
                        if (r == 2 || r == 3 || r == 7 || r == 9)
                        {
                            child.Visibility = isPie ? Visibility.Collapsed : Visibility.Visible;
                        }
                    }
                }
                var txt = GroupChartConfig.Children[0] as TextBlock;
                if (txt != null) txt.Text = isPie ? "饼图" : "折线图";
            }
            if (GroupGaugeConfig != null) GroupGaugeConfig.Visibility = _currentControlType == ControlType.GaugeDisplay ? Visibility.Visible : Visibility.Collapsed;
            GroupSliderConfig.Visibility = _currentControlType == ControlType.SliderInput ? Visibility.Visible : Visibility.Collapsed;
            GroupProgressConfig.Visibility = _currentControlType == ControlType.ProgressBarInput ? Visibility.Visible : Visibility.Collapsed;
            GroupToggleConfig.Visibility = _currentControlType == ControlType.ToggleSwitch ? Visibility.Visible : Visibility.Collapsed;
            GroupDataGridConfig.Visibility = _currentControlType == ControlType.DataGridDisplay ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnStyleChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressUpdate || _style == null) return;
            SyncStyleFromUI();
            UpdateColorSwatches();
            UpdatePreview();
        }

        private void OnGenericStyleChanged(object sender, RoutedEventArgs e)
        {
            if (_suppressUpdate || _style == null) return;
            SyncStyleFromUI();
            UpdatePreview();
        }

        private void OnSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_suppressUpdate || _style == null) return;
            UpdateSliderLabels();
            SyncStyleFromUI();
            UpdatePreview();
        }

        private void PresetBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var preset = (PresetTheme)btn.Tag;
            ApplyPreset(preset);
        }

        private void ApplyPreset(PresetTheme p)
        {
            _style.ControlBackground = p.ControlBackground;
            _style.GradientStart = p.GradientStart;
            _style.GradientMid = p.GradientMid;
            _style.GradientEnd = p.GradientEnd;
            _style.BorderColor = p.BorderColor;
            _style.BorderThickness = p.BorderThickness;
            _style.CornerRadius = p.CornerRadius;
            _style.ShadowBlur = p.ShadowBlur;
            _style.ShadowDepth = p.ShadowDepth;
            _style.ShadowColor = p.ShadowColor;
            _style.ShadowOpacity = p.ShadowOpacity;
            _style.HighlightColor = p.HighlightColor;
            _style.HighlightOpacity = p.HighlightOpacity;
            _style.FontFamily = p.FontFamily;
            _style.FontSize = p.FontSize;
            _style.FontColor = p.FontColor;
            _style.CaretColor = p.CaretColor;
            _style.LabelColor = p.LabelColor;
            _style.LabelFontSize = p.LabelFontSize;
            _style.FocusBorderColor = p.FocusBorderColor;
            _style.AccentColor = p.AccentColor;
            _style.CardPadding = p.CardPadding;
            _style.ChartShowGridLines = p.ChartShowGridLines; // Added this line
            _style.ChartShowSeriesCards = true; // 预设默认显示

            SyncUIFromStyle();
            UpdatePreview();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            ExportControl(false);
        }

        private void BtnExportAll_Click(object sender, RoutedEventArgs e)
        {
            ExportControl(true);
        }

        private void ExportControl(bool exportAll)
        {
            // 读取并验证程序集名称
            string assemblyName = TxtControlName.Text.Trim();
            if (string.IsNullOrEmpty(assemblyName))
            {
                MessageBox.Show("请输入程序集名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtControlName.Focus();
                return;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(assemblyName, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                MessageBox.Show("程序集名称只能包含字母、数字和下划线，且不能以数字开头",
                    "名称无效", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtControlName.Focus();
                return;
            }

            string defaultFileName = exportAll ? "MyControlAll.dll" : assemblyName + ".dll";

            var dlg = new SaveFileDialog
            {
                Title = exportAll ? "导出 DLL（包含全部控件）" : string.Format("导出 DLL（{0}）", _currentControlType),
                Filter = "DLL 文件|*.dll",
                FileName = defaultFileName,
            };

            try
            {
                string defaultDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (Directory.Exists(defaultDir))
                    dlg.InitialDirectory = defaultDir;
            }
            catch { }

            if (dlg.ShowDialog() == true)
            {
                TxtStatus.Text = exportAll ? "⏳ 正在编译所有控件..." : string.Format("⏳ 正在编译 {0}...", _currentControlType);
                TxtStatus.Foreground = TryParseBrush("#FFD080", Brushes.Yellow);

                var style = _style.Clone();
                var outputPath = dlg.FileName;
                string name = System.IO.Path.GetFileNameWithoutExtension(outputPath);
                name = System.Text.RegularExpressions.Regex.Replace(name, @"[^A-Za-z0-9_]", "_");
                if (name.Length > 0 && char.IsDigit(name[0])) name = "_" + name;
                ControlType targetControl = _currentControlType;

                System.Threading.Tasks.Task.Run(() =>
                {
                    var result = exportAll ? _exporter.ExportAll(style, outputPath, name) : _exporter.Export(style, outputPath, name, targetControl);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        if (result.Success)
                        {
                            try
                            {
                                string infoPath = Path.ChangeExtension(outputPath, ".style.txt");
                                
                                // 1. 定义各控件的专有 API 说明
                                var apiDocs = new Dictionary<ControlType, string>
                                {
                                     { ControlType.TextInput, @"  ▶ TextInputPanel (文本输入框)
    - Text      (属性) : 获取或设置内部文本内容
    - LabelText (属性) : 获取或设置左上方的标签文字
    - Write     (方法) : 写入文本进入主存灌入
    - Read      (方法) : 获取拉取当前内部的链路字符文本
    - Clear     (方法) : 自动抹除清空输入
    - SetLabelVisible (方法) : 隐藏或展示组件顶部的标签名称
    - SetScrollBarVisible (方法) : 配置打开或关闭横向/纵向滚轮条支撑
    - SetReadOnly (方法) : 驱逐配置其进入非交互静止只读模式" },
 
                                     { ControlType.NumericDisplay, @"  ▶ NumericDisplayPanel (数值显示框)
    - ValueStr  (属性) : 主体数值呈现带（带格式，字符串形态）
    - Unit      (属性) : 连线右侧的工程常驻变量单位
    - LabelText (属性) : 获取或设置左上方的标签文字说明
    - WriteDouble (方法) : 写入数值（自动贴底带 fmt 格式如 ""F2""）
    - WriteString (方法) : 写入纯文本直达板注入
    - Clear      (方法) : 彻底洗净清空显示
    - SetLabelVisible (方法) : 隐藏或展示最外部控制文本锚位
    - SetUnitVisible (方法) : 隐藏或展示最右端追加的刻印字段" },
 
                                     { ControlType.SliderInput, @"  ▶ SliderPanel (滑动杆)
    - Value     (属性) : 当前游标对应的有效浮显数值
    - Minimum   (属性) : 整体底部向左端靠拢的绝对下陷标极
    - Maximum   (属性) : 整体顶部向右端靠拢的绝对顶峰标极
    - TickFrequency (属性) : 拉拽期间的最小标刻浮盈增幅（步进值）
    - IsSnapToTickEnabled (属性) : 在拖拽间自动对齐至对应的吸附阻滞节点
    - LabelText (属性) : 标签文字
    - StartColor (属性) : 进位背景渐变起点色彩 (**HEX**)
    - EndColor   (属性) : 进位背景渐变终点色彩 (**HEX**)
    - StartColorValue (属性) : 背景起点色彩 (**数字，标准 RGB**)
    - EndColorValue   (属性) : 背景终点色彩 (**数字，标准 RGB**)
    - SetLabelVisible (方法) : 擦除或展平顶部的标签占位框
    - SetValueVisible (方法) : 隐藏或展示滑杆右浮悬的微调数值文本" },
 
                                     { ControlType.ProgressBarInput, @"  ▶ ProgressBarPanel (进度条)
    - Value     (属性) : 当前进度值
    - Minimum   (属性) : 进度底色 0% 指向起始极值
    - Maximum   (属性) : 进度饱和 100% 指向封顶极值
    - ShowPercentage (属性) : 控制是否在进度最右端浮显追加比例说明码
    - LabelText (属性) : 面板常驻主说明框
    - StartColor (属性) : 轨道起点主色 (**HEX**)
    - EndColor   (属性) : 轨道终点主色 (**HEX**)
    - StartColorValue (属性) : 轨道色彩极化起点 (**数字，标准 RGB**)
    - EndColorValue   (属性) : 轨道色彩极化终点 (**数字，标准 RGB**)
    - SetLabelVisible (方法) : 强制擦除顶部的标签描述锚位" },
 
                                     { ControlType.LedIndicator, @"  ▶ LedPanel (LED 指示灯)
    - IsOn        (属性) : 设为 true 开启亮灯，false 熄灭
    - ActiveColor (属性) : 亮灯点燃触发的核心球体主色 (**HEX**)
    - ActiveColorValue (属性) : 亮灯核心球体主色 (**数字，标准 RGB**)
    - LabelText   (属性) : 细粒度描述文本标签
    - SetLabelVisible (方法) : 配置常驻抹平左侧标签占位框" },
 
                                     { ControlType.ToggleSwitch, @"  ▶ ToggleSwitchPanel (开关)
    - IsOn        (属性) : 设为 true 开启，false 关闭
    - ActiveColor (属性) : 开状态对应的全景背景轨道主色 (**HEX**)
    - ActiveColorValue (属性) : 开状态轨道主色 (**数字，标准 RGB**)
    - InactiveColor (属性) : 关状态对应的全景背景轨道主色 (**HEX**)
    - InactiveColorValue (属性) : 关状态轨道主色 (**数字，标准 RGB**)
    - LabelText   (属性) : 开关左上翼绑定的标签文字说明
    - SetLabelVisible (方法) : 高度剥离抹平文字对应的 label 外置区目
    - ValueChanged (事件) : 触发拨动后向 LabVIEW 倾覆 (old, new) 状态对" },
 
                                     { ControlType.ComboBoxInput, @"  ▶ ComboBoxPanel (下拉框)
    - Items        (属性) : 一维数组全额拉通直接覆盖并展开下拉条目
    - SelectedIndex(属性) : 被拉取的当前项数组序号标号 (-1为空)
    - TextValue    (属性) : 选中那一行所浮显出的最终文本绝对字符串率
    - LabelText    (属性) : 标题文字
    - AddItem      (方法) : 动态增入单项下拉选项卡
    - ClearItems   (方法) : 彻底洗净清除下拉弹窗中所拥有的多余条目
    - SetLabelVisible (方法) : 设置标签描述极高压制状态" },
 
                                     { ControlType.ButtonInput, @"  ▶ ButtonPanel (按钮)
    - Value        (属性) : 按钮当前所释放的动作逻辑电平
    - Behavior     (属性) : 动作机制分立（按下、抬起释放、脉冲锁定、常亮等）
    - LabelText    (属性) : 横陈按钮中心表面最立端的标志性标题
    - SetLabelVisible (方法) : 擦除中间浮现文字
    - Click        (事件) : 触发连环驱动的弹压节点" },
 
                                     { ControlType.ChartDisplay, @"  ▶ ChartPanel (折线图/波形图)
    - LabelText   (属性) : 主标题
    - DescText    (属性) : 副标题
    - YMin        (属性) : Y 轴显示极下点
    - YMax        (属性) : Y 轴显示极上点
    - AutoScaleY  (属性) : 启用 Y 轴波峰自动标测拉伸锁止 (True/False)
    - MaxPoints   (属性) : X轴流向的最大颗粒占用水泵极限阀门值 
    - ShowGridLines (属性) : 背景点阵辅助线可见性 (True/False)
    - LineThickness (属性) : 各折线带的连结线条粗细 (2.5 等)
    - FillOpacity   (属性) : 多翼背部的软性充注透明度 (0.0-1.0)
    - IsXAxisVisible (属性) : 物理背侧时间轴线标码可见性 (True/False)
    - IsYAxisVisible (属性) : 物理翼侧阈值轴线标码可见性 (True/False)
    - ShowLegends   (属性) : 关闭或浮显整个下沉式小系列多线图例提示
    - ShowSeriesCards (属性) : 弹出或关闭左上方数值观测总账折跃卡片
    - SetLabelVisible (方法) : 隐藏顶层主说明标签占位锚点
    - ClearSeries (方法) : 洗净所有连线缓存
    - AddSeries   (方法) : 增入新波线。参数: (标题, double数据组, 线条颜色, 渐变色) 颜色支持 HEX、I32 通量或 .NET Color
    - SetupSeries (方法) : 批量配置曲线：参数 (标签数组, 颜色数组)。支持簇数组拆分。
    - SetXLabels  (方法) : 配置横向底部轴线常驻说明字段对应映射数组
    - AppendPoint (方法) : 追加最新单点进单线
    - AppendPoints(方法) : 叠加多个点入单线 (数组形式)
    - AppendBatch (方法) : 并行向全部 Setup 后定义连线塞入点集
    - SetAllData  (方法) : 覆写灌入全量曲线(接受二维双精度数组)" },
 
                                     { ControlType.PieDisplay, @"  ▶ PiePanel (饼图)
    - LabelText (属性) : 饼框中心的绝对主标百分比填充文本
    - DescText  (属性) : 浮悬居中于主说明栏正下方的极细粒度字段
    - ShowSeriesCards (属性) : 隐藏或弹起右翼卡片分色观测数值报表列
    - SeriesNames (属性) : 获取目前已知所有的饼扇页骨架标签数组表
    - ClearSeries (方法) : 彻底摧毁所有切线，回归百分量模式
    - AddSeries(title, value, colorI32) : 累加单条饼翼。color 接收 I32
    - SetSeries(titles[], u[], colorIDs[]) : 批量覆写并轨同步
    - SetValue(title, double value) : 针对单一扇页执行微量动态补值重画" },
 
                                     { ControlType.GaugeDisplay, @"  ▶ GaugePanel (仪表)
    - LabelText (属性) : 表盘中部三维主标题
    - DescText  (属性) : 表盘下翼极细粒度字段解读小字
    - Minimum   (属性) : 左翼起始标刻缩放
    - Maximum   (属性) : 右翼饱和标刻缩放
    - Value     (属性) : 当前的环弦推进数值
    - StartColor (属性) : 外围环线起点渐变色彩 (**HEX**)
    - EndColor   (属性) : 外围环线终点渐变色彩 (**HEX**)
    - StartColorValue (属性) : 渐变起点色彩 (**数字，标准 RGB**)
    - EndColorValue   (属性) : 渐变终点色彩 (**数字，标准 RGB**)
    - SetRange(min, max) (方法) : 统一覆写同步量程双重界限
    - SetValue(value)    (方法) : 单线填充写入核心数值极" },
 
                                     { ControlType.DataGridDisplay, @"  ▶ DataGridPanel (数据表格)
    - LabelText     (属性) : 面板大头部标题名
    - ShowHeader    (属性) : 高亮列名可见性 (True/False)
    - RowHeight     (属性) : 行高度配置（紧致性调节）
    - HeaderColor   (属性) : 表头通栏高亮背景色 (**HEX字符串**)
    - HeaderColorValue (属性) : 表头背景色 (**数字，标准 RGB**)
    - SetLabelVisible (方法) : 去除标题常亮占位提示器
    - SetHeaders    (方法) : 设置列名分布段 Title 组
    - SetData       (方法) : 填充表格数据. 参数: [String 二维数组]
    - AddRow        (方法) : 在尾部独立叠加单条字符行 [String 一维数组]
    - Clear         (方法) : 全面抹除附着于面板上的全部条目
    - BindDataTable (方法) : 直接绑定传统托管 .NET DataTable 数据源
    - GetHeaders    (方法) : 获取拉出现有的合法表头列表
    - GetAllData    (方法) : 读取对应真实的全局二维字符串数组镜像表" }
                                 };

                                // 2. 构建 DLL 内部对象列表
                                string objectList = "";
                                if (exportAll)
                                {
                                    foreach (var kv in apiDocs)
                                    {
                                        objectList += string.Format("     -> {0}.{1}Panel\n", name, kv.Key);
                                    }
                                }
                                else
                                {
                                    objectList = string.Format("     -> {0}.{1}Panel", name, targetControl);
                                }

                                // 3. 构建 API 详细说明
                                string apiDetails = "";
                                if (exportAll)
                                {
                                    foreach (var kv in apiDocs)
                                    {
                                        apiDetails += kv.Value + "\n\n";
                                    }
                                }
                                else if (apiDocs.ContainsKey(targetControl))
                                {
                                    apiDetails = apiDocs[targetControl];
                                }

                                 var info = string.Format(@"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
【 {0} 控件库 API 使用说明书 】
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

■ LabVIEW 引用方式：
  1. 在前面板放置一个 [.NET 容器] (位于.NET与ActiveX选板)
  2. 右键容器 -> 插入 .NET 控件... -> 右上角 [浏览...]
  3. 选择生成的 DLL：{1}
  4. 从对象列表中选择您需要的控件：
{2}


■ 通用外观属性 (所有控件可用)：
  - BackColor   (属性) : 背景底色 (对应透明变色，控制最外围颜色)
  - ForeColor   (属性) : 前置字色/标签色
  - Font        (属性) : 字体样式及大小


■ 各控件专有 API：
{4}

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
■ 导出信息
名称: {0}   时间: {3:yyyy-MM-dd HH:mm:ss}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━", name, Path.GetFileName(outputPath), objectList, DateTime.Now, apiDetails);
                                try
                                {
                                    string guidePath = @"D:\Tyler\公众号\LabVIEW-WPF-UI-Generator-main\Exposed_APIs_Guide.md";
                                    if (!System.IO.File.Exists(guidePath))
                                    {
                                        guidePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Exposed_APIs_Guide.md");
                                    }
                                    if (System.IO.File.Exists(guidePath))
                                    {
                                        string guideContent = System.IO.File.ReadAllText(guidePath);
                                        info = info + "\n\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n【 附录：全量完整 API 指导手册 (Exposed_APIs_Guide.md) 】\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n" + guideContent;
                                    }
                                }
                                catch { }

                                File.WriteAllText(infoPath, info);

                                string jsonPath = Path.ChangeExtension(outputPath, ".style.json");
                                var serializer = new JavaScriptSerializer();
                                string json = serializer.Serialize(style);
                                json = json.Replace("\",\"", "\",\n  \"").Replace("{\"", "{\n  \"").Replace("\"}", "\"\n}");
                                File.WriteAllText(jsonPath, json);
                            }
                            catch { }

                            string msgText = exportAll ? "✅ {0}.dll 导出成功（含 12 种控件）" : "✅ {0}.dll 导出成功";
                            TxtStatus.Text = string.Format(msgText, name);
                            TxtStatus.Foreground = TryParseBrush("#70E070", Brushes.LightGreen);

                            string boxMsg = exportAll 
                                ? string.Format("DLL 已导出：\n{0}\n\n━━ 包含 12 种控件 ━━\nTextInput · Numeric · ComboBox · Slider · Button\nLed · Toggle · ProgressBar · Chart · Pie\nGauge · DataGrid\n\n📄 详细文档已同时生成", outputPath)
                                : string.Format("DLL 已导出：\n{0}\n\n📄 API 使用说明及配置已同时生成", outputPath);

                            MessageBox.Show(
                                boxMsg,
                                "导出成功 — " + name,
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            string errMsg = result.ErrorMessage ?? "导出失败";
                            if (!string.IsNullOrEmpty(result.BuildErrors))
                                errMsg += "\n\n编译错误:\n" + result.BuildErrors;

                            try
                            {
                                string logPath = Path.Combine(Path.GetDirectoryName(outputPath) ?? "", name + "_ErrorLog.txt");
                                string logContent = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] 导出控件 {1} 失败\r\n\r\n", DateTime.Now, name) +
                                                    string.Format("【错误详情】\r\n{0}\r\n\r\n", errMsg) +
                                                    string.Format("【完整编译输出】\r\n{0}", result.BuildOutput);
                                File.WriteAllText(logPath, logContent);
                                errMsg += string.Format("\n\n👉 详细报错日志已保存至:\n{0}", logPath);
                            }
                            catch { }

                            TxtStatus.Text = "❌ 导出失败";
                            TxtStatus.Foreground = TryParseBrush("#FF6060", Brushes.Red);
                            MessageBox.Show(errMsg, "导出失败", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }));
                });
            }
        }

        #endregion

        #region 辅助方法

        private static Brush TryParseBrush(string hex, Brush fallback)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hex)) return fallback;
                string cleanHex = hex.Trim();
                if (cleanHex.Equals("Transparent", StringComparison.OrdinalIgnoreCase)) return Brushes.Transparent;
                
                // 去除可能存在的多余井号并确保以单个井号开头
                cleanHex = cleanHex.TrimStart('#');
                if (cleanHex.Length > 0) cleanHex = "#" + cleanHex;
                else return fallback;

                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(cleanHex));
            }
            catch { return fallback; }
        }

        private static Color TryParseColor(string hex, Color fallback)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hex)) return fallback;
                string cleanHex = hex.Trim();
                if (cleanHex.Equals("Transparent", StringComparison.OrdinalIgnoreCase)) return Colors.Transparent;
                
                // 去除可能存在的多余井号并确保以单个井号开头
                cleanHex = cleanHex.TrimStart('#');
                if (cleanHex.Length > 0) cleanHex = "#" + cleanHex;
                else return fallback;

                return (Color)ColorConverter.ConvertFromString(cleanHex);
            }
            catch { return fallback; }
        }

        private static Thickness ParseThickness(string s)
        {
            try
            {
                var parts = s.Split(',');
                if (parts.Length == 4)
                    return new Thickness(double.Parse(parts[0]), double.Parse(parts[1]), double.Parse(parts[2]), double.Parse(parts[3]));
                if (parts.Length == 1)
                    return new Thickness(double.Parse(parts[0]));
            }
            catch { }
            return new Thickness(12, 8, 12, 6);
        }

        private static string CleanColorString(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return "#FFFFFF";
            string s = hex.Trim();
            if (s.Equals("Transparent", StringComparison.OrdinalIgnoreCase)) return "Transparent";
            
            // 核心逻辑：去除所有井号前缀，然后补回一个标准井号
            s = s.TrimStart('#');
            if (string.IsNullOrEmpty(s)) return "#FFFFFF";
            
            return "#" + s;
        }

        #endregion
    }

    #region 预设主题和简单 JSON 解析

    public class PresetTheme
    {
        public string Name { get; set; }
        public string ControlBackground { get; set; }
        public string GradientStart { get; set; }
        public string GradientMid { get; set; }
        public string GradientEnd { get; set; }
        public string BorderColor { get; set; }
        public double BorderThickness { get; set; }
        public double CornerRadius { get; set; }
        public double ShadowBlur { get; set; }
        public double ShadowDepth { get; set; }
        public string ShadowColor { get; set; }
        public double ShadowOpacity { get; set; }
        public string HighlightColor { get; set; }
        public double HighlightOpacity { get; set; }
        public string FontFamily { get; set; }
        public double FontSize { get; set; }
        public string FontColor { get; set; }
        public string CaretColor { get; set; }
        public string LabelColor { get; set; }
        public double LabelFontSize { get; set; }
        public string FocusBorderColor { get; set; }
        public string AccentColor { get; set; }
        public string CardPadding { get; set; }
        public bool ChartShowGridLines { get; set; }
    }

    /// <summary>
    /// 简单 JSON 解析器（避免引用 Newtonsoft）
    /// </summary>
    public static class SimpleJsonParser
    {
        public static List<PresetTheme> ParsePresets(string json)
        {
            var list = new List<PresetTheme>();
            // 用 System.Web.Script 的简易替代：手动解析
            // 按 '},' 分割各对象
            json = json.Trim().TrimStart('[').TrimEnd(']');
            var blocks = SplitObjects(json);

            foreach (var block in blocks)
            {
                var p = new PresetTheme();
                p.Name = GetString(block, "Name");
                p.ControlBackground = GetString(block, "ControlBackground");
                p.GradientStart = GetString(block, "GradientStart");
                p.GradientMid = GetString(block, "GradientMid");
                p.GradientEnd = GetString(block, "GradientEnd");
                p.BorderColor = GetString(block, "BorderColor");
                p.BorderThickness = GetDouble(block, "BorderThickness");
                p.CornerRadius = GetDouble(block, "CornerRadius");
                p.ShadowBlur = GetDouble(block, "ShadowBlur");
                p.ShadowDepth = GetDouble(block, "ShadowDepth");
                p.ShadowColor = GetString(block, "ShadowColor");
                p.ShadowOpacity = GetDouble(block, "ShadowOpacity");
                p.HighlightColor = GetString(block, "HighlightColor");
                p.HighlightOpacity = GetDouble(block, "HighlightOpacity");
                p.FontFamily = GetString(block, "FontFamily");
                p.FontSize = GetDouble(block, "FontSize");
                p.FontColor = GetString(block, "FontColor");
                p.CaretColor = GetString(block, "CaretColor");
                p.LabelColor = GetString(block, "LabelColor");
                p.LabelFontSize = GetDouble(block, "LabelFontSize");
                p.FocusBorderColor = GetString(block, "FocusBorderColor");
                p.AccentColor = GetString(block, "AccentColor");
                p.CardPadding = GetString(block, "CardPadding");
                p.ChartShowGridLines = GetBool(block, "ChartShowGridLines");
                if (!string.IsNullOrEmpty(p.Name))
                    list.Add(p);
            }
            return list;
        }

        private static List<string> SplitObjects(string json)
        {
            var result = new List<string>();
            int depth = 0;
            int start = -1;
            for (int i = 0; i < json.Length; i++)
            {
                if (json[i] == '{') { if (depth == 0) start = i; depth++; }
                else if (json[i] == '}') { depth--; if (depth == 0 && start >= 0) { result.Add(json.Substring(start, i - start + 1)); start = -1; } }
            }
            return result;
        }

        private static string GetString(string json, string key)
        {
            string pattern = "\"" + key + "\"";
            int idx = json.IndexOf(pattern);
            if (idx < 0) return "";
            int colon = json.IndexOf(':', idx + pattern.Length);
            if (colon < 0) return "";
            int qStart = json.IndexOf('"', colon + 1);
            if (qStart < 0) return "";
            int qEnd = json.IndexOf('"', qStart + 1);
            if (qEnd < 0) return "";
            return json.Substring(qStart + 1, qEnd - qStart - 1);
        }

        private static double GetDouble(string json, string key)
        {
            string pattern = "\"" + key + "\"";
            int idx = json.IndexOf(pattern);
            if (idx < 0) return 0;
            int colon = json.IndexOf(':', idx + pattern.Length);
            if (colon < 0) return 0;
            int start = colon + 1;
            while (start < json.Length && (json[start] == ' ' || json[start] == '\r' || json[start] == '\n')) start++;
            int end = start;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '.' || json[end] == '-')) end++;
            string val = json.Substring(start, end - start);
            double result;
            double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result);
            return result;
        }

        private static bool GetBool(string json, string key)
        {
            string pattern = "\"" + key + "\"";
            int idx = json.IndexOf(pattern);
            if (idx < 0) return true; // 默认显示网格
            int colon = json.IndexOf(':', idx + pattern.Length);
            if (colon < 0) return true;
            int start = colon + 1;
            while (start < json.Length && (json[start] == ' ' || json[start] == '\r' || json[start] == '\n')) start++;
            if (start + 4 <= json.Length && json.Substring(start, 4).ToLower() == "true") return true;
            if (start + 5 <= json.Length && json.Substring(start, 5).ToLower() == "false") return false;
            return true;
        }
    }

    #endregion
}
