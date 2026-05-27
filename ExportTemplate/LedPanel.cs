using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WpfTextInput
{
    [ToolboxItem(true)]
    [Description("指示灯 - LabVIEW .NET 容器包装")]
    public class LedPanel : Panel
    {
        private ElementHost _host;
        private LedControl _wpfControl;

        /// <summary>
        /// 获取内部嵌入的 WPF 控件实例 (可测试性自检接口)
        /// </summary>
        [Browsable(false)]
        public LedControl WpfControl { get { return _wpfControl; } }

        public event EventHandler ValueChanged;

        [Category("Led"), Description("指示灯状态(On/Off)")]
        public bool IsOn
        {
            get { return _wpfControl != null ? _wpfControl.Value : false; }
            set { if (_wpfControl != null) _wpfControl.Value = value; }
        }


        [Category("Led"), Description("亮起时的颜色 (HEX)")]
        public string ActiveColor
        {
            get { return _wpfControl != null ? _wpfControl.ActiveColor : "#00FF00"; }
            set { if (_wpfControl != null) _wpfControl.ActiveColor = value; }
        }

        [Category("Led"), Description("亮起时的颜色 (数字)")]
        public int ActiveColorValue
        {
            get 
            { 
                if (_wpfControl == null) return 0;
                try {
                    var c = System.Drawing.ColorTranslator.FromHtml(_wpfControl.ActiveColor);
                    return (c.R << 16) | (c.G << 8) | c.B;
                } catch { return 0; }
            }
            set 
            { 
                if (_wpfControl != null)
                {
                    _wpfControl.ActiveColor = string.Format("#{0:X6}", value & 0xFFFFFF);
                }
            }
        }


        [Category("Led"), Description("指示灯标签")]
        public string LabelText
        {
            get { return _wpfControl != null ? _wpfControl.LabelText : ""; }
            set { if (_wpfControl != null) _wpfControl.LabelText = value; }
        }


        public void SetLabelVisible(bool visible)
        {
            if (_wpfControl != null) _wpfControl.SetLabelVisible(visible);
        }

        /// <summary>
        /// 设置标签文字 (UTF8 字节流方案，解决乱码)
        /// </summary>
        public void SetLabelTextUTF8(byte[] bytes)
        {
            if (bytes == null) return;
            try { LabelText = System.Text.Encoding.UTF8.GetString(bytes); } catch { }
        }


        public LedPanel()
        {
            this.BackColor = System.Drawing.Color.Transparent;

            _wpfControl = new LedControl();

            _host = new ElementHost
            {
                Dock = DockStyle.Fill,
                BackColorTransparent = true,
                Child = _wpfControl
            };
            this.Controls.Add(_host);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
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

                // 2. 将样式字典透传给内嵌 of WPF 控件
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
