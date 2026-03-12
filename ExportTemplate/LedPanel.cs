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
            get => _wpfControl?.Value ?? false;
            set { if (_wpfControl != null) _wpfControl.Value = value; }
        }

        [Category("Led"), Description("亮起时的颜色")]
        public string ActiveColor
        {
            get => _wpfControl?.OnColor ?? "#00FF00";
            set { if (_wpfControl != null) _wpfControl.OnColor = value; }
        }

        [Category("Led"), Description("指示灯标签")]
        public string LabelText
        {
            get => _wpfControl?.LabelText ?? "";
            set { if (_wpfControl != null) _wpfControl.LabelText = value; }
        }

        public void SetLabelVisible(bool visible)
        {
            _wpfControl?.SetLabelVisible(visible);
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
            if (disposing) _host?.Dispose();
            base.Dispose(disposing);
        }
    }
}
