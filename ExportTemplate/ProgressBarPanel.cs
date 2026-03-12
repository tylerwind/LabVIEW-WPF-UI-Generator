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
            get => _wpfControl?.LabelText ?? "";
            set { if (_wpfControl != null) _wpfControl.LabelText = value; }
        }

        [Category("ProgressBar"), Description("当前值")]
        public double Value
        {
            get => _wpfControl?.Value ?? 0;
            set { if (_wpfControl != null) _wpfControl.Value = value; }
        }

        [Category("ProgressBar"), Description("最小值")]
        public double Minimum
        {
            get => _wpfControl?.Minimum ?? 0;
            set { if (_wpfControl != null) _wpfControl.Minimum = value; }
        }

        [Category("ProgressBar"), Description("最大值")]
        public double Maximum
        {
            get => _wpfControl?.Maximum ?? 100;
            set { if (_wpfControl != null) _wpfControl.Maximum = value; }
        }

        [Category("ProgressBar"), Description("是否显示百分比")]
        public bool ShowPercentage
        {
            get => _wpfControl?.ShowPercentage ?? true;
            set { if (_wpfControl != null) _wpfControl.ShowPercentage = value; }
        }

        #endregion

        #region 方法

        public void SetLabelVisible(bool visible)
        {
            _wpfControl?.SetLabelVisible(visible);
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
            if (disposing) _host?.Dispose();
            base.Dispose(disposing);
        }
    }
}
