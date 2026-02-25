using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ControlDesigner
{
    public partial class ColorPickerWindow : Window
    {
        private bool _suppressHexUpdate;

        /// <summary>
        /// 用户选择的颜色（Hex 字符串）
        /// </summary>
        public string SelectedColor { get; private set; }

        public ColorPickerWindow(string initialColor = "#FFFFFF")
        {
            InitializeComponent();
            BuildPalettes();
            SelectedColor = initialColor ?? "#FFFFFF";
            TxtHex.Text = SelectedColor;
            UpdatePreview();
        }

        #region 色板构建

        private void BuildPalettes()
        {
            // === 基础色板 (12色 × 5明度) ===
            string[,] basicColors = {
                // 浅 → 深
                { "#FFCDD2", "#EF9A9A", "#EF5350", "#D32F2F", "#B71C1C" }, // 红
                { "#F8BBD0", "#F48FB1", "#EC407A", "#C2185B", "#880E4F" }, // 粉
                { "#E1BEE7", "#CE93D8", "#AB47BC", "#7B1FA2", "#4A148C" }, // 紫
                { "#C5CAE9", "#9FA8DA", "#5C6BC0", "#303F9F", "#1A237E" }, // 靛
                { "#BBDEFB", "#90CAF9", "#42A5F5", "#1976D2", "#0D47A1" }, // 蓝
                { "#B2EBF2", "#80DEEA", "#26C6DA", "#0097A7", "#006064" }, // 青
                { "#B2DFDB", "#80CBC4", "#26A69A", "#00796B", "#004D40" }, // 青绿
                { "#C8E6C9", "#A5D6A7", "#66BB6A", "#388E3C", "#1B5E20" }, // 绿
                { "#F0F4C3", "#E6EE9C", "#D4E157", "#AFB42B", "#827717" }, // 黄绿
                { "#FFF9C4", "#FFF176", "#FFEE58", "#FBC02D", "#F57F17" }, // 黄
                { "#FFE0B2", "#FFCC80", "#FFA726", "#F57C00", "#E65100" }, // 橙
                { "#FFCCBC", "#FFAB91", "#FF7043", "#E64A19", "#BF360C" }, // 深橙
            };

            BasicColorPanel.Children.Clear();
            for (int row = 0; row < basicColors.GetLength(0); row++)
            {
                for (int col = 0; col < basicColors.GetLength(1); col++)
                {
                    AddColorSwatch(BasicColorPanel, basicColors[row, col], 24);
                }
            }

            // === 灰度色板 ===
            string[] grayColors = {
                "#FFFFFF", "#FAFAFA", "#F5F5F5", "#EEEEEE", "#E0E0E0",
                "#BDBDBD", "#9E9E9E", "#757575", "#616161", "#424242",
                "#303030", "#212121", "#1A1A1A", "#0D0D0D", "#000000",
            };

            GrayColorPanel.Children.Clear();
            foreach (var hex in grayColors)
            {
                AddColorSwatch(GrayColorPanel, hex, 24);
            }

            // === 推荐配色 (新拟态 / UI 常用) ===
            string[] recommended = {
                // 新拟态
                "#E3E6EC", "#EAEDF2", "#E0E3E9", "#D8DCE3", "#DDE0E6",
                // 暗色 UI
                "#1E1E2E", "#2A2A3C", "#252538", "#202032", "#3A3A50",
                // 玻璃拟态
                "#E8EBF0", "#F0F2F8", "#E8EAF2", "#C8CCE0",
                // 强调色
                "#4A6FA5", "#5868A8", "#7A8AA8", "#6878A0", "#3B82F6",
                "#60A5FA", "#8B5CF6", "#EC4899", "#10B981", "#F59E0B",
                // 透明色
                "Transparent"
            };

            RecommendedPanel.Children.Clear();
            foreach (var hex in recommended)
            {
                AddColorSwatch(RecommendedPanel, hex, 26);
            }
        }

        private void AddColorSwatch(WrapPanel panel, string hex, int size)
        {
            var swatch = new Border
            {
                Width = size,
                Height = size,
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(2),
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(1),
                Tag = hex,
                ToolTip = hex,
            };

            // 边框色：暗色用亮框，亮色用暗框
            try
            {
                if (hex.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
                {
                    // 给透明色画一个斜线背景或简单标识
                    swatch.Background = new DrawingBrush
                    {
                        TileMode = TileMode.Tile,
                        Viewport = new Rect(0, 0, 8, 8),
                        ViewportUnits = BrushMappingMode.Absolute,
                        Drawing = new GeometryDrawing(Brushes.White, null, new RectangleGeometry(new Rect(0, 0, 8, 8)))
                        {
                            Geometry = new GeometryGroup
                            {
                                Children = new GeometryCollection {
                                    new RectangleGeometry(new Rect(0,0,4,4)),
                                    new RectangleGeometry(new Rect(4,4,4,4))
                                }
                            },
                            Brush = Brushes.LightGray
                        }
                    };
                    swatch.BorderBrush = Brushes.Gray;
                }
                else
                {
                    var color = (Color)ColorConverter.ConvertFromString(hex);
                    swatch.Background = new SolidColorBrush(color);
                    double brightness = 0.299 * color.R + 0.587 * color.G + 0.114 * color.B;
                    swatch.BorderBrush = new SolidColorBrush(brightness > 180
                        ? Color.FromRgb(0x88, 0x88, 0x88)
                        : Color.FromRgb(0x55, 0x55, 0x55));
                }
            }
            catch
            {
                swatch.Background = Brushes.Transparent;
                swatch.BorderBrush = Brushes.Gray;
            }

            swatch.MouseLeftButtonDown += Swatch_Click;
            panel.Children.Add(swatch);
        }

        #endregion

        #region 事件

        private void Swatch_Click(object sender, MouseButtonEventArgs e)
        {
            var swatch = (Border)sender;
            string hex = swatch.Tag as string;
            if (!string.IsNullOrEmpty(hex))
            {
                _suppressHexUpdate = true;
                TxtHex.Text = hex;
                SelectedColor = hex;
                UpdatePreview();
                _suppressHexUpdate = false;
            }
        }

        private void TxtHex_Changed(object sender, TextChangedEventArgs e)
        {
            if (_suppressHexUpdate) return;
            string hex = TxtHex.Text.Trim();
            if (!hex.Equals("Transparent", StringComparison.OrdinalIgnoreCase) && !hex.StartsWith("#") && hex.Length > 0) 
                hex = "#" + hex;
            SelectedColor = hex;
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            try
            {
                if (SelectedColor.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
                {
                    CurrentColorPreview.Background = Brushes.Transparent;
                    return;
                }
                var color = (Color)ColorConverter.ConvertFromString(SelectedColor);
                CurrentColorPreview.Background = new SolidColorBrush(color);
            }
            catch
            {
                CurrentColorPreview.Background = Brushes.Transparent;
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion
    }
}
