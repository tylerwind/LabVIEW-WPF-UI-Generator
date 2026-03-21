using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Runtime.InteropServices;

namespace WpfChart
{
    /// <summary>
    /// 带有动态多线平滑贝塞尔算法及渐变样式的进阶图表控件
    /// </summary>
    [ComVisible(true)]
    [ToolboxItem(true)]
    [Description("带有动态多线平滑贝塞尔算法及渐变样式的进阶图表控件")]
    public class ChartPanel : Panel
    {
        private ElementHost _host;
        private ChartControl _wpfControl;

        public ChartPanel()
        {
            try {
                this.BackColor = ColorTranslator.FromHtml("{{ControlBackground}}");
            } catch {
                this.BackColor = Color.White;
            }

            _host = new ElementHost
            {
                Dock = DockStyle.Fill,
                BackColorTransparent = true
            };

            _wpfControl = new ChartControl();
            _host.Child = _wpfControl;

            this.Controls.Add(_host);
            this.SizeChanged += delegate { if (_host != null) _host.Invalidate(); };
        }

        #region 给 LabVIEW 或外部代码暴露的属性与方法

        [Category("Appearance")]
        public string LabelText
        {
            get { return _wpfControl.LabelText; }
            set { _wpfControl.LabelText = value; }
        }

        [Category("Appearance")]
        public string DescText
        {
            get { return _wpfControl.DescText; }
            set { _wpfControl.DescText = value; }
        }

        public void SetLabelVisible(bool visible)
        {
            _wpfControl.SetLabelVisible(visible);
        }

        [Category("Axis")]
        public double YMin
        {
            get { return _wpfControl.YMin; }
            set { _wpfControl.YMin = value; }
        }

        [Category("Axis")]
        public double YMax
        {
            get { return _wpfControl.YMax; }
            set { _wpfControl.YMax = value; }
        }

        [Category("Axis")]
        public bool AutoScaleY
        {
            get { return _wpfControl.AutoScaleY; }
            set { _wpfControl.AutoScaleY = value; }
        }

        [Category("Axis")]
        public bool IsXAxisVisible
        {
            get { return _wpfControl.IsXAxisVisible; }
            set { _wpfControl.IsXAxisVisible = value; }
        }

        [Category("Axis")]
        public bool IsYAxisVisible
        {
            get { return _wpfControl.IsYAxisVisible; }
            set { _wpfControl.IsYAxisVisible = value; }
        }

        [Category("Series")]
        public double LineThickness
        {
            get { return _wpfControl.LineThickness; }
            set { _wpfControl.LineThickness = value; }
        }

        [Category("Series")]
        public double FillOpacity
        {
            get { return _wpfControl.FillOpacity; }
            set { _wpfControl.FillOpacity = value; }
        }

        [Category("Series")]
        [Description("图表显示的最大数据点数（滑动窗口大小）")]
        public int MaxPoints
        {
            get { return _wpfControl.MaxPoints; }
            set { _wpfControl.MaxPoints = value; }
        }

        [Category("Appearance")]
        [Description("是否显示网格线")]
        public bool ShowGridLines { get { return _wpfControl.ShowGridLines; } set { _wpfControl.ShowGridLines = value; } }

        [Category("Appearance")]
        [Description("是否显示图例")]
        public bool ShowLegends
        {
            get { return _wpfControl.ShowLegends; }
            set { _wpfControl.ShowLegends = value; }
        }

        [Category("Appearance")]
        [Description("是否显示左侧数值卡片")]
        public bool ShowSeriesCards
        {
            get { return _wpfControl.ShowSeriesCards; }
            set { _wpfControl.ShowSeriesCards = value; }
        }

        /// <summary>
        /// 清空所有图表数据序列
        /// </summary>
        public void ClearSeries()
        {
            _wpfControl.ClearSeries();
        }

        /// <summary>
        /// 增加一条新的数据曲线 (使用 .NET Color 对象)
        /// </summary>
        public void AddSeries(string title, double[] data, Color lineColor, Color fillColor)
        {
            System.Windows.Media.Color wpLineColor = System.Windows.Media.Color.FromArgb(lineColor.A, lineColor.R, lineColor.G, lineColor.B);
            System.Windows.Media.Color wpFillColor = System.Windows.Media.Color.FromArgb(fillColor.A, fillColor.R, fillColor.G, fillColor.B);
            _wpfControl.AddSeries(title, data, wpLineColor, wpFillColor);
        }

        /// <summary>
        /// 增加一条新的数据曲线 (使用 HEX 字符串，如 "#FF0000")
        /// </summary>
        public void AddSeries(string title, double[] data, string lineColorHex, string fillColorHex)
        {
            try {
                if (string.IsNullOrEmpty(lineColorHex)) lineColorHex = "#000000"; // 默认黑色
                if (string.IsNullOrEmpty(fillColorHex)) fillColorHex = "#00000000"; // 默认透明
                
                Color cLine = ColorTranslator.FromHtml(lineColorHex);
                Color cFill = string.IsNullOrEmpty(fillColorHex) ? Color.FromArgb(40, cLine) : ColorTranslator.FromHtml(fillColorHex);
                AddSeries(title, data, cLine, cFill);
            } catch {
                AddSeries(title, data, Color.Black, Color.Transparent);
            }
        }

        /// <summary>
        /// 增加一条新的数据曲线 (使用 LabVIEW 颜色整数 U32/I32)
        /// </summary>
        public void AddSeries(string title, double[] data, int lineColorI32, int fillColorI32)
        {
            // 如果输入全为0且未明确颜色，默认黑色
            if (lineColorI32 == 0 && fillColorI32 == 0) lineColorI32 = 0x000000;

            Color cLine = Color.FromArgb(unchecked((int)((uint)lineColorI32 | 0xFF000000)));
            Color cFill;
            
            if (fillColorI32 == 0) {
                 cFill = Color.FromArgb(40, cLine); // 默认半透明
            } else {
                 cFill = Color.FromArgb(100, Color.FromArgb(unchecked((int)((uint)fillColorI32 | 0xFF000000))));
            }

            AddSeries(title, data, cLine, cFill);
        }

        /// <summary>
        /// 设置X轴文本标签数组 (如月份)
        /// </summary>
        public void SetXLabels(string[] labels)
        {
            _wpfControl.SetXLabels(labels);
        }

        /// <summary>
        /// 追加单个数据点到指定标题的序列
        /// </summary>
        public void AppendPoint(string title, double value)
        {
            _wpfControl.AppendData(title, value);
        }

        /// <summary>
        /// 批量追加多个数据点到指定标题的序列
        /// </summary>
        public void AppendPoints(string title, double[] values)
        {
            if (values == null) return;
            foreach (var v in values)
            {
                _wpfControl.AppendData(title, v);
            }
        }

        /// <summary>
        /// 使用并行数组设置所有曲线（名称数组 + 颜色数组）
        /// 这种方式可以直接在 LabVIEW 中接线，避开 .NET 引用。
        /// </summary>
        public void SetupSeries(string[] labels, int[] colors)
        {
            if (labels == null || labels.Length == 0) return;

            _wpfControl.ClearSeries();
            for (int i = 0; i < labels.Length; i++)
            {
                string label = labels[i];
                if (string.IsNullOrEmpty(label)) continue;

                int colorVal = (colors != null && i < colors.Length) ? colors[i] : 0x000000;
                
                // 调用 I32 颜色版本的 AddSeries，以便触发自动半透明填充逻辑
                AddSeries(label, null, colorVal, 0);
            }
        }

        /// <summary>
        /// 批量追加一组数值到所有曲线（数组顺序需与 SetupSeries 定义顺序一致）
        /// </summary>
        public void AppendBatch(double[] values)
        {
            _wpfControl.AppendBatch(values);
        }

        /// <summary>
        /// 一次性加载所有曲线的历史数据 (2D 数组：行代表曲线，列代表时间点)
        /// </summary>
        public void SetAllData(double[,] data2D)
        {
            _wpfControl.SetAllData(data2D);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing && _host != null) _host.Dispose();
            base.Dispose(disposing);
        }
    }
}
