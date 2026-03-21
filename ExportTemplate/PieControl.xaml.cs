using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfPie
{
    public class PieSeries
    {
        public string Title { get; set; }
        public double Value { get; set; }
        public Color Color { get; set; }
    }

    public partial class PieControl : UserControl
    {
        private readonly List<PieSeries> _series = new List<PieSeries>();
        private bool _showSeriesCards = {{ChartShowSeriesCards}};

        public PieControl()
        {
            InitializeComponent();
            AddSeries("System A", 45, (Color)ColorConverter.ConvertFromString("{{ChartColor1}}"));
            AddSeries("System B", 30, (Color)ColorConverter.ConvertFromString("{{ChartColor2}}"));
            AddSeries("System C", 25, (Color)ColorConverter.ConvertFromString("{{ChartColor3}}"));
        }

        public string LabelText { get { return LabelBlock.Text; } set { LabelBlock.Text = value; } }
        public string DescText { get { return DescBlock.Text; } set { DescBlock.Text = value; } }
        public void SetLabelVisible(bool visible) { LabelBlock.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }

        public bool ShowSeriesCards
        {
            get { return _showSeriesCards; }
            set
            {
                _showSeriesCards = value;
                if (SeriesCardHost != null) SeriesCardHost.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                if (SeriesColumn != null) SeriesColumn.Width = value ? GridLength.Auto : new GridLength(0);
                UpdateCanvasSize();
                Redraw();
            }
        }

        public string[] GetAllTitles() { return _series.Select(s => s.Title).ToArray(); }
        public double[] GetAllValues() { return _series.Select(s => s.Value).ToArray(); }

        public void ClearSeries()
        {
            _series.Clear();
            Redraw();
        }

        public void AddSeries(string title, double value, Color color)
        {
            _series.Add(new PieSeries { Title = title, Value = Math.Max(0, value), Color = color });
            Redraw();
        }

        public void SetSeries(string[] titles, double[] values, int[] colors)
        {
            _series.Clear();
            if (titles == null || values == null) { Redraw(); return; }

            int n = Math.Min(titles.Length, values.Length);
            for (int i = 0; i < n; i++)
            {
                int c = (colors != null && i < colors.Length) ? colors[i] : 0x000000;
                var col = Color.FromArgb(unchecked((byte)255), unchecked((byte)((c >> 16) & 0xFF)), unchecked((byte)((c >> 8) & 0xFF)), unchecked((byte)(c & 0xFF)));
                _series.Add(new PieSeries { Title = titles[i], Value = Math.Max(0, values[i]), Color = col });
            }
            Redraw();
        }

        public void SetValue(string title, double value)
        {
            var s = _series.FirstOrDefault(x => x.Title == title);
            if (s != null) s.Value = Math.Max(0, value);
            Redraw();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCanvasSize();
            Redraw();
        }

        private void UpdateCanvasSize()
        {
            if (PieCanvas == null) return;
            // Available height for pie (minus header/padding estimation)
            double availH = this.ActualHeight - 60; 
            double availW = this.ActualWidth;
            if (ShowSeriesCards) availW -= 240; // account for card width and margins

            double size = Math.Min(availH, availW);
            if (size < 50) size = 50;

            PieCanvas.Width = size;
            PieCanvas.Height = size;
        }

        private void Redraw()
        {
            DrawPie();
            // DrawLegends(); // 移除多余图例绘制
            DrawCards();
        }

        private void DrawPie()
        {
            if (PieCanvas == null) return;
            PieCanvas.Children.Clear();
            if (_series.Count == 0) return;

            double total = _series.Sum(s => s.Value);
            if (total <= 0) return;

            double w = PieCanvas.ActualWidth;
            double h = PieCanvas.ActualHeight;
            if (w <= 0 || h <= 0) return;

            double radius = Math.Min(w, h) * 0.38;
            Point center = new Point(w / 2.0, h / 2.0);

            // 外部框架 (增加层次感)
            var bgGradient = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 1) };
            bgGradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("{{GradientStart}}"), 0));
            bgGradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("{{GradientMid}}"), 0.5));
            bgGradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("{{GradientEnd}}"), 1));

            var outerRim = new Ellipse { 
                Width = radius * 2 + 20, Height = radius * 2 + 20, 
                Fill = bgGradient, 
                Stroke = new SolidColorBrush(Color.FromArgb(15, 0,0,0)), 
                StrokeThickness = 1,
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 10, ShadowDepth = 3, Opacity = 0.15, Color = Colors.Gray } 
            };
            Canvas.SetLeft(outerRim, center.X - outerRim.Width / 2);
            Canvas.SetTop(outerRim, center.Y - outerRim.Height / 2);
            PieCanvas.Children.Add(outerRim);

            double start = -90;
            foreach (var s in _series)
            {
                double sweep = (s.Value / total) * 360.0;
                var path = BuildSlice(center, radius, start, sweep, s.Color);
                PieCanvas.Children.Add(path);
                start += sweep;
            }

            var hole = new Ellipse { Width = radius * 0.85, Height = radius * 0.85, Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("{{ChartPlotBackground}}")) };
            Canvas.SetLeft(hole, center.X - hole.Width / 2);
            Canvas.SetTop(hole, center.Y - hole.Height / 2);
            PieCanvas.Children.Add(hole);

            // 移除中心数值显示
            // var txt = new TextBlock { Text = total.ToString("0.##"), FontSize = LabelBlock.FontSize + 2, FontWeight = FontWeights.Bold, Foreground = LabelBlock.Foreground };
            // txt.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            // Canvas.SetLeft(txt, center.X - txt.DesiredSize.Width / 2);
            // Canvas.SetTop(txt, center.Y - txt.DesiredSize.Height / 2);
            // PieCanvas.Children.Add(txt);
            
        }

        private Path BuildSlice(Point center, double radius, double startAngle, double sweepAngle, Color color)
        {
            double startRad = startAngle * Math.PI / 180.0;
            double endRad = (startAngle + sweepAngle) * Math.PI / 180.0;
            var p1 = new Point(center.X + radius * Math.Cos(startRad), center.Y + radius * Math.Sin(startRad));
            var p2 = new Point(center.X + radius * Math.Cos(endRad), center.Y + radius * Math.Sin(endRad));
            bool large = sweepAngle > 180;

            var fig = new PathFigure { StartPoint = center, IsClosed = true };
            fig.Segments.Add(new LineSegment(p1, true));
            fig.Segments.Add(new ArcSegment(p2, new Size(radius, radius), 0, large, SweepDirection.Clockwise, true));
            fig.Segments.Add(new LineSegment(center, true));

            return new Path
            {
                Data = new PathGeometry(new[] { fig }),
                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                StrokeThickness = 1
            };
        }

        /*
        private void DrawLegends()
        {
            if (LegendStack == null) return;
            LegendStack.Children.Clear();
            foreach (var s in _series)
            {
                var p = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 16, 0) };
                p.Children.Add(new Border { Width = 12, Height = 12, CornerRadius = new CornerRadius(3), Background = new SolidColorBrush(s.Color), Margin = new Thickness(0, 0, 6, 0) });
                p.Children.Add(new TextBlock { Text = s.Title, Foreground = LabelBlock.Foreground, FontSize = DescBlock.FontSize });
                LegendStack.Children.Add(p);
            }
        }
        */

        private void DrawCards()
        {
            if (SeriesPanel == null) return;
            SeriesPanel.Children.Clear();
            double total = _series.Sum(s => s.Value);

            foreach (var s in _series)
            {
                var row = new Grid { Margin = new Thickness(0, 6, 0, 6) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, SharedSizeGroup = "ColorCol" });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.MaxWidth = 240;

                var t = new TextBlock 
                { 
                    Text = s.Title, 
                    Foreground = DescBlock.Foreground, 
                    FontSize = DescBlock.FontSize, 
                    VerticalAlignment = VerticalAlignment.Center, 
                    TextWrapping = TextWrapping.Wrap,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxWidth = 150
                };

                var dot = new Border 
                { 
                    Width = 10, Height = 10, 
                    CornerRadius = new CornerRadius(5), 
                    Background = new SolidColorBrush(s.Color), 
                    Margin = new Thickness(12, 0, 8, 0), 
                    VerticalAlignment = VerticalAlignment.Center,
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                // Store current item locally for lambda
                var currentS = s;

                // Add interactive color change
                dot.MouseDown += (sender, args) =>
                {
                    var dlg = new System.Windows.Forms.ColorDialog();
                    dlg.FullOpen = true;
                    dlg.Color = System.Drawing.Color.FromArgb(currentS.Color.A, currentS.Color.R, currentS.Color.G, currentS.Color.B);

                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        var c = dlg.Color;
                        var newMediaColor = System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
                        currentS.Color = newMediaColor;
                        Redraw(); // Redraw pie and legends to reflect new color
                    }
                };

                double pct = total <= 0 ? 0 : (currentS.Value / total * 100.0);
                var v = new TextBlock 
                { 
                    Text = currentS.Value.ToString("0.##") + "  (" + pct.ToString("0.#") + "%)", 
                    Foreground = LabelBlock.Foreground, 
                    FontSize = LabelBlock.FontSize, 
                    FontWeight = FontWeights.SemiBold, 
                    TextAlignment = TextAlignment.Right, 
                    VerticalAlignment = VerticalAlignment.Center,
                    MinWidth = 80
                };

                Grid.SetColumn(t, 0); Grid.SetColumn(dot, 1); Grid.SetColumn(v, 2);
                row.Children.Add(t); row.Children.Add(dot); row.Children.Add(v);
                SeriesPanel.Children.Add(row);
            }
        }
    }
}