using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfTextInput
{
    /// <summary>
    /// LED 指示灯控件
    /// </summary>
    public partial class LedControl : UserControl
    {
        #region 依赖属性

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
        }

        private static void OnColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (LedControl)d;
            c.UpdateLedVisual();
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

            // 灯体底色
            LedOffBrush.BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation(isOn ? onCol : offCol, dur));

            // 发光层
            if (GlowCenter != null) GlowCenter.Color = onCol;
            if (GlowEdge != null) GlowEdge.Color = Color.FromArgb(128, onCol.R, onCol.G, onCol.B);
            LedGlow.BeginAnimation(OpacityProperty, new DoubleAnimation(isOn ? 1.0 : 0.0, dur));

            // 外发光晕
            if (HaloBrush != null) HaloBrush.Color = onCol;
            LedHalo.BeginAnimation(OpacityProperty, new DoubleAnimation(isOn ? double.Parse("{{ShadowOpacity}}") : 0.0, dur));
        }

        #endregion
    }
}
