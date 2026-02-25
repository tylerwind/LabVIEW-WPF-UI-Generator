using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfSlider
{
    /// <summary>
    /// 新拟态质感滑动杆控件
    /// </summary>
    public partial class SliderControl : UserControl
    {
        #region 依赖属性

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(SliderControl),
                new PropertyMetadata("标签", OnLabelTextPropertyChanged));

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        #endregion

        #region 事件

        public delegate void ValueChangedHandler(double oldValue, double newValue);
        public event ValueChangedHandler ValueChanged;

        #endregion

        public SliderControl()
        {
            InitializeComponent();
        }

        #region 公共属性/方法

        public double Value
        {
            get => InputBox.Value;
            set => InputBox.Value = value;
        }

        public double Minimum
        {
            get => InputBox.Minimum;
            set => InputBox.Minimum = value;
        }

        public double Maximum
        {
            get => InputBox.Maximum;
            set => InputBox.Maximum = value;
        }

        public double TickFrequency
        {
            get => InputBox.TickFrequency;
            set => InputBox.TickFrequency = value;
        }

        public bool IsSnapToTickEnabled
        {
            get => InputBox.IsSnapToTickEnabled;
            set => InputBox.IsSnapToTickEnabled = value;
        }

        /// <summary>
        /// 设置标签是否可见
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

        #region 属性变更回调

        private static void OnLabelTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SliderControl)d;
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

        private void InputBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ValueBlock != null)
                ValueBlock.Text = e.NewValue.ToString("F2");

            ValueChanged?.Invoke(e.OldValue, e.NewValue);
        }

        #endregion
    }
}
