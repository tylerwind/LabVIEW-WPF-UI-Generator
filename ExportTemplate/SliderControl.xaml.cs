using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfSlider
{
    /// <summary>
    /// 鏂版嫙鎬佽川鎰熸粦鍔ㄦ潌鎺т欢
    /// </summary>
    public partial class SliderControl : UserControl
    {
        #region 渚濊禆灞炴€?

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(SliderControl),
                new PropertyMetadata("鏍囩", OnLabelTextPropertyChanged));

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public static readonly DependencyProperty StartColorProperty =
            DependencyProperty.Register("StartColor", typeof(string), typeof(SliderControl),
                new PropertyMetadata("{{SliderColor1}}"));

        public string StartColor
        {
            get { return (string)GetValue(StartColorProperty); }
            set { SetValue(StartColorProperty, value); }
        }

        public static readonly DependencyProperty EndColorProperty =
            DependencyProperty.Register("EndColor", typeof(string), typeof(SliderControl),
                new PropertyMetadata("{{SliderColor2}}"));

        public string EndColor
        {
            get { return (string)GetValue(EndColorProperty); }
            set { SetValue(EndColorProperty, value); }
        }

        #endregion

        #region 浜嬩欢

        public delegate void ValueChangedHandler(double oldValue, double newValue);
        public event ValueChangedHandler ValueChanged;

        #endregion

        public SliderControl()
        {
            InitializeComponent();
        }

        #region 鍏叡灞炴€?鏂规硶

        public double Value
        {
            get { return InputBox.Value; }
            set { InputBox.Value = value; }
        }


        public double Minimum
        {
            get { return InputBox.Minimum; }
            set { InputBox.Minimum = value; }
        }


        public double Maximum
        {
            get { return InputBox.Maximum; }
            set { InputBox.Maximum = value; }
        }


        public double TickFrequency
        {
            get { return InputBox.TickFrequency; }
            set { InputBox.TickFrequency = value; }
        }


        public bool IsSnapToTickEnabled
        {
            get { return InputBox.IsSnapToTickEnabled; }
            set { InputBox.IsSnapToTickEnabled = value; }
        }


        /// <summary>
        /// 璁剧疆鏍囩鏄惁鍙
        /// </summary>
        public void SetLabelVisible(bool visible)
        {
            if (LabelBlock != null)
                LabelBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
        
        public void SetValueVisible(bool visible)
        {
            if (ValueBlock != null)
                ValueBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region 灞炴€у彉鏇村洖璋?

        private static void OnLabelTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SliderControl)d;
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
        }

        private void InputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            LabelBlock.Foreground = new SolidColorBrush(Color.FromRgb(0x8A, 0x90, 0xA0));
        }

        private void InputBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (ValueBlock != null)
                    ValueBlock.Text = e.NewValue.ToString("F2");

                if (ValueChanged != null) ValueChanged(e.OldValue, e.NewValue);

            }
            catch (Exception ex)
            {
                string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SliderCrashLog.txt");
                System.IO.File.AppendAllText(path, DateTime.Now.ToString() + " : " + ex.ToString() + Environment.NewLine);
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

                // 1. 婊戝潡娓愬彉鑹查噸缁?(閫氳繃淇敼 StartColor 鍜?EndColor 渚濊禆灞炴€цЕ鍙戠粦瀹氭洿鏂?
                if (style.ContainsKey("SliderColor1"))
                {
                    this.StartColor = style["SliderColor1"] as string;
                }
                if (style.ContainsKey("SliderColor2"))
                {
                    this.EndColor = style["SliderColor2"] as string;
                }

                // 2. 鏍囩涓庢暟鍊肩殑瀛椾綋鍙婇鑹查噸缁?
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

                if (ValueBlock != null)
                {
                    if (style.ContainsKey("FontFamily")) ValueBlock.FontFamily = new FontFamily(style["FontFamily"] as string);
                    if (style.ContainsKey("LabelColor"))
                    {
                        Color? val = ParseColor(style["LabelColor"]);
                        if (val.HasValue) ValueBlock.Foreground = new SolidColorBrush(val.Value);
                    }
                    if (style.ContainsKey("LabelFontSize"))
                    {
                        double? val = ParseDouble(style["LabelFontSize"]);
                        if (val.HasValue) ValueBlock.FontSize = val.Value;
                    }
                    if (style.ContainsKey("FontWeight"))
                    {
                        FontWeight? val = ParseFontWeight(style["FontWeight"]);
                        if (val.HasValue) ValueBlock.FontWeight = val.Value;
                    }
                }
            }
            catch {}
        }

        #endregion
    }
}



