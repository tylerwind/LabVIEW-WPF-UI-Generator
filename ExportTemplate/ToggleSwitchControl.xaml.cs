using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfTextInput
{
    /// <summary>
    /// Toggle 开关事件委托
    /// </summary>
    public delegate void ToggleChangedEventHandler(bool oldValue, bool newValue);

    /// <summary>
    /// Toggle 开关控件
    /// </summary>
    public partial class ToggleSwitchControl : UserControl
    {
        #region 依赖属性

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(ToggleSwitchControl),
                new PropertyMetadata("开关", OnLabelTextChanged));

        public static readonly DependencyProperty ActiveColorProperty =
            DependencyProperty.Register("ActiveColor", typeof(string), typeof(ToggleSwitchControl),
                new PropertyMetadata("{{ToggleActiveColor}}", OnColorPropertyChanged));

        public static readonly DependencyProperty InactiveColorProperty =
            DependencyProperty.Register("InactiveColor", typeof(string), typeof(ToggleSwitchControl),
                new PropertyMetadata("{{ToggleInactiveColor}}", OnColorPropertyChanged));

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

        #region 事件与状态

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
            // 初始化为 OFF 状态
            UpdateToggleVisual(false);
        }

        #region 公共方法

        public void SetLabelVisible(bool visible)
        {
            if (LabelBlock != null)
                LabelBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region 用户交互

        private void UserControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Value = !Value;
        }

        #endregion

        #region 视觉更新

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

        private Color ParseColor(string hex, Color fallback)
        {
            try { return (Color)ColorConverter.ConvertFromString(hex); }
            catch { return fallback; }
        }

        private void UpdateToggleVisual(bool animate)
        {
            if (ThumbTranslate == null || TrackBrush == null) return;

            // 把手滑动: OFF=0, ON=22 (48 - 22把手 - 4边距)
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
    }
}
