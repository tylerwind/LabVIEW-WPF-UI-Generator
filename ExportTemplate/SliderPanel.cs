using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WpfSlider
{
    /// <summary>
    /// 滑动杆数值改变的事件委托，LabVIEW 会将其参数直接展开在事件数据节点上
    /// </summary>
    public delegate void SliderValueChangedHandler(double oldValue, double newValue);

    /// <summary>
    /// 用于在 LabVIEW / WinForms 中托管 WpfSlider 的容器面板
    /// </summary>
    [ToolboxItem(true)]
    [Description("带有新拟态样式的滑动杆控件")]
    public class SliderPanel : Panel
    {
        private ElementHost _host;
        private SliderControl _wpfControl;

        /// <summary>
        /// 当用户滑动更改值时触发
        /// </summary>
        [Category("Action"), Description("当滑动杆数值变化时触发")]
        public event SliderValueChangedHandler ValueChanged;

        public SliderPanel()
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
            _wpfControl = new SliderControl();
            _host.Child = _wpfControl;

            this.Controls.Add(_host);

            // 订阅事件
            _wpfControl.ValueChanged += WpfControl_ValueChanged;

            // 订阅宿主大小改变以刷新阴影区域，防止被裁切
            this.SizeChanged += (s, e) => { _host.Invalidate(); };
        }

        private void WpfControl_ValueChanged(double oldValue, double newValue)
        {
            ValueChanged?.Invoke(oldValue, newValue);
        }

        #region 给 LabVIEW 或外部代码暴露的属性与方法

        /// <summary>
        /// 获取或设置标签文本
        /// </summary>
        [Category("Appearance"), Description("滑动杆左上方显示的标签文本")]
        public string LabelText
        {
            get => _wpfControl.LabelText;
            set => _wpfControl.LabelText = value;
        }

        /// <summary>
        /// 获取或设置当前数值
        /// </summary>
        [Category("Data"), Description("滑动杆的当前数值")]
        public double Value
        {
            get => _wpfControl.Value;
            set => _wpfControl.Value = value;
        }

        /// <summary>
        /// 获取或设置最小值
        /// </summary>
        [Category("Data"), Description("允许的最小数值")]
        public double Minimum
        {
            get => _wpfControl.Minimum;
            set => _wpfControl.Minimum = value;
        }

        /// <summary>
        /// 获取或设置最大值
        /// </summary>
        [Category("Data"), Description("允许的最大数值")]
        public double Maximum
        {
            get => _wpfControl.Maximum;
            set => _wpfControl.Maximum = value;
        }

        /// <summary>
        /// 离散步进值
        /// </summary>
        [Category("Data"), Description("步进值")]
        public double TickFrequency
        {
            get => _wpfControl.TickFrequency;
            set => _wpfControl.TickFrequency = value;
        }

        /// <summary>
        /// 是否吸附到步进值
        /// </summary>
        [Category("Behavior"), Description("是否在拖拽时自动吸附到 TickFrequency 步长位置")]
        public bool IsSnapToTickEnabled
        {
            get => _wpfControl.IsSnapToTickEnabled;
            set => _wpfControl.IsSnapToTickEnabled = value;
        }

        /// <summary>
        /// 显示或隐藏标签
        /// </summary>
        public void SetLabelVisible(bool visible)
        {
            _wpfControl.SetLabelVisible(visible);
        }

        /// <summary>
        /// 显示或隐藏右侧数值文字
        /// </summary>
        public void SetValueVisible(bool visible)
        {
            _wpfControl.SetValueVisible(visible);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_wpfControl != null)
                {
                    _wpfControl.ValueChanged -= WpfControl_ValueChanged;
                }
                _host?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
