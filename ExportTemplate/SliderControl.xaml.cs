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

        #region 运行时风格重绘

        /// <summary>
        /// 获取当前应用的动态重绘配置
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> CurrentStyle { get; private set; }

        public void ApplyStyle(System.Collections.Generic.Dictionary<string, object> style)
        {
            if (style == null) return;
            this.CurrentStyle = style;
            try
            {
                // 1. 滑块渐变色重绘 (通过修改 StartColor 和 EndColor 依赖属性触发绑定更新)
                if (style.ContainsKey("SliderColor1"))
                {
                    this.StartColor = style["SliderColor1"] as string;
                }
                if (style.ContainsKey("SliderColor2"))
                {
                    this.EndColor = style["SliderColor2"] as string;
                }

                // 2. 标签与数值的字体及颜色重绘
                if (LabelBlock != null)
                {
                    if (style.ContainsKey("FontFamily")) LabelBlock.FontFamily = new FontFamily(style["FontFamily"] as string);
                    if (style.ContainsKey("LabelColor")) LabelBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(style["LabelColor"] as string));
                    if (style.ContainsKey("LabelFontSize")) LabelBlock.FontSize = Convert.ToDouble(style["LabelFontSize"]);
                }

                if (ValueBlock != null)
                {
                    if (style.ContainsKey("FontFamily")) ValueBlock.FontFamily = new FontFamily(style["FontFamily"] as string);
                    if (style.ContainsKey("LabelColor")) ValueBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(style["LabelColor"] as string));
                    if (style.ContainsKey("LabelFontSize")) ValueBlock.FontSize = Convert.ToDouble(style["LabelFontSize"]);
                }
            }
            catch {}
        }

        #endregion
    }
}
