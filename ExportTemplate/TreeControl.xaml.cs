using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace {{Namespace}}
{
    public partial class TreeControl : UserControl
    {
        public ObservableCollection<TreeNode> RootNodes { get; private set; }
        
        // Lookup dictionary for quick access by ID
        private Dictionary<string, TreeNode> _nodeDictionary;

        public event EventHandler<NodeExpandedEventArgs> NodeExpanding;
        public event EventHandler<NodeSelectedEventArgs> NodeSelected;
        public event EventHandler<NodeCheckedEventArgs> NodeChecked;
        public event EventHandler<NodeDoubleClickedEventArgs> NodeDoubleClicked;
        public event EventHandler<NodeMenuClickedEventArgs> NodeMenuClicked;

        private Brush _customMenuBackground = null;

        public TreeControl()
        {
            InitializeComponent();
            
            RootNodes = new ObservableCollection<TreeNode>();
            _nodeDictionary = new Dictionary<string, TreeNode>();
            
            // Build the HierarchicalDataTemplate
            var template = new HierarchicalDataTemplate(typeof(TreeNode));
            template.ItemsSource = new System.Windows.Data.Binding("Children");
            
            // Create Grid factory
            var gridFactory = new FrameworkElementFactory(typeof(StackPanel));
            gridFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            // Add Image (Icon)
            var imageFactory = new FrameworkElementFactory(typeof(Image));
            imageFactory.SetBinding(Image.SourceProperty, new System.Windows.Data.Binding("IconSource"));
            imageFactory.SetValue(Image.WidthProperty, 16.0);
            imageFactory.SetValue(Image.HeightProperty, 16.0);
            imageFactory.SetValue(Image.MarginProperty, new Thickness(2, 0, 4, 0));
            // Only show if IconSource is not null
            var iconVisibilityBinding = new System.Windows.Data.Binding("IconSource");
            iconVisibilityBinding.Converter = new NullToVisibilityConverter();
            imageFactory.SetBinding(Image.VisibilityProperty, iconVisibilityBinding);
            gridFactory.AppendChild(imageFactory);

            // Add TextBlock
            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Text"));
            textBlockFactory.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("{{FontColor}}")));
            textBlockFactory.SetValue(TextBlock.FontFamilyProperty, new FontFamily("{{FontFamily}}"));
            textBlockFactory.SetValue(TextBlock.FontSizeProperty, (double){{FontSize}});
            textBlockFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            gridFactory.AppendChild(textBlockFactory);
            
            template.VisualTree = gridFactory;
            InnerTree.ItemTemplate = template;
            InnerTree.ItemsSource = RootNodes;

            // Handle selection event from TreeView
            InnerTree.SelectedItemChanged += InnerTree_SelectedItemChanged;
            InnerTree.MouseDoubleClick += InnerTree_MouseDoubleClick;
            // Handle context menu right click
            InnerTree.PreviewMouseRightButtonDown += InnerTree_PreviewMouseRightButtonDown;
        }

        private void InnerTree_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DependencyObject source = e.OriginalSource as DependencyObject;
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            
            TreeViewItem treeViewItem = source as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.Focus(); // 确保右键能够强制选中目标节点
                e.Handled = true;     // 彻底切断底层其余鼠标事件抢占

                var node = treeViewItem.DataContext as TreeNode;
                if (node != null)
                {
                    if (node.ContextMenuItems != null && node.ContextMenuItems.Length > 0)
                    {
                        var menu = new ContextMenu();
                        menu.Style = this.TryFindResource("FlatContextMenuStyle") as Style;
                        if (_customMenuBackground != null) menu.Background = _customMenuBackground;
                        menu.PlacementTarget = treeViewItem;
                        foreach (var itemText in node.ContextMenuItems)
                        {
                            var mi = new MenuItem { Header = itemText };
                            mi.Style = this.TryFindResource("FlatMenuItemStyle") as Style;
                            mi.Click += (s, args) => {
                                if (NodeMenuClicked != null)
                                    NodeMenuClicked(this, new NodeMenuClickedEventArgs { NodeId = node.Id, MenuText = itemText });
                            };
                            menu.Items.Add(mi);
                        }
                        // 利用输入队列优先级推迟到鼠标抬起后执行弹拉操作，避开阻断
                        this.Dispatcher.BeginInvoke(new Action(() => {
                            menu.IsOpen = true;
                        }), System.Windows.Threading.DispatcherPriority.Input);
                    }
                    else 
                    {
                        treeViewItem.ContextMenu = null;
                    }
                }
            }
        }

        private void InnerTree_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                var node = InnerTree.SelectedItem as TreeNode;
                if (node != null && NodeDoubleClicked != null)
                {
                    NodeDoubleClicked(this, new NodeDoubleClickedEventArgs { NodeId = node.Id, NodeText = node.Text });
                }
            }
        }

        private void InnerTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeNode node = e.NewValue as TreeNode;
            if (node != null)
            {
                if (NodeSelected != null) NodeSelected(this, new NodeSelectedEventArgs { NodeId = node.Id, NodeText = node.Text });
            }
        }

        public void SetLabelVisible(bool isVisible)
        {
            MainLabel.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public bool AddNode(string id, string parentId, string text, bool isChecked = false, bool showCheckBox = true, bool hasDummyChild = false, string iconPath = null)
        {
            try 
            {
                if (_nodeDictionary.ContainsKey(id))
                {
                    return false; // ID already exists
                }

                var newNode = new TreeNode(this)
                {
                    Id = id,
                    Text = text,
                    IsChecked = isChecked,
                    ShowCheckBox = showCheckBox,
                    IconPath = iconPath
                };

                // Setup for lazy loading if requested
                if (hasDummyChild)
                {
                    newNode.Children.Add(new TreeNode(this) { Id = "dummy_" + id, Text = "Loading...", ParentNode = newNode });
                }

                if (string.IsNullOrEmpty(parentId) || !_nodeDictionary.ContainsKey(parentId))
                {
                    // Add to root
                    RootNodes.Add(newNode);
                }
                else
                {
                    // Add to parent
                    var parent = _nodeDictionary[parentId];
                    
                    // If the parent only had a dummy child, remove it first
                    if (parent.Children.Count == 1 && parent.Children[0].Id.StartsWith("dummy_"))
                    {
                        parent.Children.Clear();
                    }
                    
                    newNode.ParentNode = parent;
                    parent.Children.Add(newNode);
                }

                _nodeDictionary[id] = newNode;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool RemoveNode(string id)
        {
            TreeNode node;

            if (!_nodeDictionary.TryGetValue(id, out node))
                return false;

            // O(1) Remove using ParentNode
            if (node.ParentNode == null)
            {
                RootNodes.Remove(node);
            }
            else
            {
                node.ParentNode.Children.Remove(node);
            }

            RemoveFromDictionaryRecursive(node);
            return true;
        }



        private void RemoveFromDictionaryRecursive(TreeNode node)
        {
            _nodeDictionary.Remove(node.Id);
            foreach (var child in node.Children)
            {
                RemoveFromDictionaryRecursive(child);
            }
        }

        public void ClearNodes()
        {
            RootNodes.Clear();
            _nodeDictionary.Clear();
        }

        public TreeNode GetNode(string id)
        {
            TreeNode node;

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
            TreeNode node;

            if (_nodeDictionary.TryGetValue(id, out node))
            {
                node.IsChecked = isChecked;
            }
        }

        public void UpdateNodeText(string id, string text)
        {
            TreeNode node;
            if (_nodeDictionary.TryGetValue(id, out node))
            {
                node.Text = text;
            }
        }

        public void UpdateNodeIcon(string id, string iconPath)
        {
            TreeNode node;
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
            TreeNode node;
            if (_nodeDictionary.TryGetValue(id, out node))
            {
                node.ContextMenuItems = menuItems;
            }
        }

        public void ExpandNode(string id)
        {
            TreeNode node;

            if (_nodeDictionary.TryGetValue(id, out node))
            {
                node.IsExpanded = true;
                
                // 向上递归展开所有父级，否则子节点在闭合的父级内部是看不见的
                TreeNode parent = node.ParentNode;
                while (parent != null)
                {
                    parent.IsExpanded = true;
                    parent = parent.ParentNode;
                }
            }
        }

        public void CollapseNode(string id)
        {
            TreeNode node;

            if (_nodeDictionary.TryGetValue(id, out node))
            {
                node.IsExpanded = false;
            }
        }

        public void ExpandAll()
        {
            foreach (var node in _nodeDictionary.Values)
            {
                node.IsExpanded = true;
            }
        }

        public void CollapseAll()
        {
            foreach (var node in _nodeDictionary.Values)
            {
                node.IsExpanded = false;
            }
        }

        internal void RaiseNodeExpanding(TreeNode node)
        {
            if (NodeExpanding != null) NodeExpanding(this, new NodeExpandedEventArgs { NodeId = node.Id });
        }

        internal void RaiseNodeChecked(TreeNode node)
        {
            if (NodeChecked != null) NodeChecked(this, new NodeCheckedEventArgs { NodeId = node.Id, IsChecked = node.IsChecked });
        }
    }

    public class TreeNode : INotifyPropertyChanged
    {
        private TreeControl _owner;
        private string _id;
        private string _text;
        private bool _isChecked;
        private bool _isExpanded;
        private bool _showCheckBox = true;
        private string _iconPath;
        private string[] _contextMenuItems;
        
        internal TreeNode ParentNode { get; set; }

        public ObservableCollection<TreeNode> Children { get; private set; }

        public TreeNode(TreeControl owner)
        {
            _owner = owner;
            Children = new ObservableCollection<TreeNode>();
        }

        public string Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged("Id"); }
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; OnPropertyChanged("Text"); }
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
                    if (_isExpanded)
                    {
                        if (_owner != null) _owner.RaiseNodeExpanding(this);
                    }
                }
            }
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

        public System.Windows.Media.ImageSource IconSource
        {
            get {
                if (string.IsNullOrEmpty(_iconPath)) return null;
                try { 
                    var bm = new System.Windows.Media.Imaging.BitmapImage();
                    bm.BeginInit();
                    bm.UriSource = new Uri(_iconPath, UriKind.RelativeOrAbsolute);
                    bm.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad; // 防止文件被锁定
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

        public class NullToVisibilityConverter : System.Windows.Data.IValueConverter
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

public class NodeExpandedEventArgs : EventArgs
    {
        public string NodeId { get; set; }
    }

    public class NodeDoubleClickedEventArgs : EventArgs
    {
        public string NodeId { get; set; }
        public string NodeText { get; set; }
    }

    public class NodeSelectedEventArgs : EventArgs
    {
        public string NodeId { get; set; }
        public string NodeText { get; set; }
    }

    public class NodeCheckedEventArgs : EventArgs
    {
        public string NodeId { get; set; }
        public bool IsChecked { get; set; }
    }

    public class NodeMenuClickedEventArgs : EventArgs
    {
        public string NodeId { get; set; }
        public string MenuText { get; set; }
    }
}
