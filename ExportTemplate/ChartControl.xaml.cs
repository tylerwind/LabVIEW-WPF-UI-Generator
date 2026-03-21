using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
// 移除歧义引用，改为在代码中使用全路径

namespace WpfChart
{
    [ComVisible(true)]
    public class ChartSeries
    {
        public string Title { get; set; }
        public List<double> Data { get; set; }
        public Color LineColor { get; set; }
        public Color FillStartColor { get; set; }
        
        // Cache internal WPF shapes
        public Path LinePath { get; set; }
        public Path FillPath { get; set; }
    }

    [ComVisible(true)]
    public partial class ChartControl : UserControl
    {
        private List<ChartSeries> _seriesList = new List<ChartSeries>();
        private List<string> _xLabels = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug" };
        public double YMin { get { return _yMin; } set { _yMin = value; RedrawChart(); } }
        public double YMax { get { return _yMax; } set { _yMax = value; RedrawChart(); } }
        public bool AutoScaleY { get { return _autoScaleY; } set { _autoScaleY = value; RecalculateScale(); RedrawChart(); } }
        public bool IsXAxisVisible { get { return _isXAxisVisible; } set { _isXAxisVisible = value; RedrawChart(); } }
        public bool IsYAxisVisible { get { return _isYAxisVisible; } set { _isYAxisVisible = value; RedrawChart(); } }
        public double LineThickness { get { return _lineThickness; } set { _lineThickness = value; RedrawChart(); } }
        public double FillOpacity { get { return _fillOpacity; } set { _fillOpacity = value; RedrawChart(); } }
        
        private double _yMin = 0;
        private double _yMax = 100;
        private bool _autoScaleY = true;
        private bool _isXAxisVisible = true;
        private bool _isYAxisVisible = true;
        private double _lineThickness = {{ChartLineWeight}};
        private double _fillOpacity = {{ChartFillOpacity}};
        private int _renderMode = 0; // 0:Smooth, 1:Linear, 2:Step
        private int _maxPoints = 100; // 滑动窗口点数限制

        public int MaxPoints { get { return _maxPoints; } set { _maxPoints = value; RedrawChart(); } }
        public bool ShowGridLines { get { return _showGridLines; } set { _showGridLines = value; RedrawChart(); } }
        public bool ShowLegends
        {
            get { return _showLegends; }
            set
            {
                _showLegends = value;
                if (LegendStack != null)
                    LegendStack.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public bool ShowSeriesCards 
        { 
            get { return _showSeriesCards; } 
            set 
            { 
                _showSeriesCards = value; 
                if (SeriesCardHost != null)
                    SeriesCardHost.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                if (SeriesColumn != null)
                    SeriesColumn.Width = value ? GridLength.Auto : new GridLength(0);
                UpdateLayout();
                RedrawChart(); 
            } 
        }
        
        private bool _showGridLines = {{ChartShowGridLines}};
        private bool _showSeriesCards = {{ChartShowSeriesCards}};
        private bool _showLegends = true;

        public ChartControl()
        {
            InitializeComponent();
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
            TextOptions.SetTextHintingMode(this, TextHintingMode.Fixed);

            BtnSmooth.IsChecked = true;
            _renderMode = 0;
            BtnSmooth.Background = new SolidColorBrush(Color.FromArgb(40, 128, 128, 128));

            // Setup Demo Data for Visualizer Editor
            AddSeries("Product A", new double[] { 110, 150, 190, 130, 210, 260, 230, 280 }, (Color)ColorConverter.ConvertFromString("{{ChartColor1}}"), (Color)ColorConverter.ConvertFromString("{{ChartColor1}}"));
            AddSeries("Product B", new double[] { 90, 130, 150, 100, 180, 220, 200, 240 }, (Color)ColorConverter.ConvertFromString("{{ChartColor2}}"), (Color)ColorConverter.ConvertFromString("{{ChartColor2}}"));
            AddSeries("Product C", new double[] { 50, 80, 110, 60, 130, 170, 120, 160 }, (Color)ColorConverter.ConvertFromString("{{ChartColor3}}"), (Color)ColorConverter.ConvertFromString("{{ChartColor3}}"));
        }

        public string LabelText
        {
            get { return LabelBlock.Text; }
            set { LabelBlock.Text = value; }
        }

        public string DescText
        {
            get { return DescBlock.Text; }
            set { DescBlock.Text = value; }
        }

        public void SetLabelVisible(bool visible)
        {
            LabelBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            DescBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ClearSeries()
        {
            _seriesList.Clear();
            if(ChartAreaCanvas != null) ChartAreaCanvas.Children.Clear();
            if(LegendStack != null) LegendStack.Children.Clear();
            if(SeriesPanel != null) SeriesPanel.Children.Clear();
            RedrawChart();
        }

        public void AddSeries(string title, double[] data, Color lineColor, Color fillStartColor)
        {
            var existing = _seriesList.FirstOrDefault(s => s.Title == title);
            if (existing != null)
            {
                existing.Data = (data != null) ? new List<double>(data) : new List<double>();
                existing.LineColor = lineColor;
                existing.FillStartColor = fillStartColor;
                existing.LinePath.Stroke = new SolidColorBrush(lineColor);
                
                // 更新填充颜色逻辑
                {
                    byte alpha = (byte)(_fillOpacity * 255);
                    var fillCol = Color.FromArgb(alpha, fillStartColor.R, fillStartColor.G, fillStartColor.B);
                    var brush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1) };
                    brush.GradientStops.Add(new GradientStop(fillCol, 0));
                    brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1));
                    existing.FillPath.Fill = brush;
                }

                RecalculateScale();
                RedrawChart();
                return;
            }

            var series = new ChartSeries
            {
                Title = title,
                Data = (data != null) ? new List<double>(data) : new List<double>(),
                LineColor = lineColor,
                FillStartColor = fillStartColor
            };

            // Init Shapes
            {
                // 应用当前透明度
                byte alpha = (byte)(_fillOpacity * 255);
                var fillCol = Color.FromArgb(alpha, series.FillStartColor.R, series.FillStartColor.G, series.FillStartColor.B);
                
                series.FillPath = new Path { StrokeThickness = 0, Opacity = 1.0 }; // Opacity 设为 1.0, 使用 Alpha 通道控制
                var brush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1) };
                brush.GradientStops.Add(new GradientStop(fillCol, 0));
                brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1));
                series.FillPath.Fill = brush;
            }

            series.LinePath = new Path
            {
                Stroke = new SolidColorBrush(series.LineColor),
                StrokeThickness = _lineThickness,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            series.LinePath.Effect = new DropShadowEffect { Color = series.LineColor, BlurRadius = 8, ShadowDepth = 2, Direction = 270, Opacity = 0.5 };

            _seriesList.Add(series);

            if (ChartAreaCanvas != null)
            {
                ChartAreaCanvas.Children.Add(series.FillPath);
                ChartAreaCanvas.Children.Add(series.LinePath);
            }

            BuildLegends();
            RecalculateScale();
            RedrawChart();
        }

        public void AppendData(string title, double value)
        {
            var series = _seriesList.FirstOrDefault(s => s.Title == title);
            if (series == null) return;

            series.Data.Add(value);
            
            // 应用滑动窗口
            if (series.Data.Count > _maxPoints)
            {
                series.Data.RemoveAt(0);
            }

            RecalculateScale();
            RedrawChart();
        }

        public void AppendBatch(double[] values)
        {
            if (values == null || values.Length == 0) return;

            for (int i = 0; i < Math.Min(values.Length, _seriesList.Count); i++)
            {
                var series = _seriesList[i];
                series.Data.Add(values[i]);
                
                if (series.Data.Count > _maxPoints)
                {
                    series.Data.RemoveAt(0);
                }
            }

            RecalculateScale();
            RedrawChart();
        }

        public void SetAllData(double[,] data2D)
        {
            if (data2D == null) return;

            int rows = data2D.GetLength(0);
            int cols = data2D.GetLength(1);

            for (int i = 0; i < Math.Min(rows, _seriesList.Count); i++)
            {
                var series = _seriesList[i];
                series.Data.Clear();
                for (int j = 0; j < cols; j++)
                {
                    series.Data.Add(data2D[i, j]);
                }
            }

            RecalculateScale();
            RedrawChart();
        }

        private void BuildLegends()
        {
            if (LegendStack == null) return;
            LegendStack.Children.Clear();
            if (SeriesPanel != null) SeriesPanel.Children.Clear();

            StackPanel innerPanel = null;
            if (SeriesPanel != null)
            {
                innerPanel = new StackPanel { Orientation = Orientation.Vertical };
                Grid.SetIsSharedSizeScope(innerPanel, true);
                SeriesPanel.Children.Add(innerPanel);
            }

            foreach (var s in _seriesList)
            {
                var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 16, 0), VerticalAlignment = VerticalAlignment.Center };
                var symbol = new Border { Width = 12, Height = 12, CornerRadius = new CornerRadius(3), Background = new SolidColorBrush(s.LineColor), Margin = new Thickness(0, 0, 8, 0), Cursor = System.Windows.Input.Cursors.Hand };
                
                // 添加点击更换颜色功能
                symbol.MouseDown += (s_sender, e_args) => {
                    var dlg = new System.Windows.Forms.ColorDialog();
                    dlg.FullOpen = true;
                    // 使用显式命名空间以避免歧义
                    dlg.Color = System.Drawing.Color.FromArgb(s.LineColor.A, s.LineColor.R, s.LineColor.G, s.LineColor.B);
                    
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                        var c = dlg.Color;
                        var newMediaColor = System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
                        s.LineColor = newMediaColor;
                        s.FillStartColor = newMediaColor; // 同步更新填充色
                        
                        // 更新形状样式
                        s.LinePath.Stroke = new SolidColorBrush(newMediaColor);
                        var brush = new LinearGradientBrush { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                        
                        // 使用当前设置的 FillOpacity
                        byte alpha = (byte)(_fillOpacity * 255);
                        var fillCol = System.Windows.Media.Color.FromArgb(alpha, newMediaColor.R, newMediaColor.G, newMediaColor.B);
                        
                        brush.GradientStops.Add(new GradientStop(fillCol, 0));
                        brush.GradientStops.Add(new GradientStop(System.Windows.Media.Colors.Transparent, 1));
                        s.FillPath.Fill = brush;
                        
                        // 更新色块自身
                        symbol.Background = new SolidColorBrush(newMediaColor);
                        
                        // 如果有阴影效果也同步更新
                        if (s.LinePath.Effect is DropShadowEffect) {
                             ((DropShadowEffect)s.LinePath.Effect).Color = newMediaColor;
                        }

                        UpdateSeriesCardColor(s, newMediaColor);
                    }
                };

                var text = new TextBlock { Text = s.Title, FontFamily = DescBlock.FontFamily, FontSize = DescBlock.FontSize, Foreground = LabelBlock.Foreground, VerticalAlignment = VerticalAlignment.Center };
                
                panel.Children.Add(symbol);
                panel.Children.Add(text);
                LegendStack.Children.Add(panel);

                if (innerPanel != null)
                {
                    BuildSeriesRow(s, innerPanel);
                }
            }
        }

        private void BuildSeriesRow(ChartSeries series, StackPanel parentPanel)
        {
            var grid = new Grid { Margin = new Thickness(0, 6, 0, 6) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, SharedSizeGroup = "ColorCol" });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(64) });
            grid.MaxWidth = 200;

            var title = new TextBlock
            {
                Text = series.Title,
                FontFamily = DescBlock.FontFamily,
                FontSize = DescBlock.FontSize,
                Foreground = DescBlock.Foreground,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = 150
            };
            Grid.SetColumn(title, 0);

            var colorSwatch = new Border
            {
                Width = 10,
                Height = 10,
                CornerRadius = new CornerRadius(5),
                Background = new SolidColorBrush(series.LineColor),
                Margin = new Thickness(12, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var value = new TextBlock
            {
                Text = GetSeriesLatestText(series),
                FontFamily = LabelBlock.FontFamily,
                FontSize = LabelBlock.FontSize,
                FontWeight = FontWeights.SemiBold,
                Foreground = LabelBlock.Foreground,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Right,
                MinWidth = 56
            };

            colorSwatch.MouseDown += (sender, args) =>
            {
                var dlg = new System.Windows.Forms.ColorDialog();
                dlg.FullOpen = true;
                dlg.Color = System.Drawing.Color.FromArgb(series.LineColor.A, series.LineColor.R, series.LineColor.G, series.LineColor.B);

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var c = dlg.Color;
                    var newMediaColor = System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
                    series.LineColor = newMediaColor;
                    series.FillStartColor = newMediaColor;

                    series.LinePath.Stroke = new SolidColorBrush(newMediaColor);
                    var brush = new LinearGradientBrush { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                    byte alpha = (byte)(_fillOpacity * 255);
                    var fillCol = System.Windows.Media.Color.FromArgb(alpha, newMediaColor.R, newMediaColor.G, newMediaColor.B);
                    brush.GradientStops.Add(new GradientStop(fillCol, 0));
                    brush.GradientStops.Add(new GradientStop(System.Windows.Media.Colors.Transparent, 1));
                    series.FillPath.Fill = brush;

                    if (series.LinePath.Effect is DropShadowEffect)
                    {
                        ((DropShadowEffect)series.LinePath.Effect).Color = newMediaColor;
                    }

                    colorSwatch.Background = new SolidColorBrush(newMediaColor);
                    UpdateSeriesCardColor(series, newMediaColor);
                }
            };

            Grid.SetColumn(colorSwatch, 1);
            Grid.SetColumn(value, 2);

            grid.Children.Add(title);
            grid.Children.Add(colorSwatch);
            grid.Children.Add(value);

            parentPanel.Children.Add(grid);

            grid.Tag = new SeriesCardRefs
            {
                Series = series,
                ValueBlock = value,
                ColorSwatch = colorSwatch
            };
        }

        private string GetSeriesLatestText(ChartSeries series)
        {
            if (series.Data == null || series.Data.Count == 0) return "--";
            return series.Data[series.Data.Count - 1].ToString("0.##");
        }

        private void UpdateSeriesCardsValue()
        {
            if (SeriesPanel == null || SeriesPanel.Children.Count == 0) return;

            var stack = SeriesPanel.Children[0] as StackPanel;
            if (stack == null) return;

            foreach (var child in stack.Children)
            {
                var grid = child as Grid;
                if (grid == null) continue;

                var refs = grid.Tag as SeriesCardRefs;
                if (refs == null || refs.Series == null || refs.ValueBlock == null) continue;

                refs.ValueBlock.Text = GetSeriesLatestText(refs.Series);
            }
        }

        private void UpdateSeriesCardColor(ChartSeries series, Color newColor)
        {
            if (SeriesPanel == null || SeriesPanel.Children.Count == 0) return;

            var stack = SeriesPanel.Children[0] as StackPanel;
            if (stack == null) return;

            foreach (var child in stack.Children)
            {
                var grid = child as Grid;
                if (grid == null) continue;

                var refs = grid.Tag as SeriesCardRefs;
                if (refs == null || refs.Series != series || refs.ColorSwatch == null) continue;

                refs.ColorSwatch.Background = new SolidColorBrush(newColor);
                break;
            }
            
            if (LegendStack != null)
            {
                foreach (var child in LegendStack.Children)
                {
                    var panel = child as StackPanel;
                    if (panel == null || panel.Children.Count == 0) continue;

                    var symbol = panel.Children[0] as Border;
                    if (symbol == null) continue;

                    var textBlock = panel.Children.Count > 1 ? panel.Children[1] as TextBlock : null;
                    if (textBlock != null && textBlock.Text == series.Title)
                    {
                        symbol.Background = new SolidColorBrush(newColor);
                        break;
                    }
                }
            }
        }

        private class SeriesCardRefs
        {
            public ChartSeries Series { get; set; }
            public TextBlock ValueBlock { get; set; }
            public Border ColorSwatch { get; set; }
        }


        private void RecalculateScale()
        {
            if (!_autoScaleY) return;

            var allValues = _seriesList.SelectMany(s => s.Data).ToList();
            if (allValues.Count > 0)
            {
                _yMin = allValues.Min();
                _yMax = allValues.Max();
                double span = _yMax - _yMin;
                if (span == 0) span = 1;
                _yMax += span * 0.1;
                _yMin -= span * 0.1;
                _yMin = Math.Max(0, _yMin);
            }
        }

        public void SetXLabels(string[] labels)
        {
            if (labels != null) _xLabels = labels.ToList();
            RedrawChart();
        }

        private void BtnSmooth_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (BtnSmooth == null) return;
            
            // 循环切换模式: 0(Smooth) -> 1(Linear) -> 2(Step)
            _renderMode = (_renderMode + 1) % 3;
            
            // 更新按钮文字
            switch(_renderMode)
            {
                case 0: BtnSmooth.Content = "Smooth"; break;
                case 1: BtnSmooth.Content = "Linear"; break;
                case 2: BtnSmooth.Content = "Step"; break;
            }
            
            // 保持选中状态背景色
            BtnSmooth.Background = new SolidColorBrush(Color.FromArgb(40, 128, 128, 128));
            
            RedrawChart();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawChart();
        }

        private void RedrawChart()
        {
            if (ChartAreaCanvas == null || ChartAreaCanvas.ActualWidth <= 0 || ChartAreaCanvas.ActualHeight <= 0) return;

            foreach (var series in _seriesList)
            {
                series.LinePath.StrokeThickness = _lineThickness;
                
                // 动态更新填充透明度
                byte alpha = (byte)(_fillOpacity * 255);
                var fillCol = Color.FromArgb(alpha, series.FillStartColor.R, series.FillStartColor.G, series.FillStartColor.B);
                
                var brush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1) };
                brush.GradientStops.Add(new GradientStop(fillCol, 0));
                brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1));
                series.FillPath.Fill = brush;

                RenderSeries(series);
            }
            
            RenderGridLines();
            
            RenderXLabels();
            RenderYLabels();
            UpdateSeriesCardsValue();
        }

        private void RenderGridLines()
        {
            if (GridCanvas == null) return;
            GridCanvas.Children.Clear(); // 清理旧线条，修复“奇怪色块”堆叠问题
            
            if (!_showGridLines) return;

            double width = GridCanvas.ActualWidth;
            double height = GridCanvas.ActualHeight;
            
            // 使用辅助线颜色
            var gridBrush = new SolidColorBrush(Color.FromArgb(40, 128, 128, 128));
            
            // 水平网格线 (对应 Y 轴刻度)
            int yCount = 5;
            for (int i = 0; i < yCount; i++)
            {
                double y = height - (height * i / (yCount - 1));
                var line = new Line { X1 = 0, X2 = width, Y1 = y, Y2 = y, Stroke = gridBrush, StrokeThickness = 0.5, StrokeDashCap = PenLineCap.Round };
                line.StrokeDashArray = new DoubleCollection { 4, 4 };
                GridCanvas.Children.Add(line); 
            }

            // 垂直网格线
            int xCount = _xLabels.Count > 1 ? _xLabels.Count : 5;
            for (int i = 0; i < xCount; i++)
            {
                double x = (width / (xCount - 1)) * i;
                var line = new Line { X1 = x, X2 = x, Y1 = 0, Y2 = height, Stroke = gridBrush, StrokeThickness = 0.5 };
                line.StrokeDashArray = new DoubleCollection { 4, 4 };
                GridCanvas.Children.Add(line);
            }
        }

        private void RenderSeries(ChartSeries series)
        {
            if (series.Data == null || series.Data.Count < 2)
            {
                series.FillPath.Data = null;
                series.LinePath.Data = null;
                return;
            }

            double width = ChartAreaCanvas.ActualWidth;
            double height = ChartAreaCanvas.ActualHeight;
            double span = _yMax - _yMin;
            if (span == 0) span = 1;

            var points = new List<Point>();
            for (int i = 0; i < series.Data.Count; i++)
            {
                double x = (width / (series.Data.Count - 1)) * i;
                double normalizedY = (series.Data[i] - _yMin) / span;
                double y = height - (normalizedY * height);
                y = Math.Max(0, Math.Min(height, y));
                points.Add(new Point(x, y));
            }

            if (_renderMode == 0)
            {
                series.FillPath.Data = CreateCurve(points, true, height);
                series.LinePath.Data = CreateCurve(points, false, height);
            }
            else if (_renderMode == 1)
            {
                series.FillPath.Data = CreateStraight(points, true, height);
                series.LinePath.Data = CreateStraight(points, false, height);
            }
            else
            {
                series.FillPath.Data = CreateStep(points, true, height);
                series.LinePath.Data = CreateStep(points, false, height);
            }
        }

        private PathGeometry CreateStraight(List<Point> points, bool isFill, double height)
        {
            var geo = new PathGeometry();
            var fig = new PathFigure { StartPoint = isFill ? new Point(points[0].X, height) : points[0] };
            if (isFill) fig.Segments.Add(new LineSegment(points[0], true));

            for (int i = 1; i < points.Count; i++)
            {
                fig.Segments.Add(new LineSegment(points[i], true));
            }

            if (isFill)
            {
                fig.Segments.Add(new LineSegment(new Point(points[points.Count - 1].X, height), true));
                fig.Segments.Add(new LineSegment(new Point(points[0].X, height), true));
                fig.IsClosed = true;
            }
            geo.Figures.Add(fig);
            return geo;
        }

        private PathGeometry CreateStep(List<Point> points, bool isFill, double height)
        {
            var geo = new PathGeometry();
            var fig = new PathFigure { StartPoint = isFill ? new Point(points[0].X, height) : points[0] };
            if (isFill) fig.Segments.Add(new LineSegment(points[0], true));

            for (int i = 0; i < points.Count - 1; i++)
            {
                // Step logic: move horizontal then vertical
                Point mid = new Point(points[i + 1].X, points[i].Y);
                fig.Segments.Add(new LineSegment(mid, true));
                fig.Segments.Add(new LineSegment(points[i + 1], true));
            }

            if (isFill)
            {
                fig.Segments.Add(new LineSegment(new Point(points[points.Count - 1].X, height), true));
                fig.Segments.Add(new LineSegment(new Point(points[0].X, height), true));
                fig.IsClosed = true;
            }
            geo.Figures.Add(fig);
            return geo;
        }

        private PathGeometry CreateCurve(List<Point> points, bool isFill, double height)
        {
            var geo = new PathGeometry();
            var fig = new PathFigure { StartPoint = isFill ? new Point(points[0].X, height) : points[0] };
            if (isFill) fig.Segments.Add(new LineSegment(points[0], true));

            Point[] pt = points.ToArray();
            for (int i = 0; i < pt.Length - 1; i++)
            {
                Point p0 = (i > 0) ? pt[i - 1] : pt[0];
                Point p1 = pt[i];
                Point p2 = pt[i + 1];
                Point p3 = (i < pt.Length - 2) ? pt[i + 2] : p2;

                double t = 0.2; 
                Point cp1 = new Point(p1.X + (p2.X - p0.X) * t, p1.Y + (p2.Y - p0.Y) * t);
                Point cp2 = new Point(p2.X - (p3.X - p1.X) * t, p2.Y - (p3.Y - p1.Y) * t);

                cp1.Y = Math.Max(0, Math.Min(height, cp1.Y));
                cp2.Y = Math.Max(0, Math.Min(height, cp2.Y));

                fig.Segments.Add(new BezierSegment(cp1, cp2, p2, true));
            }

            if (isFill)
            {
                fig.Segments.Add(new LineSegment(new Point(pt[pt.Length - 1].X, height), true));
                fig.Segments.Add(new LineSegment(new Point(pt[0].X, height), true));
                fig.IsClosed = true;
            }
            geo.Figures.Add(fig);
            return geo;
        }

        private void RenderXLabels()
        {
            if (XAxisCanvas == null) return;
            XAxisCanvas.Children.Clear();
            XAxisCanvas.Visibility = _isXAxisVisible ? Visibility.Visible : Visibility.Collapsed;
            if (!_isXAxisVisible || _xLabels == null || _xLabels.Count == 0) return;
            
            double width = ChartAreaCanvas.ActualWidth;
            for (int i = 0; i < _xLabels.Count; i++)
            {
                var tb = new TextBlock
                {
                    Text = _xLabels[i],
                    Foreground = DescBlock.Foreground,
                    FontSize = DescBlock.FontSize,
                    FontFamily = DescBlock.FontFamily
                };
                
                double x = (width / (_xLabels.Count - 1)) * i;
                tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(tb, x - (tb.DesiredSize.Width / 2));
                Canvas.SetTop(tb, 0);
                
                XAxisCanvas.Children.Add(tb);
            }
        }

        private void RenderYLabels()
        {
            if (YAxisCanvas == null || ChartAreaCanvas == null) return;
            YAxisCanvas.Children.Clear();
            YAxisCanvas.Visibility = _isYAxisVisible ? Visibility.Visible : Visibility.Collapsed;
            if (!_isYAxisVisible) return;

            // 强制布局更新以确保获取正确的 ActualHeight
            this.UpdateLayout();

            double height = ChartAreaCanvas.ActualHeight;
            if (height <= 0) return;

            // 绘制 5 个刻度数值
            int count = 5;
            for (int i = 0; i < count; i++)
            {
                double val = _yMin + (_yMax - _yMin) * i / (count - 1);
                var tb = new TextBlock
                {
                    Text = val.ToString("0.#"),
                    Foreground = DescBlock.Foreground,
                    FontSize = DescBlock.FontSize - 2,
                    FontFamily = DescBlock.FontFamily,
                    TextAlignment = TextAlignment.Right,
                    Width = YAxisCanvas.ActualWidth - 6
                };

                double y = height - (height * i / (count - 1));
                Canvas.SetTop(tb, y - 7); 
                Canvas.SetLeft(tb, 0);
                YAxisCanvas.Children.Add(tb);
            }
        }
    }
}
