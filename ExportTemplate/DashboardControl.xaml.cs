using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WpfDashboard
{
    public class ChartSeries
    {
        public string Name { get; set; }
        public Color Color { get; set; }
        public double StrokeThickness { get; set; }
        public double[] Data { get; set; }
    }

    public partial class DashboardControl : UserControl
    {
        private ChartSeries[] _series = new ChartSeries[0];
        private Polyline[] _seriesLines = new Polyline[0];
        private PointCollection[] _seriesPoints = new PointCollection[0];
        private double _yMin = 0;
        private double _yMax = 100;

        public DashboardControl()
        {
            InitializeComponent();
        }

        public string PanelTitle
        {
            get { return LabelBlock.Text; }
            set { LabelBlock.Text = value; }
        }

        /// <summary>
        /// 更新图表数据
        /// </summary>
        public void UpdateChartData(double[] data)
        {
            if (_series == null || _series.Length == 0)
            {
                SetSeries(new[] { new ChartSeries { Name = "Series", Color = (Color)ColorConverter.ConvertFromString("{{AccentColor}}"), StrokeThickness = 2, Data = data ?? new double[0] } });
                return;
            }

            UpdateSeriesData(0, data);
        }

        public void SetNumericValue(int index, double value, string format)
        {
            string s = string.IsNullOrEmpty(format) ? value.ToString() : value.ToString(format);
        }

        public void SetSeries(ChartSeries[] series)
        {
            _series = series ?? new ChartSeries[0];
            _seriesPoints = new PointCollection[_series.Length];
            _seriesLines = new Polyline[_series.Length];

            ChartAreaCanvas.Children.Clear();

            for (int i = 0; i < _series.Length; i++)
            {
                var line = new Polyline
                {
                    Stroke = new SolidColorBrush(_series[i].Color),
                    StrokeThickness = Math.Max(1, _series[i].StrokeThickness),
                    StrokeLineJoin = PenLineJoin.Round
                };
                _seriesLines[i] = line;
                ChartAreaCanvas.Children.Add(line);
            }

            RedrawChart();
        }

        public void UpdateSeriesData(int seriesIndex, double[] data)
        {
            if (_series == null || seriesIndex < 0 || seriesIndex >= _series.Length) return;
            _series[seriesIndex].Data = data ?? new double[0];
            RedrawChart();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawChart();
        }

        private void RedrawChart()
        {
            if (ChartAreaCanvas.ActualWidth <= 0 || ChartAreaCanvas.ActualHeight <= 0) return;
            if (_series == null || _series.Length == 0) return;

            var allValues = new List<double>();
            for (int i = 0; i < _series.Length; i++)
            {
                if (_series[i].Data != null) allValues.AddRange(_series[i].Data);
            }

            if (allValues.Count < 2) return;

            _yMin = allValues.Min();
            _yMax = allValues.Max();
            double span = _yMax - _yMin;
            if (span == 0) span = 1;
            _yMax += span * 0.1;
            _yMin -= span * 0.1;

            double width = ChartAreaCanvas.ActualWidth;
            double height = ChartAreaCanvas.ActualHeight;

            for (int i = 0; i < _series.Length; i++)
            {
                var data = _series[i].Data ?? new double[0];
                if (data.Length < 2)
                {
                    if (_seriesLines.Length > i) _seriesLines[i].Points = new PointCollection();
                    continue;
                }

                var pts = new PointCollection();
                for (int j = 0; j < data.Length; j++)
                {
                    double x = (width / (data.Length - 1)) * j;
                    double normalizedY = (data[j] - _yMin) / span;
                    double y = height - (normalizedY * height);
                    y = Math.Max(0, Math.Min(height, y));
                    pts.Add(new Point(x, y));
                }

                if (_seriesLines.Length > i)
                {
                    _seriesLines[i].Stroke = new SolidColorBrush(_series[i].Color);
                    _seriesLines[i].StrokeThickness = Math.Max(1, _series[i].StrokeThickness);
                    _seriesLines[i].Points = pts;
                }
            }
        }
    }
}
