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
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private bool Set<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }

        // === 背景 ===
        private string _controlBackground = "#E3E6EC";
        public string ControlBackground { get => _controlBackground; set => Set(ref _controlBackground, value); }

        private string _gradientStart = "#EAEDF2";
        public string GradientStart { get => _gradientStart; set => Set(ref _gradientStart, value); }

        private string _gradientMid = "#E0E3E9";
        public string GradientMid { get => _gradientMid; set => Set(ref _gradientMid, value); }

        private string _gradientEnd = "#D8DCE3";
        public string GradientEnd { get => _gradientEnd; set => Set(ref _gradientEnd, value); }

        // === 边框 ===
        private string _borderColor = "#DDE0E6";
        public string BorderColor { get => _borderColor; set => Set(ref _borderColor, value); }

        private double _borderThickness = 1;
        public double BorderThickness { get => _borderThickness; set => Set(ref _borderThickness, value); }

        private double _cornerRadius = 12;
        public double CornerRadius { get => _cornerRadius; set => Set(ref _cornerRadius, value); }

        // === 阴影 ===
        private double _shadowBlur = 10;
        public double ShadowBlur { get => _shadowBlur; set => Set(ref _shadowBlur, value); }

        private double _shadowDepth = 4;
        public double ShadowDepth { get => _shadowDepth; set => Set(ref _shadowDepth, value); }

        private string _shadowColor = "#A3A9B5";
        public string ShadowColor { get => _shadowColor; set => Set(ref _shadowColor, value); }

        private double _shadowOpacity = 0.5;
        public double ShadowOpacity { get => _shadowOpacity; set => Set(ref _shadowOpacity, value); }

        // === 高光 ===
        private string _highlightColor = "#FFFFFF";
        public string HighlightColor { get => _highlightColor; set => Set(ref _highlightColor, value); }

        private double _highlightOpacity = 0.65;
        public double HighlightOpacity { get => _highlightOpacity; set => Set(ref _highlightOpacity, value); }

        // === 字体 ===
        private string _fontFamily = "Segoe UI";
        public string FontFamily { get => _fontFamily; set => Set(ref _fontFamily, value); }

        private double _fontSize = 14;
        public double FontSize { get => _fontSize; set => Set(ref _fontSize, value); }

        private string _fontColor = "#3A3F50";
        public string FontColor { get => _fontColor; set => Set(ref _fontColor, value); }

        private string _caretColor = "#5A6070";
        public string CaretColor { get => _caretColor; set => Set(ref _caretColor, value); }

        // === 标签 ===
        private string _labelColor = "#8A90A0";
        public string LabelColor { get => _labelColor; set => Set(ref _labelColor, value); }

        private double _labelFontSize = 11;
        public double LabelFontSize { get => _labelFontSize; set => Set(ref _labelFontSize, value); }

        // === 聚焦 ===
        private string _focusBorderColor = "#B0B8C8";
        public string FocusBorderColor { get => _focusBorderColor; set => Set(ref _focusBorderColor, value); }

        private string _accentColor = "#7A8AA8";
        public string AccentColor { get => _accentColor; set => Set(ref _accentColor, value); }

        // === 间距 ===
        private string _cardPadding = "12,8,12,6";
        public string CardPadding { get => _cardPadding; set => Set(ref _cardPadding, value); }

        /// <summary>
        /// 创建当前样式的深拷贝
        /// </summary>
        public ControlStyle Clone()
        {
            return (ControlStyle)this.MemberwiseClone();
        }
    }
}
