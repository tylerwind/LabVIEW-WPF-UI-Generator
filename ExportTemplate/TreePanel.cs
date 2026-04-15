using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace {{Namespace}}
{
    public delegate void NodeExpandingHandler(string nodeId);
    public delegate void NodeSelectedHandler(string nodeId, string nodeText, byte[] nodeTextUTF8);
    public delegate void NodeCheckedHandler(string nodeId, bool isChecked);
    public delegate void NodeDoubleClickedHandler(string nodeId, string nodeText, byte[] nodeTextUTF8);
    public delegate void NodeMenuClickedHandler(string nodeId, string menuText, byte[] menuTextUTF8);

    public class TreePanel : Panel
    {
        private ElementHost _host;
        private TreeControl _treeControl;

        public event NodeExpandingHandler NodeExpanding;
        public event NodeSelectedHandler NodeSelected;
        public event NodeCheckedHandler NodeChecked;
        public event NodeDoubleClickedHandler NodeDoubleClicked;
        public event NodeMenuClickedHandler NodeMenuClicked;

        public TreePanel()
        {
            try
            {
                _host = new ElementHost();
                _treeControl = new TreeControl();
                _host.Child = _treeControl;
                _host.Dock = DockStyle.Fill;
                this.Controls.Add(_host);

                _treeControl.NodeExpanding += (s, e) => { if (NodeExpanding != null) NodeExpanding(e.NodeId); };
                _treeControl.NodeSelected += (s, e) => { 
                    if (NodeSelected != null) {
                        byte[] utf8Bytes = string.IsNullOrEmpty(e.NodeText) ? new byte[0] : System.Text.Encoding.UTF8.GetBytes(e.NodeText);
                        NodeSelected(e.NodeId, e.NodeText, utf8Bytes); 
                    }
                };
                _treeControl.NodeChecked += (s, e) => { if (NodeChecked != null) NodeChecked(e.NodeId, e.IsChecked); };
                _treeControl.NodeDoubleClicked += (s, e) => { 
                    if (NodeDoubleClicked != null) {
                        byte[] utf8Bytes = string.IsNullOrEmpty(e.NodeText) ? new byte[0] : System.Text.Encoding.UTF8.GetBytes(e.NodeText);
                        NodeDoubleClicked(e.NodeId, e.NodeText, utf8Bytes); 
                    }
                };
                _treeControl.NodeMenuClicked += (s, e) => {
                    if (NodeMenuClicked != null) {
                        byte[] utf8Bytes = string.IsNullOrEmpty(e.MenuText) ? new byte[0] : System.Text.Encoding.UTF8.GetBytes(e.MenuText);
                        NodeMenuClicked(e.NodeId, e.MenuText, utf8Bytes);
                    }
                };
                // 强制创建句柄保障Invoke可靠性
                var h = this.Handle;
            }
            catch (Exception ex) { LogError(ex, "Constructor"); }
        }

        private void LogError(Exception ex, string method)
        {
            try
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WpfTreeError.log"),
                    DateTime.Now.ToString("HH:mm:ss.fff") + " ERROR [" + method + "]: " + ex.ToString() + "\r\n");
            }
            catch { }
        }

        public string LabelText
        {
            get
            {
                try
                {
                    if (!_treeControl.Dispatcher.CheckAccess()) { return (string)_treeControl.Dispatcher.Invoke(new Func<string>(() => _treeControl.MainLabel.Text)); }
                    return _treeControl.MainLabel.Text;
                }
                catch (Exception ex) { LogError(ex, "LabelText_Get"); return ""; }
            }
            set
            {
                try
                {
                    if (!_treeControl.Dispatcher.CheckAccess()) { _treeControl.Dispatcher.Invoke(new Action(() => _treeControl.MainLabel.Text = value)); return; }
                    _treeControl.MainLabel.Text = value;
                }
                catch (Exception ex) { LogError(ex, "LabelText_Set"); }
            }
        }

        public void SetLabelVisible(bool isVisible)
        {
            try
            {
                if (!_treeControl.Dispatcher.CheckAccess()) { _treeControl.Dispatcher.Invoke(new Action(() => _treeControl.SetLabelVisible(isVisible))); return; }
                _treeControl.SetLabelVisible(isVisible);
            }
            catch (Exception ex) { LogError(ex, "SetLabelVisible"); }
        }

        /// <summary>
        /// 设置标签文字 (UTF8 字节流方案，解决乱码)
        /// </summary>
        public void SetLabelTextUTF8(byte[] bytes)
        {
            if (bytes == null) return;
            try { LabelText = System.Text.Encoding.UTF8.GetString(bytes); } catch { }
        }

        public bool AddNode(string id, string parentId, string text, bool isChecked, bool showCheckBox, bool hasDummyChild, string iconPath)
        {
            try
            {
                if (!_treeControl.Dispatcher.CheckAccess()) 
                { 
                    return (bool)_treeControl.Dispatcher.Invoke(new Func<bool>(() => _treeControl.AddNode(id, parentId, text, isChecked, showCheckBox, hasDummyChild, iconPath))); 
                }
                return _treeControl.AddNode(id, parentId, text, isChecked, showCheckBox, hasDummyChild, iconPath);
            }
            catch (Exception ex) { LogError(ex, "AddNode"); return false; }
        }

        public bool AddNodeUTF8(string id, string parentId, byte[] textBytes, bool isChecked, bool showCheckBox, bool hasDummyChild, string iconPath)
        {
            try
            {
                string text = System.Text.Encoding.UTF8.GetString(textBytes);
                return AddNode(id, parentId, text, isChecked, showCheckBox, hasDummyChild, iconPath);
            }
            catch (Exception ex) { LogError(ex, "AddNodeUTF8"); return false; }
        }

        public bool RemoveNode(string id)
        {
            try
            {
                if (!_treeControl.Dispatcher.CheckAccess()) { return (bool)_treeControl.Dispatcher.Invoke(new Func<bool>(() => _treeControl.RemoveNode(id))); }
                return _treeControl.RemoveNode(id);
            }
            catch (Exception ex) { LogError(ex, "RemoveNode"); return false; }
        }

        public void ClearNodes()
        {
            try
            {
                if (!_treeControl.Dispatcher.CheckAccess()) 
                { 
                    _treeControl.Dispatcher.Invoke(new Action(() => _treeControl.ClearNodes())); 
                    return; 
                }
                _treeControl.ClearNodes();
            }
            catch (Exception ex) { LogError(ex, "ClearNodes"); }
        }

        public TreeNode GetNode(string id)
        {
            try
            {
                if (!_treeControl.Dispatcher.CheckAccess()) { return (TreeNode)_treeControl.Dispatcher.Invoke(new Func<TreeNode>(() => _treeControl.GetNode(id))); }
                return _treeControl.GetNode(id);
            }
            catch (Exception ex) { LogError(ex, "GetNode"); return null; }
        }

        public string[] GetCheckedNodes()
        {
            try
            {
                if (!_treeControl.Dispatcher.CheckAccess()) { return (string[])_treeControl.Dispatcher.Invoke(new Func<string[]>(() => _treeControl.GetCheckedNodes().ToArray())); }
                return _treeControl.GetCheckedNodes().ToArray();
            }
            catch (Exception ex) { LogError(ex, "GetCheckedNodes"); return new string[0]; }
        }

        public void SetNodeChecked(string id, bool isChecked)
        {
            try
            {
                if (!_treeControl.Dispatcher.CheckAccess()) { _treeControl.Dispatcher.Invoke(new Action(() => _treeControl.SetNodeChecked(id, isChecked))); return; }
                _treeControl.SetNodeChecked(id, isChecked);
            }
            catch (Exception ex) { LogError(ex, "SetNodeChecked"); }
        }

        public void SetNodeContextMenu(string id, string menuItemsStr)
        {
            try
            {
                if (string.IsNullOrEmpty(menuItemsStr)) return;
                string[] items = menuItemsStr.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (!_treeControl.Dispatcher.CheckAccess()) { _treeControl.Dispatcher.Invoke(new Action(() => _treeControl.SetNodeContextMenu(id, items))); return; }
                _treeControl.SetNodeContextMenu(id, items);
            }
            catch (Exception ex) { LogError(ex, "SetNodeContextMenu"); }
        }

        public void SetNodeContextMenuUTF8(string id, byte[] menuItemsBytes)
        {
            try
            {
                if (menuItemsBytes == null || menuItemsBytes.Length == 0) return;
                string menuItemsStr = System.Text.Encoding.UTF8.GetString(menuItemsBytes);
                string[] items = menuItemsStr.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (!_treeControl.Dispatcher.CheckAccess()) { _treeControl.Dispatcher.Invoke(new Action(() => _treeControl.SetNodeContextMenu(id, items))); return; }
                _treeControl.SetNodeContextMenu(id, items);
            }
            catch (Exception ex) { LogError(ex, "SetNodeContextMenuUTF8"); }
        }

        public void UpdateNodeText(string id, string text)
        {
            try
            {
                if (!_treeControl.Dispatcher.CheckAccess()) { _treeControl.Dispatcher.Invoke(new Action(() => _treeControl.UpdateNodeText(id, text))); return; }
                _treeControl.UpdateNodeText(id, text);
            }
            catch (Exception ex) { LogError(ex, "UpdateNodeText"); }
        }

        public void UpdateNodeTextUTF8(string id, byte[] textBytes)
        {
            try
            {
                if (textBytes == null) return;
                string text = System.Text.Encoding.UTF8.GetString(textBytes);
                if (!_treeControl.Dispatcher.CheckAccess()) { _treeControl.Dispatcher.Invoke(new Action(() => _treeControl.UpdateNodeText(id, text))); return; }
                _treeControl.UpdateNodeText(id, text);
            }
            catch (Exception ex) { LogError(ex, "UpdateNodeTextUTF8"); }
        }

        public void UpdateNodeIcon(string id, string iconPath)
        {
            try
            {
                if (!_treeControl.Dispatcher.CheckAccess()) { _treeControl.Dispatcher.Invoke(new Action(() => _treeControl.UpdateNodeIcon(id, iconPath))); return; }
                _treeControl.UpdateNodeIcon(id, iconPath);
            }
            catch (Exception ex) { LogError(ex, "UpdateNodeIcon"); }
        }

        public void SetTreeBackground(uint color)
        {
            try
            {
                if (!_treeControl.Dispatcher.CheckAccess()) { _treeControl.Dispatcher.Invoke(new Action(() => _treeControl.SetTreeBackground(color))); return; }
                _treeControl.SetTreeBackground(color);
            }
            catch (Exception ex) { LogError(ex, "SetTreeBackground"); }
        }

        public void SetMenuBackground(uint color)
        {
            try
            {
                if (!_treeControl.Dispatcher.CheckAccess()) { _treeControl.Dispatcher.Invoke(new Action(() => _treeControl.SetMenuBackground(color))); return; }
                _treeControl.SetMenuBackground(color);
            }
            catch (Exception ex) { LogError(ex, "SetMenuBackground"); }
        }

        public void ExpandNode(string id)
        {
            try
            {
                if (!_treeControl.Dispatcher.CheckAccess()) { _treeControl.Dispatcher.Invoke(new Action(() => _treeControl.ExpandNode(id))); return; }
                _treeControl.ExpandNode(id);
            }
            catch (Exception ex) { LogError(ex, "ExpandNode"); }
        }

        public void CollapseNode(string id)
        {
            try
            {
                if (!_treeControl.Dispatcher.CheckAccess()) { _treeControl.Dispatcher.Invoke(new Action(() => _treeControl.CollapseNode(id))); return; }
                _treeControl.CollapseNode(id);
            }
            catch (Exception ex) { LogError(ex, "CollapseNode"); }
        }

        public void ExpandAll()
        {
            try
            {
                if (!_treeControl.Dispatcher.CheckAccess()) { _treeControl.Dispatcher.Invoke(new Action(() => _treeControl.ExpandAll())); return; }
                _treeControl.ExpandAll();
            }
            catch (Exception ex) { LogError(ex, "ExpandAll"); }
        }

        public void CollapseAll()
        {
            try
            {
                if (!_treeControl.Dispatcher.CheckAccess()) { _treeControl.Dispatcher.Invoke(new Action(() => _treeControl.CollapseAll())); return; }
                _treeControl.CollapseAll();
            }
            catch (Exception ex) { LogError(ex, "CollapseAll"); }
        }
    }
}
