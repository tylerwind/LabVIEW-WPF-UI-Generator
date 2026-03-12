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
            get => _wpfControl?.Value ?? false;
            set { if (_wpfControl != null) _wpfControl.Value = value; }
        }

        private string _activeColor = "#4A90E2";
        [Category("ToggleSwitch"), Description("亮起时的颜色")]
        public string ActiveColor
        {
            get => _activeColor;
            set { 
                _activeColor = value;
                if (_wpfControl != null) _wpfControl.SetAccentColor(value); 
            }
        }

        [Category("ToggleSwitch"), Description("开关标签")]
        public string LabelText
        {
            get => _wpfControl?.LabelText ?? "";
            set { if (_wpfControl != null) _wpfControl.LabelText = value; }
        }

        public void SetLabelVisible(bool visible)
        {
            _wpfControl?.SetLabelVisible(visible);
        }

        public ToggleSwitchPanel()
        {
            this.BackColor = System.Drawing.Color.Transparent;

            _wpfControl = new ToggleSwitchControl();
            _wpfControl.ValueChanged += (o, n) => ValueChanged?.Invoke(o, n);

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
