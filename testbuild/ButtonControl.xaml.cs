using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace WpfButton
{
    /// <summary>
    /// 按钮事件委托，传入 old 和 new 布尔量以迎合事件节点
    /// </summary>
    public delegate void ButtonClickEventHandler(bool oldValue, bool newValue);

    public enum ButtonActionBehavior
    {
        SwitchWhenPressed = 0,     // 按下时切换状态并保持
        SwitchWhenReleased = 1,    // 抬起时切换状态并保持
        SwitchUntilReleased = 2,   // 保持按下直到抬起
        LatchWhenPressed = 3,      // 按下时触发脉冲 (true 然后 false)
        LatchWhenReleased = 4      // 抬起时触发脉冲 (true 然后 false)
    }

    /// <summary>
    /// 新拟态质感按钮控件
    /// </summary>
    public partial class ButtonControl : UserControl
    {
        #region 依赖属性

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(ButtonControl),
                new PropertyMetadata("按钮", OnLabelTextPropertyChanged));

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        #endregion

        #region 事件与状态

        public event ButtonClickEventHandler Click;

        public ButtonActionBehavior Behavior { get; set; } = ButtonActionBehavior.SwitchWhenReleased;

        private bool _value = false;

        /// <summary>
        /// 获取或设置按钮的当前状态（开关量）
        /// </summary>
        public bool Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    bool old = _value;
                    _value = value;
                    UpdateActiveStateVisual(value);
                    UpdatePhysicalDepthState();
                    Click?.Invoke(old, value);
                }
            }
        }

        #endregion

        public ButtonControl()
        {
            InitializeComponent();
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
            var control = (ButtonControl)d;
            if (control.LabelBlock != null)
            {
                control.LabelBlock.Text = e.NewValue as string ?? "按钮";
            }
        }

        #endregion

        #region UI 完全安全交互动画层 (基于属性内指针重发动画，避免NameScope寻找异常)

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
                    Click?.Invoke(false, true);
                    Click?.Invoke(true, false);
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
                        Click?.Invoke(false, true);
                        Click?.Invoke(true, false);
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

            double targetScale = isDown ? 0.95 : 1.0;
            double targetTrans = isDown ? Math.Max(1.0, _defaultShadowDepth * 0.7) : 0.0;
            double targetDepth = isDown ? 0.0 : _defaultShadowDepth;
            double targetBlur = isDown ? Math.Max(0, _defaultShadowBlur * 0.2) : _defaultShadowBlur;
            double targetOpacity = isDown ? _defaultShadowOpacity * 0.3 : _defaultShadowOpacity;
            
            TimeSpan duration = TimeSpan.FromSeconds(0.1);

            var scaleAnim = new DoubleAnimation(targetScale, duration);
            if (ScaleTrans != null)
            {
                ScaleTrans.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                ScaleTrans.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
            }

            var transAnim = new DoubleAnimation(targetTrans, duration);
            if (PosTrans != null)
            {
                PosTrans.BeginAnimation(TranslateTransform.XProperty, transAnim);
                PosTrans.BeginAnimation(TranslateTransform.YProperty, transAnim);
            }

            if (PartShadow != null && _defaultShadowDepth >= 0)
            {
                PartShadow.BeginAnimation(DropShadowEffect.ShadowDepthProperty, new DoubleAnimation(targetDepth, duration));
                PartShadow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, new DoubleAnimation(targetBlur, duration));
                PartShadow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(targetOpacity, duration));
            }
        }

        private void UpdateActiveStateVisual(bool isActive)
        {
            var anim = new DoubleAnimation(isActive ? 1.0 : 0.0, TimeSpan.FromSeconds(0.2));
            CheckedOverlay.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        #endregion
    }
}
