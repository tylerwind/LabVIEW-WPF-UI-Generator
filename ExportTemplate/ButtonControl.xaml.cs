using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Runtime.InteropServices;

namespace WpfButton
{
    /// <summary>
    /// 鎸夐挳浜嬩欢濮旀墭锛屼紶鍏?old 鍜?new 甯冨皵閲忎互杩庡悎浜嬩欢鑺傜偣
    /// </summary>
    [ComVisible(true)]
    public delegate void ButtonClickEventHandler(bool oldValue, bool newValue);

    [ComVisible(true)]
    public enum ButtonActionBehavior
    {
        SwitchWhenPressed = 0,     // 鎸変笅鏃跺垏鎹㈢姸鎬佸苟淇濇寔
        SwitchWhenReleased = 1,    // 鎶捣鏃跺垏鎹㈢姸鎬佸苟淇濇寔
        SwitchUntilReleased = 2,   // 淇濇寔鎸変笅鐩村埌鎶捣
        LatchWhenPressed = 3,      // 鎸変笅鏃惰Е鍙戣剦鍐?(true 鐒跺悗 false)
        LatchWhenReleased = 4      // 鎶捣鏃惰Е鍙戣剦鍐?(true 鐒跺悗 false)
    }

    /// <summary>
    /// 鏂版嫙鎬佽川鎰熸寜閽帶浠?
    /// </summary>
    [ComVisible(true)]
    public partial class ButtonControl : UserControl
    {
        #region 渚濊禆灞炴€?

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(ButtonControl),
                new PropertyMetadata("按钮", OnLabelTextPropertyChanged));

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(double), typeof(ButtonControl),
                new PropertyMetadata(12.0, OnCornerRadiusPropertyChanged));

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

        private static void OnCornerRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (ButtonControl)d;
            if (ctrl != null && e.NewValue is double)
            {
                ctrl.UpdateCornerRadius((double)e.NewValue);
            }
        }

        public void UpdateCornerRadius(double radius)
        {
            var cr = new CornerRadius(radius);
            if (LightShadowBorder != null) LightShadowBorder.CornerRadius = cr;
            if (MainBorder != null) MainBorder.CornerRadius = cr;
            if (HoverOverlay != null) HoverOverlay.CornerRadius = cr;
            if (InnerBorder != null) InnerBorder.CornerRadius = cr;
            if (InsetBorder != null) InsetBorder.CornerRadius = cr;
        }

        #endregion

        #region 浜嬩欢涓庣姸鎬?

        public event ButtonClickEventHandler Click;

        public ButtonActionBehavior Behavior { get; set; }


        private bool _value = false;

        /// <summary>
        /// 鑾峰彇鎴栬缃寜閽殑褰撳墠鐘舵€侊紙寮€鍏抽噺锛?
        /// </summary>
        public bool Value
        {
            get { return _value; }

            set
            {
                if (_value != value)
                {
                    bool old = _value;
                    _value = value;
                    UpdatePhysicalDepthState();
                    if (Click != null) Click(old, value);

                }
            }
        }

        #endregion

        public ButtonControl()
        {
            InitializeComponent();
            Behavior = ButtonActionBehavior.SwitchWhenReleased;
        }


        #region 鍏叡鏂规硶

        public void SetLabelVisible(bool visible)
        {
            if (LabelBlock != null)
                LabelBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region 灞炴€у彉鏇村洖璋?

        private static void OnLabelTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ButtonControl)d;
            if (control.LabelBlock != null)
            {
                control.LabelBlock.Text = e.NewValue as string ?? "鎸夐挳";
            }
        }

        #endregion

        #region UI 瀹屽叏瀹夊叏浜や簰鍔ㄧ敾灞?(鍩轰簬灞炴€у唴鎸囬拡閲嶅彂鍔ㄧ敾锛岄伩鍏峃ameScope瀵绘壘寮傚父)

        private bool _isPressedByMouse = false;

        private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var anim = new DoubleAnimation(0.5, TimeSpan.FromSeconds(0.2));
            HoverOverlay.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var anim = new DoubleAnimation(0.0, TimeSpan.FromSeconds(0.3));
            HoverOverlay.BeginAnimation(UIElement.OpacityProperty, anim);

            if (_isPressedByMouse)
            {
                _isPressedByMouse = false;
                UpdatePhysicalDepthState();
            }
        }

        private void UserControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isPressedByMouse = true;
            UpdatePhysicalDepthState();
            MainBorder.CaptureMouse();

            switch (Behavior)
            {
                case ButtonActionBehavior.SwitchWhenPressed:
                    Value = !Value;
                    break;
                case ButtonActionBehavior.SwitchUntilReleased:
                    Value = true;
                    break;
                case ButtonActionBehavior.LatchWhenPressed:
                    if (Click != null)
                    {
                        Click(false, true);
                        Click(true, false);
                    }
                    break;

            }
        }

        private void UserControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isPressedByMouse)
            {
                _isPressedByMouse = false;
                UpdatePhysicalDepthState();
                MainBorder.ReleaseMouseCapture();

                switch (Behavior)
                {
                    case ButtonActionBehavior.SwitchWhenReleased:
                        Value = !Value;
                        break;
                    case ButtonActionBehavior.SwitchUntilReleased:
                        Value = false;
                        break;
                    case ButtonActionBehavior.LatchWhenReleased:
                        if (Click != null)
                        {
                            Click(false, true);
                            Click(true, false);
                        }
                        break;

                }
            }
        }

        private double _defaultShadowDepth = -1;
        private double _defaultShadowBlur = -1;
        private double _defaultShadowOpacity = -1;

        private void EnsureShadowInit()
        {
            if (_defaultShadowDepth < 0 && PartShadow != null)
            {
                _defaultShadowDepth = PartShadow.ShadowDepth;
                _defaultShadowBlur = PartShadow.BlurRadius;
                _defaultShadowOpacity = PartShadow.Opacity;
            }
        }

        private void UpdatePhysicalDepthState()
        {
            EnsureShadowInit();
            bool isDown = _isPressedByMouse || Value;

            double targetScale = 1.0;
            double targetTrans = isDown ? Math.Round(Math.Max(1.0, _defaultShadowDepth * 0.7)) : 0.0;
            double targetDepth = isDown ? 0.0 : _defaultShadowDepth;
            double targetBlur = isDown ? Math.Max(0, _defaultShadowBlur * 0.2) : _defaultShadowBlur;
            double targetOpacity = isDown ? _defaultShadowOpacity * 0.3 : _defaultShadowOpacity;
            
            TimeSpan duration = TimeSpan.FromSeconds(0.1);

            var scaleAnim = new DoubleAnimation(targetScale, duration);
            var transAnim = new DoubleAnimation(targetTrans, duration);

            TransformGroup group = MainBorder.RenderTransform as TransformGroup;
            if (group != null)
            {
                foreach (var t in group.Children)
                {
                    ScaleTransform st = t as ScaleTransform;
                    if (st != null)
                    {
                        st.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                        st.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
                    }
                    else
                    {
                        TranslateTransform tt = t as TranslateTransform;
                        if (tt != null)
                        {
                            tt.BeginAnimation(TranslateTransform.XProperty, transAnim);
                            tt.BeginAnimation(TranslateTransform.YProperty, transAnim);
                        }
                    }
                }
            }

            if (PartShadow != null && _defaultShadowDepth >= 0)
            {
                PartShadow.BeginAnimation(DropShadowEffect.ShadowDepthProperty, new DoubleAnimation(targetDepth, duration));
                PartShadow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, new DoubleAnimation(targetBlur, duration));
                PartShadow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(targetOpacity, duration));
            }

            // 更新扁平容器按下视觉样式 (凹陷效果与高光控制)
            if (InsetBorder != null && LightShadowBorder != null)
            {
                InsetBorder.Visibility = isDown ? Visibility.Visible : Visibility.Collapsed;
                LightShadowBorder.Visibility = isDown ? Visibility.Collapsed : Visibility.Visible;
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
            if (val is double) return (double)val;
            if (val is int) return (double)(int)val;
            if (val is float) return (double)(float)val;
            double d;
            if (double.TryParse(val.ToString(), out d)) return d;
            return null;
        }

        private FontWeight? ParseFontWeight(object val)
        {
            if (val == null) return null;
            string s = val.ToString().ToLower().Trim();
            if (s.Contains("bold")) return FontWeights.Bold;
            if (s.Contains("semi")) return FontWeights.SemiBold;
            if (s.Contains("medium")) return FontWeights.Medium;
            if (s.Contains("light")) return FontWeights.Light;
            return FontWeights.Normal;
        }

        public void ApplyStyle(System.Collections.Generic.Dictionary<string, object> style)
        {
            if (style == null) return;
            CurrentStyle = style;

            try
            {
                // 0. 控件背景色
                if (style.ContainsKey("ControlBackground"))
                {
                    Color? bgVal = ParseColor(style["ControlBackground"]);
                    if (bgVal.HasValue)
                    {
                        this.Background = new SolidColorBrush(bgVal.Value);
                        if (InsetBorder != null) InsetBorder.Background = new SolidColorBrush(bgVal.Value);
                    }
                }

                // 1. 渐变背景
                if (MainBorder != null && style.ContainsKey("GradientStart") && style.ContainsKey("GradientMid") && style.ContainsKey("GradientEnd"))
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
                        MainBorder.Background = brush;
                    }
                }

                // 2. 圆角与边框粗细
                if (style.ContainsKey("CornerRadius"))
                {
                    double? crVal = ParseDouble(style["CornerRadius"]);
                    if (crVal.HasValue)
                    {
                        var cr = new CornerRadius(crVal.Value);
                        if (LightShadowBorder != null) LightShadowBorder.CornerRadius = cr;
                        if (MainBorder != null) MainBorder.CornerRadius = cr;
                        if (HoverOverlay != null) HoverOverlay.CornerRadius = cr;
                        if (InnerBorder != null) InnerBorder.CornerRadius = cr;
                        if (InsetBorder != null) InsetBorder.CornerRadius = cr;
                    }
                }

                if (InnerBorder != null && style.ContainsKey("BorderThickness"))
                {
                    double? btVal = ParseDouble(style["BorderThickness"]);
                    if (btVal.HasValue)
                    {
                        InnerBorder.BorderThickness = new Thickness(btVal.Value);
                    }
                }

                // 3. 杈规棰滆壊
                if (InputBorderBrush != null && style.ContainsKey("BorderColor"))
                {
                    Color? bcVal = ParseColor(style["BorderColor"]);
                    if (bcVal.HasValue)
                    {
                        InputBorderBrush.Color = bcVal.Value;
                    }
                }

                // 4. 楂樺厜棰滆壊 (HoverOverlay)
                if (HoverOverlay != null && style.ContainsKey("HighlightColor"))
                {
                    Color? hcVal = ParseColor(style["HighlightColor"]);
                    if (hcVal.HasValue)
                    {
                        HoverOverlay.Background = new SolidColorBrush(hcVal.Value);
                    }
                }

                // 5. 闃村奖閲嶇粯
                if (PartShadow != null)
                {
                    if (style.ContainsKey("ShadowBlur"))
                    {
                        double? val = ParseDouble(style["ShadowBlur"]);
                        if (val.HasValue)
                        {
                            PartShadow.BlurRadius = val.Value;
                            _defaultShadowBlur = PartShadow.BlurRadius;
                        }
                    }
                    if (style.ContainsKey("ShadowDepth"))
                    {
                        double? val = ParseDouble(style["ShadowDepth"]);
                        if (val.HasValue)
                        {
                            PartShadow.ShadowDepth = val.Value;
                            _defaultShadowDepth = PartShadow.ShadowDepth;
                        }
                    }
                    if (style.ContainsKey("ShadowColor"))
                    {
                        Color? val = ParseColor(style["ShadowColor"]);
                        if (val.HasValue)
                        {
                            PartShadow.Color = val.Value;
                        }
                    }
                    if (style.ContainsKey("ShadowOpacity"))
                    {
                        double? val = ParseDouble(style["ShadowOpacity"]);
                        if (val.HasValue)
                        {
                            PartShadow.Opacity = val.Value;
                            _defaultShadowOpacity = PartShadow.Opacity;
                        }
                    }
                }

                // 5.1 浜儴楂樺厜闃村奖閲嶇粯 (LightShadow)
                if (LightShadow != null)
                {
                    if (style.ContainsKey("HighlightColor"))
                    {
                        Color? val = ParseColor(style["HighlightColor"]);
                        if (val.HasValue)
                        {
                            LightShadow.Color = val.Value;
                        }
                    }
                    if (style.ContainsKey("HighlightOpacity"))
                    {
                        double? val = ParseDouble(style["HighlightOpacity"]);
                        if (val.HasValue)
                        {
                            LightShadow.Opacity = val.Value;
                        }
                    }
                }

                // 6. 瀛椾綋鏍峰紡涓庨鑹查噸缁?
                if (LabelBlock != null)
                {
                    if (style.ContainsKey("FontFamily")) LabelBlock.FontFamily = new FontFamily(style["FontFamily"] as string);
                    if (style.ContainsKey("FontSize"))
                    {
                        double? val = ParseDouble(style["FontSize"]);
                        if (val.HasValue) LabelBlock.FontSize = val.Value;
                    }
                    if (style.ContainsKey("FontColor"))
                    {
                        Color? val = ParseColor(style["FontColor"]);
                        if (val.HasValue) LabelBlock.Foreground = new SolidColorBrush(val.Value);
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


