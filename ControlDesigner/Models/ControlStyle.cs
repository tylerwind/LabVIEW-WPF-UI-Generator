using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ControlDesigner.Models
{
    /// <summary>
    /// 控件样式数据模型 — 所有样式参数
    /// </summary>
    public class ControlStyle : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
        }


        private bool Set<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }

        // === 背景 ===
        private string _controlBackground = "#E3E6EC";
        public string ControlBackground { get { return _controlBackground; } set { Set(ref _controlBackground, value); } }


        private string _gradientStart = "#EAEDF2";
        public string GradientStart { get { return _gradientStart; } set { Set(ref _gradientStart, value); } }


        private string _gradientMid = "#E0E3E9";
        public string GradientMid { get { return _gradientMid; } set { Set(ref _gradientMid, value); } }


        private string _gradientEnd = "#D8DCE3";
        public string GradientEnd { get { return _gradientEnd; } set { Set(ref _gradientEnd, value); } }


        // === 边框 ===
        private string _borderColor = "#DDE0E6";
        public string BorderColor { get { return _borderColor; } set { Set(ref _borderColor, value); } }


        private double _borderThickness = 1;
        public double BorderThickness { get { return _borderThickness; } set { Set(ref _borderThickness, value); } }


        private double _cornerRadius = 12;
        public double CornerRadius { get { return _cornerRadius; } set { Set(ref _cornerRadius, value); } }


        // === 阴影 ===
        private double _shadowBlur = 10;
        public double ShadowBlur { get { return _shadowBlur; } set { Set(ref _shadowBlur, value); } }


        private double _shadowDepth = 4;
        public double ShadowDepth { get { return _shadowDepth; } set { Set(ref _shadowDepth, value); } }


        private string _shadowColor = "#A3A9B5";
        public string ShadowColor { get { return _shadowColor; } set { Set(ref _shadowColor, value); } }


        private double _shadowOpacity = 0.5;
        public double ShadowOpacity { get { return _shadowOpacity; } set { Set(ref _shadowOpacity, value); } }


        // === 高光 ===
        private string _highlightColor = "#FFFFFF";
        public string HighlightColor { get { return _highlightColor; } set { Set(ref _highlightColor, value); } }


        private double _highlightOpacity = 0.65;
        public double HighlightOpacity { get { return _highlightOpacity; } set { Set(ref _highlightOpacity, value); } }


        // === 字体 ===
        private string _fontFamily = "Segoe UI";
        public string FontFamily { get { return _fontFamily; } set { Set(ref _fontFamily, value); } }


        private double _fontSize = 14;
        public double FontSize { get { return _fontSize; } set { Set(ref _fontSize, value); } }


        private string _fontColor = "#3A3F50";
        public string FontColor { get { return _fontColor; } set { Set(ref _fontColor, value); } }


        private string _caretColor = "#5A6070";
        public string CaretColor { get { return _caretColor; } set { Set(ref _caretColor, value); } }


        // === 标签 ===
        private string _labelColor = "#8A90A0";
        public string LabelColor { get { return _labelColor; } set { Set(ref _labelColor, value); } }


        private double _labelFontSize = 11;
        public double LabelFontSize { get { return _labelFontSize; } set { Set(ref _labelFontSize, value); } }


        // === 聚焦 ===
        private string _focusBorderColor = "#B0B8C8";
        public string FocusBorderColor { get { return _focusBorderColor; } set { Set(ref _focusBorderColor, value); } }


        private string _accentColor = "#7A8AA8";
        public string AccentColor { get { return _accentColor; } set { Set(ref _accentColor, value); } }


        // === LED ===
        private string _ledOnColor = "#4CAF50";
        public string LedOnColor { get { return _ledOnColor; } set { Set(ref _ledOnColor, value); } }


        private string _ledOffColor = "#808080";
        public string LedOffColor { get { return _ledOffColor; } set { Set(ref _ledOffColor, value); } }


        // === Chart 折线图 ===
        private int _chartLineMode = 0; // 0=Smooth, 1=Linear, 2=Step
        public int ChartLineMode { get { return _chartLineMode; } set { Set(ref _chartLineMode, value); } }

        private string _chartTitle = "实时曲线监控";
        public string ChartTitle { get { return _chartTitle; } set { Set(ref _chartTitle, value); } }

        private string _chartSubtitle = "Multi-Series Analytics";
        public string ChartSubtitle { get { return _chartSubtitle; } set { Set(ref _chartSubtitle, value); } }

        private double _chartLineWeight = 2.0;
        public double ChartLineWeight { get { return _chartLineWeight; } set { Set(ref _chartLineWeight, value); } }

        private double _chartFillOpacity = 0.2;
        public double ChartFillOpacity { get { return _chartFillOpacity; } set { Set(ref _chartFillOpacity, value); } }

        private string _chartColor1 = "#1E90FF"; // DodgerBlue
        public string ChartColor1 { get { return _chartColor1; } set { Set(ref _chartColor1, value); } }

        private string _chartColor2 = "#00FA9A"; // MediumSpringGreen
        public string ChartColor2 { get { return _chartColor2; } set { Set(ref _chartColor2, value); } }

        private string _chartColor3 = "#FF4500"; // OrangeRed
        public string ChartColor3 { get { return _chartColor3; } set { Set(ref _chartColor3, value); } }
        
        private bool _chartShowGridLines = true;
        public bool ChartShowGridLines { get { return _chartShowGridLines; } set { Set(ref _chartShowGridLines, value); } }

        private string _chartPlotBackground = "#08000000"; // 默认淡灰色
        public string ChartPlotBackground { get { return _chartPlotBackground; } set { Set(ref _chartPlotBackground, value); } }

        private bool _chartShowSeriesCards = true;
        public bool ChartShowSeriesCards { get { return _chartShowSeriesCards; } set { Set(ref _chartShowSeriesCards, value); } }

        // === DataGrid 数据表 ===
        private double _dataGridRowHeight = 40;
        public double DataGridRowHeight { get { return _dataGridRowHeight; } set { Set(ref _dataGridRowHeight, value); } }

        private string _dataGridHeaderBackground = "#F8F9FB";
        public string DataGridHeaderBackground { get { return _dataGridHeaderBackground; } set { Set(ref _dataGridHeaderBackground, value); } }

        private string _dataGridBackground = "#FFFFFF";
        public string DataGridBackground { get { return _dataGridBackground; } set { Set(ref _dataGridBackground, value); } }

        private double _dataGridAlternatingOpacity = 0.04;
        public double DataGridAlternatingOpacity { get { return _dataGridAlternatingOpacity; } set { Set(ref _dataGridAlternatingOpacity, value); } }

        private bool _dataGridGridLinesVisible = false;
        public bool DataGridGridLinesVisible { get { return _dataGridGridLinesVisible; } set { Set(ref _dataGridGridLinesVisible, value); } }

        private string _dataGridLabelText = "数据表格";
        public string DataGridLabelText { get { return _dataGridLabelText; } set { Set(ref _dataGridLabelText, value); } }

        private bool _dataGridShowHeader = true;
        public bool DataGridShowHeader { get { return _dataGridShowHeader; } set { Set(ref _dataGridShowHeader, value); } }

        // === Gauge 仪表 ===
        private string _gaugeColor1 = "#00BFFF"; // DeepSkyBlue
        public string GaugeColor1 { get { return _gaugeColor1; } set { Set(ref _gaugeColor1, value); } }

        private string _gaugeColor2 = "#00FA9A"; // MediumSpringGreen
        public string GaugeColor2 { get { return _gaugeColor2; } set { Set(ref _gaugeColor2, value); } }

        // === Slider 滑动杆 ===
        private string _sliderColor1 = "#7A8AA8";
        public string SliderColor1 { get { return _sliderColor1; } set { Set(ref _sliderColor1, value); } }

        private string _sliderColor2 = "#4682B4";
        public string SliderColor2 { get { return _sliderColor2; } set { Set(ref _sliderColor2, value); } }

        // === ProgressBar 进度条 ===
        private string _progressColor1 = "#7A8AA8";
        public string ProgressColor1 { get { return _progressColor1; } set { Set(ref _progressColor1, value); } }

        private string _progressColor2 = "#4682B4";
        public string ProgressColor2 { get { return _progressColor2; } set { Set(ref _progressColor2, value); } }

        // === ComboBox 下拉框 ===
        private string _comboBoxArrowColor = "#7A8AA8";
        public string ComboBoxArrowColor { get { return _comboBoxArrowColor; } set { Set(ref _comboBoxArrowColor, value); } }

        // === ToggleSwitch 开关 ===
        private string _toggleColorOn = "#7A8AA8";
        public string ToggleColorOn { get { return _toggleColorOn; } set { Set(ref _toggleColorOn, value); } }

        private string _toggleColorOff = "#C8CCD0";
        public string ToggleColorOff { get { return _toggleColorOff; } set { Set(ref _toggleColorOff, value); } }

        // === Tree 树形控件 ===
        private double _treeItemHeight = 36;
        public double TreeItemHeight { get { return _treeItemHeight; } set { Set(ref _treeItemHeight, value); } }

        private double _treeIndentSize = 24;
        public double TreeIndentSize { get { return _treeIndentSize; } set { Set(ref _treeIndentSize, value); } }

        private string _treeLabelText = "配置节点";
        public string TreeLabelText { get { return _treeLabelText; } set { Set(ref _treeLabelText, value); } }

        private string _treeBackground = "#FFFFFF";
        public string TreeBackground { get { return _treeBackground; } set { Set(ref _treeBackground, value); } }

        private bool _treeShowCheckBox = true;
        public bool TreeShowCheckBox { get { return _treeShowCheckBox; } set { Set(ref _treeShowCheckBox, value); } }

        // === 间距 ===
        private string _cardPadding = "12,8,12,6";
        public string CardPadding { get { return _cardPadding; } set { Set(ref _cardPadding, value); } }


        /// <summary>
        /// 创建当前样式的深拷贝
        /// </summary>
        public ControlStyle Clone()
        {
            return (ControlStyle)this.MemberwiseClone();
        }
    }
}
