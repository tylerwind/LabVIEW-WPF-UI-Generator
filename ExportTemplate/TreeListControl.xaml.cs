using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;

namespace {{Namespace}}
{
    public partial class TreeListControl : UserControl
    {
        private ObservableCollection<TreeListNode> _flatList;
        private Dictionary<string, TreeListNode> _nodeDictionary;
        private List<TreeListNode> _rootNodes;

        public event EventHandler<TreeList_NodeExpandedEventArgs> NodeExpanding;
        public event EventHandler<TreeList_NodeSelectedEventArgs> NodeSelected;
        public event EventHandler<TreeList_NodeCheckedEventArgs> NodeChecked;
        public event EventHandler<TreeList_NodeDoubleClickedEventArgs> NodeDoubleClicked;
        public event EventHandler<TreeList_NodeMenuClickedEventArgs> NodeMenuClicked;

        private Brush _customMenuBackground = null;
        private string[] _columnHeaders;
        private double[] _columnWidthsInput;

        public TreeListControl()
        {
            InitializeComponent();

            _flatList = new ObservableCollection<TreeListNode>();
            _nodeDictionary = new Dictionary<string, TreeListNode>();
            _rootNodes = new List<TreeListNode>();

            InnerList.ItemsSource = _flatList;

            InnerList.SelectionChanged += InnerList_SelectionChanged;
            InnerList.MouseDoubleClick += InnerList_MouseDoubleClick;
            InnerList.PreviewMouseRightButtonDown += InnerList_PreviewMouseRightButtonDown;
            InnerList.SizeChanged += (s, e) => AutoSizeColumns();
        }

        public void SetColumns(string[] headers, double[] widths)
        {
            _columnHeaders = headers;
            _columnWidthsInput = widths;
            InnerGridView.Columns.Clear();
            if (headers == null) return;

            for (int i = 0; i < headers.Length; i++)
            {
                var col = new GridViewColumn();
                col.Header = headers[i];

                if (i == 0)
                {
                    col.CellTemplate = this.Resources["FirstColumnTemplate"] as DataTemplate;
                }
                else
                {
                    var factory = new FrameworkElementFactory(typeof(TextBlock));
                    factory.SetBinding(TextBlock.TextProperty, new Binding(string.Format("ColumnTexts[{0}]", i)));
                    factory.SetResourceReference(TextBlock.ForegroundProperty, "TreeListFontBrush");
                    factory.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
                    factory.SetValue(MarginProperty, new Thickness(4, 0, 4, 0));
                    col.CellTemplate = new DataTemplate { VisualTree = factory };
                }

                InnerGridView.Columns.Add(col);
            }

            AutoSizeColumns();
        }

        private void AutoSizeColumns()
        {
            if (InnerGridView.Columns.Count == 0) return;

            double totalWidth = InnerList.ActualWidth;
            if (totalWidth <= 0) return;

            // 预留滚动条和边缘宽度 (约 25 像素)
            double availableWidth = totalWidth - 25;
            if (availableWidth < 100) availableWidth = 100;

            int columnCount = InnerGridView.Columns.Count;

            // 找出固定宽度和自适应宽度的列
            var fixedWidths = new double[columnCount];
            int autoColCount = 0;
            double sumFixed = 0;

            for (int i = 0; i < columnCount; i++)
            {
                double w = -1;
                if (_columnWidthsInput != null && i < _columnWidthsInput.Length)
                {
                    w = _columnWidthsInput[i];
                }

                if (w > 0)
                {
                    fixedWidths[i] = w;
                    sumFixed += w;
                }
                else
                {
                    fixedWidths[i] = -1; // 表示需要自适应
                    autoColCount++;
                }
            }

            if (autoColCount > 0)
            {
                double remaining = availableWidth - sumFixed;
                double avgWidth = remaining / autoColCount;
                if (avgWidth < 80) avgWidth = 80; // 设置最小列宽

                for (int i = 0; i < columnCount; i++)
                {
                    if (fixedWidths[i] > 0)
                    {
                        InnerGridView.Columns[i].Width = fixedWidths[i];
                    }
                    else
                    {
                        InnerGridView.Columns[i].Width = avgWidth;
                    }
                }
            }
            else
            {
                // 如果全是固定宽度，把剩余空间补给最后一列以撑满
                for (int i = 0; i < columnCount; i++)
                {
                    InnerGridView.Columns[i].Width = fixedWidths[i];
                }
                if (availableWidth > sumFixed)
                {
                    InnerGridView.Columns[columnCount - 1].Width = fixedWidths[columnCount - 1] + (availableWidth - sumFixed);
                }
            }
        }

        public void SetLabelVisible(bool isVisible)
        {
            MainLabel.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public bool AddNode(string id, string parentId, string[] columnTexts, bool isChecked = false, bool showCheckBox = true, bool hasDummyChild = false, string iconPath = null)
        {
            try
            {
                // 安全防线：如果未配置列首，自动按一列默认显示（与单列树兼容）
                if (InnerGridView.Columns.Count == 0)
                {
                    SetColumns(new string[] { "Nodes" }, new double[] { 0 });
                }

                if (_nodeDictionary.ContainsKey(id))
                    return false;

                var newNode = new TreeListNode(this)
                {
                    Id = id,
                    ParentId = parentId,
                    ColumnTexts = columnTexts,
                    IsChecked = isChecked,
                    ShowCheckBox = showCheckBox,
                    IconPath = iconPath
                };

                if (hasDummyChild)
                {
                    newNode.Children.Add(new TreeListNode(this) { Id = "dummy_" + id, ColumnTexts = new[] { "Loading..." }, ParentNode = newNode, IndentLevel = 1 });
                    newNode.HasChildren = true;
                }

                if (string.IsNullOrEmpty(parentId) || !_nodeDictionary.ContainsKey(parentId))
                {
                    newNode.IndentLevel = 0;
                    _rootNodes.Add(newNode);
                    _flatList.Add(newNode);
                }
                else
                {
                    var parent = _nodeDictionary[parentId];
                    
                    if (parent.Children.Count == 1 && parent.Children[0].Id.StartsWith("dummy_"))
                    {
                        parent.Children.Clear();
                    }

                    newNode.IndentLevel = parent.IndentLevel + 1;
                    newNode.ParentNode = parent;
                    parent.Children.Add(newNode);
                    parent.HasChildren = true;

                    // 仅当父节点当前为展开状态时，才将新子节点直接插入到 _flatList
                    if (parent.IsExpanded)
                    {
                        int parentIdx = _flatList.IndexOf(parent);
                        if (parentIdx >= 0)
                        {
                            // 找到父节点的最后一个子孙节点的位置
                            int insertIdx = parentIdx + 1;
                            while (insertIdx < _flatList.Count && _flatList[insertIdx].IndentLevel > parent.IndentLevel)
                            {
                                insertIdx++;
                            }
                            _flatList.Insert(insertIdx, newNode);
                            
                            // 如果带有 dummy child 并且当前节点展开了，还要插入 children
                            if (newNode.IsExpanded)
                            {
                                InsertChildrenToFlatList(newNode, ref insertIdx);
                            }
                        }
                    }
                }

                _nodeDictionary[id] = newNode;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveNode(string id)
        {
            TreeListNode node;
            if (!_nodeDictionary.TryGetValue(id, out node))
                return false;

            if (node.ParentNode == null)
            {
                _rootNodes.Remove(node);
            }
            else
            {
                node.ParentNode.Children.Remove(node);
                if (node.ParentNode.Children.Count == 0)
                    node.ParentNode.HasChildren = false;
            }

            RemoveNodeFromFlatList(node);
            RemoveFromDictionaryRecursive(node);
            return true;
        }

        private void RemoveNodeFromFlatList(TreeListNode node)
        {
            int idx = _flatList.IndexOf(node);
            if (idx >= 0)
            {
                _flatList.RemoveAt(idx);
                // 移除其下方所有层级更深的子孙节点
                while (idx < _flatList.Count && _flatList[idx].IndentLevel > node.IndentLevel)
                {
                    _flatList.RemoveAt(idx);
                }
            }
        }

        private void RemoveFromDictionaryRecursive(TreeListNode node)
        {
            _nodeDictionary.Remove(node.Id);
            foreach (var child in node.Children)
            {
                RemoveFromDictionaryRecursive(child);
            }
        }

        public void ClearNodes()
        {
            _rootNodes.Clear();
            _nodeDictionary.Clear();
            _flatList.Clear();
        }

        public TreeListNode GetNode(string id)
        {
            TreeListNode node;
            if (_nodeDictionary.TryGetValue(id, out node))
                return node;
            return null;
        }

        public List<string> GetCheckedNodes()
        {
            return _nodeDictionary.Values.Where(n => n.IsChecked).Select(n => n.Id).ToList();
        }

        public void SetNodeChecked(string id, bool isChecked)
        {
            TreeListNode node;
            if (_nodeDictionary.TryGetValue(id, out node))
            {
                node.IsChecked = isChecked;
            }
        }

        public void UpdateNodeText(string id, string text)
        {
            TreeListNode node;
            if (_nodeDictionary.TryGetValue(id, out node))
            {
                if (node.ColumnTexts == null || node.ColumnTexts.Length == 0)
                {
                    node.ColumnTexts = new[] { text };
                }
                else
                {
                    var newCols = (string[])node.ColumnTexts.Clone();
                    newCols[0] = text;
                    node.ColumnTexts = newCols;
                }
            }
        }

        public void UpdateNodeColumns(string id, string[] columns)
        {
            TreeListNode node;
            if (_nodeDictionary.TryGetValue(id, out node))
            {
                node.ColumnTexts = columns;
            }
        }

        public string[] GetNodeColumnTexts(string id)
        {
            TreeListNode node;
            if (_nodeDictionary.TryGetValue(id, out node) && node.ColumnTexts != null)
            {
                return (string[])node.ColumnTexts.Clone();
            }
            return new string[0];
        }

        public string GetNodeColumnText(string id, int columnIndex)
        {
            TreeListNode node;
            if (_nodeDictionary.TryGetValue(id, out node) && node.ColumnTexts != null && columnIndex >= 0 && columnIndex < node.ColumnTexts.Length)
            {
                return node.ColumnTexts[columnIndex] ?? string.Empty;
            }
            return string.Empty;
        }

        public string GetParentNodeId(string id)
        {
            TreeListNode node;
            if (_nodeDictionary.TryGetValue(id, out node) && node.ParentNode != null)
            {
                return node.ParentNode.Id ?? string.Empty;
            }
            return string.Empty;
        }

        public TreeListNode GetParentNode(string id)
        {
            TreeListNode node;
            if (_nodeDictionary.TryGetValue(id, out node))
            {
                return node.ParentNode;
            }
            return null;
        }

        public string[] GetParentNodeColumnTexts(string id)
        {
            TreeListNode node;
            if (_nodeDictionary.TryGetValue(id, out node) && node.ParentNode != null && node.ParentNode.ColumnTexts != null)
            {
                return (string[])node.ParentNode.ColumnTexts.Clone();
            }
            return new string[0];
        }

        public void UpdateNodeIcon(string id, string iconPath)
        {
            TreeListNode node;
            if (_nodeDictionary.TryGetValue(id, out node))
            {
                node.IconPath = iconPath;
            }
        }

        public void SetTreeBackground(uint color)
        {
            CardBackgroundBorder.Background = UintToBrush(color);
        }

        public void SetMenuBackground(uint color)
        {
            _customMenuBackground = UintToBrush(color);
        }

        private Brush UintToBrush(uint color)
        {
            byte a = (byte)((color >> 24) & 0xFF);
            byte r = (byte)((color >> 16) & 0xFF);
            byte g = (byte)((color >> 8) & 0xFF);
            byte b = (byte)(color & 0xFF);
            if (a == 0 && color != 0) a = 255;
            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }

        public void SetNodeContextMenu(string id, string[] menuItems)
        {
            TreeListNode node;
            if (_nodeDictionary.TryGetValue(id, out node))
            {
                node.ContextMenuItems = menuItems;
            }
        }

        public void ExpandNode(string id)
        {
            TreeListNode node;
            if (_nodeDictionary.TryGetValue(id, out node))
            {
                node.IsExpanded = true;
                
                TreeListNode parent = node.ParentNode;
                while (parent != null)
                {
                    parent.IsExpanded = true;
                    parent = parent.ParentNode;
                }
            }
        }

        public void CollapseNode(string id)
        {
            TreeListNode node;
            if (_nodeDictionary.TryGetValue(id, out node))
            {
                node.IsExpanded = false;
            }
        }

        internal void RaiseNodeExpanding(TreeListNode node)
        {
            if (NodeExpanding != null) NodeExpanding(this, new TreeList_NodeExpandedEventArgs { NodeId = node.Id });
        }

        internal void RaiseNodeChecked(TreeListNode node)
        {
            if (NodeChecked != null) NodeChecked(this, new TreeList_NodeCheckedEventArgs { NodeId = node.Id, IsChecked = node.IsChecked });
        }

        internal void HandleNodeExpandedChanged(TreeListNode node)
        {
            int idx = _flatList.IndexOf(node);
            if (idx < 0) return;

            if (node.IsExpanded)
            {
                idx++;
                InsertChildrenToFlatList(node, ref idx);
                RaiseNodeExpanding(node);
            }
            else
            {
                idx++;
                while (idx < _flatList.Count && _flatList[idx].IndentLevel > node.IndentLevel)
                {
                    _flatList.RemoveAt(idx);
                }
            }
        }

        private void InsertChildrenToFlatList(TreeListNode node, ref int insertIdx)
        {
            foreach (var child in node.Children)
            {
                _flatList.Insert(insertIdx, child);
                insertIdx++;
                if (child.IsExpanded)
                {
                    InsertChildrenToFlatList(child, ref insertIdx);
                }
            }
        }

        private void InnerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var node = InnerList.SelectedItem as TreeListNode;
            if (node != null && NodeSelected != null)
            {
                string text = null;
                if (node.ColumnTexts != null && node.ColumnTexts.Length > 0) text = node.ColumnTexts[0];
                NodeSelected(this, new TreeList_NodeSelectedEventArgs { NodeId = node.Id, NodeText = text });
            }
        }

        private void InnerList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var node = InnerList.SelectedItem as TreeListNode;
                if (node != null && NodeDoubleClicked != null)
                {
                    string text = null;
                    if (node.ColumnTexts != null && node.ColumnTexts.Length > 0) text = node.ColumnTexts[0];
                    NodeDoubleClicked(this, new TreeList_NodeDoubleClickedEventArgs { NodeId = node.Id, NodeText = text });
                }
            }
        }

        private void InnerList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject source = e.OriginalSource as DependencyObject;
            while (source != null && !(source is ListViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }

            ListViewItem lvi = source as ListViewItem;
            if (lvi != null)
            {
                lvi.Focus();
                e.Handled = true;

                var node = lvi.DataContext as TreeListNode;
                if (node != null)
                {
                    if (node.ContextMenuItems != null && node.ContextMenuItems.Length > 0)
                    {
                        var menu = new ContextMenu();
                        menu.Style = this.TryFindResource("FlatContextMenuStyle") as Style;
                        if (_customMenuBackground != null) menu.Background = _customMenuBackground;
                        menu.PlacementTarget = lvi;
                        foreach (var itemText in node.ContextMenuItems)
                        {
                            var mi = new MenuItem { Header = itemText };
                            mi.Style = this.TryFindResource("FlatMenuItemStyle") as Style;
                            mi.Click += (s, args) => {
                                if (NodeMenuClicked != null)
                                    NodeMenuClicked(this, new TreeList_NodeMenuClickedEventArgs { NodeId = node.Id, MenuText = itemText });
                            };
                            menu.Items.Add(mi);
                        }
                        this.Dispatcher.BeginInvoke(new Action(() => {
                            menu.IsOpen = true;
                        }), System.Windows.Threading.DispatcherPriority.Input);
                    }
                }
            }
        }

        #region 运行时风格重绘

        public void ApplyStyle(System.Collections.Generic.Dictionary<string, object> style)
        {
            if (style == null) return;
            try
            {
                // 1. 树节点文字颜色 (FontColor)
                if (style.ContainsKey("FontColor"))
                {
                    string fc = style["FontColor"] as string;
                    if (!string.IsNullOrEmpty(fc))
                    {
                        try {
                            var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fc.StartsWith("#") ? fc : "#" + fc));
                            this.Resources["TreeListFontBrush"] = b;
                            if (InnerList != null) InnerList.Foreground = b;
                        } catch { }
                    }
                }

                // 2. 树卡片主体背景色 (TreeBackground / ControlBackground)
                string tbColor = null;
                if (style.ContainsKey("TreeBackground")) tbColor = style["TreeBackground"] as string;
                else if (style.ContainsKey("ControlBackground")) tbColor = style["ControlBackground"] as string;
                if (!string.IsNullOrEmpty(tbColor))
                {
                    try { 
                        var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tbColor.StartsWith("#") ? tbColor : "#" + tbColor));
                        this.Resources["TreeListBgBrush"] = b;
                        if (CardBackgroundBorder != null) CardBackgroundBorder.Background = b;
                    } catch { }
                }

                // 3. 表头背景色 (DataGridHeaderBackground / DataGridHeaderColor)
                string hbColor = null;
                if (style.ContainsKey("DataGridHeaderBackground")) hbColor = style["DataGridHeaderBackground"] as string;
                else if (style.ContainsKey("DataGridHeaderColor")) hbColor = style["DataGridHeaderColor"] as string;
                if (!string.IsNullOrEmpty(hbColor))
                {
                    try {
                        var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hbColor.StartsWith("#") ? hbColor : "#" + hbColor));
                        this.Resources["TreeListHeaderBgBrush"] = b;
                    } catch { }
                }

                // 4. 边框颜色与粗细
                if (style.ContainsKey("BorderColor"))
                {
                    string bc = style["BorderColor"] as string;
                    if (!string.IsNullOrEmpty(bc))
                    {
                        try {
                            var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bc.StartsWith("#") ? bc : "#" + bc));
                            this.Resources["TreeListBorderBrush"] = b;
                            if (CardBorder != null) CardBorder.BorderBrush = b;
                        } catch { }
                    }
                }
                if (style.ContainsKey("BorderThickness") && CardBorder != null)
                {
                    try { CardBorder.BorderThickness = new Thickness(Convert.ToDouble(style["BorderThickness"])); } catch { }
                }

                // 5. 强调色 (AccentColor)
                if (style.ContainsKey("AccentColor"))
                {
                    string ac = style["AccentColor"] as string;
                    if (!string.IsNullOrEmpty(ac))
                    {
                        try {
                            this.Resources["TreeListAccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ac.StartsWith("#") ? ac : "#" + ac));
                        } catch { }
                    }
                }

                // 6. 圆角
                if (style.ContainsKey("CornerRadius"))
                {
                    try {
                        var cr = new CornerRadius(Convert.ToDouble(style["CornerRadius"]));
                        if (CardBackgroundBorder != null) CardBackgroundBorder.CornerRadius = cr;
                        if (CardBorder != null) CardBorder.CornerRadius = cr;
                    } catch { }
                }

                // 7. 标签颜色与字体
                if (style.ContainsKey("FontFamily"))
                {
                    string ff = style["FontFamily"] as string;
                    if (!string.IsNullOrEmpty(ff))
                    {
                        var fontFamily = new FontFamily(ff);
                        if (MainLabel != null) MainLabel.FontFamily = fontFamily;
                        if (InnerList != null) InnerList.FontFamily = fontFamily;
                    }
                }
                if (style.ContainsKey("FontSize") && InnerList != null)
                {
                    try { InnerList.FontSize = Convert.ToDouble(style["FontSize"]); } catch { }
                }
                if (style.ContainsKey("LabelColor"))
                {
                    string lc = style["LabelColor"] as string;
                    if (!string.IsNullOrEmpty(lc))
                    {
                        try {
                            var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lc.StartsWith("#") ? lc : "#" + lc));
                            this.Resources["TreeListLabelBrush"] = b;
                            if (MainLabel != null) MainLabel.Foreground = b;
                        } catch { }
                    }
                }
            }
            catch { }
        }

        #endregion
    }

    public class TreeListNode : INotifyPropertyChanged
    {
        private TreeListControl _owner;
        private string _id;
        private string[] _columnTexts;
        private bool _isChecked;
        private bool _isExpanded;
        private bool _showCheckBox = true;
        private string _iconPath;
        private string[] _contextMenuItems;
        private bool _hasChildren;
        private int _indentLevel;

        public TreeListNode ParentNode { get; internal set; }
        public string ParentId { get; set; }
        public ObservableCollection<TreeListNode> Children { get; private set; }

        public TreeListNode(TreeListControl owner)
        {
            _owner = owner;
            Children = new ObservableCollection<TreeListNode>();
        }

        public string Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged("Id"); }
        }

        public string[] ColumnTexts
        {
            get { return _columnTexts; }
            set { _columnTexts = value; OnPropertyChanged("ColumnTexts"); }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set 
            { 
                if (_isChecked != value)
                {
                    _isChecked = value; 
                    OnPropertyChanged("IsChecked");
                    if (_owner != null) _owner.RaiseNodeChecked(this);
                }
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set 
            { 
                if (_isExpanded != value)
                {
                    _isExpanded = value; 
                    OnPropertyChanged("IsExpanded");
                    if (_owner != null) _owner.HandleNodeExpandedChanged(this);
                }
            }
        }

        public int IndentLevel
        {
            get { return _indentLevel; }
            set { _indentLevel = value; OnPropertyChanged("IndentLevel"); }
        }

        public bool HasChildren
        {
            get { return _hasChildren; }
            set { _hasChildren = value; OnPropertyChanged("HasChildren"); }
        }

        public string[] ContextMenuItems
        {
            get { return _contextMenuItems; }
            set { _contextMenuItems = value; OnPropertyChanged("ContextMenuItems"); }
        }

        public string IconPath
        {
            get { return _iconPath; }
            set { _iconPath = value; OnPropertyChanged("IconPath"); OnPropertyChanged("IconSource"); }
        }

        public ImageSource IconSource
        {
            get {
                if (string.IsNullOrEmpty(_iconPath)) return null;
                try { 
                    var bm = new System.Windows.Media.Imaging.BitmapImage();
                    bm.BeginInit();
                    bm.UriSource = new Uri(_iconPath, UriKind.RelativeOrAbsolute);
                    bm.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bm.EndInit();
                    return bm;
                }
                catch { return null; }
            }
        }

        public bool ShowCheckBox
        {
            get { return _showCheckBox; }
            set { _showCheckBox = value; OnPropertyChanged("ShowCheckBox"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }

    public class TreeList_NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TreeList_IndentConverter : IValueConverter
    {
        private double _indentSize = 24;
        public double IndentSize { get { return _indentSize; } set { _indentSize = value; } }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int)
            {
                int level = (int)value;
                return new Thickness(level * IndentSize, 0, 0, 0);
            }
            return new Thickness(0);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TreeList_NodeExpandedEventArgs : EventArgs
    {
        public string NodeId { get; set; }
    }

    public class TreeList_NodeDoubleClickedEventArgs : EventArgs
    {
        public string NodeId { get; set; }
        public string NodeText { get; set; }
    }

    public class TreeList_NodeSelectedEventArgs : EventArgs
    {
        public string NodeId { get; set; }
        public string NodeText { get; set; }
    }

    public class TreeList_NodeCheckedEventArgs : EventArgs
    {
        public string NodeId { get; set; }
        public bool IsChecked { get; set; }
    }

    public class TreeList_NodeMenuClickedEventArgs : EventArgs
    {
        public string NodeId { get; set; }
        public string MenuText { get; set; }
    }
}
