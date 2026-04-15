using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace {{Namespace}}
{
    public delegate void NavItemSelectedHandler(int index, string label, string tag);
    public delegate void SidebarStateChangedHandler(bool isCollapsed);

    public class SidebarPanel : Panel
    {
        private ElementHost _host;
        private SidebarControl _sidebar;

        public event NavItemSelectedHandler ItemSelected;
        public event SidebarStateChangedHandler StateChanged;

        public SidebarPanel()
        {
            try
            {
                _host = new ElementHost();
                _sidebar = new SidebarControl();
                _host.Child = _sidebar;
                _host.Dock = DockStyle.Fill;
                this.Controls.Add(_host);

                _sidebar.ItemSelected += (index, label, tag) => {
                    if (ItemSelected != null) {
                        ItemSelected(index, label, tag);
                    }
                };

                _sidebar.StateChanged += (isCollapsed) => {
                    if (StateChanged != null) {
                        StateChanged(isCollapsed);
                    }
                };

                // 强制创建句柄保障Invoke可靠性
                var h = this.Handle;
            }
            catch (Exception ex) { LogError(ex, "Constructor"); }
        }

        private void LogError(Exception ex, string method)
        {
            try
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WpfSidebarError.log"),
                    DateTime.Now.ToString("HH:mm:ss.fff") + " ERROR [" + method + "]: " + ex.ToString() + "\r\n");
            }
            catch { }
        }

        #region 核心属性

        public string LogoText
        {
            get { return (string)InvokeOnUI(() => _sidebar.LogoText); }
            set { InvokeOnUI(() => _sidebar.LogoText = value); }
        }

        public string LogoIconText
        {
            get { return (string)InvokeOnUI(() => _sidebar.LogoIconText); }
            set { InvokeOnUI(() => _sidebar.LogoIconText = value); }
        }

        public string LogoImagePath
        {
            get { return (string)InvokeOnUI(() => _sidebar.LogoImagePath); }
            set { InvokeOnUI(() => _sidebar.SetLogoImagePath(value)); }
        }

        public bool LogoUseImage
        {
            get { return (bool)InvokeOnUI(() => _sidebar.LogoUseImage); }
            set { InvokeOnUI(() => _sidebar.SetLogoUseImage(value)); }
        }

        public double LogoMarginLeft
        {
            get { return (double)InvokeOnUI(() => _sidebar.LogoMargin.Left); }
            set { InvokeOnUI(() => _sidebar.LogoMargin = new System.Windows.Thickness(value, _sidebar.LogoMargin.Top, _sidebar.LogoMargin.Right, _sidebar.LogoMargin.Bottom)); }
        }

        public double LogoMarginTop
        {
            get { return (double)InvokeOnUI(() => _sidebar.LogoMargin.Top); }
            set { InvokeOnUI(() => _sidebar.LogoMargin = new System.Windows.Thickness(_sidebar.LogoMargin.Left, value, _sidebar.LogoMargin.Right, _sidebar.LogoMargin.Bottom)); }
        }

        public double LogoMarginRight
        {
            get { return (double)InvokeOnUI(() => _sidebar.LogoMargin.Right); }
            set { InvokeOnUI(() => _sidebar.LogoMargin = new System.Windows.Thickness(_sidebar.LogoMargin.Left, _sidebar.LogoMargin.Top, value, _sidebar.LogoMargin.Bottom)); }
        }

        public double LogoMarginBottom
        {
            get { return (double)InvokeOnUI(() => _sidebar.LogoMargin.Bottom); }
            set { InvokeOnUI(() => _sidebar.LogoMargin = new System.Windows.Thickness(_sidebar.LogoMargin.Left, _sidebar.LogoMargin.Top, _sidebar.LogoMargin.Right, value)); }
        }

        public void SetLogoMargin(double left, double top, double right, double bottom)
        {
            InvokeOnUI(() => _sidebar.LogoMargin = new System.Windows.Thickness(left, top, right, bottom));
        }

        public bool IsCollapsed
        {
            get { return (bool)InvokeOnUI(() => _sidebar.IsCollapsed); }
            set { InvokeOnUI(() => _sidebar.IsCollapsed = value); }
        }

        public int SelectedIndex
        {
            get { return (int)InvokeOnUI(() => _sidebar.SelectedIndex); }
            set { InvokeOnUI(() => _sidebar.SelectedIndex = value); }
        }

        #endregion

        #region 动态操作

        /// <summary>
        /// 清除所有菜单项
        /// </summary>
        public void ClearItems()
        {
            InvokeOnUI(() => _sidebar.MenuItems.Clear());
        }

        /// <summary>
        /// 添加导航项
        /// </summary>
        /// <param name="label">显示文本</param>
        /// <param name="iconPath">图片路径</param>
        /// <param name="tag">附加标记</param>
        public void AddItem(string label, string iconPath, string tag)
        {
            InvokeOnUI(() => _sidebar.MenuItems.Add(new SidebarItem { Label = label, IconPath = iconPath, Tag = tag }));
        }

        #endregion

        private object InvokeOnUI(Func<object> func)
        {
            if (!_sidebar.Dispatcher.CheckAccess())
                return _sidebar.Dispatcher.Invoke(func);
            return func();
        }

        private void InvokeOnUI(Action action)
        {
            if (!_sidebar.Dispatcher.CheckAccess())
                _sidebar.Dispatcher.Invoke(action);
            else
                action();
        }
    }
}
