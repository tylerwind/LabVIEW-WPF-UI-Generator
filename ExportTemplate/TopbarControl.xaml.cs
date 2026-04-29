using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace {{Namespace}}
{
    public class TopbarItem
    {
        public string Label { get; set; }
        public string IconPath { get; set; }
        public string Tag { get; set; }

        public TopbarItem()
        {
            Label = "菜单项";
            IconPath = "";
            Tag = "";
        }
    }

    [System.Runtime.InteropServices.ComVisible(true)]
    public delegate void TopbarItemSelectedEventHandler(int index, string label, string tag);

    public partial class TopbarControl : UserControl
    {
        public static readonly DependencyProperty LogoTextProperty =
            DependencyProperty.Register("LogoText", typeof(string), typeof(TopbarControl), new PropertyMetadata("WPF TOPBAR", OnLogoChanged));

        public static readonly DependencyProperty LogoImagePathProperty =
            DependencyProperty.Register("LogoImagePath", typeof(string), typeof(TopbarControl), new PropertyMetadata("", OnLogoChanged));

        public static readonly DependencyProperty LogoIconTextProperty =
            DependencyProperty.Register("LogoIconText", typeof(string), typeof(TopbarControl), new PropertyMetadata("🌟", OnLogoChanged));

        public static readonly DependencyProperty LogoUseImageProperty =
            DependencyProperty.Register("LogoUseImage", typeof(bool), typeof(TopbarControl), new PropertyMetadata(false, OnLogoChanged));

        private static void OnLogoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TopbarControl;
            if (ctrl != null)
            {
                ctrl.UpdateLogoVisualState();
            }
        }

        public string LogoText
        {
            get { return (string)GetValue(LogoTextProperty); }
            set { SetValue(LogoTextProperty, value); }
        }

        public string LogoImagePath
        {
            get { return (string)GetValue(LogoImagePathProperty); }
            set { SetValue(LogoImagePathProperty, value); }
        }

        public string LogoIconText
        {
            get { return (string)GetValue(LogoIconTextProperty); }
            set { SetValue(LogoIconTextProperty, value); }
        }

        public bool LogoUseImage
        {
            get { return (bool)GetValue(LogoUseImageProperty); }
            set { SetValue(LogoUseImageProperty, value); }
        }

        public void SetLogoImagePath(string path)
        {
            LogoImagePath = path ?? string.Empty;
            LogoUseImage = !string.IsNullOrWhiteSpace(LogoImagePath);
        }

        public static readonly DependencyProperty MenuItemsProperty =
            DependencyProperty.Register("MenuItems", typeof(ObservableCollection<TopbarItem>), typeof(TopbarControl), 
                new PropertyMetadata(new ObservableCollection<TopbarItem>()));

        public ObservableCollection<TopbarItem> MenuItems
        {
            get { return (ObservableCollection<TopbarItem>)GetValue(MenuItemsProperty); }
            set { SetValue(MenuItemsProperty, value); }
        }

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(TopbarControl), 
                new PropertyMetadata(0));

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public event TopbarItemSelectedEventHandler ItemSelected;

        public TopbarControl()
        {
            InitializeComponent();
            this.DataContext = this;
            
            // 初始化一些默认数据（仅预览用）
            if (MenuItems.Count == 0)
            {
                MenuItems.Add(new TopbarItem { Label = "概览", Tag = "overview" });
                MenuItems.Add(new TopbarItem { Label = "分析", Tag = "analysis" });
                MenuItems.Add(new TopbarItem { Label = "系统", Tag = "system" });
            }
            UpdateLogoVisualState();
        }

        private void UpdateLogoVisualState()
        {
            if (logoImage == null || logoIconTextBlock == null) return;
            bool showImage = LogoUseImage && !string.IsNullOrWhiteSpace(LogoImagePath);
            logoImage.Visibility = showImage ? Visibility.Visible : Visibility.Collapsed;
            logoIconTextBlock.Visibility = showImage ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Item_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as RadioButton;
            var item = btn.DataContext as TopbarItem;
            int idx = MenuItems.IndexOf(item);
            
            SelectedIndex = idx;
            var handler = ItemSelected;
            if (handler != null)
            {
                handler(idx, item.Label, item.Tag);
            }
        }

        #region LabVIEW API 补全

        public void AddMenuItem(string label, string tag, string iconPath)
        {
            MenuItems.Add(new TopbarItem { Label = label, Tag = tag, IconPath = iconPath });
        }

        public void AddMenuItemUTF8(byte[] label, byte[] tag, byte[] iconPath)
        {
            string l = label != null ? System.Text.Encoding.UTF8.GetString(label) : "";
            string t = tag != null ? System.Text.Encoding.UTF8.GetString(tag) : "";
            string i = iconPath != null ? System.Text.Encoding.UTF8.GetString(iconPath) : "";
            AddMenuItem(l, t, i);
        }

        public void ClearMenuItems()
        {
            MenuItems.Clear();
        }

        public void SetLogoText(string text)
        {
            LogoText = text;
            LogoUseImage = false;
        }

        public void SetLogoTextUTF8(byte[] text)
        {
            if (text != null) LogoText = System.Text.Encoding.UTF8.GetString(text);
        }

        public void SetLogoIconText(string text)
        {
            LogoIconText = string.IsNullOrWhiteSpace(text) ? "🌟" : text;
            LogoUseImage = false;
        }

        public void SetLogoIconTextUTF8(byte[] text)
        {
            if (text != null) SetLogoIconText(System.Text.Encoding.UTF8.GetString(text));
        }

        public void SetSelectedIndex(int index)
        {
            SelectedIndex = index;
        }

        #endregion
    }

    // 辅助转换器：用于同步项的选中状态
    public class TopbarIndexToCheckedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return false;
            var item = values[0] as TopbarItem;
            var selectedIdx = (int)values[1];
            var items = values[2] as ObservableCollection<TopbarItem>;
            
            if (item == null || items == null) return false;
            return items.IndexOf(item) == selectedIdx;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
