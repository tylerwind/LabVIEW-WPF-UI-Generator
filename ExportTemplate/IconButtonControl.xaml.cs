using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Runtime.InteropServices;

namespace WpfIconButton
{
#if !EXPORT_ALL
    [ComVisible(true)]
    public delegate void ButtonClickEventHandler(bool oldValue, bool newValue);

    [ComVisible(true)]
    public enum ButtonActionBehavior
    {
        SwitchWhenPressed = 0,     // 按下时切换状态并保持
        SwitchWhenReleased = 1,    // 抬起时切换状态并保持
        SwitchUntilReleased = 2,   // 保持按下直到抬起
        LatchWhenPressed = 3,      // 按下时触发脉冲 (true 然后 false)
        LatchWhenReleased = 4      // 抬起时触发脉冲 (true 然后 false)
    }
#endif

    public partial class IconButtonControl : UserControl
    {
        #region 依赖属性

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(IconButtonControl), 
                new PropertyMetadata("按钮", OnLabelTextPropertyChanged));

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public static readonly DependencyProperty IconTextProperty =
            DependencyProperty.Register("IconText", typeof(string), typeof(IconButtonControl), 
                new PropertyMetadata("Icon"));

        public string IconText
        {
            get { return (string)GetValue(IconTextProperty); }
            set { SetValue(IconTextProperty, value); }
        }

        public static readonly DependencyProperty IconPathProperty =
            DependencyProperty.Register("IconPath", typeof(string), typeof(IconButtonControl), 
                new PropertyMetadata(string.Empty, OnIconPathChanged));

        public string IconPath
        {
            get { return (string)GetValue(IconPathProperty); }
            set { SetValue(IconPathProperty, value); }
        }

        public static readonly DependencyProperty UseImageProperty =
            DependencyProperty.Register("UseImage", typeof(bool), typeof(IconButtonControl), 
                new PropertyMetadata(false, OnUseImageChanged));

        public bool UseImage
        {
            get { return (bool)GetValue(UseImageProperty); }
            set { SetValue(UseImageProperty, value); }
        }

        public static readonly DependencyProperty ActiveColorProperty =
            DependencyProperty.Register("ActiveColor", typeof(string), typeof(IconButtonControl),
                new PropertyMetadata("{{AccentColor}}", OnActiveColorChanged));

        public string ActiveColor
        {
            get { return (string)GetValue(ActiveColorProperty); }
            set { SetValue(ActiveColorProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(double), typeof(IconButtonControl),
                new PropertyMetadata(12.0, OnCornerRadiusPropertyChanged));

        public double CornerRadius
        {
            get { return (double)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(bool), typeof(IconButtonControl),
                new PropertyMetadata(false, OnValuePropertyChanged));

        public bool Value
        {
            get { return (bool)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        #endregion

        #region 事件与状态

        public event ButtonClickEventHandler Click;

        public ButtonActionBehavior Behavior { get; set; }

        private bool _isPressedByMouse = false;
        private string _fontColorHex = "{{FontColor}}";

        #endregion

        public IconButtonControl()
        {
            InitializeComponent();
            Behavior = ButtonActionBehavior.SwitchWhenReleased;
            UpdateIconVisual();
            UpdateActiveColor();
        }

        #region 公共方法

        public void SetLabelVisible(bool visible)
        {
            if (LabelBlock != null)
                LabelBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region 属性变更回调

        private static void OnLabelTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (IconButtonControl)d;
            if (control.LabelBlock != null)
            {
                control.LabelBlock.Text = e.NewValue as string ?? "按钮";
            }
        }

        private static void OnIconPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (IconButtonControl)d;
            if (ctrl != null)
            {
                if (!string.IsNullOrEmpty(e.NewValue as string))
                {
                    ctrl.UseImage = true;
                }
                ctrl.UpdateIconVisual();
            }
        }

        private static void OnUseImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (IconButtonControl)d;
            ctrl.UpdateIconVisual();
        }

        private static void OnActiveColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (IconButtonControl)d;
            ctrl.UpdateActiveColor();
        }

        private static void OnCornerRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (IconButtonControl)d;
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

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (IconButtonControl)d;
            ctrl.UpdatePhysicalDepthState();
        }

        #endregion

        #region 内部方法

        private void UpdateIconVisual()
        {
            if (TxtIcon != null && ImgIcon != null)
            {
                TxtIcon.Visibility = UseImage ? Visibility.Collapsed : Visibility.Visible;
                ImgIcon.Visibility = UseImage ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void UpdateActiveColor()
        {
            if (ActiveIndicator != null)
            {
                Color col = ParseColor(ActiveColor, GetTemplateColor("{{AccentColor}}", Colors.DodgerBlue));
                ActiveIndicator.Background = new SolidColorBrush(col);
            }
        }

        #endregion

        #region UI 交互动画层

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

            // 更新扁平容器按下视觉样式
            if (InsetBorder != null && LightShadowBorder != null)
            {
                InsetBorder.Visibility = isDown ? Visibility.Visible : Visibility.Collapsed;
                LightShadowBorder.Visibility = isDown ? Visibility.Collapsed : Visibility.Visible;
            }
            if (ActiveIndicator != null)
            {
                ActiveIndicator.Visibility = isDown ? Visibility.Visible : Visibility.Collapsed;
            }

            // 更新文字高亮颜色
            if (LabelBlock != null)
            {
                if (isDown)
                {
                    Color activeCol = ParseColor(ActiveColor, GetTemplateColor("{{AccentColor}}", Colors.DodgerBlue));
                    LabelBlock.Foreground = new SolidColorBrush(activeCol);
                    LabelBlock.FontWeight = FontWeights.Bold;
                }
                else
                {
                    Color normalCol = ParseColor(_fontColorHex, GetTemplateColor("{{FontColor}}", Colors.Black));
                    LabelBlock.Foreground = new SolidColorBrush(normalCol);
                    
                    if (CurrentStyle != null && CurrentStyle.ContainsKey("FontWeight"))
                    {
                        FontWeight? fwVal = ParseFontWeight(CurrentStyle["FontWeight"]);
                        if (fwVal.HasValue) LabelBlock.FontWeight = fwVal.Value;
                    }
                    else
                    {
                        LabelBlock.FontWeight = FontWeights.SemiBold;
                    }
                }
            }

            // 触发事件通知 (Click)
            if (Click != null)
            {
                Click(!Value, Value);
            }
        }

        #endregion

        #region 运行时风格重绘

        public System.Collections.Generic.Dictionary<string, object> CurrentStyle { get; private set; }

        private Color? ParseColor(object val)
        {
            if (val == null) return null;
            string str = val as string;
            if (string.IsNullOrEmpty(str)) return null;
            try { return (Color)ColorConverter.ConvertFromString(str.StartsWith("#") ? str : "#" + str); }
            catch { return null; }
        }

        private Color ParseColor(string hex, Color fallback)
        {
            try { return (Color)ColorConverter.ConvertFromString(hex); }
            catch { return fallback; }
        }

        private Color GetTemplateColor(string templateStr, Color fallback)
        {
            if (string.IsNullOrEmpty(templateStr) || templateStr.StartsWith("{{")) return fallback;
            try { return (Color)ColorConverter.ConvertFromString(templateStr.StartsWith("#") ? templateStr : "#" + templateStr); }
            catch { return fallback; }
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

                // 3. 边框颜色
                if (InputBorderBrush != null && style.ContainsKey("BorderColor"))
                {
                    Color? bcVal = ParseColor(style["BorderColor"]);
                    if (bcVal.HasValue)
                    {
                        InputBorderBrush.Color = bcVal.Value;
                    }
                }

                // 4. 高光颜色 (HoverOverlay)
                if (HoverOverlay != null && style.ContainsKey("HighlightColor"))
                {
                    Color? hcVal = ParseColor(style["HighlightColor"]);
                    if (hcVal.HasValue)
                    {
                        HoverOverlay.Background = new SolidColorBrush(hcVal.Value);
                    }
                }

                // 5. 阴影重绘
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

                // 5.1 亮部高光阴影重绘 (LightShadow)
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

                // 6. 字体样式与颜色重绘
                if (style.ContainsKey("FontColor"))
                {
                    _fontColorHex = style["FontColor"] as string;
                }

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

                // 7. ActiveColor 重绘
                if (style.ContainsKey("AccentColor"))
                {
                    ActiveColor = style["AccentColor"] as string;
                }
            }
            catch {}
        }

        #endregion
    }
}







