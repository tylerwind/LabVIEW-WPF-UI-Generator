using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace MyComboBox
{
    /// <summary>
    /// 用于�?LabVIEW / WinForms 中托�?MyComboBox 的容器面�?
    /// </summary>
    [ToolboxItem(true)]
    [Description("带有新拟态样式的下拉框控�?)]
    public class ComboBoxPanel : Panel
    {
        private ElementHost _host;
        private ComboBoxControl _wpfControl;

        /// <summary>
        /// 当用户选择更改时触�?
        /// </summary>
        [Category("Action"), Description("当下拉框选择项发生变化时触发")]
        public event EventHandler<ComboBoxEventArgs> ValueChanged;

        public ComboBoxPanel()
        {
            this.BackColor = Color.Transparent;

            // 初始�?WPF 宿主
            _host = new ElementHost
            {
                Dock = DockStyle.Fill,
                BackColorTransparent = true
            };

            // 实例�?WPF 控件
            _wpfControl = new ComboBoxControl();
            _host.Child = _wpfControl;

            this.Controls.Add(_host);

            // 订阅事件
            _wpfControl.SelectionChanged += WpfControl_SelectionChanged;

            // 订阅宿主大小改变以刷新阴影区域，防止被裁�?
            this.SizeChanged += (s, e) => { _host.Invalidate(); };
        }

        private void WpfControl_SelectionChanged(int selectedIndex, object selectedItem)
        {
            ValueChanged?.Invoke(this, new ComboBoxEventArgs(selectedIndex, selectedItem));
        }

        #region �?LabVIEW 或外部代码暴露的属性与方法

        /// <summary>
        /// 获取或设置标签文�?
        /// </summary>
        [Category("Appearance"), Description("下拉框左上方显示的标签文�?)]
        public string LabelText
        {
            get => _wpfControl.LabelText;
            set => _wpfControl.LabelText = value;
        }

        /// <summary>
        /// 获取或设置当前选中项的索引
        /// </summary>
        [Category("Data"), Description("选中的项目索�?)]
        public int SelectedIndex
        {
            get => _wpfControl.SelectedIndex;
            set => _wpfControl.SelectedIndex = value;
        }

        /// <summary>
        /// 获取或设置当前选中项的文本
        /// </summary>
        [Category("Data"), Description("选中的文本�?)]
        public string TextValue
        {
            get => _wpfControl.Text;
            set => _wpfControl.Text = value;
        }

        /// <summary>
        /// 添加选项
        /// </summary>
        public void AddItem(string item)
        {
            _wpfControl.AddItem(item);
        }

        /// <summary>
        /// 清空选项
        /// </summary>
        public void ClearItems()
        {
            _wpfControl.ClearItems();
        }

        /// <summary>
        /// 显示或隐藏标�?
        /// </summary>
        public void SetLabelVisible(bool visible)
        {
            _wpfControl.SetLabelVisible(visible);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_wpfControl != null)
                {
                    _wpfControl.SelectionChanged -= WpfControl_SelectionChanged;
                }
                _host?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// 事件参数：传递选中的�?
    /// </summary>
    public class ComboBoxEventArgs : EventArgs
    {
        public int SelectedIndex { get; }
        public object SelectedItem { get; }

        public ComboBoxEventArgs(int index, object item)
        {
            SelectedIndex = index;
            SelectedItem = item;
        }
    }
}
