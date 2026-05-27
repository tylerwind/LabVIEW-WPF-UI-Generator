using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WpfButton
{
    /// <summary>
    /// 用于在 LabVIEW / WinForms 中托管 WpfButton 的容器面板
    /// </summary>
    [ToolboxItem(true)]
    [Description("带有新拟态样式的按钮控件")]
    public class ButtonPanel : Panel
    {
        private ElementHost _host;
        private ButtonControl _wpfControl;

        /// <summary>
        /// 获取内部嵌入的 WPF 控件实例 (可测试性自检接口)
        /// </summary>
        [Browsable(false)]
        public ButtonControl WpfControl { get { return _wpfControl; } }

        /// <summary>
        /// 当按钮被点击时触发的事件
        /// </summary>
        [Category("Action"), Description("当用户点击按钮时触发（抛出 oldValue, newValue）")]
        public new event ButtonClickEventHandler Click;

        public ButtonPanel()
        {
            try {
                this.BackColor = ColorTranslator.FromHtml("{{ControlBackground}}");
            } catch {
                this.BackColor = Color.White;
            }

            // 初始化 WPF 宿主
            _host = new ElementHost
            {
                Dock = DockStyle.Fill,
                BackColorTransparent = true
            };

            // 实例化 WPF 控件
            _wpfControl = new ButtonControl();
            _host.Child = _wpfControl;

            this.Controls.Add(_host);

            // 订阅事件
            _wpfControl.Click += WpfControl_Click;

            // 订阅宿主大小改变以刷新阴影区域，防止被裁切
            this.SizeChanged += delegate(object s, EventArgs e) { 
                if (_host != null) _host.Invalidate(); 
            };

        }

        private void WpfControl_Click(bool oldValue, bool newValue)
        {
            if (Click != null) Click(oldValue, newValue);
        }


        #region 给 LabVIEW 或外部代码暴露的属性与方法

        /// <summary>
        /// 获取或设置按钮文本
        /// </summary>
        [Category("Appearance"), Description("按钮显示的文本")]
        public string LabelText
        {
            get { return _wpfControl.LabelText; }
            set { _wpfControl.LabelText = value; }
        }


        [Category("Behavior"), Description("动作模式支持：按下切换、抬起切换包、脉冲与保持等")]
        public ButtonActionBehavior Behavior
        {
            get { return _wpfControl.Behavior; }
            set { _wpfControl.Behavior = value; }
        }


        [Category("Data"), Description("按钮的当前激活状态（布尔量）")]
        public bool Value
        {
            get { return _wpfControl.Value; }
            set { _wpfControl.Value = value; }
        }


        public void SetLabelVisible(bool visible)
        {
            _wpfControl.SetLabelVisible(visible);
        }

        /// <summary>
        /// 设置标签文字 (UTF8 字节流方案，解决乱码)
        /// </summary>
        public void SetLabelTextUTF8(byte[] bytes)
        {
            if (bytes == null) return;
            try { LabelText = System.Text.Encoding.UTF8.GetString(bytes); } catch { }
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_wpfControl != null)
                {
                    _wpfControl.Click -= WpfControl_Click;
                }
                if (_host != null) _host.Dispose();

            }
            base.Dispose(disposing);
        }

        #region 运行时风格重绘

        /// <summary>
        /// 根据指定的 JSON 样式配置文件实时重绘 UI
        /// </summary>
        public void UpdateStyleFromJson(string jsonPath)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonPath) || !System.IO.File.Exists(jsonPath))
                    return;

                string json = System.IO.File.ReadAllText(jsonPath, System.Text.Encoding.UTF8);
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var dict = serializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(json);
                if (dict != null)
                {
                    ApplyStyleDictionary(dict);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    System.IO.File.AppendAllText("StyleUpdateError.txt",
                        DateTime.Now.ToString() + " : " + ex.Message + "\n" + ex.StackTrace + "\n");
                }
                catch { }
            }
        }

        /// <summary>
        /// 在内存中直接覆盖视觉原件的属性以重载样式
        /// </summary>
        public void ApplyStyleDictionary(System.Collections.Generic.Dictionary<string, object> style)
        {
            if (style == null) return;
            try
            {
                // 1. 重写 Panel 自体 BackColor
                if (style.ContainsKey("ControlBackground"))
                {
                    string ctrlBg = style["ControlBackground"] as string;
                    if (!string.IsNullOrEmpty(ctrlBg))
                    {
                        this.BackColor = System.Drawing.ColorTranslator.FromHtml(ctrlBg.StartsWith("#") ? ctrlBg : "#" + ctrlBg);
                    }
                }

                // 2. 将样式字典透传给内嵌的 WPF 控件
                if (_wpfControl != null)
                {
                    _wpfControl.ApplyStyle(style);
                }
            }
            catch { }
        }

        #endregion
    }
}
