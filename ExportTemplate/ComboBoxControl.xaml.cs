using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfComboBox
{
    /// <summary>
    /// 新拟态质感下拉框控件
    /// </summary>
    public partial class ComboBoxControl : UserControl
    {
        #region 依赖属性

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(ComboBoxControl),
                new PropertyMetadata("标签", OnLabelTextPropertyChanged));

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        #endregion

        #region 事件

        public delegate void SelectionChangedHandler(int selectedIndex, object selectedItem);
        public event SelectionChangedHandler SelectionChanged;

        #endregion

        public ComboBoxControl()
        {
            InitializeComponent();
        }

        #region 公共属性/方法
        
        public ItemCollection Items
        {
            get { return InputBox.Items; }
        }


        public int SelectedIndex
        {
            get { return InputBox.SelectedIndex; }
            set { InputBox.SelectedIndex = value; }
        }


        public object SelectedItem
        {
            get { return InputBox.SelectedItem; }
            set { InputBox.SelectedItem = value; }
        }


        public string Text
        {
            get { return InputBox.Text; }
            set { InputBox.Text = value; }
        }


        /// <summary>
        /// 设置标签是否可见
        /// </summary>
        public void SetLabelVisible(bool visible)
        {
            if (LabelBlock != null)
                LabelBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ClearItems()
        {
            InputBox.Items.Clear();
        }

        public void AddItem(object item)
        {
            InputBox.Items.Add(item);
        }

        #endregion

        #region 属性变更回调

        private static void OnLabelTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ComboBoxControl)d;
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

        private void InputBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null) SelectionChanged(InputBox.SelectedIndex, InputBox.SelectedItem);

        }

        #endregion

        #region 运行时风格重绘

        /// <summary>
        /// 获取当前应用的动态重绘配置
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
                // 0. 控件底色重绘
                if (style.ContainsKey("ControlBackground"))
                {
                    Color? ctrlBg = ParseColor(style["ControlBackground"]);
                    if (ctrlBg.HasValue)
                    {
                        this.Background = new SolidColorBrush(ctrlBg.Value);
                    }
                }

                // 1. 背景渐变重绘
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

                // 2. 圆角与边框粗细
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

                // 3. 边框颜色
                if (InputBorderBrush != null && style.ContainsKey("BorderColor"))
                {
                    Color? bcVal = ParseColor(style["BorderColor"]);
                    if (bcVal.HasValue) InputBorderBrush.Color = bcVal.Value;
                }

                // 4. 下划强调线
                if (AccentLine != null && style.ContainsKey("GradientStart"))
                {
                    Color? acVal = ParseColor(style["GradientStart"]);
                    if (acVal.HasValue) AccentLine.Color = acVal.Value;
                }

                // 5. 阴影重绘
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

                // 6. 字体样式与颜色重绘
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
                    if (style.ContainsKey("FontWeight"))
                    {
                        FontWeight? val = ParseFontWeight(style["FontWeight"]);
                        if (val.HasValue) InputBox.FontWeight = val.Value;
                    }
                }

                // 7. 标签字体与颜色重绘
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
            catch { }
        }

        #endregion
    }
}
