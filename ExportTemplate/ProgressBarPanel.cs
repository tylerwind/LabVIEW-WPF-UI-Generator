using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WpfTextInput
{
    /// <summary>
    /// 进度条 - LabVIEW .NET 容器包装
    /// </summary>
    [ToolboxItem(true)]
    [Description("拟态风格进度条")]
    public class ProgressBarPanel : Panel
    {
        private ElementHost _host;
        private ProgressBarControl _wpfControl;

        #region LabVIEW 可见属性
        [Category("ProgressBar"), Description("标签文字")]
        public string LabelText
        {
            get { return _wpfControl != null ? _wpfControl.LabelText : ""; }
            set { if (_wpfControl != null) _wpfControl.LabelText = value; }
        }


        [Category("ProgressBar"), Description("当前值")]
        public double Value
        {
            get { return _wpfControl != null ? _wpfControl.Value : 0; }
            set { if (_wpfControl != null) _wpfControl.Value = value; }
        }


        [Category("ProgressBar"), Description("最小值")]
        public double Minimum
        {
            get { return _wpfControl != null ? _wpfControl.Minimum : 0; }
            set { if (_wpfControl != null) _wpfControl.Minimum = value; }
        }


        [Category("ProgressBar"), Description("最大值")]
        public double Maximum
        {
            get { return _wpfControl != null ? _wpfControl.Maximum : 100; }
            set { if (_wpfControl != null) _wpfControl.Maximum = value; }
        }


        [Category("ProgressBar"), Description("是否显示百分比")]
        public bool ShowPercentage
        {
            get { return _wpfControl != null ? _wpfControl.ShowPercentage : true; }
            set { if (_wpfControl != null) _wpfControl.ShowPercentage = value; }
        }

        [Category("ProgressBar"), Description("渐变起点颜色 (HEX)")]
        public string StartColor
        {
            get { return _wpfControl != null ? _wpfControl.StartColor : ""; }
            set { if (_wpfControl != null) _wpfControl.StartColor = value; }
        }

        [Category("ProgressBar"), Description("渐变终点颜色 (HEX)")]
        public string EndColor
        {
            get { return _wpfControl != null ? _wpfControl.EndColor : ""; }
            set { if (_wpfControl != null) _wpfControl.EndColor = value; }
        }

        [Category("ProgressBar"), Description("渐变起点颜色 (数字)")]
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

        [Category("ProgressBar"), Description("渐变终点颜色 (数字)")]
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

        #endregion

        #region 方法

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
        #endregion

        public ProgressBarPanel()
        {
            this.BackColor = System.Drawing.Color.Transparent;

            _wpfControl = new ProgressBarControl();
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
