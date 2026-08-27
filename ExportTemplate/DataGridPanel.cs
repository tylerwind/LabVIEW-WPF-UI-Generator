using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WpfDataGrid
{
    [ToolboxItem(true)]
    [Description("极简圆角玻璃态数据表格")]
    public class DataGridPanel : WpfPanelBase
    {
        private ElementHost _host;
        private DataGridControl _wpfControl;

        public DataGridPanel()
        {
            try {
                this.BackColor = ColorTranslator.FromHtml("{{ControlBackground}}");
            } catch {
                this.BackColor = Color.White;
            }

            _host = new ElementHost
            {
                Dock = DockStyle.Fill,
                BackColorTransparent = true
            };

            _wpfControl = new DataGridControl();
            _host.Child = _wpfControl;

            this.Controls.Add(_host);
            this.SizeChanged += delegate { if (_host != null) _host.Invalidate(); };
        }

        public void BindDataTable(DataTable dt)
        {
            _wpfControl.BindDataTable(dt);
        }

        public void SetHeaders(string[] headers)
        {
            _wpfControl.SetHeaders(headers);
        }

        public void SetData(string[,] data)
        {
            _wpfControl.SetData(data);
        }

        public int AddRow(string[] rowData)
        {
            if (_wpfControl != null)
            {
                return _wpfControl.AddRow(rowData);
            }
            return -1;
        }

        public void Clear()
        {
            _wpfControl.Clear();
        }

        public object ItemsSource
        {
            get { return _wpfControl.Grid.ItemsSource; }
            set { _wpfControl.BindData(value); }
        }

        [Category("外观")]
        [Description("获取或设置控件标题文字")]
        public override string LabelText
        {
            get { return _wpfControl.LabelText; }
            set { _wpfControl.LabelText = value; }
        }

        [Category("外观")]
        [Description("获取或设置是否显示列标题(表头)")]
        public bool ShowHeader
        {
            get { return _wpfControl.ShowHeader; }
            set { _wpfControl.ShowHeader = value; }
        }

        [Category("外观")]
        [Description("获取或设置表格行高度")]
        public double RowHeight
        {
            get { return _wpfControl.RowHeight; }
            set { _wpfControl.RowHeight = value; }
        }

        [Category("外观")]
        [Description("获取或设置状态徽章字号大小")]
        public double BadgeFontSize
        {
            get { return _wpfControl.BadgeFontSize; }
            set { _wpfControl.BadgeFontSize = value; }
        }

        [Category("外观")]
        [Description("获取或设置表头颜色 (HEX)")]
        public string HeaderColor
        {
            get { return _wpfControl.HeaderColor; }
            set { _wpfControl.HeaderColor = value; }
        }

        [Category("外观")]
        [Description("获取或设置表头颜色 (数字)")]
        public int HeaderColorValue
        {
            get 
            { 
                if (_wpfControl == null) return 0;
                try {
                    var c = System.Drawing.ColorTranslator.FromHtml(_wpfControl.HeaderColor);
                    return (c.R << 16) | (c.G << 8) | c.B;
                } catch { return 0; }
            }
            set 
            { 
                if (_wpfControl != null)
                {
                    _wpfControl.HeaderColor = string.Format("#{0:X6}", value & 0xFFFFFF);
                }
            }
        }

        public override void SetLabelVisible(bool visible)
        {
            _wpfControl.SetLabelVisible(visible);
        }

        /// <summary>
        /// 设置标签文字 (UTF8 字节流方案，解决乱码)
        /// </summary>
        public override void SetLabelTextUTF8(byte[] bytes)
        {
            if (bytes == null) return;
            try { LabelText = System.Text.Encoding.UTF8.GetString(bytes); } catch { }
        }

        [Description("获取当前表格的所有表头列名")]
        public string[] GetHeaders()
        {
            return _wpfControl.GetHeaders();
        }

        [Description("获取当前表格的全部二维字符串数据")]
        public string[,] GetAllData()
        {
            return _wpfControl.GetAllData();
        }

        [Description("将文字和颜色值格式化为嵌入颜色标签的字符串以供 Badge 渲染")]
        public static string FormatBadge(string text, int colorValue)
        {
            return DataGridControl.FormatBadge(text, colorValue);
        }

        [Description("动态更新特定单元格的内容和背景色")]
        public void UpdateCell(int rowIndex, int colIndex, string value)
        {
            if (_wpfControl != null)
            {
                _wpfControl.UpdateCell(rowIndex, colIndex, value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _host != null) _host.Dispose();
            base.Dispose(disposing);
        }

        #region 运行时风格重绘

        public override void ApplyStyleDictionary(System.Collections.Generic.Dictionary<string, object> style)
        {
            base.ApplyStyleDictionary(style);
            if (style == null) return;
            try
            {
                if (_wpfControl != null)
                {
                    if (!_wpfControl.Dispatcher.CheckAccess())
                    {
                        _wpfControl.Dispatcher.Invoke(new Action(() => _wpfControl.ApplyStyle(style)));
                    }
                    else
                    {
                        _wpfControl.ApplyStyle(style);
                    }
                }
            }
            catch { }
        }

        #endregion
    }
}
