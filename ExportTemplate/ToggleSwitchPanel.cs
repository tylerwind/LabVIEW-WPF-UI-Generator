using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WpfTextInput
{
    public delegate void ToggleValueChangedHandler(bool oldValue, bool newValue);

    [ToolboxItem(true)]
    [Description("开关 - LabVIEW .NET 容器包装")]
    public class ToggleSwitchPanel : Panel
    {
        private ElementHost _host;
        private ToggleSwitchControl _wpfControl;

        public event ToggleValueChangedHandler ValueChanged;

        [Category("ToggleSwitch"), Description("开关状态(On/Off)")]
        public bool IsOn
        {
            get { return _wpfControl != null ? _wpfControl.Value : false; }
            set { if (_wpfControl != null) _wpfControl.Value = value; }
        }


        [Category("ToggleSwitch"), Description("开启时的轨道颜色 (HEX)")]
        public string ActiveColor
        {
            get { return _wpfControl != null ? _wpfControl.ActiveColor : "#4A90E2"; }
            set { if (_wpfControl != null) _wpfControl.ActiveColor = value; }
        }

        [Category("ToggleSwitch"), Description("关闭时的轨道颜色 (HEX)")]
        public string InactiveColor
        {
            get { return _wpfControl != null ? _wpfControl.InactiveColor : "#CCCCCC"; }
            set { if (_wpfControl != null) _wpfControl.InactiveColor = value; }
        }

        [Category("ToggleSwitch"), Description("开启时的轨道颜色 (数字)")]
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

        [Category("ToggleSwitch"), Description("关闭时的轨道颜色 (数字)")]
        public int InactiveColorValue
        {
            get 
            { 
                if (_wpfControl == null) return 0;
                try {
                    var c = System.Drawing.ColorTranslator.FromHtml(_wpfControl.InactiveColor);
                    return (c.R << 16) | (c.G << 8) | c.B;
                } catch { return 0; }
            }
            set 
            { 
                if (_wpfControl != null)
                {
                    _wpfControl.InactiveColor = string.Format("#{0:X6}", value & 0xFFFFFF);
                }
            }
        }

        [Category("ToggleSwitch"), Description("开关标签")]
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


        public ToggleSwitchPanel()
        {
            this.BackColor = System.Drawing.Color.Transparent;

            _wpfControl = new ToggleSwitchControl();
            _wpfControl.ValueChanged += delegate(bool o, bool n) {
                if (ValueChanged != null) ValueChanged(o, n);
            };


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
