using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WpfTextInput
{
    [ToolboxItem(true)]
    [Description("指示灯 - LabVIEW .NET 容器包装")]
    public class LedPanel : WpfPanelBase
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


        [Category("Appearance"), Description("指示灯外廓与发光层的圆角半径")]
        public double CornerRadius
        {
            get { return _wpfControl != null ? _wpfControl.CornerRadius : 14.0; }
            set { if (_wpfControl != null) _wpfControl.CornerRadius = value; }
        }

        public void SetCornerRadius(double radius)
        {
            if (_wpfControl != null) _wpfControl.CornerRadius = radius;
        }

        [Category("Led"), Description("指示灯标签")]
        public override string LabelText
        {
            get { return _wpfControl != null ? _wpfControl.LabelText : ""; }
            set { if (_wpfControl != null) _wpfControl.LabelText = value; }
        }

        public override void SetLabelVisible(bool visible)
        {
            if (_wpfControl != null) _wpfControl.SetLabelVisible(visible);
        }

        /// <summary>
        /// 设置标签文字 (UTF8 字节流方案，解决乱码)
        /// </summary>
        public override void SetLabelTextUTF8(byte[] bytes)
        {
            if (bytes == null) return;
            try { LabelText = System.Text.Encoding.UTF8.GetString(bytes); } catch { }
        }


        public LedPanel()
        {
            try {
                this.BackColor = ColorTranslator.FromHtml("{{ControlBackground}}");
            } catch {
                this.BackColor = Color.White;
            }

            _wpfControl = new LedControl();
            _wpfControl.ValueChanged += WpfControl_ValueChanged;

            _host = new ElementHost
            {
                Dock = DockStyle.Fill,
                BackColorTransparent = true,
                Child = _wpfControl
            };
            this.Controls.Add(_host);
        }

        private void WpfControl_ValueChanged(object sender, EventArgs e)
        {
            if (ValueChanged != null) ValueChanged(this, e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_wpfControl != null)
                {
                    _wpfControl.ValueChanged -= WpfControl_ValueChanged;
                }
                if (_host != null) _host.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 运行时风格重绘

        /// <summary>
        /// 在内存中直接覆盖视觉原件的属性以重载样式
        /// </summary>
        public override void ApplyStyleDictionary(System.Collections.Generic.Dictionary<string, object> style)
        {
            base.ApplyStyleDictionary(style);
            if (style == null) return;
            try
            {
                if (_wpfControl != null)
                {
                    if (!_wpfControl.Dispatcher.CheckAccess())
                    {
                        _wpfControl.Dispatcher.Invoke(new Action(() => _wpfControl.ApplyStyle(style)));
                    }
                    else
                    {
                        _wpfControl.ApplyStyle(style);
                    }
                }
            }
            catch { }
        }

        #endregion
    }
}
