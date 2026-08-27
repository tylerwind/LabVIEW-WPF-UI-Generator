using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfTextInput
{
    /// <summary>
    /// 数值显示控件
    /// </summary>
    public partial class NumericDisplayControl : UserControl
    {
        #region 依赖属性

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(NumericDisplayControl),
                new PropertyMetadata("0.00", OnValuePropertyChanged));

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(NumericDisplayControl),
                new PropertyMetadata("Unit", OnUnitPropertyChanged));

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(NumericDisplayControl),
                new PropertyMetadata("标签", OnLabelTextPropertyChanged));

        public static readonly DependencyProperty ValueFontSizeProperty =
            DependencyProperty.Register("ValueFontSize", typeof(double), typeof(NumericDisplayControl),
                new PropertyMetadata(24.0, OnValueFontSizePropertyChanged));

        public static readonly DependencyProperty UnitFontSizeProperty =
            DependencyProperty.Register("UnitFontSize", typeof(double), typeof(NumericDisplayControl),
                new PropertyMetadata(14.0, OnUnitFontSizePropertyChanged));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public double ValueFontSize
        {
            get { return (double)GetValue(ValueFontSizeProperty); }
            set { SetValue(ValueFontSizeProperty, value); }
        }

        public double UnitFontSize
        {
            get { return (double)GetValue(UnitFontSizeProperty); }
            set { SetValue(UnitFontSizeProperty, value); }
        }

        #endregion

        public NumericDisplayControl()
        {
            InitializeComponent();
            if (ValueBlock != null) ValueFontSize = ValueBlock.FontSize;
            if (UnitBlock != null) UnitFontSize = UnitBlock.FontSize;
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
        /// 设置单位是否可见
        /// </summary>
        public void SetUnitVisible(bool visible)
        {
            if (UnitBlock != null)
                UnitBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region 属性变更回调

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericDisplayControl)d;
            if (control.ValueBlock != null)
            {
                control.ValueBlock.Text = e.NewValue as string ?? "";
            }
        }

        private static void OnUnitPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericDisplayControl)d;
            if (control.UnitBlock != null)
            {
                control.UnitBlock.Text = e.NewValue as string ?? "";
            }
        }

        private static void OnLabelTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericDisplayControl)d;
            if (control.LabelBlock != null)
            {
                control.LabelBlock.Text = e.NewValue as string ?? "标签";
            }
        }

        private static void OnValueFontSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericDisplayControl)d;
            if (control.ValueBlock != null && e.NewValue is double)
            {
                control.ValueBlock.FontSize = (double)e.NewValue;
            }
        }

        private static void OnUnitFontSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericDisplayControl)d;
            if (control.UnitBlock != null && e.NewValue is double)
            {
                control.UnitBlock.FontSize = (double)e.NewValue;
            }
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
                if (ValueBlock != null)
                {
                    if (style.ContainsKey("FontFamily")) ValueBlock.FontFamily = new FontFamily(style["FontFamily"] as string);
                    if (style.ContainsKey("FontSize"))
                    {
                        double? val = ParseDouble(style["FontSize"]);
                        if (val.HasValue) ValueBlock.FontSize = val.Value;
                    }
                    if (style.ContainsKey("FontColor"))
                    {
                        Color? val = ParseColor(style["FontColor"]);
                        if (val.HasValue) ValueBlock.Foreground = new SolidColorBrush(val.Value);
                    }
                    if (style.ContainsKey("FontWeight"))
                    {
                        FontWeight? val = ParseFontWeight(style["FontWeight"]);
                        if (val.HasValue) ValueBlock.FontWeight = val.Value;
                    }
                }

                // 7. 单位文字重绘
                if (UnitBlock != null)
                {
                    if (style.ContainsKey("FontFamily")) UnitBlock.FontFamily = new FontFamily(style["FontFamily"] as string);
                    if (style.ContainsKey("FontColor"))
                    {
                        Color? val = ParseColor(style["FontColor"]);
                        if (val.HasValue) UnitBlock.Foreground = new SolidColorBrush(val.Value);
                    }
                }

                // 8. 标签字体与颜色重绘
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
