using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfTextInput
{
    /// <summary>
    /// 新拟态质感文本输入控件
    /// </summary>
    public partial class TextInputControl : UserControl
    {
        private string _previousText = string.Empty;

        #region 依赖属性

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextInputControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnTextPropertyChanged));

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(TextInputControl),
                new PropertyMetadata("标签", OnLabelTextPropertyChanged));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        #endregion

        #region 事件

        public event ValueChangedHandler ValueChanged;

        #endregion

        public TextInputControl()
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
        /// 设置是否显示垂直滚动条
        /// </summary>
        public void SetScrollBarVisible(bool visible)
        {
            if (InputBox != null)
            {
                InputBox.VerticalScrollBarVisibility = visible
                    ? ScrollBarVisibility.Auto
                    : ScrollBarVisibility.Hidden;
                InputBox.AcceptsReturn = visible; // 有滚动条时允许多行
                InputBox.TextWrapping = visible ? TextWrapping.Wrap : TextWrapping.NoWrap;
            }
        }

        #endregion

        #region 属性变更回调

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TextInputControl)d;
            var newVal = e.NewValue as string ?? string.Empty;
            if (control.InputBox != null && control.InputBox.Text != newVal)
            {
                control.InputBox.Text = newVal;
            }
        }

        private static void OnLabelTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TextInputControl)d;
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

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string newText = InputBox.Text ?? string.Empty;
            string oldText = _previousText;

            if (Text != newText)
            {
                Text = newText;
            }

            if (oldText != newText)
            {
                _previousText = newText;
                ValueChanged?.Invoke(oldText, newText);
            }
        }

        #endregion
    }
}
