using System;
using System.Data;
using System.Windows.Controls;

namespace WpfDataGrid
{
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

        public DataGridControl()
        {
            InitializeComponent();
            _internalTable = new DataTable();
            MainDataGrid.ItemsSource = _internalTable.DefaultView;
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

        public void AddRow(string[] rowData)
        {
            if (_internalTable == null || _internalTable.Columns.Count == 0) return;
            DataRow dr = _internalTable.NewRow();
            for (int i = 0; i < Math.Min(rowData.Length, _internalTable.Columns.Count); i++)
            {
                dr[i] = rowData[i];
            }
            _internalTable.Rows.Add(dr);
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
    }
}
