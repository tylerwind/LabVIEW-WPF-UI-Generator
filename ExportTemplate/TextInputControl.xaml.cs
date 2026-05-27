using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfTextInput
{
    /// <summary>
    /// 新拟态质感文本输入控件
    /// </summary>
    public partial class TextInputControl : UserControl
    {
        private string _previousText = string.Empty;

        #region 依赖属性

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextInputControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnTextPropertyChanged));

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(TextInputControl),
                new PropertyMetadata("标签", OnLabelTextPropertyChanged));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        #endregion

        #region 事件

        public event ValueChangedHandler ValueChanged;

        #endregion

        public TextInputControl()
        {
            InitializeComponent();
        }

        #region 公共方法

        /// <summary>
        /// 设置标签是否可见
        /// </summary>
        public void SetLabelVisible(bool visible)
        {
            if (LabelBlock != null)
                LabelBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 设置是否显示垂直滚动条
        /// </summary>
        public void SetScrollBarVisible(bool visible)
        {
            if (InputBox != null)
            {
                InputBox.VerticalScrollBarVisibility = visible
                    ? ScrollBarVisibility.Auto
                    : ScrollBarVisibility.Hidden;
                InputBox.AcceptsReturn = visible; // 有滚动条时允许多行
                InputBox.TextWrapping = visible ? TextWrapping.Wrap : TextWrapping.NoWrap;
            }
        }

        #endregion

        #region 属性变更回调

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TextInputControl)d;
            var newVal = e.NewValue as string ?? string.Empty;
            if (control.InputBox != null && control.InputBox.Text != newVal)
            {
                control.InputBox.Text = newVal;
            }
        }

        private static void OnLabelTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TextInputControl)d;
            if (control.LabelBlock != null)
            {
                control.LabelBlock.Text = e.NewValue as string ?? "标签";
            }
        }

        #endregion

        #region UI 事件处理

        private void InputBox_GotFocus(object sender, RoutedEventArgs e)
        {
            LabelBlock.Foreground = new SolidColorBrush(Color.FromRgb(0x4A, 0x50, 0x68));
            var focusIn = (Storyboard)FindResource("FocusIn");
            focusIn.Begin(this);
        }

        private void InputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            LabelBlock.Foreground = new SolidColorBrush(Color.FromRgb(0x8A, 0x90, 0xA0));
            var focusOut = (Storyboard)FindResource("FocusOut");
            focusOut.Begin(this);
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string newText = InputBox.Text ?? string.Empty;
            string oldText = _previousText;

            if (Text != newText)
            {
                Text = newText;
            }

            if (oldText != newText)
            {
                _previousText = newText;
                if (ValueChanged != null) 
                {
                    byte[] utf8Bytes = string.IsNullOrEmpty(newText) ? new byte[0] : System.Text.Encoding.UTF8.GetBytes(newText);
                    ValueChanged(oldText, newText, utf8Bytes);
                }

            }
        }

        #endregion

        #region 运行时风格重绘

        /// <summary>
        /// 获取当前应用的动态重绘配置
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> CurrentStyle { get; private set; }

        public void ApplyStyle(System.Collections.Generic.Dictionary<string, object> style)
        {
            if (style == null) return;
            this.CurrentStyle = style;
            try
            {
                // 1. 背景渐变重绘
                string startCol = style.ContainsKey("GradientStart") ? style["GradientStart"] as string : null;
                string midCol = style.ContainsKey("GradientMid") ? style["GradientMid"] as string : null;
                string endCol = style.ContainsKey("GradientEnd") ? style["GradientEnd"] as string : null;
                if (MainCard != null && !string.IsNullOrEmpty(startCol) && !string.IsNullOrEmpty(midCol) && !string.IsNullOrEmpty(endCol))
                {
                    var brush = new LinearGradientBrush();
                    brush.StartPoint = new Point(0, 0);
                    brush.EndPoint = new Point(1, 1);
                    brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(startCol), 0));
                    brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(midCol), 0.5));
                    brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(endCol), 1));
                    MainCard.Background = brush;
                }

                // 2. 圆角与边框粗细
                if (MainCard != null)
                {
                    if (style.ContainsKey("CornerRadius"))
                        MainCard.CornerRadius = new CornerRadius(Convert.ToDouble(style["CornerRadius"]));
                    if (style.ContainsKey("BorderThickness"))
                        MainCard.BorderThickness = new Thickness(Convert.ToDouble(style["BorderThickness"]));
                }

                // 3. 边框颜色 (动画引用)
                if (InputBorderBrush != null && style.ContainsKey("BorderColor"))
                {
                    InputBorderBrush.Color = (Color)ColorConverter.ConvertFromString(style["BorderColor"] as string);
                }

                // 4. 阴影重绘
                var shadow = (MainCard != null) ? (MainCard.Effect as System.Windows.Media.Effects.DropShadowEffect) : null;
                if (shadow != null)
                {
                    if (style.ContainsKey("ShadowBlur")) shadow.BlurRadius = Convert.ToDouble(style["ShadowBlur"]);
                    if (style.ContainsKey("ShadowDepth")) shadow.ShadowDepth = Convert.ToDouble(style["ShadowDepth"]);
                    if (style.ContainsKey("ShadowColor")) shadow.Color = (Color)ColorConverter.ConvertFromString(style["ShadowColor"] as string);
                    if (style.ContainsKey("ShadowOpacity")) shadow.Opacity = Convert.ToDouble(style["ShadowOpacity"]);
                }

                // 5. 字体样式与颜色重绘
                if (InputBox != null)
                {
                    if (style.ContainsKey("FontFamily")) InputBox.FontFamily = new FontFamily(style["FontFamily"] as string);
                    if (style.ContainsKey("FontSize")) InputBox.FontSize = Convert.ToDouble(style["FontSize"]);
                    if (style.ContainsKey("FontColor")) InputBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(style["FontColor"] as string));
                    if (style.ContainsKey("CaretColor")) InputBox.CaretBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(style["CaretColor"] as string));
                }

                // 6. 标签字体与颜色重绘
                if (LabelBlock != null)
                {
                    if (style.ContainsKey("FontFamily")) LabelBlock.FontFamily = new FontFamily(style["FontFamily"] as string);
                    if (style.ContainsKey("LabelColor")) LabelBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(style["LabelColor"] as string));
                    if (style.ContainsKey("LabelFontSize")) LabelBlock.FontSize = Convert.ToDouble(style["LabelFontSize"]);
                }
            }
            catch {}
        }

        #endregion
    }
}
