using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfTextInput
{
    /// <summary>
    /// 进度条控件
    /// </summary>
    public partial class ProgressBarControl : UserControl
    {
        #region 依赖属性

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(ProgressBarControl),
                new PropertyMetadata("进度", OnLabelTextChanged));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(ProgressBarControl),
                new PropertyMetadata(0.0, OnValueChanged));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(ProgressBarControl),
                new PropertyMetadata(0.0, OnValueChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(ProgressBarControl),
                new PropertyMetadata(100.0, OnValueChanged));

        public static readonly DependencyProperty ShowPercentageProperty =
            DependencyProperty.Register("ShowPercentage", typeof(bool), typeof(ProgressBarControl),
                new PropertyMetadata(true, OnShowPercentageChanged));

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public bool ShowPercentage
        {
            get { return (bool)GetValue(ShowPercentageProperty); }
            set { SetValue(ShowPercentageProperty, value); }
        }

        #endregion

        #region 梯度色彩属性

        private string _startColor = "{{ProgressColor1}}";
        private string _endColor = "{{ProgressColor2}}";

        public string StartColor
        {
            get { return _startColor; }
            set 
            { 
                _startColor = value; 
                if (FillColorStart != null) 
                    FillColorStart.Color = ParseColor(value, Colors.Green); 
            }
        }

        public string EndColor
        {
            get { return _endColor; }
            set 
            { 
                _endColor = value; 
                if (FillColorEnd != null) 
                    FillColorEnd.Color = ParseColor(value, Colors.Blue); 
            }
        }

        private Color ParseColor(string hex, Color fallback)
        {
            try { return (Color)ColorConverter.ConvertFromString(hex); }
            catch { return fallback; }
        }

        #endregion

        public ProgressBarControl()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateFillBar();
        }


        #region 公共方法

        public void SetLabelVisible(bool visible)
        {
            if (LabelBlock != null)
                LabelBlock.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region 内部逻辑

        private static void OnLabelTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (ProgressBarControl)d;
            if (c.LabelBlock != null)
                c.LabelBlock.Text = e.NewValue as string ?? "进度";
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (ProgressBarControl)d;
            c.UpdateFillBar();
        }

        private static void OnShowPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (ProgressBarControl)d;
            if (c.PercentBlock != null)
                c.PercentBlock.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateFillBar()
        {
            if (FillBar == null || TrackBorder == null) return;

            double range = Maximum - Minimum;
            if (range <= 0) range = 1;

            double ratio = Math.Max(0, Math.Min(1, (Value - Minimum) / range));
            double trackWidth = TrackBorder.ActualWidth;
            if (trackWidth <= 0) trackWidth = TrackBorder.Width;
            if (double.IsNaN(trackWidth) || trackWidth <= 0) return;

            double targetWidth = Math.Max(0, trackWidth * ratio);

            var anim = new DoubleAnimation(targetWidth, TimeSpan.FromSeconds(0.3))
            {
                EasingFunction = new QuadraticEase()
            };
            FillBar.BeginAnimation(WidthProperty, anim);

            // 更新百分比文字
            if (PercentBlock != null)
                PercentBlock.Text = string.Format("{0:F0}%", ratio * 100);

        }

        #endregion
    }
}
