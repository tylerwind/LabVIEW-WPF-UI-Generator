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
            Redraw();
        }

        public string LabelText { get { return LabelBlock.Text; } set { LabelBlock.Text = value; } }
        public string DescText { get { return DescBlock.Text; } set { DescBlock.Text = value; } }
        public void SetLabelVisible(bool visible) { LabelBlock.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }

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
            if (w <= 0 || h <= 0) return;

            double radius = Math.Min(w, h) * 0.45;
            Point center = new Point(w / 2.0, h / 2.0);

            var baseBrush = TryFindResource("GaugeBackground") as Brush ?? new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
            var baseEffect = TryFindResource("GaugeShadow") as System.Windows.Media.Effects.DropShadowEffect;
            
            var baseCirc = new Ellipse { Width = radius * 2, Height = radius * 2, Fill = baseBrush };
            // 恢复背景圆与阴影，对齐预览风格
            if (baseEffect != null) baseCirc.Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = baseEffect.BlurRadius, ShadowDepth = baseEffect.ShadowDepth, Direction = baseEffect.Direction, Color = baseEffect.Color, Opacity = baseEffect.Opacity };
            Canvas.SetLeft(baseCirc, center.X - radius);
            Canvas.SetTop(baseCirc, center.Y - radius);
            GaugeCanvas.Children.Add(baseCirc);

            double trackRadius = radius * 0.75;
            double trackThickness = radius * 0.28;
            var bgTrack = new Ellipse { Width = trackRadius * 2, Height = trackRadius * 2, Stroke = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0)), StrokeThickness = trackThickness };
            Canvas.SetLeft(bgTrack, center.X - trackRadius);
            Canvas.SetTop(bgTrack, center.Y - trackRadius);
            GaugeCanvas.Children.Add(bgTrack);

            double pct = (_max <= _min) ? 0 : Math.Max(0, Math.Min(1, (_value - _min) / (_max - _min)));
            var accent = (Color)ColorConverter.ConvertFromString("{{AccentColor}}");
            
            double startAngle = -90;
            double sweep = Math.Max(0.01, pct * 360.0);
            
            Color c1 = ParseColor(StartColor, (Color)ColorConverter.ConvertFromString("#4facfe"));
            Color c2 = ParseColor(EndColor, (Color)ColorConverter.ConvertFromString("#00f2fe"));
            Brush arcGradient = new LinearGradientBrush(c1, c2, new Point(0.5, 0), new Point(0.5, 1));

            if (pct > 0) {
                var valArc = BuildArc(center, trackRadius, startAngle, sweep, arcGradient, trackThickness);
                GaugeCanvas.Children.Add(valArc);

                double endAngle = startAngle + sweep;
                double rad = endAngle * Math.PI / 180.0;
                double px = center.X + trackRadius * Math.Cos(rad);
                double py = center.Y + trackRadius * Math.Sin(rad);
                
                // 移除轨道末端圆点
                // var dotColor = Color.FromArgb(255, (byte)(accent.R*0.7), (byte)(accent.G*0.7), (byte)(accent.B*0.7));
                // var dot = new Ellipse { Width = trackThickness, Height = trackThickness, ... };
                // dot.Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 4, ShadowDepth = 2, Opacity = 0.3 };
                // Canvas.SetLeft(dot, px - trackThickness / 2);
                // Canvas.SetTop(dot, py - trackThickness / 2);
                // GaugeCanvas.Children.Add(dot);
            }

            var valText = new TextBlock { Text = pct.ToString("0.#%"), FontWeight = FontWeights.Bold, FontSize = radius * 0.26, Foreground = LabelBlock.Foreground };
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
    }
}