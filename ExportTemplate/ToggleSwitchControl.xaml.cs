using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfTextInput
{
    /// <summary>
    /// Toggle 寮€鍏充簨浠跺鎵?
    /// </summary>
    public delegate void ToggleChangedEventHandler(bool oldValue, bool newValue);

    /// <summary>
    /// Toggle 寮€鍏虫帶浠?
    /// </summary>
    public partial class ToggleSwitchControl : UserControl
    {
        #region 渚濊禆灞炴€?

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(ToggleSwitchControl),
                new PropertyMetadata("开关", OnLabelTextChanged));

        public static readonly DependencyProperty ActiveColorProperty =
            DependencyProperty.Register("ActiveColor", typeof(string), typeof(ToggleSwitchControl),
                new PropertyMetadata("{{ToggleActiveColor}}", OnColorPropertyChanged));

        public static readonly DependencyProperty InactiveColorProperty =
            DependencyProperty.Register("InactiveColor", typeof(string), typeof(ToggleSwitchControl),
                new PropertyMetadata("{{ToggleInactiveColor}}", OnColorPropertyChanged));

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(double), typeof(ToggleSwitchControl),
                new PropertyMetadata(13.0, OnCornerRadiusChanged));

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

        public string ActiveColor
        {
            get { return (string)GetValue(ActiveColorProperty); }
            set { SetValue(ActiveColorProperty, value); }
        }

        public string InactiveColor
        {
            get { return (string)GetValue(InactiveColorProperty); }
            set { SetValue(InactiveColorProperty, value); }
        }

        #endregion

        #region 浜嬩欢涓庣姸鎬?

        public event ToggleChangedEventHandler ValueChanged;

        private bool _value = false;

        public bool Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    bool old = _value;
                    _value = value;
                    UpdateToggleVisual(true);
                    if (ValueChanged != null) ValueChanged(old, value);
                }
            }
        }


        #endregion

        public ToggleSwitchControl()
        {
            InitializeComponent();
            // 鍒濆鍖栦负 OFF 鐘舵€?
            UpdateToggleVisual(false);
        }

        #region 鍏叡鏂规硶

        public void SetLabelVisible(bool visible)
        {
            if (LabelBlock != null)
                LabelBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region 鐢ㄦ埛浜や簰

        private void UserControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Value = !Value;
        }

        #endregion

        #region 瑙嗚鏇存柊

        private static void OnLabelTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (ToggleSwitchControl)d;
            if (c.LabelBlock != null)
                c.LabelBlock.Text = e.NewValue as string ?? "开关";
        }

        private static void OnColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (ToggleSwitchControl)d;
            c.UpdateToggleVisual(true);
        }

        private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (ToggleSwitchControl)d;
            if (c != null && e.NewValue is double)
            {
                c.UpdateCornerRadius((double)e.NewValue);
            }
        }

        public void UpdateCornerRadius(double radius)
        {
            if (Track != null) Track.CornerRadius = new CornerRadius(radius);
            if (Thumb != null) Thumb.CornerRadius = new CornerRadius(Math.Max(0, radius - 2.0));
        }

        private Color ParseColor(string hex, Color fallback)
        {
            try { return (Color)ColorConverter.ConvertFromString(hex); }
            catch { return fallback; }
        }

        private void UpdateToggleVisual(bool animate)
        {
            if (ThumbTranslate == null || TrackBrush == null) return;

            // 鎶婃墜婊戝姩: OFF=0, ON=22 (48 - 22鎶婃墜 - 4杈硅窛)
            double targetX = _value ? 22.0 : 0.0;
            
            Color activeCol = ParseColor(ActiveColor, (Color)ColorConverter.ConvertFromString("{{ToggleActiveColor}}"));
            Color inactiveCol = ParseColor(InactiveColor, (Color)ColorConverter.ConvertFromString("{{ToggleInactiveColor}}"));
            Color targetTrackColor = _value ? activeCol : inactiveCol;

            if (animate)
            {
                var dur = TimeSpan.FromSeconds(0.2);
                ThumbTranslate.BeginAnimation(TranslateTransform.XProperty,
                    new DoubleAnimation(targetX, dur) { EasingFunction = new QuadraticEase() });
                TrackBrush.BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation(targetTrackColor, dur));
            }
            else
            {
                ThumbTranslate.X = targetX;
                TrackBrush.Color = targetTrackColor;
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

                // 1. 开关颜色重绘
                if (style.ContainsKey("ActiveColor"))
                {
                    this.ActiveColor = style["ActiveColor"] as string;
                }
                else if (style.ContainsKey("ToggleColorOn"))
                {
                    this.ActiveColor = style["ToggleColorOn"] as string;
                }

                if (style.ContainsKey("InactiveColor"))
                {
                    this.InactiveColor = style["InactiveColor"] as string;
                }
                else if (style.ContainsKey("ToggleColorOff"))
                {
                    this.InactiveColor = style["ToggleColorOff"] as string;
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

                // 3. 圆角重绘
                if (style.ContainsKey("CornerRadius"))
                {
                    double? val = ParseDouble(style["CornerRadius"]);
                    if (val.HasValue)
                    {
                        UpdateCornerRadius(val.Value);
                    }
                }
            }
            catch {}
        }

        #endregion
    }
}


