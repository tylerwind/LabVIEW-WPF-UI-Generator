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

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        #endregion

        #region 事件与状态

        public event ToggleChangedEventHandler ValueChanged;

        private bool _value = false;

        public bool Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    bool old = _value;
                    _value = value;
                    UpdateToggleVisual(true);
                    ValueChanged?.Invoke(old, value);
                }
            }
        }

        #endregion

        // AccentColor 用于 ON 状态滑轨颜色（从模板注入）
        private Color _accentColor = (Color)ColorConverter.ConvertFromString("{{AccentColor}}");

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

        public void SetAccentColor(string hex)
        {
            try { _accentColor = (Color)ColorConverter.ConvertFromString(hex); }
            catch { }
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

        private void UpdateToggleVisual(bool animate)
        {
            if (ThumbTranslate == null || TrackBrush == null) return;

            // 把手滑动: OFF=0, ON=22 (48 - 22把手 - 4边距)
            double targetX = _value ? 22.0 : 0.0;
            Color targetTrackColor = _value ? _accentColor : (Color)ColorConverter.ConvertFromString("#C8CCD0");

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
