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
    }
}
