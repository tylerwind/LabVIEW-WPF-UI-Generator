using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace {{Namespace}}
{
    public partial class IconButtonControl : UserControl
    {
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(IconButtonControl), new PropertyMetadata("{{IconButtonText}}"));

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public static readonly DependencyProperty IconTextProperty =
            DependencyProperty.Register("IconText", typeof(string), typeof(IconButtonControl), new PropertyMetadata("{{IconButtonIconText}}"));

        public string IconText
        {
            get { return (string)GetValue(IconTextProperty); }
            set { SetValue(IconTextProperty, value); }
        }

        public static readonly DependencyProperty IconPathProperty =
            DependencyProperty.Register("IconPath", typeof(string), typeof(IconButtonControl), 
                new PropertyMetadata(@"{{IconButtonIconPath}}", OnIconPathChanged));

        private static void OnIconPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as IconButtonControl;
            if (ctrl != null && !string.IsNullOrEmpty(e.NewValue as string))
            {
                ctrl.UseImage = true;
            }
        }

        public string IconPath
        {
            get { return (string)GetValue(IconPathProperty); }
            set { SetValue(IconPathProperty, value); }
        }

        public static readonly DependencyProperty UseImageProperty =
            DependencyProperty.Register("UseImage", typeof(bool), typeof(IconButtonControl), new PropertyMetadata({{IconButtonUseImage}}));

        public bool UseImage
        {
            get { return (bool)GetValue(UseImageProperty); }
            set { SetValue(UseImageProperty, value); }
        }

        public event RoutedEventHandler Click;

        public IconButtonControl()
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(IconPath)) UseImage = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null)
            {
                Click(this, e);
            }
        }
    }
}
