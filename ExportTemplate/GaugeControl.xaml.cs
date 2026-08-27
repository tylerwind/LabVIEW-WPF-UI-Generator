using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfGauge
{
    public partial class GaugeControl : UserControl
    {
        private double _min = 0;
        private double _max = 100;
        private double _value = 65;

        private Brush _currentControlBgBrush;
        private Brush _currentPlateGradientBrush;
        private Brush _currentBorderBrush;
        private Color _currentShadowColor;
        private double _currentShadowBlur;
        private double _currentShadowDepth;
        private double _currentShadowOpacity;
        private Brush _currentTrackBgBrush;
 
        public static readonly DependencyProperty StartColorProperty =
            DependencyProperty.Register("StartColor", typeof(string), typeof(GaugeControl),
                new PropertyMetadata("{{GaugeStartColor}}", OnColorChanged));
 
        public static readonly DependencyProperty EndColorProperty =
            DependencyProperty.Register("EndColor", typeof(string), typeof(GaugeControl),
                new PropertyMetadata("{{GaugeEndColor}}", OnColorChanged));
 
        public string StartColor
        {
            get { return (string)GetValue(StartColorProperty); }
            set { SetValue(StartColorProperty, value); }
        }
 
        public string EndColor
        {
            get { return (string)GetValue(EndColorProperty); }
            set { SetValue(EndColorProperty, value); }
        }
 
        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GaugeControl)d).Redraw();
        }

        public GaugeControl()
        {
            InitializeComponent();
            _currentControlBgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("{{ControlBackground}}"));

            var g = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 1) };
            g.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("{{GradientStart}}"), 0));
            g.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("{{GradientMid}}"), 0.5));
            g.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("{{GradientEnd}}"), 1));
            _currentPlateGradientBrush = g;

            _currentBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("{{BorderColor}}"));
            _currentShadowColor = (Color)ColorConverter.ConvertFromString("{{ShadowColor}}");
            _currentShadowBlur = double.Parse("{{ShadowBlur}}");
            _currentShadowDepth = double.Parse("{{ShadowDepth}}");
            _currentShadowOpacity = double.Parse("{{ShadowOpacity}}");
            _currentTrackBgBrush = new SolidColorBrush(Color.FromArgb(25, 128, 128, 128));

            Redraw();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == BackgroundProperty)
            {
                if (this.Background != null)
                {
                    _currentControlBgBrush = this.Background;
                }
                Redraw();
            }
        }

        public string LabelText { get { return LabelBlock.Text; } set { LabelBlock.Text = value; } }
        public string DescText { get { return DescBlock.Text; } set { DescBlock.Text = value; } }
        public void SetLabelVisible(bool visible)
        {
            LabelBlock.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            TitleArea.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public double Minimum
        {
            get { return _min; }
            set { _min = value; Redraw(); }
        }

        public double Maximum
        {
            get { return _max; }
            set { _max = value; Redraw(); }
        }

        public double Value
        {
            get { return _value; }
            set { _value = value; Redraw(); }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Redraw();
        }

        private Color ParseColor(string hex, Color fallback)
        {
            try { return (Color)ColorConverter.ConvertFromString(hex); }
            catch { return fallback; }
        }
 
        private void Redraw()
        {
            if (GaugeCanvas == null) return;
            GaugeCanvas.Children.Clear();
            double w = GaugeCanvas.ActualWidth;
            double h = GaugeCanvas.ActualHeight;
            if (w <= 0 || double.IsNaN(w)) w = GaugeCanvas.Width;
            if (h <= 0 || double.IsNaN(h)) h = GaugeCanvas.Height;
            if (w <= 0 || h <= 0 || double.IsNaN(w) || double.IsNaN(h))
            {
                w = this.ActualWidth;
                h = this.ActualHeight;
            }
            if (w <= 0 || h <= 0 || double.IsNaN(w) || double.IsNaN(h)) return;

            double radius = Math.Min(w, h) * 0.48;
            Point center = new Point(w / 2.0, h / 2.0);

            // 拟态底座圆盘 (随主题动态更新渐变背景与拟态阴影)
            var baseBrush = _currentPlateGradientBrush ?? new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
            var baseCirc = new Ellipse { 
                Width = radius * 2, 
                Height = radius * 2, 
                Fill = baseBrush,
                Stroke = _currentBorderBrush,
                StrokeThickness = 1,
                Effect = new System.Windows.Media.Effects.DropShadowEffect { 
                    BlurRadius = _currentShadowBlur, 
                    ShadowDepth = _currentShadowDepth, 
                    Direction = 315, 
                    Color = _currentShadowColor, 
                    Opacity = _currentShadowOpacity 
                }
            };
            Canvas.SetLeft(baseCirc, center.X - radius);
            Canvas.SetTop(baseCirc, center.Y - radius);
            GaugeCanvas.Children.Add(baseCirc);

            double trackRadius = radius * 0.75;
            double trackThickness = radius * 0.28;
            var bgTrack = new Ellipse { 
                Width = trackRadius * 2, 
                Height = trackRadius * 2, 
                Stroke = _currentTrackBgBrush ?? new SolidColorBrush(Color.FromArgb(25, 128, 128, 128)), 
                StrokeThickness = trackThickness 
            };
            Canvas.SetLeft(bgTrack, center.X - trackRadius);
            Canvas.SetTop(bgTrack, center.Y - trackRadius);
            GaugeCanvas.Children.Add(bgTrack);

            double pct = (_max <= _min) ? 0 : Math.Max(0, Math.Min(1, (_value - _min) / (_max - _min)));
            
            double startAngle = -90;
            double sweep = Math.Max(0.01, pct * 360.0);
            
            Color c1 = ParseColor(StartColor, (Color)ColorConverter.ConvertFromString("#4facfe"));
            Color c2 = ParseColor(EndColor, (Color)ColorConverter.ConvertFromString("#00f2fe"));
            Brush arcGradient = new LinearGradientBrush(c1, c2, new Point(0.5, 0), new Point(0.5, 1));

            if (pct > 0) {
                var valArc = BuildArc(center, trackRadius, startAngle, sweep, arcGradient, trackThickness);
                GaugeCanvas.Children.Add(valArc);
            }

            var valText = new TextBlock { 
                Text = pct.ToString("0.#%"), 
                FontWeight = FontWeights.Bold, 
                FontSize = radius * 0.26, 
                Foreground = LabelBlock != null ? LabelBlock.Foreground : Brushes.Black,
                FontFamily = LabelBlock != null ? LabelBlock.FontFamily : new FontFamily("Segoe UI")
            };
            valText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(valText, center.X - valText.DesiredSize.Width / 2);
            Canvas.SetTop(valText, center.Y - valText.DesiredSize.Height / 2);
            GaugeCanvas.Children.Add(valText);
        }

        private Path BuildArc(Point center, double radius, double startAngle, double sweepAngle, Brush stroke, double thickness)
        {
            if (sweepAngle >= 360) sweepAngle = 359.99;
            double startRad = startAngle * Math.PI / 180.0;
            double endRad = (startAngle + sweepAngle) * Math.PI / 180.0;
            var p1 = new Point(center.X + radius * Math.Cos(startRad), center.Y + radius * Math.Sin(startRad));
            var p2 = new Point(center.X + radius * Math.Cos(endRad), center.Y + radius * Math.Sin(endRad));
            bool large = sweepAngle > 180;

            var fig = new PathFigure { StartPoint = p1, IsClosed = false };
            fig.Segments.Add(new ArcSegment(p2, new Size(radius, radius), 0, large, SweepDirection.Clockwise, true));
            return new Path { Data = new PathGeometry(new[] { fig }), Stroke = stroke, StrokeThickness = thickness, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round };
        }

        #region 运行时风格重绘

        public void ApplyStyle(System.Collections.Generic.Dictionary<string, object> style)
        {
            if (style == null) return;
            try
            {
                // 0. 控件背景色
                if (style.ContainsKey("ControlBackground"))
                {
                    string cb = style["ControlBackground"] as string;
                    if (!string.IsNullOrEmpty(cb))
                    {
                        try { 
                            var bg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cb.StartsWith("#") ? cb : "#" + cb));
                            this.Background = bg;
                            _currentControlBgBrush = bg;
                        } catch { }
                    }
                }

                // 1. 仪表圆盘底座渐变
                if (style.ContainsKey("GradientStart") && style.ContainsKey("GradientMid") && style.ContainsKey("GradientEnd"))
                {
                    try
                    {
                        Color c1 = (Color)ColorConverter.ConvertFromString(style["GradientStart"] as string);
                        Color c2 = (Color)ColorConverter.ConvertFromString(style["GradientMid"] as string);
                        Color c3 = (Color)ColorConverter.ConvertFromString(style["GradientEnd"] as string);
                        var brush = new LinearGradientBrush();
                        brush.StartPoint = new Point(0, 0);
                        brush.EndPoint = new Point(1, 1);
                        brush.GradientStops.Add(new GradientStop(c1, 0));
                        brush.GradientStops.Add(new GradientStop(c2, 0.5));
                        brush.GradientStops.Add(new GradientStop(c3, 1));
                        _currentPlateGradientBrush = brush;
                    }
                    catch { }
                }

                // 2. 边框
                if (style.ContainsKey("BorderColor"))
                {
                    try
                    {
                        string bc = style["BorderColor"] as string;
                        if (!string.IsNullOrEmpty(bc))
                        {
                            _currentBorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bc.StartsWith("#") ? bc : "#" + bc));
                        }
                    }
                    catch { }
                }

                // 3. 阴影
                if (style.ContainsKey("ShadowBlur")) try { _currentShadowBlur = Convert.ToDouble(style["ShadowBlur"]); } catch { }
                if (style.ContainsKey("ShadowDepth")) try { _currentShadowDepth = Convert.ToDouble(style["ShadowDepth"]); } catch { }
                if (style.ContainsKey("ShadowOpacity")) try { _currentShadowOpacity = Convert.ToDouble(style["ShadowOpacity"]); } catch { }
                if (style.ContainsKey("ShadowColor"))
                {
                    try { _currentShadowColor = (Color)ColorConverter.ConvertFromString(style["ShadowColor"] as string); } catch { }
                }

                // 4. 仪表环形主色调
                if (style.ContainsKey("GaugeColor1") || style.ContainsKey("ChartColor1"))
                {
                    string g1 = (style.ContainsKey("GaugeColor1") ? style["GaugeColor1"] : style["ChartColor1"]) as string;
                    if (!string.IsNullOrEmpty(g1)) this.StartColor = g1;
                }
                if (style.ContainsKey("GaugeColor2") || style.ContainsKey("ChartColor2"))
                {
                    string g2 = (style.ContainsKey("GaugeColor2") ? style["GaugeColor2"] : style["ChartColor2"]) as string;
                    if (!string.IsNullOrEmpty(g2)) this.EndColor = g2;
                }

                // 5. 字体与颜色
                if (style.ContainsKey("FontFamily"))
                {
                    var ff = new FontFamily(style["FontFamily"] as string);
                    if (LabelBlock != null) LabelBlock.FontFamily = ff;
                    if (DescBlock != null) DescBlock.FontFamily = ff;
                }
                if (style.ContainsKey("FontColor"))
                {
                    string fc = style["FontColor"] as string;
                    if (!string.IsNullOrEmpty(fc))
                    {
                        try {
                            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fc.StartsWith("#") ? fc : "#" + fc));
                            if (LabelBlock != null) LabelBlock.Foreground = brush;
                        } catch { }
                    }
                }
                if (style.ContainsKey("LabelColor") && DescBlock != null)
                {
                    string lc = style["LabelColor"] as string;
                    if (!string.IsNullOrEmpty(lc))
                    {
                        try { DescBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lc.StartsWith("#") ? lc : "#" + lc)); } catch { }
                    }
                }

                Redraw();
            }
            catch { }
        }

        #endregion
    }
}