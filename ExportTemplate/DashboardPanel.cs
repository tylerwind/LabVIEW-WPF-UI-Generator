using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WpfDashboard
{
    [ToolboxItem(true)]
    [Description("综合仪表盘（多曲线趋势图）")]
    public class DashboardPanel : WpfPanelBase
    {
        private ElementHost _host;
        private DashboardControl _wpfControl;

        public DashboardPanel()
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

            _wpfControl = new DashboardControl();
            _host.Child = _wpfControl;

            this.Controls.Add(_host);

            this.SizeChanged += delegate(object s, EventArgs e) { 
                if (_host != null) _host.Invalidate(); 
            };
        }

        [Category("Appearance")]
        public override string LabelText
        {
            get { return _wpfControl.PanelTitle; }
            set { _wpfControl.PanelTitle = value; }
        }

        [Category("Appearance")]
        public string PanelTitle
        {
            get { return _wpfControl.PanelTitle; }
            set { _wpfControl.PanelTitle = value; }
        }

        public void PlotChart(double[] data)
        {
            _wpfControl.UpdateChartData(data);
        }

        public void SetSeries(ChartSeries[] series)
        {
            _wpfControl.SetSeries(series);
        }

        public void UpdateSeriesData(int seriesIndex, double[] data)
        {
            _wpfControl.UpdateSeriesData(seriesIndex, data);
        }

        #region 运行时风格重绘

        public override void ApplyStyleDictionary(System.Collections.Generic.Dictionary<string, object> style)
        {
            base.ApplyStyleDictionary(style);
        }

        #endregion
    }
}
