using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfTextInput
{
    /// <summary>
    /// 杩涘害鏉℃帶浠?
    /// </summary>
    public partial class ProgressBarControl : UserControl
    {
        #region 渚濊禆灞炴€?

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(ProgressBarControl),
                new PropertyMetadata("杩涘害", OnLabelTextChanged));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(ProgressBarControl),
                new PropertyMetadata(0.0, OnValueChanged));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(ProgressBarControl),
                new PropertyMetadata(0.0, OnValueChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(ProgressBarControl),
                new PropertyMetadata(100.0, OnValueChanged));

        public static readonly DependencyProperty ShowPercentageProperty =
            DependencyProperty.Register("ShowPercentage", typeof(bool), typeof(ProgressBarControl),
                new PropertyMetadata(true, OnShowPercentageChanged));

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public bool ShowPercentage
        {
            get { return (bool)GetValue(ShowPercentageProperty); }
            set { SetValue(ShowPercentageProperty, value); }
        }

        #endregion

        #region 姊害鑹插僵灞炴€?

        private string _startColor = "{{ProgressColor1}}";
        private string _endColor = "{{ProgressColor2}}";

        public string StartColor
        {
            get { return _startColor; }
            set 
            { 
                _startColor = value; 
                if (FillColorStart != null) 
                    FillColorStart.Color = ParseColor(value, Colors.Green); 
            }
        }

        public string EndColor
        {
            get { return _endColor; }
            set 
            { 
                _endColor = value; 
                if (FillColorEnd != null) 
                    FillColorEnd.Color = ParseColor(value, Colors.Blue); 
            }
        }

        private Color ParseColor(string hex, Color fallback)
        {
            try { return (Color)ColorConverter.ConvertFromString(hex); }
            catch { return fallback; }
        }

        #endregion

        public ProgressBarControl()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateFillBar();
        }


        #region 鍏叡鏂规硶

        public void SetLabelVisible(bool visible)
        {
            if (LabelBlock != null)
                LabelBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region 鍐呴儴閫昏緫

        private static void OnLabelTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (ProgressBarControl)d;
            if (c.LabelBlock != null)
                c.LabelBlock.Text = e.NewValue as string ?? "杩涘害";
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (ProgressBarControl)d;
            c.UpdateFillBar();
        }

        private static void OnShowPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (ProgressBarControl)d;
            if (c.PercentBlock != null)
                c.PercentBlock.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateFillBar()
        {
            if (FillBar == null || TrackBorder == null) return;

            double range = Maximum - Minimum;
            if (range <= 0) range = 1;

            double ratio = Math.Max(0, Math.Min(1, (Value - Minimum) / range));
            double trackWidth = TrackBorder.ActualWidth;
            if (trackWidth <= 0) trackWidth = TrackBorder.Width;
            if (double.IsNaN(trackWidth) || trackWidth <= 0) return;

            double targetWidth = Math.Max(0, trackWidth * ratio);

            var anim = new DoubleAnimation(targetWidth, TimeSpan.FromSeconds(0.3))
            {
                EasingFunction = new QuadraticEase()
            };
            FillBar.BeginAnimation(WidthProperty, anim);

            // Ensure the gradient stretches over the full track width (0 to Max)
            var brush = FillBar.Background as LinearGradientBrush;
            if (brush != null)
            {
                brush.MappingMode = BrushMappingMode.Absolute;
                brush.EndPoint = new Point(trackWidth, 0);
            }

            // 鏇存柊鐧惧垎姣旀枃瀛?
            if (PercentBlock != null)
                PercentBlock.Text = string.Format("{0:F0}%", ratio * 100);

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

                // 1. 杩涘害鏉℃笎鍙樿壊閲嶇粯
                if (style.ContainsKey("ProgressColor1"))
                {
                    this.StartColor = style["ProgressColor1"] as string;
                }
                if (style.ContainsKey("ProgressColor2"))
                {
                    this.EndColor = style["ProgressColor2"] as string;
                }

                // 2. 鏍囩涓庣櫨鍒嗘瘮瀛椾綋鍙婇鑹查噸缁?
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

                if (PercentBlock != null)
                {
                    if (style.ContainsKey("FontFamily")) PercentBlock.FontFamily = new FontFamily(style["FontFamily"] as string);
                    if (style.ContainsKey("FontColor"))
                    {
                        Color? val = ParseColor(style["FontColor"]);
                        if (val.HasValue) PercentBlock.Foreground = new SolidColorBrush(val.Value);
                    }
                    if (style.ContainsKey("LabelFontSize"))
                    {
                        double? val = ParseDouble(style["LabelFontSize"]);
                        if (val.HasValue) PercentBlock.FontSize = val.Value;
                    }
                    if (style.ContainsKey("FontWeight"))
                    {
                        FontWeight? val = ParseFontWeight(style["FontWeight"]);
                        if (val.HasValue) PercentBlock.FontWeight = val.Value;
                    }
                }
            }
            catch {}
        }

        #endregion
    }
}



