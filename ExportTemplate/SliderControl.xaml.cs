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
    }
}
