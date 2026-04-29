using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace {{Namespace}}
{
    public delegate void TopbarNavItemSelectedHandler(int index, string label, string tag);

    public class TopbarPanel : Panel
    {
        private ElementHost _host;
        private TopbarControl _topbar;

        public event TopbarNavItemSelectedHandler ItemSelected;

        public TopbarPanel()
        {
            try
            {
                _host = new ElementHost();
                _host.BackColorTransparent = true; // 开启透明支持
                _topbar = new TopbarControl();
                _host.Child = _topbar;
                _host.Dock = DockStyle.Fill;
                this.BackColor = System.Drawing.Color.Transparent; // Panel 也设为透明
                
                int totalHeight = (int)({{TopbarHeight}}) + 20; // 基础高度 + 阴影缓冲
                this.Height = totalHeight;
                this.MaximumSize = new System.Drawing.Size(0, totalHeight); // 锁定最大高度，防止 LabVIEW 容器拉伸出空白区域
                
                this.Controls.Add(_host);

                _topbar.ItemSelected += (index, label, tag) => {
                    if (ItemSelected != null) {
                        ItemSelected(index, label, tag);
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
                System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WpfTopbarError.log"),
                    DateTime.Now.ToString("HH:mm:ss.fff") + " ERROR [" + method + "]: " + ex.ToString() + "\r\n");
            }
            catch { }
        }

        #region 核心属性

        public string LogoText
        {
            get { return (string)InvokeOnUI(() => _topbar.LogoText); }
            set { InvokeOnUI(() => _topbar.LogoText = value); }
        }

        public string LogoIconText
        {
            get { return (string)InvokeOnUI(() => _topbar.LogoIconText); }
            set { InvokeOnUI(() => _topbar.LogoIconText = value); }
        }

        public string LogoImagePath
        {
            get { return (string)InvokeOnUI(() => _topbar.LogoImagePath); }
            set { InvokeOnUI(() => _topbar.SetLogoImagePath(value)); }
        }

        public bool LogoUseImage
        {
            get { return (bool)InvokeOnUI(() => _topbar.LogoUseImage); }
            set { InvokeOnUI(() => _topbar.LogoUseImage = value); }
        }

        public int SelectedIndex
        {
            get { return (int)InvokeOnUI(() => _topbar.SelectedIndex); }
            set { InvokeOnUI(() => _topbar.SelectedIndex = value); }
        }

        #endregion

        #region 动态操作

        /// <summary>
        /// 清除所有菜单项
        /// </summary>
        public void ClearItems()
        {
            InvokeOnUI(() => _topbar.MenuItems.Clear());
        }

        /// <summary>
        /// 添加导航项
        /// </summary>
        /// <param name="label">显示文本</param>
        /// <param name="iconPath">图片路径</param>
        /// <param name="tag">附加标记</param>
        public void AddItem(string label, string iconPath, string tag)
        {
            InvokeOnUI(() => _topbar.MenuItems.Add(new TopbarItem { Label = label, IconPath = iconPath, Tag = tag }));
        }

        #endregion

        private object InvokeOnUI(Func<object> func)
        {
            if (!_topbar.Dispatcher.CheckAccess())
                return _topbar.Dispatcher.Invoke(func);
            return func();
        }

        private void InvokeOnUI(Action action)
        {
            if (!_topbar.Dispatcher.CheckAccess())
                _topbar.Dispatcher.Invoke(action);
            else
                action();
        }
    }
}
