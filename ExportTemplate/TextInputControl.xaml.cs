using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfTextInput
{
    /// <summary>
    /// 鏂版嫙鎬佽川鎰熸枃鏈緭鍏ユ帶浠?
    /// </summary>
    public partial class TextInputControl : UserControl
    {
        private string _previousText = string.Empty;

        #region 渚濊禆灞炴€?

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextInputControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnTextPropertyChanged));

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(TextInputControl),
                new PropertyMetadata("鏍囩", OnLabelTextPropertyChanged));

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

        #region 浜嬩欢

        public event ValueChangedHandler ValueChanged;

        #endregion

        public TextInputControl()
        {
            InitializeComponent();
        }

        #region 鍏叡鏂规硶

        /// <summary>
        /// 璁剧疆鏍囩鏄惁鍙
        /// </summary>
        public void SetLabelVisible(bool visible)
        {
            if (LabelBlock != null)
                LabelBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 璁剧疆鏄惁鏄剧ず鍨傜洿婊氬姩鏉?
        /// </summary>
        public void SetScrollBarVisible(bool visible)
        {
            if (InputBox != null)
            {
                InputBox.VerticalScrollBarVisibility = visible
                    ? ScrollBarVisibility.Auto
                    : ScrollBarVisibility.Hidden;
                InputBox.AcceptsReturn = visible; // 鏈夋粴鍔ㄦ潯鏃跺厑璁稿琛?
                InputBox.TextWrapping = visible ? TextWrapping.Wrap : TextWrapping.NoWrap;
            }
        }

        #endregion

        #region 灞炴€у彉鏇村洖璋?

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
                control.LabelBlock.Text = e.NewValue as string ?? "鏍囩";
            }
        }

        #endregion

        #region UI 浜嬩欢澶勭悊

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

        #region 杩愯鏃堕鏍奸噸缁?

        /// <summary>
        /// 鑾峰彇褰撳墠搴旂敤鐨勫姩鎬侀噸缁橀厤缃?
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> CurrentStyle { get; private set; }

        private Color? ParseColor(object val)
        {
            if (val == null) return null;
            string str = val as string;
            if (string.IsNullOrEmpty(str)) return null;
            try { return (Color)ColorConverter.ConvertFromString(str.StartsWith("#") ? str : "#" + str); }
            catch { return null; }
        }

        private double? ParseDouble(object val)
        {
            if (val == null) return null;
            try { return Convert.ToDouble(val); }
            catch { return null; }
        }

        private FontWeight? ParseFontWeight(object val)
        {
            if (val == null) return null;
            string str = val as string;
            if (string.IsNullOrEmpty(str)) return null;
            try
            {
                var converter = new FontWeightConverter();
                return (FontWeight)converter.ConvertFromString(str);
            }
            catch { return null; }
        }

        public void ApplyStyle(System.Collections.Generic.Dictionary<string, object> style)
        {
            if (style == null) return;
            this.CurrentStyle = style;
            try
            {
                // 0. 鎺т欢搴曡壊閲嶇粯
                if (style.ContainsKey("ControlBackground"))
                {
                    Color? ctrlBg = ParseColor(style["ControlBackground"]);
                    if (ctrlBg.HasValue)
                    {
                        this.Background = new SolidColorBrush(ctrlBg.Value);
                    }
                }

                // 1. 鑳屾櫙娓愬彉閲嶇粯
                if (MainCard != null && style.ContainsKey("GradientStart") && style.ContainsKey("GradientMid") && style.ContainsKey("GradientEnd"))
                {
                    Color? startCol = ParseColor(style["GradientStart"]);
                    Color? midCol = ParseColor(style["GradientMid"]);
                    Color? endCol = ParseColor(style["GradientEnd"]);
                    if (startCol.HasValue && midCol.HasValue && endCol.HasValue)
                    {
                        var brush = new LinearGradientBrush();
                        brush.StartPoint = new Point(0, 0);
                        brush.EndPoint = new Point(1, 1);
                        brush.GradientStops.Add(new GradientStop(startCol.Value, 0));
                        brush.GradientStops.Add(new GradientStop(midCol.Value, 0.5));
                        brush.GradientStops.Add(new GradientStop(endCol.Value, 1));
                        MainCard.Background = brush;
                    }
                }

                // 2. 鍦嗚涓庤竟妗嗙矖缁?
                if (MainCard != null)
                {
                    if (style.ContainsKey("CornerRadius"))
                    {
                        double? val = ParseDouble(style["CornerRadius"]);
                        if (val.HasValue) MainCard.CornerRadius = new CornerRadius(val.Value);
                    }
                    if (style.ContainsKey("BorderThickness"))
                    {
                        double? val = ParseDouble(style["BorderThickness"]);
                        if (val.HasValue) MainCard.BorderThickness = new Thickness(val.Value);
                    }
                }

                // 3. 杈规棰滆壊 (鍔ㄧ敾寮曠敤)
                if (InputBorderBrush != null && style.ContainsKey("BorderColor"))
                {
                    Color? bcVal = ParseColor(style["BorderColor"]);
                    if (bcVal.HasValue) InputBorderBrush.Color = bcVal.Value;
                }

                // 4. 闃村奖閲嶇粯
                var shadow = (MainCard != null) ? (MainCard.Effect as System.Windows.Media.Effects.DropShadowEffect) : null;
                if (shadow != null)
                {
                    if (style.ContainsKey("ShadowBlur"))
                    {
                        double? val = ParseDouble(style["ShadowBlur"]);
                        if (val.HasValue) shadow.BlurRadius = val.Value;
                    }
                    if (style.ContainsKey("ShadowDepth"))
                    {
                        double? val = ParseDouble(style["ShadowDepth"]);
                        if (val.HasValue) shadow.ShadowDepth = val.Value;
                    }
                    if (style.ContainsKey("ShadowColor"))
                    {
                        Color? val = ParseColor(style["ShadowColor"]);
                        if (val.HasValue) shadow.Color = val.Value;
                    }
                    if (style.ContainsKey("ShadowOpacity"))
                    {
                        double? val = ParseDouble(style["ShadowOpacity"]);
                        if (val.HasValue) shadow.Opacity = val.Value;
                    }
                }

                // 5. 瀛椾綋鏍峰紡涓庨鑹查噸缁?
                if (InputBox != null)
                {
                    if (style.ContainsKey("FontFamily")) InputBox.FontFamily = new FontFamily(style["FontFamily"] as string);
                    if (style.ContainsKey("FontSize"))
                    {
                        double? val = ParseDouble(style["FontSize"]);
                        if (val.HasValue) InputBox.FontSize = val.Value;
                    }
                    if (style.ContainsKey("FontColor"))
                    {
                        Color? val = ParseColor(style["FontColor"]);
                        if (val.HasValue) InputBox.Foreground = new SolidColorBrush(val.Value);
                    }
                    if (style.ContainsKey("CaretColor"))
                    {
                        Color? val = ParseColor(style["CaretColor"]);
                        if (val.HasValue) InputBox.CaretBrush = new SolidColorBrush(val.Value);
                    }
                    if (style.ContainsKey("FontWeight"))
                    {
                        FontWeight? val = ParseFontWeight(style["FontWeight"]);
                        if (val.HasValue) InputBox.FontWeight = val.Value;
                    }
                }

                // 6. 鏍囩瀛椾綋涓庨鑹查噸缁?
                if (LabelBlock != null)
                {
                    if (style.ContainsKey("FontFamily")) LabelBlock.FontFamily = new FontFamily(style["FontFamily"] as string);
                    if (style.ContainsKey("LabelColor"))
                    {
                        Color? val = ParseColor(style["LabelColor"]);
                        if (val.HasValue) LabelBlock.Foreground = new SolidColorBrush(val.Value);
                    }
                    if (style.ContainsKey("LabelFontSize"))
                    {
                        double? val = ParseDouble(style["LabelFontSize"]);
                        if (val.HasValue) LabelBlock.FontSize = val.Value;
                    }
                    if (style.ContainsKey("FontWeight"))
                    {
                        FontWeight? val = ParseFontWeight(style["FontWeight"]);
                        if (val.HasValue) LabelBlock.FontWeight = val.Value;
                    }
                }
            }
            catch {}
        }

        #endregion
    }
}



