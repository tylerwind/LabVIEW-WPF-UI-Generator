using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WpfPie
{
    [ComVisible(true)]
    [ToolboxItem(true)]
    [Description("环形饼图控件，支持卡片数据展示")]
    public class PiePanel : Panel
    {
        private readonly ElementHost _host;
        private readonly PieControl _wpfControl;

        public PiePanel()
        {
            try { this.BackColor = ColorTranslator.FromHtml("{{ControlBackground}}"); }
            catch { this.BackColor = Color.White; }

            _host = new ElementHost { Dock = DockStyle.Fill, BackColorTransparent = true };
            _wpfControl = new PieControl();
            _host.Child = _wpfControl;
            Controls.Add(_host);
        }

        [Category("Appearance")]
        public string LabelText { get { return _wpfControl.LabelText; } set { _wpfControl.LabelText = value; } }

        [Category("Appearance")]
        public string DescText { get { return _wpfControl.DescText; } set { _wpfControl.DescText = value; } }

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

        /// <summary>
        /// 设置描述文字 (UTF8 字节流方案，解决乱码)
        /// </summary>
        public void SetDescTextUTF8(byte[] bytes)
        {
            if (bytes == null) return;
            try { DescText = System.Text.Encoding.UTF8.GetString(bytes); } catch { }
        }

        /// <summary>
        /// 增加扇区 (UTF8 字节流方案，解决乱码)
        /// </summary>
        public void AddSeriesUTF8(byte[] titleBytes, double value, int colorI32)
        {
            if (titleBytes == null) return;
            try { AddSeries(System.Text.Encoding.UTF8.GetString(titleBytes), value, colorI32); } catch { }
        }

        /// <summary>
        /// 修改特定扇区值 (UTF8 字节流方案，解决乱码)
        /// </summary>
        public void SetValueUTF8(byte[] titleBytes, double value)
        {
            if (titleBytes == null) return;
            try { SetValue(System.Text.Encoding.UTF8.GetString(titleBytes), value); } catch { }
        }
        

        [Category("Appearance")]
        public bool ShowSeriesCards { get { return _wpfControl.ShowSeriesCards; } set { _wpfControl.ShowSeriesCards = value; } }

        [Browsable(true)]
        [Category("Data"), Description("获取所有系列名称")]
        public string[] SeriesNames { get { return _wpfControl.GetAllTitles(); } }

        [Browsable(true)]
        [Category("Data"), Description("获取所有系列数值")]
        public double[] SeriesValues { get { return _wpfControl.GetAllValues(); } }

        public void ClearSeries() { _wpfControl.ClearSeries(); }

        public void AddSeries(string title, double value, int colorI32)
        {
            var c = Color.FromArgb(unchecked((int)((uint)colorI32 | 0xFF000000)));
            var media = System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
            _wpfControl.AddSeries(title, value, media);
        }

        public void SetSeries(string[] titles, double[] values, int[] colors)
        {
            _wpfControl.SetSeries(titles, values, colors);
        }

        public void SetValue(string title, double value)
        {
            _wpfControl.SetValue(title, value);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _host.Dispose();
            base.Dispose(disposing);
        }
    }
}