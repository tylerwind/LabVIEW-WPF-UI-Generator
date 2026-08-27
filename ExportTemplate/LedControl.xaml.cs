using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfTextInput
{
    /// <summary>
    /// LED 鎸囩ず鐏帶浠?
    /// </summary>
    public partial class LedControl : UserControl
    {
        #region 渚濊禆灞炴€?

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(LedControl),
                new PropertyMetadata("指示灯", OnLabelTextChanged));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(bool), typeof(LedControl),
                new PropertyMetadata(false, OnValueChanged));

        public static readonly DependencyProperty ActiveColorProperty =
            DependencyProperty.Register("ActiveColor", typeof(string), typeof(LedControl),
                new PropertyMetadata("{{LedActiveColor}}", OnColorPropertyChanged));

        public static readonly DependencyProperty OffColorProperty =
            DependencyProperty.Register("OffColor", typeof(string), typeof(LedControl),
                new PropertyMetadata("{{LedOffColor}}", OnColorPropertyChanged));

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(double), typeof(LedControl),
                new PropertyMetadata(14.0, OnCornerRadiusPropertyChanged));

        public double CornerRadius
        {
            get { return (double)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public bool Value
        {
            get { return (bool)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public string ActiveColor
        {
            get { return (string)GetValue(ActiveColorProperty); }
            set { SetValue(ActiveColorProperty, value); }
        }

        public string OffColor
        {
            get { return (string)GetValue(OffColorProperty); }
            set { SetValue(OffColorProperty, value); }
        }

        #endregion

        public event EventHandler ValueChanged;

        public LedControl()
        {
            InitializeComponent();
        }

        private void OnClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Value = !Value;
        }

        #region 公共方法

        public void SetLabelVisible(bool visible)
        {
            if (LabelBlock != null)
                LabelBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region 内部逻辑

        private static void OnLabelTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (LedControl)d;
            if (c.LabelBlock != null)
                c.LabelBlock.Text = e.NewValue as string ?? "指示灯";
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (LedControl)d;
            c.UpdateLedVisual();
            if (c.ValueChanged != null) c.ValueChanged(c, EventArgs.Empty);
        }

        private static void OnColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (LedControl)d;
            c.UpdateLedVisual();
        }

        private static void OnCornerRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (LedControl)d;
            if (c != null && e.NewValue is double)
            {
                c.UpdateCornerRadius((double)e.NewValue);
            }
        }

        public void UpdateCornerRadius(double radius)
        {
            var corner = new CornerRadius(radius);
            if (BaseBorder != null) BaseBorder.CornerRadius = corner;
            if (LedBorder != null) LedBorder.CornerRadius = corner;
            if (LedGlow != null) LedGlow.CornerRadius = corner;
            if (ReflectBorder != null) ReflectBorder.CornerRadius = corner;
            if (LedHalo != null) LedHalo.CornerRadius = corner;
        }

        private Color ParseColor(string hex, Color fallback)
        {
            try { return (Color)ColorConverter.ConvertFromString(hex); }
            catch { return fallback; }
        }

        private void UpdateLedVisual()
        {
            if (LedGlow == null || LedOffBrush == null) return;

            var dur = TimeSpan.FromSeconds(0.25);
            bool isOn = Value;

            Color onCol = ParseColor(ActiveColor, Colors.Green);
            Color offCol = ParseColor(OffColor, Colors.Gray);

            // 鐏綋搴曡壊
            LedOffBrush.BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation(isOn ? onCol : offCol, dur));

            // 鍙戝厜灞?
            if (GlowCenter != null) GlowCenter.Color = onCol;
            if (GlowEdge != null) GlowEdge.Color = Color.FromArgb(128, onCol.R, onCol.G, onCol.B);
            LedGlow.BeginAnimation(OpacityProperty, new DoubleAnimation(isOn ? 1.0 : 0.0, dur));

            // 澶栧彂鍏夋檿
            if (HaloBrush != null) HaloBrush.Color = onCol;
            LedHalo.BeginAnimation(OpacityProperty, new DoubleAnimation(isOn ? double.Parse("{{ShadowOpacity}}") : 0.0, dur));
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

                // 1. LED 鐏鑹查噸缁?
                if (style.ContainsKey("LedOnColor"))
                {
                    this.ActiveColor = style["LedOnColor"] as string;
                }
                if (style.ContainsKey("LedOffColor"))
                {
                    this.OffColor = style["LedOffColor"] as string;
                }

                // 2. 鏍囩瀛椾綋鍙婇鑹查噸缁?
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

                // 3. 晕光范围重绘
                if (LedHalo != null && style.ContainsKey("ShadowBlur"))
                {
                    var blur = LedHalo.Effect as System.Windows.Media.Effects.BlurEffect;
                    if (blur != null)
                    {
                        double? val = ParseDouble(style["ShadowBlur"]);
                        if (val.HasValue) blur.Radius = val.Value;
                    }
                }

                // 4. 圆角重绘
                if (style.ContainsKey("CornerRadius"))
                {
                    double? val = ParseDouble(style["CornerRadius"]);
                    if (val.HasValue)
                    {
                        var corner = new CornerRadius(val.Value);
                        if (BaseBorder != null) BaseBorder.CornerRadius = corner;
                        if (LedBorder != null) LedBorder.CornerRadius = corner;
                        if (LedGlow != null) LedGlow.CornerRadius = corner;
                        if (ReflectBorder != null) ReflectBorder.CornerRadius = corner;
                        if (LedHalo != null) LedHalo.CornerRadius = corner;
                    }
                }
            }
            catch {}
        }

        #endregion
    }
}


