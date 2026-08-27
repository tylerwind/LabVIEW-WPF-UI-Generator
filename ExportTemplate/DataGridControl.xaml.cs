using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfDataGrid
{
    public class CellBadgeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;
            string str = value.ToString();
            bool isBadge = str.StartsWith("[#") && str.Contains("]");
            string param = parameter as string;

            if (param == "Visibility")
            {
                return isBadge ? Visibility.Visible : Visibility.Collapsed;
            }
            if (param == "NormalVisibility")
            {
                return isBadge ? Visibility.Collapsed : Visibility.Visible;
            }
            if (param == "Text")
            {
                if (isBadge)
                {
                    int closeIndex = str.IndexOf(']');
                    return str.Substring(closeIndex + 1);
                }
                return str;
            }
            if (param == "Background")
            {
                if (isBadge)
                {
                    int closeIndex = str.IndexOf(']');
                    string colorStr = str.Substring(1, closeIndex - 1);
                    try
                    {
                        var converter = new BrushConverter();
                        return (Brush)converter.ConvertFromString(colorStr);
                    }
                    catch
                    {
                        return Brushes.Transparent;
                    }
                }
                return Brushes.Transparent;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class DataGridControl : UserControl
    {
        private DataTable _internalTable;
 
        public static readonly System.Windows.DependencyProperty HeaderColorProperty =
            System.Windows.DependencyProperty.Register("HeaderColor", typeof(string), typeof(DataGridControl),
                new System.Windows.PropertyMetadata("{{DataGridHeaderColor}}"));
 
        public string HeaderColor
        {
            get { return (string)GetValue(HeaderColorProperty); }
            set { SetValue(HeaderColorProperty, value); }
        }

        public static readonly System.Windows.DependencyProperty BadgeFontSizeProperty =
            System.Windows.DependencyProperty.Register("BadgeFontSize", typeof(double), typeof(DataGridControl),
                new System.Windows.PropertyMetadata(9.0));

        public double BadgeFontSize
        {
            get { return (double)GetValue(BadgeFontSizeProperty); }
            set { SetValue(BadgeFontSizeProperty, value); }
        }

        public DataGridControl()
        {
            InitializeComponent();
            _internalTable = new DataTable();
            MainDataGrid.ItemsSource = _internalTable.DefaultView;
            MainDataGrid.AutoGeneratingColumn += MainDataGrid_AutoGeneratingColumn;
        }

        private void MainDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            DataGridTextColumn textColumn = e.Column as DataGridTextColumn;
            if (textColumn != null)
            {
                var templateColumn = new DataGridTemplateColumn
                {
                    Header = e.Column.Header,
                    SortMemberPath = e.PropertyName
                };

                if (e.Column.HeaderStyle != null)
                {
                    templateColumn.HeaderStyle = e.Column.HeaderStyle;
                }
                if (e.Column.CellStyle != null)
                {
                    templateColumn.CellStyle = e.Column.CellStyle;
                }
                if (e.Column.Width != DataGridLength.Auto)
                {
                    templateColumn.Width = e.Column.Width;
                }

                var cellTemplate = new DataTemplate();
                var gridFactory = new FrameworkElementFactory(typeof(Grid));

                // Normal TextBlock
                var normalTextFactory = new FrameworkElementFactory(typeof(TextBlock));
                normalTextFactory.SetBinding(TextBlock.TextProperty, new Binding(e.PropertyName));
                normalTextFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                normalTextFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                normalTextFactory.SetBinding(TextBlock.VisibilityProperty, new Binding(e.PropertyName)
                {
                    Converter = new CellBadgeConverter(),
                    ConverterParameter = "NormalVisibility"
                });
                gridFactory.AppendChild(normalTextFactory);

                // Badge Border
                var badgeBorderFactory = new FrameworkElementFactory(typeof(Border));
                badgeBorderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
                badgeBorderFactory.SetValue(Border.PaddingProperty, new Thickness(8, 2, 8, 2));
                badgeBorderFactory.SetValue(Border.VerticalAlignmentProperty, VerticalAlignment.Center);
                badgeBorderFactory.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                badgeBorderFactory.SetBinding(Border.BackgroundProperty, new Binding(e.PropertyName)
                {
                    Converter = new CellBadgeConverter(),
                    ConverterParameter = "Background"
                });
                badgeBorderFactory.SetBinding(Border.VisibilityProperty, new Binding(e.PropertyName)
                {
                    Converter = new CellBadgeConverter(),
                    ConverterParameter = "Visibility"
                });

                // TextBlock inside Badge
                var badgeTextFactory = new FrameworkElementFactory(typeof(TextBlock));
                badgeTextFactory.SetBinding(TextBlock.TextProperty, new Binding(e.PropertyName)
                {
                    Converter = new CellBadgeConverter(),
                    ConverterParameter = "Text"
                });
                badgeTextFactory.SetValue(TextBlock.ForegroundProperty, Brushes.White);
                badgeTextFactory.SetBinding(TextBlock.FontSizeProperty, new Binding("BadgeFontSize")
                {
                    Source = this
                });
                badgeTextFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
                badgeTextFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                badgeTextFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);

                badgeBorderFactory.AppendChild(badgeTextFactory);
                gridFactory.AppendChild(badgeBorderFactory);

                cellTemplate.VisualTree = gridFactory;
                templateColumn.CellTemplate = cellTemplate;

                e.Column = templateColumn;
            }
        }

        public static string FormatBadge(string text, int colorValue)
        {
            return string.Format("[#{0:X6}]{1}", colorValue & 0xFFFFFF, text);
        }

        public void UpdateCell(int rowIndex, int colIndex, string value)
        {
            if (_internalTable == null) return;
            if (rowIndex >= 0 && rowIndex < _internalTable.Rows.Count)
            {
                if (colIndex >= 0 && colIndex < _internalTable.Columns.Count)
                {
                    _internalTable.Rows[rowIndex][colIndex] = value;
                }
            }
        }

        public void BindDataTable(DataTable dt)
        {
            _internalTable = dt;
            MainDataGrid.ItemsSource = _internalTable.DefaultView;
        }

        public void SetHeaders(string[] headers)
        {
            _internalTable = new DataTable();
            foreach (string header in headers)
            {
                _internalTable.Columns.Add(header);
            }
            MainDataGrid.ItemsSource = _internalTable.DefaultView;
        }

        public void SetData(string[,] data)
        {
            if (_internalTable == null || _internalTable.Columns.Count == 0) return;
            _internalTable.Rows.Clear();
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            int tableCols = _internalTable.Columns.Count;

            for (int i = 0; i < rows; i++)
            {
                DataRow dr = _internalTable.NewRow();
                for (int j = 0; j < Math.Min(cols, tableCols); j++)
                {
                    dr[j] = data[i, j];
                }
                _internalTable.Rows.Add(dr);
            }
        }

        public int AddRow(string[] rowData)
        {
            if (_internalTable == null || _internalTable.Columns.Count == 0) return -1;
            DataRow dr = _internalTable.NewRow();
            for (int i = 0; i < Math.Min(rowData.Length, _internalTable.Columns.Count); i++)
            {
                dr[i] = rowData[i];
            }
            _internalTable.Rows.Add(dr);
            return _internalTable.Rows.Count - 1;
        }

        public void Clear()
        {
            if (_internalTable != null)
                _internalTable.Rows.Clear();
        }

        public string LabelText
        {
            get { return LabelBlock.Text; }
            set { LabelBlock.Text = value; }
        }

        public void SetLabelVisible(bool visible)
        {
            LabelBlock.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public bool ShowHeader
        {
            get { return MainDataGrid.HeadersVisibility == DataGridHeadersVisibility.Column; }
            set { MainDataGrid.HeadersVisibility = value ? DataGridHeadersVisibility.Column : DataGridHeadersVisibility.None; }
        }

        public double RowHeight
        {
            get { return MainDataGrid.RowHeight; }
            set { MainDataGrid.RowHeight = value; }
        }

        public void BindData(object data)
        {
            MainDataGrid.ItemsSource = (System.Collections.IEnumerable)data;
        }
        
        public string[] GetHeaders()
        {
            if (_internalTable == null) return new string[0];
            string[] headers = new string[_internalTable.Columns.Count];
            for (int i = 0; i < _internalTable.Columns.Count; i++)
            {
                headers[i] = _internalTable.Columns[i].ColumnName;
            }
            return headers;
        }

        public string[,] GetAllData()
        {
            if (_internalTable == null) return new string[0, 0];
            int rows = _internalTable.Rows.Count;
            int cols = _internalTable.Columns.Count;
            string[,] data = new string[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    object val = _internalTable.Rows[i][j];
                    data[i, j] = (val == null || val == DBNull.Value) ? "" : val.ToString();
                }
            }
            return data;
        }

        public DataGrid Grid 
        { 
            get { return MainDataGrid; } 
        }

        #region 运行时风格重绘

        public void ApplyStyle(System.Collections.Generic.Dictionary<string, object> style)
        {
            if (style == null) return;
            try
            {
                // 1. 卡片底座背景渐变与纯色 (DataGridBackground / GradientStart/Mid/End / ControlBackground)
                if (MainBorder != null)
                {
                    if (style.ContainsKey("GradientStart") && style.ContainsKey("GradientMid") && style.ContainsKey("GradientEnd"))
                    {
                        try {
                            Color c1 = (Color)ColorConverter.ConvertFromString(style["GradientStart"] as string);
                            Color c2 = (Color)ColorConverter.ConvertFromString(style["GradientMid"] as string);
                            Color c3 = (Color)ColorConverter.ConvertFromString(style["GradientEnd"] as string);
                            var g = new LinearGradientBrush { StartPoint = new Point(0,0), EndPoint = new Point(1,1) };
                            g.GradientStops.Add(new GradientStop(c1, 0));
                            g.GradientStops.Add(new GradientStop(c2, 0.5));
                            g.GradientStops.Add(new GradientStop(c3, 1));
                            MainBorder.Background = g;
                        } catch { }
                    }
                    else if (style.ContainsKey("DataGridBackground"))
                    {
                        try {
                            string dgb = style["DataGridBackground"] as string;
                            MainBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dgb.StartsWith("#") ? dgb : "#" + dgb));
                        } catch { }
                    }
                    else if (style.ContainsKey("ControlBackground"))
                    {
                        try {
                            string cb = style["ControlBackground"] as string;
                            MainBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cb.StartsWith("#") ? cb : "#" + cb));
                        } catch { }
                    }
                }

                if (MainDataGrid != null)
                {
                    MainDataGrid.Background = Brushes.Transparent;
                }

                // 2. 边框颜色与粗细
                if (style.ContainsKey("BorderColor"))
                {
                    string bc = style["BorderColor"] as string;
                    if (!string.IsNullOrEmpty(bc))
                    {
                        try {
                            var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bc.StartsWith("#") ? bc : "#" + bc));
                            this.Resources["DataGridBorderBrush"] = b;
                            if (InnerBorder != null) InnerBorder.BorderBrush = b;
                        } catch { }
                    }
                }
                if (style.ContainsKey("BorderThickness") && InnerBorder != null)
                {
                    try { InnerBorder.BorderThickness = new Thickness(Convert.ToDouble(style["BorderThickness"])); } catch { }
                }

                // 3. 强调色
                if (style.ContainsKey("AccentColor"))
                {
                    string ac = style["AccentColor"] as string;
                    if (!string.IsNullOrEmpty(ac))
                    {
                        try {
                            this.Resources["DataGridAccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ac.StartsWith("#") ? ac : "#" + ac));
                        } catch { }
                    }
                }

                // 4. 圆角
                if (style.ContainsKey("CornerRadius"))
                {
                    try {
                        var cr = new CornerRadius(Convert.ToDouble(style["CornerRadius"]));
                        if (MainBorder != null) MainBorder.CornerRadius = cr;
                        if (InnerBorder != null) InnerBorder.CornerRadius = cr;
                    } catch { }
                }

                // 5. 阴影
                if (PartShadow != null)
                {
                    if (style.ContainsKey("ShadowBlur")) try { PartShadow.BlurRadius = Convert.ToDouble(style["ShadowBlur"]); } catch { }
                    if (style.ContainsKey("ShadowDepth")) try { PartShadow.ShadowDepth = Convert.ToDouble(style["ShadowDepth"]); } catch { }
                    if (style.ContainsKey("ShadowOpacity")) try { PartShadow.Opacity = Convert.ToDouble(style["ShadowOpacity"]); } catch { }
                    if (style.ContainsKey("ShadowColor"))
                    {
                        try { PartShadow.Color = (Color)ColorConverter.ConvertFromString(style["ShadowColor"] as string); } catch { }
                    }
                }

                // 6. 字体与文字颜色
                if (style.ContainsKey("FontFamily"))
                {
                    string ff = style["FontFamily"] as string;
                    if (!string.IsNullOrEmpty(ff))
                    {
                        var fontFamily = new FontFamily(ff);
                        if (LabelBlock != null) LabelBlock.FontFamily = fontFamily;
                        if (MainDataGrid != null) MainDataGrid.FontFamily = fontFamily;
                    }
                }
                if (style.ContainsKey("FontSize") && MainDataGrid != null)
                {
                    try { MainDataGrid.FontSize = Convert.ToDouble(style["FontSize"]); } catch { }
                }
                if (style.ContainsKey("LabelColor"))
                {
                    string lc = style["LabelColor"] as string;
                    if (!string.IsNullOrEmpty(lc))
                    {
                        try {
                            var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lc.StartsWith("#") ? lc : "#" + lc));
                            this.Resources["DataGridLabelBrush"] = b;
                            if (LabelBlock != null) LabelBlock.Foreground = b;
                        } catch { }
                    }
                }
                if (style.ContainsKey("FontColor"))
                {
                    string fc = style["FontColor"] as string;
                    if (!string.IsNullOrEmpty(fc))
                    {
                        try {
                            Color c = (Color)ColorConverter.ConvertFromString(fc.StartsWith("#") ? fc : "#" + fc);
                            var b = new SolidColorBrush(c);
                            this.Resources["DataGridFontBrush"] = b;
                            this.Resources[SystemColors.HighlightTextBrushKey] = b;
                            this.Resources[SystemColors.InactiveSelectionHighlightTextBrushKey] = b;
                            if (MainDataGrid != null) MainDataGrid.Foreground = b;
                        } catch { }
                    }
                }

                // 7. 表头背景色
                string hbColor = null;
                if (style.ContainsKey("DataGridHeaderBackground")) hbColor = style["DataGridHeaderBackground"] as string;
                else if (style.ContainsKey("DataGridHeaderColor")) hbColor = style["DataGridHeaderColor"] as string;
                if (!string.IsNullOrEmpty(hbColor))
                {
                    try { this.HeaderColor = hbColor.StartsWith("#") ? hbColor : "#" + hbColor; } catch { }
                }
            }
            catch { }
        }

        #endregion
    }
}
