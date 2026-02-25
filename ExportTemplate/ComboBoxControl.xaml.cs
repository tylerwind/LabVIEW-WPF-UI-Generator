using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfComboBox
{
    /// <summary>
    /// 新拟态质感下拉框控件
    /// </summary>
    public partial class ComboBoxControl : UserControl
    {
        #region 依赖属性

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(ComboBoxControl),
                new PropertyMetadata("标签", OnLabelTextPropertyChanged));

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        #endregion

        #region 事件

        public delegate void SelectionChangedHandler(int selectedIndex, object selectedItem);
        public event SelectionChangedHandler SelectionChanged;

        #endregion

        public ComboBoxControl()
        {
            InitializeComponent();
        }

        #region 公共属性/方法
        
        public ItemCollection Items => InputBox.Items;

        public int SelectedIndex
        {
            get => InputBox.SelectedIndex;
            set => InputBox.SelectedIndex = value;
        }

        public object SelectedItem
        {
            get => InputBox.SelectedItem;
            set => InputBox.SelectedItem = value;
        }

        public string Text
        {
            get => InputBox.Text;
            set => InputBox.Text = value;
        }

        /// <summary>
        /// 设置标签是否可见
        /// </summary>
        public void SetLabelVisible(bool visible)
        {
            if (LabelBlock != null)
                LabelBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ClearItems()
        {
            InputBox.Items.Clear();
        }

        public void AddItem(object item)
        {
            InputBox.Items.Add(item);
        }

        #endregion

        #region 属性变更回调

        private static void OnLabelTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ComboBoxControl)d;
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

        private void InputBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(InputBox.SelectedIndex, InputBox.SelectedItem);
        }

        #endregion
    }
}
