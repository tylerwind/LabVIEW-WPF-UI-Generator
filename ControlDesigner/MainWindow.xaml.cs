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
            _style.ControlBackground = TxtControlBg.Text.Trim();
            _style.GradientStart = TxtGradStart.Text.Trim();
            _style.GradientMid = TxtGradMid.Text.Trim();
            _style.GradientEnd = TxtGradEnd.Text.Trim();

            _style.BorderColor = TxtBorderColor.Text.Trim();
            _style.BorderThickness = SliderBorderW.Value;
            _style.CornerRadius = SliderCorner.Value;

            _style.ShadowColor = TxtShadowColor.Text.Trim();
            _style.ShadowBlur = SliderShadowBlur.Value;
            _style.ShadowDepth = SliderShadowDepth.Value;
            _style.ShadowOpacity = SliderShadowOp.Value;

            _style.FontFamily = TxtFontFamily.Text.Trim();
            _style.FontSize = SliderFontSize.Value;
            _style.FontColor = TxtFontColor.Text.Trim();
            _style.LabelColor = TxtLabelColor.Text.Trim();
            _style.LabelFontSize = SliderLabelSize.Value;

            _style.FocusBorderColor = TxtFocusBorder.Text.Trim();
            _style.AccentColor = TxtAccentColor.Text.Trim();
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

            // 高光层
            var highlight = new Border
            {
                CornerRadius = new CornerRadius(cr),
                Margin = new Thickness(-2, -2, 2, 2),
                Background = TryParseBrush(_style.HighlightColor, Brushes.White),
                Opacity = _style.HighlightOpacity,
            };

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
                var grid = new Grid();
                var lbl = new TextBlock
                {
                    Text = "标签",
                    FontFamily = new FontFamily(_style.FontFamily),
                    FontSize = _style.LabelFontSize,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = TryParseBrush(_style.LabelColor, Brushes.Gray),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                var val = new TextBlock
                {
                    Text = "50.00",
                    FontFamily = new FontFamily(_style.FontFamily),
                    FontSize = _style.LabelFontSize,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = TryParseBrush(_style.LabelColor, Brushes.Gray),
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                grid.Children.Add(lbl);
                grid.Children.Add(val);

                DockPanel.SetDock(grid, Dock.Top);

                var slider = new Slider
                {
                    Foreground = TryParseBrush(_style.FontColor, Brushes.Black),
                    Background = Brushes.Transparent,
                    Padding = new Thickness(10, 4, 10, 4),
                    Minimum = 0,
                    Maximum = 100,
                    Value = 50,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                // 由于之前已经加过了 Label，Slider 不需要再显示默认顶部的标签
                dock.Children.Remove(label);
                
                dock.Children.Add(grid);
                dock.Children.Add(accentLine);
                dock.Children.Add(slider);
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

                outerGrid.Children.Clear(); // 按钮样式自带外部阴影和框体，不需要叠加默认的 card 组建
                outerGrid.Children.Add(button);
                return outerGrid;
            }

            card.Child = dock;

            outerGrid.Children.Add(highlight);
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

            // 替换所有占位符
            xaml = xaml.Replace("{{FontColor}}", _style.FontColor);
            xaml = xaml.Replace("{{HighlightColor}}", _style.HighlightColor);
            xaml = xaml.Replace("{{AccentColor}}", _style.AccentColor);
            xaml = xaml.Replace("{{ControlBackground}}", _style.ControlBackground);
            xaml = xaml.Replace("{{BorderColor}}", _style.BorderColor);
            xaml = xaml.Replace("{{CornerRadius}}", _style.CornerRadius.ToString());
            xaml = xaml.Replace("{{ShadowBlur}}", _style.ShadowBlur.ToString());
            xaml = xaml.Replace("{{ShadowDepth}}", _style.ShadowDepth.ToString());
            xaml = xaml.Replace("{{ShadowColor}}", _style.ShadowColor);
            xaml = xaml.Replace("{{ShadowOpacity}}", _style.ShadowOpacity.ToString("F2"));
            xaml = xaml.Replace("{{ShadowMargin}}", CalcShadowMargin(_style.ShadowBlur, _style.ShadowDepth));
            xaml = xaml.Replace("{{GradientStart}}", _style.GradientStart);
            xaml = xaml.Replace("{{GradientMid}}", _style.GradientMid);
            xaml = xaml.Replace("{{GradientEnd}}", _style.GradientEnd);

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
                    <Grid.Resources>
                    </Grid.Resources>
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
                                <DoubleAnimation Storyboard.TargetName=""ScaleTrans"" Storyboard.TargetProperty=""ScaleX"" To=""0.95"" Duration=""0:0:0.1""/> 
                                <DoubleAnimation Storyboard.TargetName=""ScaleTrans"" Storyboard.TargetProperty=""ScaleY"" To=""0.95"" Duration=""0:0:0.1""/> 
                                <DoubleAnimation Storyboard.TargetName=""PosTrans"" Storyboard.TargetProperty=""X"" To=""2.0"" Duration=""0:0:0.1""/> 
                                <DoubleAnimation Storyboard.TargetName=""PosTrans"" Storyboard.TargetProperty=""Y"" To=""2.0"" Duration=""0:0:0.1""/> 
                                <DoubleAnimation Storyboard.TargetName=""PartShadow"" Storyboard.TargetProperty=""ShadowDepth"" To=""0.0"" Duration=""0:0:0.1""/> 
                                <DoubleAnimation Storyboard.TargetName=""PartShadow"" Storyboard.TargetProperty=""Opacity"" To=""0.0"" Duration=""0:0:0.1""/> 
                            </Storyboard> </BeginStoryboard>
                        </Trigger.EnterActions>
                        <Trigger.ExitActions>
                            <BeginStoryboard> <Storyboard> 
                                <DoubleAnimation Storyboard.TargetName=""ScaleTrans"" Storyboard.TargetProperty=""ScaleX"" To=""1.0"" Duration=""0:0:0.2""/> 
                                <DoubleAnimation Storyboard.TargetName=""ScaleTrans"" Storyboard.TargetProperty=""ScaleY"" To=""1.0"" Duration=""0:0:0.2""/> 
                                <DoubleAnimation Storyboard.TargetName=""PosTrans"" Storyboard.TargetProperty=""X"" To=""0.0"" Duration=""0:0:0.2""/> 
                                <DoubleAnimation Storyboard.TargetName=""PosTrans"" Storyboard.TargetProperty=""Y"" To=""0.0"" Duration=""0:0:0.2""/> 
                                <DoubleAnimation Storyboard.TargetName=""PartShadow"" Storyboard.TargetProperty=""ShadowDepth"" To=""{{ShadowDepth}}"" Duration=""0:0:0.2""/> 
                                <DoubleAnimation Storyboard.TargetName=""PartShadow"" Storyboard.TargetProperty=""Opacity"" To=""{{ShadowOpacity}}"" Duration=""0:0:0.2""/> 
                            </Storyboard> </BeginStoryboard>
                        </Trigger.ExitActions>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>";

            xaml = xaml.Replace("{{FontColor}}", _style.FontColor);
            xaml = xaml.Replace("{{FontFamily}}", _style.FontFamily);
            xaml = xaml.Replace("{{FontSize}}", _style.FontSize.ToString());
            xaml = xaml.Replace("{{HighlightColor}}", _style.HighlightColor);
            xaml = xaml.Replace("{{AccentColor}}", _style.AccentColor);
            xaml = xaml.Replace("{{ControlBackground}}", _style.ControlBackground);
            xaml = xaml.Replace("{{BorderColor}}", _style.BorderColor);
            xaml = xaml.Replace("{{BorderThickness}}", _style.BorderThickness.ToString());
            xaml = xaml.Replace("{{CardPadding}}", _style.CardPadding);
            xaml = xaml.Replace("{{CornerRadius}}", _style.CornerRadius.ToString());
            xaml = xaml.Replace("{{ShadowBlur}}", _style.ShadowBlur.ToString());
            xaml = xaml.Replace("{{ShadowDepth}}", _style.ShadowDepth.ToString());
            xaml = xaml.Replace("{{ShadowColor}}", _style.ShadowColor);
            xaml = xaml.Replace("{{ShadowOpacity}}", _style.ShadowOpacity.ToString("F2"));
            xaml = xaml.Replace("{{ShadowMargin}}", CalcShadowMargin(_style.ShadowBlur, _style.ShadowDepth));
            xaml = xaml.Replace("{{GradientStart}}", _style.GradientStart);
            xaml = xaml.Replace("{{GradientMid}}", _style.GradientMid);
            xaml = xaml.Replace("{{GradientEnd}}", _style.GradientEnd);

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

            UpdatePreview();
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
            // 读取并验证控件名称
            string controlName = TxtControlName.Text.Trim();
            if (string.IsNullOrEmpty(controlName))
            {
                MessageBox.Show("请输入控件名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtControlName.Focus();
                return;
            }
            // 验证名称合法性（C# 标识符规范）
            if (!System.Text.RegularExpressions.Regex.IsMatch(controlName, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                MessageBox.Show("控件名称只能包含字母、数字和下划线，且不能以数字开头",
                    "名称无效", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtControlName.Focus();
                return;
            }

            var dlg = new SaveFileDialog
            {
                Title = "导出 DLL",
                Filter = "DLL 文件|*.dll",
                FileName = controlName + ".dll",
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
                TxtStatus.Text = "⏳ 正在编译 " + controlName + "...";
                TxtStatus.Foreground = TryParseBrush("#FFD080", Brushes.Yellow);

                var style = _style.Clone();
                var outputPath = dlg.FileName;
                string name = controlName; // 捕获到闭包
                ControlType type = _currentControlType;

                System.Threading.Tasks.Task.Run(() =>
                {
                    var result = _exporter.Export(style, outputPath, name, type);
                    Dispatcher.Invoke(() =>
                    {
                        if (result.Success)
                        {
                            string panelName;
                            if (type == ControlType.TextInput) panelName = "TextInputPanel";
                            else if (type == ControlType.NumericDisplay) panelName = "NumericDisplayPanel";
                            else if (type == ControlType.ComboBoxInput) panelName = "ComboBoxPanel";
                            else if (type == ControlType.SliderInput) panelName = "SliderPanel";
                            else panelName = "ButtonPanel";

                            // 预备说明文档的 API 内容
                            string apiDocs = "";
                            if (type == ControlType.TextInput)
                            {
                                apiDocs = @"【属性】
- Text (String) : 获取或设置输入框的文本
- LabelText (String) : 获取或设置标签文字

【方法】
- SetLabelVisible(Boolean visible) : 隐藏/显示标签文字
- SetScrollBarVisible(Boolean visible) : 启用/关闭多行模式和长文本的垂直滚动条

【事件】
- ValueChanged : 当输入框内的文本发生改变时触发（输出 OldValue, NewValue）";
                            }
                            else if (type == ControlType.NumericDisplay)
                            {
                                apiDocs = @"【属性】
- ValueStr (String) : 获取或设置显示的当前值（字符串类型，因为带有格式化形式）
- LabelText (String) : 获取或设置标签文字
- Unit (String) : 获取或设置单位文本 (如 V, mA)

【方法】
- WriteDouble(Double value, String format) : 配合格式化字符串写入数值（如 format=""F2"" 代表保留两位小数）
- WriteString(String value) : 直接写入字符串形式的值
- Clear() : 清空数值显示
- SetLabelVisible(Boolean visible) : 显示或隐藏标签
- SetUnitVisible(Boolean visible) : 显示或隐藏单位";
                            }
                            else if (type == ControlType.ComboBoxInput)
                            {
                                apiDocs = @"【属性】
- LabelText (String) : 获取或设置标签文字
- SelectedIndex (Int32) : 获取或设置选中的项目索引
- TextValue (String) : 获取或设置选中的文本值

【方法】
- AddItem(String item) : 增加下拉框选项
- ClearItems() : 清空所有选项
- SetLabelVisible(Boolean visible) : 显示或隐藏标签

【事件】
- ValueChanged : 当选择变化时触发（输出 SelectedIndex, SelectedItem）";
                            }
                            else if (type == ControlType.SliderInput)
                            {
                                apiDocs = @"【属性】
- LabelText (String) : 获取或设置标签文字
- Value (Double) : 滑动杆的当前数值
- Minimum (Double) : 最小数值
- Maximum (Double) : 最大数值
- TickFrequency (Double) : 步进和刻度值
- IsSnapToTickEnabled (Boolean) : 是否自动吸附到步长点

【方法】
- SetLabelVisible(Boolean visible) : 显示或隐藏标签
- SetValueVisible(Boolean visible) : 显示或隐藏右侧的数值文字

【事件】
- ValueChanged : 当拖动数值变化时触发（输出 OldValue, NewValue）";
                            }
                            else if (type == ControlType.ButtonInput)
                            {
                                apiDocs = @"【属性】
- LabelText (String) : 按钮上显示的文本
- Value (Boolean) : 按钮此时是否处于按下的激活状态（支持只读与外部修改）
- Behavior (Enum/Int32) : 控制动作发生的时机逻辑。
   0: SwitchWhenPressed (按下时切换并保持状态)
   1: SwitchWhenReleased (抬起时切换并保持状态)
   2: SwitchUntilReleased (保持按下直到抬起)
   3: LatchWhenPressed (按下时即触发瞬时脉冲，发出 false..true..false)
   4: LatchWhenReleased (抬起时才触发瞬时脉冲，发出 false..true..false)

【方法】
- SetLabelVisible(Boolean visible) : 显示或隐藏文字

【事件】
- Click : 当按钮被点击时触发（无特殊参数）";
                            }

                            // 写样式信息和 JSON 配置
                            try
                            {
                                string infoPath = Path.ChangeExtension(outputPath, ".style.txt");
                                var info = $@"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
【 {name} 控件使用说明书 】
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

■ LabVIEW 引用方式：
  1. 在前面板放置一个 [.NET 容器] (位于.NET与ActiveX选板)。
  2. 右键容器 -> 插入 .NET 控件... -> 右上角 [浏览...]
  3. 选择刚刚导出的 DLL 文件：{outputPath}
  4. 从对象列表中选择对应的包装程序集类型：
     -> {name}.{panelName}
  5. 点击确定即可将此漂亮控件载入 LabVIEW！
  * (注：如果要覆盖旧版本的 DLL，必须彻底退出全部 LabVIEW 进程后重启)

■ 控件开放的 API 接口 (可通过【属性节点/调用节点/事件回调】访问)：
{apiDocs}

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
■ 导出附带信息
名称: {name}
时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
主色调: {style.ControlBackground} (底板), {style.FontColor} (文字), {style.GradientStart} (渐变)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";
                                File.WriteAllText(infoPath, info);

                                string jsonPath = Path.ChangeExtension(outputPath, ".style.json");
                                var serializer = new JavaScriptSerializer();
                                string json = serializer.Serialize(style);
                                // 简单格式化
                                json = json.Replace("\",\"", "\",\n  \"").Replace("{\"", "{\n  \"").Replace("\"}", "\"\n}");
                                File.WriteAllText(jsonPath, json);
                            }
                            catch { }

                            TxtStatus.Text = $"✅ {name}.dll 导出成功";
                            TxtStatus.Foreground = TryParseBrush("#70E070", Brushes.LightGreen);

                            MessageBox.Show(
                                $"DLL 已导出:\n{outputPath}\n\n" +
                                $"━━ LabVIEW 使用方法 ━━\n" +
                                $"程序集: {name}.dll\n" +
                                $"类型:   {name}.{panelName}\n\n" +
                                $"━━ 样式信息 ━━\n" +
                                $"背景: {style.ControlBackground}\n" +
                                $"字色: {style.FontColor}\n\n" +
                                "💡 不同名称的控件可在同一 LabVIEW 中同时使用！",
                                "导出成功 — " + name,
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            string errMsg = result.ErrorMessage ?? "导出失败";
                            if (!string.IsNullOrEmpty(result.BuildErrors))
                                errMsg += "\n\n编译错误:\n" + result.BuildErrors;
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
                if (hex.Trim().Equals("Transparent", StringComparison.OrdinalIgnoreCase)) return Brushes.Transparent;
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            }
            catch { return fallback; }
        }

        private static Color TryParseColor(string hex, Color fallback)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hex)) return fallback;
                if (hex.Trim().Equals("Transparent", StringComparison.OrdinalIgnoreCase)) return Colors.Transparent;
                return (Color)ColorConverter.ConvertFromString(hex);
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
