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

        #endregion

        public NumericDisplayControl()
        {
            InitializeComponent();
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

        #endregion
    }
}
