using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace {{Namespace}}
{
    public class SidebarItem
    {
        public string Label { get; set; }
        public string IconPath { get; set; }
        public string Tag { get; set; }

        public SidebarItem()
        {
            Label = "菜单项";
            IconPath = "";
            Tag = "";
        }
    }

    public class SidebarEventArgs : EventArgs
    {
        public int Index { get; set; }
        public string Label { get; set; }
        public string Tag { get; set; }
        public bool IsCollapsed { get; set; }
    }

    [System.Runtime.InteropServices.ComVisible(true)]
    public delegate void SidebarItemSelectedEventHandler(int index, string label, string tag);

    [System.Runtime.InteropServices.ComVisible(true)]
    public delegate void SidebarStateChangedEventHandler(bool isCollapsed);

    public partial class SidebarControl : UserControl
    {
        public static readonly DependencyProperty LogoTextProperty =
            DependencyProperty.Register("LogoText", typeof(string), typeof(SidebarControl), new PropertyMetadata("WPF SIDEBAR", OnLogoChanged));

        public static readonly DependencyProperty LogoImagePathProperty =
            DependencyProperty.Register("LogoImagePath", typeof(string), typeof(SidebarControl), new PropertyMetadata("", OnLogoChanged));

        public static readonly DependencyProperty LogoIconTextProperty =
            DependencyProperty.Register("LogoIconText", typeof(string), typeof(SidebarControl), new PropertyMetadata("Logo", OnLogoChanged));

        public static readonly DependencyProperty LogoUseImageProperty =
            DependencyProperty.Register("LogoUseImage", typeof(bool), typeof(SidebarControl), new PropertyMetadata(false, OnLogoChanged));

        public static readonly DependencyProperty LogoMarginProperty =
            DependencyProperty.Register("LogoMargin", typeof(Thickness), typeof(SidebarControl), new PropertyMetadata(new Thickness(10, 0, 12, 0), OnLogoChanged));

        private static void OnLogoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as SidebarControl;
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

        public Thickness LogoMargin
        {
            get { return (Thickness)GetValue(LogoMarginProperty); }
            set { SetValue(LogoMarginProperty, value); }
        }

        public void SetLogoImagePath(string path)
        {
            LogoImagePath = path ?? string.Empty;
            LogoUseImage = !string.IsNullOrWhiteSpace(LogoImagePath);
        }

        public void SetLogoImagePathUTF8(byte[] path)
        {
            if (path == null) return;
            SetLogoImagePath(System.Text.Encoding.UTF8.GetString(path));
        }

        public void SetLogoUseImage(bool useImage)
        {
            LogoUseImage = useImage;
        }

        public static readonly DependencyProperty IsCollapsedProperty =
            DependencyProperty.Register("IsCollapsed", typeof(bool), typeof(SidebarControl), 
                new PropertyMetadata(false, OnIsCollapsedChanged));

        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }

        public static readonly DependencyProperty MenuItemsProperty =
            DependencyProperty.Register("MenuItems", typeof(ObservableCollection<SidebarItem>), typeof(SidebarControl), 
                new PropertyMetadata(new ObservableCollection<SidebarItem>()));

        public ObservableCollection<SidebarItem> MenuItems
        {
            get { return (ObservableCollection<SidebarItem>)GetValue(MenuItemsProperty); }
            set { SetValue(MenuItemsProperty, value); }
        }

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(SidebarControl), 
                new PropertyMetadata(0));

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public event SidebarItemSelectedEventHandler ItemSelected;
        public event SidebarStateChangedEventHandler StateChanged;

        public SidebarControl()
        {
            InitializeComponent();
            this.DataContext = this;
            this.mainBorder.Width = 260; // 动画目标改为内部 Border，解决宿主容器强行居中的问题
            
            // 初始化一些默认数据（仅预览用）
            if (MenuItems.Count == 0)
            {
                MenuItems.Add(new SidebarItem { Label = "主页", Tag = "home" });
                MenuItems.Add(new SidebarItem { Label = "设置", Tag = "settings" });
                MenuItems.Add(new SidebarItem { Label = "日志", Tag = "logs" });
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

        private static void OnIsCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as SidebarControl;
            if (ctrl == null) return;
            bool isCollapsed = (bool)e.NewValue;
            
            // 宽度动画安全性处理 (针对内部 Border)
            if (double.IsNaN(ctrl.mainBorder.Width)) ctrl.mainBorder.Width = ctrl.mainBorder.ActualWidth > 0 ? ctrl.mainBorder.ActualWidth : 260;

            DoubleAnimation anima = new DoubleAnimation();
            anima.To = isCollapsed ? 70 : 260; // 复刻生成器尺寸
            anima.Duration = TimeSpan.FromSeconds(0.3);
            anima.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            
            ctrl.mainBorder.BeginAnimation(WidthProperty, anima);
            
            // 箭头更新
            if (ctrl.arrow != null)
            {
                ctrl.arrow.Text = isCollapsed ? "➡" : "⬅";
            }

            var handler = ctrl.StateChanged;
            if (handler != null)
            {
                handler(isCollapsed);
            }
        }

        private void Item_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as RadioButton;
            var item = btn.DataContext as SidebarItem;
            int idx = MenuItems.IndexOf(item);
            
            SelectedIndex = idx;
            var handler = ItemSelected;
            if (handler != null)
            {
                handler(idx, item.Label, item.Tag);
            }
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            IsCollapsed = !IsCollapsed;
        }

        #region LabVIEW API 补全

        public void AddMenuItem(string label, string tag, string iconPath)
        {
            MenuItems.Add(new SidebarItem { Label = label, Tag = tag, IconPath = iconPath });
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
            LogoIconText = string.IsNullOrWhiteSpace(text) ? "Logo" : text;
            LogoUseImage = false;
        }

        public void SetLogoIconTextUTF8(byte[] text)
        {
            if (text != null) SetLogoIconText(System.Text.Encoding.UTF8.GetString(text));
        }

        public void SetLogoMargin(double left, double top, double right, double bottom)
        {
            LogoMargin = new Thickness(left, top, right, bottom);
        }

        public void SetLogoMargin(double uniform)
        {
            LogoMargin = new Thickness(uniform);
        }

        public void SetSelectedIndex(int index)
        {
            SelectedIndex = index;
        }

        public void SetCollapsed(bool collapsed)
        {
            IsCollapsed = collapsed;
        }

        #endregion

        #region 运行时风格重绘

        public void ApplyStyle(System.Collections.Generic.Dictionary<string, object> style)
        {
            if (style == null) return;
            try
            {
                // 1. 侧边栏背景底色
                string sbColor = null;
                if (style.ContainsKey("SidebarBackground")) sbColor = style["SidebarBackground"] as string;
                else if (style.ContainsKey("ControlBackground")) sbColor = style["ControlBackground"] as string;
                if (!string.IsNullOrEmpty(sbColor))
                {
                    try {
                        var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(sbColor.StartsWith("#") ? sbColor : "#" + sbColor));
                        this.Resources["SidebarBgBrush"] = b;
                        if (mainBorder != null) mainBorder.Background = b;
                    } catch { }
                }

                // 2. 文字颜色
                if (style.ContainsKey("FontColor"))
                {
                    string fc = style["FontColor"] as string;
                    if (!string.IsNullOrEmpty(fc))
                    {
                        try {
                            var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fc.StartsWith("#") ? fc : "#" + fc));
                            this.Resources["SidebarFontBrush"] = b;
                            if (logoTxt != null) logoTxt.Foreground = b;
                            if (arrow != null) arrow.Foreground = b;
                        } catch { }
                    }
                }

                // 3. 强调高亮色 (AccentColor)
                if (style.ContainsKey("AccentColor"))
                {
                    string ac = style["AccentColor"] as string;
                    if (!string.IsNullOrEmpty(ac))
                    {
                        try {
                            var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ac.StartsWith("#") ? ac : "#" + ac));
                            this.Resources["SidebarAccentBrush"] = b;
                            if (logoIconBorder != null) logoIconBorder.Background = b;
                        } catch { }
                    }
                }

                // 4. 按钮渐变色 (GradientStart/Mid/End)
                if (style.ContainsKey("GradientStart") && style.ContainsKey("GradientMid") && style.ContainsKey("GradientEnd"))
                {
                    try {
                        Color c1 = (Color)ColorConverter.ConvertFromString(style["GradientStart"] as string);
                        Color c2 = (Color)ColorConverter.ConvertFromString(style["GradientMid"] as string);
                        Color c3 = (Color)ColorConverter.ConvertFromString(style["GradientEnd"] as string);
                        var g = new LinearGradientBrush { StartPoint = new Point(0,0), EndPoint = new Point(1,1) };
                        g.GradientStops.Add(new GradientStop(c1, 0));
                        g.GradientStops.Add(new GradientStop(c2, 0.5));
                        g.GradientStops.Add(new GradientStop(c3, 1));
                        this.Resources["SidebarBtnGradBrush"] = g;
                    } catch { }
                }

                // 5. 控件底色 (ControlBackground)
                if (style.ContainsKey("ControlBackground"))
                {
                    string cb = style["ControlBackground"] as string;
                    if (!string.IsNullOrEmpty(cb))
                    {
                        try {
                            var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cb.StartsWith("#") ? cb : "#" + cb));
                            this.Resources["SidebarCtrlBgBrush"] = b;
                        } catch { }
                    }
                }

                // 6. 圆角与字体
                if (style.ContainsKey("CornerRadius") && mainBorder != null)
                {
                    try { mainBorder.CornerRadius = new CornerRadius(Convert.ToDouble(style["CornerRadius"])); } catch { }
                }
                if (style.ContainsKey("FontFamily"))
                {
                    var ff = new FontFamily(style["FontFamily"] as string);
                    if (logoTxt != null) logoTxt.FontFamily = ff;
                }

                // 7. 阴影 (ShadowColor / ShadowBlur / ShadowOpacity)
                var shadow = mainBorder != null ? mainBorder.Effect as System.Windows.Media.Effects.DropShadowEffect : null;
                if (shadow != null)
                {
                    if (style.ContainsKey("ShadowBlur")) try { shadow.BlurRadius = Convert.ToDouble(style["ShadowBlur"]); } catch { }
                    if (style.ContainsKey("ShadowDepth")) try { shadow.ShadowDepth = Convert.ToDouble(style["ShadowDepth"]); } catch { }
                    if (style.ContainsKey("ShadowOpacity")) try { shadow.Opacity = Convert.ToDouble(style["ShadowOpacity"]); } catch { }
                    if (style.ContainsKey("ShadowColor"))
                    {
                        try { shadow.Color = (Color)ColorConverter.ConvertFromString(style["ShadowColor"] as string); } catch { }
                    }
                }
            }
            catch { }
        }

        #endregion
    }

    // 辅助转换器：用于同步项的选中状态
    public class SidebarIndexToCheckedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return false;
            var item = values[0] as SidebarItem;
            var selectedIdx = (int)values[1];
            var items = values[2] as ObservableCollection<SidebarItem>;
            
            if (item == null || items == null) return false;
            return items.IndexOf(item) == selectedIdx;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
