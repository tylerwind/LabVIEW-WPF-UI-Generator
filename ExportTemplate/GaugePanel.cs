using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WpfGauge
{
    [ComVisible(true)]
    [ToolboxItem(true)]
    [Description("半圆仪表控件")]
    public class GaugePanel : WpfPanelBase
    {
        private readonly ElementHost _host;
        private readonly GaugeControl _wpfControl;

        /// <summary>
        /// 获取内部嵌入的 WPF 控件实例 (可测试性自检接口)
        /// </summary>
        [Browsable(false)]
        public GaugeControl WpfControl { get { return _wpfControl; } }

        public GaugePanel()
        {
            try { this.BackColor = ColorTranslator.FromHtml("{{ControlBackground}}"); }
            catch { this.BackColor = Color.White; }

            _host = new ElementHost { Dock = DockStyle.Fill, BackColorTransparent = true };
            _wpfControl = new GaugeControl();
            _host.Child = _wpfControl;
            Controls.Add(_host);
        }

        [Category("Appearance")]
        public override string LabelText { get { return _wpfControl.LabelText; } set { _wpfControl.LabelText = value; } }

        [Category("Appearance")]
        public string DescText { get { return _wpfControl.DescText; } set { _wpfControl.DescText = value; } }

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

        /// <summary>
        /// 设置描述文字 (UTF8 字节流方案，解决乱码)
        /// </summary>
        public void SetDescTextUTF8(byte[] bytes)
        {
            if (bytes == null) return;
            try { DescText = System.Text.Encoding.UTF8.GetString(bytes); } catch { }
        }
        

        [Category("Data")]
        public double Minimum { get { return _wpfControl.Minimum; } set { _wpfControl.Minimum = value; } }

        [Category("Data")]
        public double Maximum { get { return _wpfControl.Maximum; } set { _wpfControl.Maximum = value; } }

        [Category("Data")]
        public double Value { get { return _wpfControl.Value; } set { _wpfControl.Value = value; } }

        [Category("Appearance"), Description("渐变起点颜色 (HEX)")]
        public string StartColor
        {
            get { return _wpfControl.StartColor; }
            set { _wpfControl.StartColor = value; }
        }

        [Category("Appearance"), Description("渐变终点颜色 (HEX)")]
        public string EndColor
        {
            get { return _wpfControl.EndColor; }
            set { _wpfControl.EndColor = value; }
        }

        [Category("Appearance"), Description("渐变起点颜色 (数字)")]
        public int StartColorValue
        {
            get 
            { 
                if (_wpfControl == null) return 0;
                try {
                    var c = System.Drawing.ColorTranslator.FromHtml(_wpfControl.StartColor);
                    return (c.R << 16) | (c.G << 8) | c.B;
                } catch { return 0; }
            }
            set 
            { 
                if (_wpfControl != null)
                {
                    _wpfControl.StartColor = string.Format("#{0:X6}", value & 0xFFFFFF);
                }
            }
        }

        [Category("Appearance"), Description("渐变终点颜色 (数字)")]
        public int EndColorValue
        {
            get 
            { 
                if (_wpfControl == null) return 0;
                try {
                    var c = System.Drawing.ColorTranslator.FromHtml(_wpfControl.EndColor);
                    return (c.R << 16) | (c.G << 8) | c.B;
                } catch { return 0; }
            }
            set 
            { 
                if (_wpfControl != null)
                {
                    _wpfControl.EndColor = string.Format("#{0:X6}", value & 0xFFFFFF);
                }
            }
        }
        public void SetRange(double min, double max)
        {
            _wpfControl.Minimum = min;
            _wpfControl.Maximum = max;
        }

        public void SetValue(double value)
        {
            _wpfControl.Value = value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _host.Dispose();
            base.Dispose(disposing);
        }

        #region 运行时风格重绘

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