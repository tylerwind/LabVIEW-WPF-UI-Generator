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

            if (TxtLedOnColor != null) TxtLedOnColor.Text = _style.LedOnColor;
            if (TxtLedOffColor != null) TxtLedOffColor.Text = _style.LedOffColor;

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

            if (TxtLedOnColor != null) _style.LedOnColor = CleanColorString(TxtLedOnColor.Text);
            if (TxtLedOffColor != null) _style.LedOffColor = CleanColorString(TxtLedOffColor.Text);
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
            if (SwatchLedOn != null) SetSwatchColor(SwatchLedOn, TxtLedOnColor.Text);
            if (SwatchLedOff != null) SetSwatchColor(SwatchLedOff, TxtLedOffColor.Text);
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
                case "SwatchLedOn": return TxtLedOnColor;
                case "SwatchLedOff": return TxtLedOffColor;
                default: return null;
            }
        }

        #endregion

        #region 实时预览

        private void UpdatePreview()
        {
            try
            {
                PreviewContainer.Background = TryParseBrush(_style.ControlBackground, Brushes.LightGray);

                // 包装在一个定宽高的 Border 内再使用 Viewbox 进行等比缩放
                var viewbox = new System.Windows.Controls.Viewbox
                {
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Child = new Border
                    {
                        Width = 360,
                        Height = 140,
                        Child = BuildPreviewControl()
                    }
                };

                // 动态构建预览控件
                PreviewContainer.Child = viewbox;
            }
            catch { }
        }

        private UIElement BuildPreviewControl()
        {
            double cr = _style.CornerRadius;

            // 外层容器
            var outerGrid = new Grid { Margin = new Thickness(20) };

            // 高光层已移除

            // 阴影层
            var shadowLayer = new Border
            {
                CornerRadius = new CornerRadius(cr),
                Background = TryParseBrush(_style.ControlBackground, Brushes.LightGray),
                Effect = new DropShadowEffect
                {
                    BlurRadius = _style.ShadowBlur,
                    ShadowDepth = _style.ShadowDepth,
                    Direction = 315,
                    Color = TryParseColor(_style.ShadowColor, Colors.Gray),
                    Opacity = _style.ShadowOpacity,
                },
            };

            // 主卡片
            var card = new Border
            {
                CornerRadius = new CornerRadius(cr),
                Padding = ParseThickness(_style.CardPadding),
                BorderThickness = new Thickness(_style.BorderThickness),
                BorderBrush = TryParseBrush(_style.BorderColor, Brushes.Gray),
            };

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

            // 内容
            var dock = new DockPanel { LastChildFill = true };

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
                    Background = new LinearGradientBrush(accentCol, TryParseColor(_style.FocusBorderColor, Colors.SteelBlue), 0)
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
                    MinWidth = 160,
                    Padding = new Thickness(0, 15, 0, 15)
                };

                var customStyle = GetPreviewButtonStyle();
                if (customStyle != null)
                {
                    button.Style = customStyle;
                }

                outerGrid.Children.Clear();
                outerGrid.Children.Add(button);
                return outerGrid;
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

                var ledContainer = new Grid { Width = 32, Height = 32, Cursor = System.Windows.Input.Cursors.Hand };
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
                    Margin = new Thickness(5, 4, 9, 14), Opacity = 0.45,
                    Fill = new RadialGradientBrush(
                        new GradientStopCollection { new GradientStop(Colors.White, 0), new GradientStop(Colors.Transparent, 1) })
                    { Center = new Point(0.5, 0.3), GradientOrigin = new Point(0.5, 0.2) }
                };
                // 外发光
                var halo = new System.Windows.Shapes.Ellipse
                {
                    Margin = new Thickness(-6), Opacity = _style.ShadowOpacity, IsHitTestVisible = false,
                    Fill = new SolidColorBrush(onCol),
                    Effect = new System.Windows.Media.Effects.BlurEffect { Radius = _style.ShadowBlur }
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
                        halo.Opacity = 0.4;
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

                outerGrid.Children.Clear();
                outerGrid.Children.Add(ledGrid);
                return outerGrid;
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

                var trackGrid = new Grid { Width = 48, Height = 26, VerticalAlignment = VerticalAlignment.Center };
                var trackBrush = new SolidColorBrush(Color.FromRgb(200, 204, 208));
                var track = new Border { CornerRadius = new CornerRadius(13), Background = trackBrush };
                var thumbTranslate = new TranslateTransform(0, 0);
                var thumb = new Border
                {
                    Width = 22, Height = 22, CornerRadius = new CornerRadius(11),
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
                    thumbTranslate.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(isOn ? 22 : 0, dur));
                    trackBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(isOn ? accentCol : Color.FromRgb(200, 204, 208), dur));
                };

                Grid.SetColumn(trackGrid, 1);
                toggleGrid.Children.Add(lbl);
                toggleGrid.Children.Add(trackGrid);

                outerGrid.Children.Clear();
                outerGrid.Children.Add(toggleGrid);
                return outerGrid;
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
                    Background = new LinearGradientBrush(accentCol, TryParseColor(_style.FocusBorderColor, Colors.SteelBlue), 0)
                };
                var trackContent = new Grid();
                trackContent.Children.Add(fillBar);
                trackBorder.Child = trackContent;

                // 点击轨道设置进度
                trackBorder.MouseLeftButtonDown += (s, ev) => {
                    double x = ev.GetPosition(trackBorder).X;
                    double ratio = Math.Max(0, Math.Min(1, x / trackBorder.ActualWidth));
                    fillBar.Width = Math.Max(0, trackBorder.ActualWidth * ratio);
                    pctBlock.Text = $"{ratio * 100:F0}%";
                };
                // 初始化填充 (65%)
                trackBorder.SizeChanged += (s, ev) => {
                    fillBar.Width = Math.Max(0, ev.NewSize.Width * 0.65);
                };

                progStack.Children.Add(headerGrid);
                progStack.Children.Add(trackBorder);

                outerGrid.Children.Clear();
                outerGrid.Children.Add(progStack);
                return outerGrid;
            }

            card.Child = dock;

            outerGrid.Children.Add(shadowLayer);
            outerGrid.Children.Add(card);

            return outerGrid;
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
                <Path x:Name='Arrow' Grid.Column='1' HorizontalAlignment='Center' VerticalAlignment='Center' Data='M 0 0 L 5 5 L 10 0' Stroke='{{AccentColor}}' StrokeThickness='2' Fill='Transparent' />
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
            return $"{margin},{margin},{margin},{margin}";
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
                                    <TextBlock x:Name=""LabelBlock"" Margin=""{{CardPadding}}"" Text=""{TemplateBinding Content}"" FontFamily=""{{FontFamily}}"" FontSize=""{{FontSize}}"" Foreground=""{{FontColor}}"" HorizontalAlignment=""Center"" VerticalAlignment=""Center"" TextAlignment=""Center"" />
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
                                <DoubleAnimation Storyboard.TargetName=""ScaleTrans"" Storyboard.TargetProperty=""ScaleX"" To=""0.98"" Duration=""0:0:0.15""/> 
                                <DoubleAnimation Storyboard.TargetName=""ScaleTrans"" Storyboard.TargetProperty=""ScaleY"" To=""0.98"" Duration=""0:0:0.15""/> 
                                <DoubleAnimation Storyboard.TargetName=""PartShadow"" Storyboard.TargetProperty=""ShadowDepth"" To=""0.0"" Duration=""0:0:0.15""/> 
                                <DoubleAnimation Storyboard.TargetName=""PartShadow"" Storyboard.TargetProperty=""BlurRadius"" To=""0.0"" Duration=""0:0:0.15""/> 
                                <DoubleAnimation Storyboard.TargetName=""PartShadow"" Storyboard.TargetProperty=""Opacity"" To=""0.0"" Duration=""0:0:0.15""/> 
                                <DoubleAnimation Storyboard.TargetName=""LightShadow"" Storyboard.TargetProperty=""Opacity"" To=""0.0"" Duration=""0:0:0.15""/> 
                                <DoubleAnimation Storyboard.TargetName=""LightShadow"" Storyboard.TargetProperty=""BlurRadius"" To=""0.0"" Duration=""0:0:0.15""/> 
                            </Storyboard> </BeginStoryboard>
                        </Trigger.EnterActions>
                        <Trigger.ExitActions>
                            <BeginStoryboard> <Storyboard> 
                                <DoubleAnimation Storyboard.TargetName=""ScaleTrans"" Storyboard.TargetProperty=""ScaleX"" To=""1.0"" Duration=""0:0:0.2""/> 
                                <DoubleAnimation Storyboard.TargetName=""ScaleTrans"" Storyboard.TargetProperty=""ScaleY"" To=""1.0"" Duration=""0:0:0.2""/> 
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
                System.Diagnostics.Debug.WriteLine($"XAML Parse Error: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        #endregion

        #region 事件处理

        private void Navigation_Checked(object sender, RoutedEventArgs e)
        {
            if (_suppressUpdate) return;
            
            string currentName = TxtControlName.Text.Trim();
            // 如果文本为空，或者以"My"开头（例如用户未删干净默认字距），或者正好是几个默认的控件名称，都允许自动更新名称
            bool isDefaultText = string.IsNullOrEmpty(currentName) || 
                                 currentName.StartsWith("My") || 
                                 currentName == "TextInput" || 
                                 currentName == "NumericDisplay" || 
                                 currentName == "ComboBox" || 
                                 currentName == "Slider";

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
        }

        private void OnStyleChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressUpdate || _style == null) return;
            SyncStyleFromUI();
            UpdateColorSwatches();
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
                Title = exportAll ? "导出 DLL（包含全部控件）" : $"导出 DLL（{_currentControlType}）",
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
                TxtStatus.Text = exportAll ? "⏳ 正在编译所有控件..." : $"⏳ 正在编译 {_currentControlType}...";
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
                    Dispatcher.Invoke(() =>
                    {
                        if (result.Success)
                        {
                            try
                            {
                                string infoPath = Path.ChangeExtension(outputPath, ".style.txt");
                                string singleControlInfo = $"     -> {name}.{targetControl}Panel";
                                string allControlInfo = $@"     -> {name}.TextInputPanel      (文本输入)
     -> {name}.NumericDisplayPanel  (数值显示)
     -> {name}.ComboBoxPanel        (下拉框)
     -> {name}.SliderPanel          (滑动杆)
     -> {name}.ButtonPanel          (按钮)
     -> {name}.LedPanel             (LED 指示灯)
     -> {name}.ToggleSwitchPanel    (开关)
     -> {name}.ProgressBarPanel     (进度条)";

                                 var info = $@"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
【 {name} 控件库 API 使用说明书 】
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

■ LabVIEW 引用方式：
  1. 在前面板放置一个 [.NET 容器] (位于.NET与ActiveX选板)
  2. 右键容器 -> 插入 .NET 控件... -> 右上角 [浏览...]
  3. 选择生成的 DLL：{outputPath}
  4. 从对象列表中选择您需要的控件：
{(exportAll ? allControlInfo : singleControlInfo)}


■ 通用外观属性 (所有控件可用)：
  - BackColor   (属性) : 背景底色 (对应透明变色，控制最外围颜色)
  - ForeColor   (属性) : 前置字色/标签色
  - Font        (属性) : 字体样式及大小


■ 各控件专有 API：
  ▶ TextInputPanel (文本输入框)
    - Text      (属性) : 获取或设置内部文本内容
    - LabelText (属性) : 获取或设置左上方的标签文字
    - SetLabelVisible(方法) : 设置标签是否可见 (true/false)

  ▶ NumericDisplayPanel (数值显示框)
    - ValueStr  (属性) : 主体数字部分（字符串格式）
    - Unit      (属性) : 显示在数字右侧的单位（如 ""mA""）
    - LabelText (属性) : 获取或设置左上方的标签文字
    - SetLabelVisible(方法) : 设置标签是否可见 (true/false)
    - SetUnitVisible (方法) : 设置单位是否可见

  ▶ SliderPanel (滑动杆)
    - Value     (属性) : 获取或设置当前游标对应的数值
    - Minimum   (属性) : 获取或设置最小值
    - Maximum   (属性) : 获取或设置最大值
    - TickFrequency (属性) : 拖动滑块时的离散步长步进值
    - IsSnapToTickEnabled (属性) : 启用或关闭拖拽时吸附
    - LabelText (属性) : 获取或设置标题文字
    - SetLabelVisible(方法) : 设置标签是否可见 (true/false)

  ▶ ProgressBarPanel (进度条)
    - Value     (属性) : 获取或设置当前进度值
    - Minimum   (属性) : 获取或设置最小值
    - Maximum   (属性) : 获取或设置最大值
    - ShowPercentage (属性) : 决定是否显示中间百分比文本
    - LabelText (属性) : 获取或设置标题文字
    - SetLabelVisible(方法) : 设置标签是否可见 (true/false)

  ▶ LedPanel (LED 指示灯)
    - IsOn        (属性) : 设为 true 亮灯，false 灭灯
    - ActiveColor (属性) : 支持动态更改亮灯时的颜色 (HEX 如 ""#FF0000"")
    - LabelText   (属性) : 获取或设置指示灯左侧显示的标签文本
    - SetLabelVisible(方法) : 设置标签是否可见 (true/false)
    - ValueChanged(事件) : 状态变化时触发

  ▶ ToggleSwitchPanel (开关)
    - IsOn        (属性) : 设为 true 开启，false 关闭
    - ActiveColor (属性) : 动态更改开状态的轨道颜色
    - LabelText   (属性) : 获取或设置开关左上方显示的标签文本
    - SetLabelVisible(方法) : 设置标签是否可见 (true/false)
    - ValueChanged(事件) : 点击切换开关时触发，回传 oldValue, newValue

  ▶ ComboBoxPanel (下拉框)
    - Items        (属性) : 支持通过一维字符串数组直接读写下拉框的所有选项
    - SelectedIndex(属性) : 当前选中的索引 (-1为空)
    - TextValue    (属性) : 获取用户当前选中项的实际字符串值
    - LabelText    (属性) : 获取或设置下拉框上方的标题标签
    - SetLabelVisible(方法) : 设置标签是否可见 (true/false)
    - ValueChanged (事件) : 点选不同选项时触发，提供选中 selectedIndex 与 selectedItem

  ▶ ButtonPanel (按钮)
    - Value        (属性) : 按钮当前的开启/激活状态
    - Behavior     (属性) : 按钮行为模式（如 Click, Switch 等）
    - LabelText    (属性) : 按钮上显示的文本
    - SetLabelVisible(方法) : 设置标签是否可见 (true/false)
    - Click        (事件) : 按钮被按下或状态发生时触发，回传 oldValue, newValue

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
■ 导出信息
名称: {name}   时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";
                                File.WriteAllText(infoPath, info);

                                string jsonPath = Path.ChangeExtension(outputPath, ".style.json");
                                var serializer = new JavaScriptSerializer();
                                string json = serializer.Serialize(style);
                                json = json.Replace("\",\"", "\",\n  \"").Replace("{\"", "{\n  \"").Replace("\"}", "\"\n}");
                                File.WriteAllText(jsonPath, json);
                            }
                            catch { }

                            string msgText = exportAll ? "✅ {0}.dll 导出成功（含 8 种控件）" : "✅ {0}.dll 导出成功";
                            TxtStatus.Text = string.Format(msgText, name);
                            TxtStatus.Foreground = TryParseBrush("#70E070", Brushes.LightGreen);

                            string boxMsg = exportAll 
                                ? $"DLL 已导出：\n{outputPath}\n\n━━ 包含 8 种控件 ━━\nTextInputPanel · NumericDisplayPanel\nComboBoxPanel · SliderPanel · ButtonPanel\nLedPanel · ToggleSwitchPanel · ProgressBarPanel\n\n📄 详细文档已同时生成"
                                : $"DLL 已导出：\n{outputPath}\n\n📄 API 使用说明及配置已同时生成";

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
                                string logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 导出控件 {name} 失败\r\n\r\n" +
                                                    $"【错误详情】\r\n{errMsg}\r\n\r\n" +
                                                    $"【完整编译输出】\r\n{result.BuildOutput}";
                                File.WriteAllText(logPath, logContent);
                                errMsg += $"\n\n👉 详细报错日志已保存至:\n{logPath}";
                            }
                            catch { }

                            TxtStatus.Text = "❌ 导出失败";
                            TxtStatus.Foreground = TryParseBrush("#FF6060", Brushes.Red);
                            MessageBox.Show(errMsg, "导出失败", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
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
    }

    #endregion
}
