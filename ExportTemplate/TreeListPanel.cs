using System;
using System.ComponentModel;
using System.Windows.Forms.Integration;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace {{Namespace}}
{
    [ToolboxBitmap(typeof(System.Windows.Forms.TreeView))]
    public class TreeListPanel : WpfPanelBase
    {
        private ElementHost _host;
        private TreeListControl _wpfControl;

        public TreeListControl WpfControl { get { return _wpfControl; } }

        public TreeListPanel()
        {
            try {
                this.BackColor = ColorTranslator.FromHtml("{{ControlBackground}}");
            } catch {
                this.BackColor = Color.White;
            }
            _host = new ElementHost
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                BackColorTransparent = true
            };
            _wpfControl = new TreeListControl();
            _host.Child = _wpfControl;
            this.Controls.Add(_host);
            this.Dock = System.Windows.Forms.DockStyle.Fill;

            _wpfControl.NodeExpanding += (s, e) => { if (NodeExpanding != null) NodeExpanding(e.NodeId); };
            _wpfControl.NodeSelected += (s, e) => {
                if (NodeSelected != null) {
                    byte[] utf8 = string.IsNullOrEmpty(e.NodeText) ? new byte[0] : Encoding.UTF8.GetBytes(e.NodeText);
                    NodeSelected(e.NodeId, e.NodeText ?? "", utf8);
                }
            };
            _wpfControl.NodeChecked += (s, e) => { if (NodeChecked != null) NodeChecked(e.NodeId, e.IsChecked); };
            _wpfControl.NodeDoubleClicked += (s, e) => {
                if (NodeDoubleClicked != null) {
                    byte[] utf8 = string.IsNullOrEmpty(e.NodeText) ? new byte[0] : Encoding.UTF8.GetBytes(e.NodeText);
                    NodeDoubleClicked(e.NodeId, e.NodeText ?? "", utf8);
                }
            };
            _wpfControl.NodeMenuClicked += (s, e) => {
                if (NodeMenuClicked != null) {
                    byte[] utf8 = string.IsNullOrEmpty(e.MenuText) ? new byte[0] : Encoding.UTF8.GetBytes(e.MenuText);
                    NodeMenuClicked(e.NodeId, e.MenuText ?? "", utf8);
                }
            };
        }

        // ========================
        // Explicit Delegates 明确的委托定义 (100% 支持 LabVIEW)
        // ========================
        public delegate void NodeExpandedEventHandler(string nodeId);
        public delegate void NodeSelectedEventHandler(string nodeId, string nodeText, byte[] nodeTextUTF8);
        public delegate void NodeCheckedEventHandler(string nodeId, bool isChecked);
        public delegate void NodeDoubleClickedEventHandler(string nodeId, string nodeText, byte[] nodeTextUTF8);
        public delegate void NodeMenuClickedEventHandler(string nodeId, string menuText, byte[] menuTextUTF8);

        [Category("LabVIEW Events"), Description("Fired when a node is expanding.")]
        public event NodeExpandedEventHandler NodeExpanding;

        [Category("LabVIEW Events"), Description("Fired when a node is selected.")]
        public event NodeSelectedEventHandler NodeSelected;

        [Category("LabVIEW Events"), Description("Fired when a node checkbox is checked or unchecked.")]
        public event NodeCheckedEventHandler NodeChecked;

        [Category("LabVIEW Events"), Description("Fired when a node is double clicked.")]
        public event NodeDoubleClickedEventHandler NodeDoubleClicked;

        [Category("LabVIEW Events"), Description("Fired when a right-click menu item is clicked.")]
        public event NodeMenuClickedEventHandler NodeMenuClicked;

        // ========================
        // API
        // ========================

        public void SetColumns(string[] headers, double[] widths)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetColumns(headers, widths)));
                return;
            }
            _wpfControl.SetColumns(headers, widths);
        }

        public bool AddNode(string id, string parentId, string[] columnTexts, bool isChecked = false, bool showCheckBox = true, bool hasDummyChild = false, string iconPath = null)
        {
            if (this.InvokeRequired)
            {
                return (bool)this.Invoke(new Func<bool>(() => AddNode(id, parentId, columnTexts, isChecked, showCheckBox, hasDummyChild, iconPath)));
            }
            return _wpfControl.AddNode(id, parentId, columnTexts, isChecked, showCheckBox, hasDummyChild, iconPath);
        }

        public bool AddNodeUTF8(string id, string parentId, byte[] columnTextsBytes, bool isChecked = false, bool showCheckBox = true, bool hasDummyChild = false, string iconPath = null)
        {
            if (columnTextsBytes == null || columnTextsBytes.Length == 0) return false;
            // LabVIEW passing string array via UTF8 can be done by joining strings with a delimiter like '|'
            string text = Encoding.UTF8.GetString(columnTextsBytes);
            string[] columns = text.Split(new[] { '|' }, StringSplitOptions.None);
            
            if (this.InvokeRequired)
            {
                return (bool)this.Invoke(new Func<bool>(() => AddNode(id, parentId, columns, isChecked, showCheckBox, hasDummyChild, iconPath)));
            }
            return _wpfControl.AddNode(id, parentId, columns, isChecked, showCheckBox, hasDummyChild, iconPath);
        }

        public bool RemoveNode(string id)
        {
            if (this.InvokeRequired)
            {
                return (bool)this.Invoke(new Func<bool>(() => RemoveNode(id)));
            }
            return _wpfControl.RemoveNode(id);
        }

        public void ClearNodes()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ClearNodes()));
                return;
            }
            _wpfControl.ClearNodes();
        }

        public string[] GetCheckedNodes()
        {
            if (this.InvokeRequired)
            {
                return (string[])this.Invoke(new Func<string[]>(() => GetCheckedNodes()));
            }
            return _wpfControl.GetCheckedNodes().ToArray();
        }

        public void SetNodeChecked(string id, bool isChecked)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetNodeChecked(id, isChecked)));
                return;
            }
            _wpfControl.SetNodeChecked(id, isChecked);
        }

        public void UpdateNodeText(string id, string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateNodeText(id, text)));
                return;
            }
            _wpfControl.UpdateNodeText(id, text);
        }

        public void UpdateNodeColumns(string id, string[] columns)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateNodeColumns(id, columns)));
                return;
            }
            _wpfControl.UpdateNodeColumns(id, columns);
        }

        public string[] GetNodeColumnTexts(string id)
        {
            if (this.InvokeRequired)
            {
                return (string[])this.Invoke(new Func<string[]>(() => _wpfControl.GetNodeColumnTexts(id)));
            }
            return _wpfControl.GetNodeColumnTexts(id);
        }

        public byte[] GetNodeColumnTextsUTF8(string id)
        {
            string[] cols = GetNodeColumnTexts(id);
            if (cols == null || cols.Length == 0) return new byte[0];
            string joined = string.Join("|", cols);
            return Encoding.UTF8.GetBytes(joined);
        }

        public string GetNodeColumnText(string id, int columnIndex)
        {
            if (this.InvokeRequired)
            {
                return (string)this.Invoke(new Func<string>(() => _wpfControl.GetNodeColumnText(id, columnIndex)));
            }
            return _wpfControl.GetNodeColumnText(id, columnIndex);
        }

        public byte[] GetNodeColumnTextUTF8(string id, int columnIndex)
        {
            string text = GetNodeColumnText(id, columnIndex);
            if (string.IsNullOrEmpty(text)) return new byte[0];
            return Encoding.UTF8.GetBytes(text);
        }

        public string GetParentNodeId(string id)
        {
            if (this.InvokeRequired)
            {
                return (string)this.Invoke(new Func<string>(() => _wpfControl.GetParentNodeId(id)));
            }
            return _wpfControl.GetParentNodeId(id);
        }

        public TreeListNode GetParentNode(string id)
        {
            if (this.InvokeRequired)
            {
                return (TreeListNode)this.Invoke(new Func<TreeListNode>(() => _wpfControl.GetParentNode(id)));
            }
            return _wpfControl.GetParentNode(id);
        }

        public string[] GetParentNodeColumnTexts(string id)
        {
            if (this.InvokeRequired)
            {
                return (string[])this.Invoke(new Func<string[]>(() => _wpfControl.GetParentNodeColumnTexts(id)));
            }
            return _wpfControl.GetParentNodeColumnTexts(id);
        }

        public byte[] GetParentNodeColumnTextsUTF8(string id)
        {
            string[] cols = GetParentNodeColumnTexts(id);
            if (cols == null || cols.Length == 0) return new byte[0];
            string joined = string.Join("|", cols);
            return Encoding.UTF8.GetBytes(joined);
        }

        public TreeListNode GetNode(string id)
        {
            if (this.InvokeRequired)
            {
                return (TreeListNode)this.Invoke(new Func<TreeListNode>(() => _wpfControl.GetNode(id)));
            }
            return _wpfControl.GetNode(id);
        }

        public void UpdateNodeTextUTF8(string id, byte[] textBytes)
        {
            if (textBytes == null || textBytes.Length == 0) return;
            string text = Encoding.UTF8.GetString(textBytes);
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateNodeText(id, text)));
                return;
            }
            _wpfControl.UpdateNodeText(id, text);
        }

        public void UpdateNodeIcon(string id, string iconPath)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateNodeIcon(id, iconPath)));
                return;
            }
            _wpfControl.UpdateNodeIcon(id, iconPath);
        }

        public void ExpandNode(string id)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ExpandNode(id)));
                return;
            }
            _wpfControl.ExpandNode(id);
        }

        public void CollapseNode(string id)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => CollapseNode(id)));
                return;
            }
            _wpfControl.CollapseNode(id);
        }

        public void SetTreeBackground(uint color)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetTreeBackground(color)));
                return;
            }
            _wpfControl.SetTreeBackground(color);
        }

        public void SetMenuBackground(uint color)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetMenuBackground(color)));
                return;
            }
            _wpfControl.SetMenuBackground(color);
        }

        [Category("Appearance")]
        public override string LabelText
        {
            get
            {
                if (this.InvokeRequired) return (string)this.Invoke(new Func<string>(() => _wpfControl.MainLabel != null ? _wpfControl.MainLabel.Text : ""));
                return _wpfControl.MainLabel != null ? _wpfControl.MainLabel.Text : "";
            }
            set
            {
                if (this.InvokeRequired) { this.Invoke(new Action(() => { if (_wpfControl.MainLabel != null) _wpfControl.MainLabel.Text = value; })); return; }
                if (_wpfControl.MainLabel != null) _wpfControl.MainLabel.Text = value;
            }
        }

        public override void SetLabelVisible(bool isVisible)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetLabelVisible(isVisible)));
                return;
            }
            _wpfControl.SetLabelVisible(isVisible);
        }

        public override void SetLabelTextUTF8(byte[] bytes)
        {
            if (bytes == null) return;
            try { LabelText = Encoding.UTF8.GetString(bytes); } catch { }
        }

        public void SetNodeContextMenu(string id, string[] menuItems)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetNodeContextMenu(id, menuItems)));
                return;
            }
            _wpfControl.SetNodeContextMenu(id, menuItems);
        }

        public void SetNodeContextMenuUTF8(string id, byte[] menuBytes)
        {
            if (menuBytes == null || menuBytes.Length == 0) return;
            string menuStr = Encoding.UTF8.GetString(menuBytes);
            string[] items = menuStr.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetNodeContextMenu(id, items)));
                return;
            }
            _wpfControl.SetNodeContextMenu(id, items);
        }

        #region 运行时风格重绘

        public override void ApplyStyleDictionary(Dictionary<string, object> style)
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
