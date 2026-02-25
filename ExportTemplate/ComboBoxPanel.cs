using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WpfComboBox
{
    /// <summary>
    /// 下拉框选择更改的事件委托，LabVIEW 会将其参数直接展开在事件数据节点上
    /// </summary>
    public delegate void ComboBoxValueChangedHandler(int selectedIndex, string selectedItem);

    /// <summary>
    /// 用于在 LabVIEW / WinForms 中托管 WpfComboBox 的容器面板
    /// </summary>
    [ToolboxItem(true)]
    [Description("带有新拟态样式的下拉框控件")]
    public class ComboBoxPanel : Panel
    {
        private ElementHost _host;
        private ComboBoxControl _wpfControl;

        /// <summary>
        /// 当用户选择更改时触发
        /// </summary>
        [Category("Action"), Description("当下拉框选择项发生变化时触发")]
        public event ComboBoxValueChangedHandler ValueChanged;

        public ComboBoxPanel()
        {
            try {
                this.BackColor = ColorTranslator.FromHtml("{{ControlBackground}}");
            } catch {
                this.BackColor = Color.White;
            }

            // 初始化 WPF 宿主
            _host = new ElementHost
            {
                Dock = DockStyle.Fill,
                BackColorTransparent = true
            };

            // 实例化 WPF 控件
            _wpfControl = new ComboBoxControl();
            _host.Child = _wpfControl;

            this.Controls.Add(_host);

            // 订阅事件
            _wpfControl.SelectionChanged += WpfControl_SelectionChanged;

            // 订阅宿主大小改变以刷新阴影区域，防止被裁切
            this.SizeChanged += (s, e) => { _host.Invalidate(); };
        }

        private void WpfControl_SelectionChanged(int selectedIndex, object selectedItem)
        {
            ValueChanged?.Invoke(selectedIndex, selectedItem?.ToString() ?? string.Empty);
        }

        #region 给 LabVIEW 或外部代码暴露的属性与方法

        /// <summary>
        /// 获取或设置标签文本
        /// </summary>
        [Category("Appearance"), Description("下拉框左上方显示的标签文本")]
        public string LabelText
        {
            get => _wpfControl.LabelText;
            set => _wpfControl.LabelText = value;
        }

        /// <summary>
        /// 获取或设置当前选中项的索引
        /// </summary>
        [Category("Data"), Description("选中的项目索引")]
        public int SelectedIndex
        {
            get => _wpfControl.SelectedIndex;
            set => _wpfControl.SelectedIndex = value;
        }

        /// <summary>
        /// 获取或设置当前选中项的文本
        /// </summary>
        [Category("Data"), Description("选中的文本值")]
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
        /// 显示或隐藏标签
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
}
