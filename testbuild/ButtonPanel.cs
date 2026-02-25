using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WpfButton
{
    /// <summary>
    /// 用于在 LabVIEW / WinForms 中托管 WpfButton 的容器面板
    /// </summary>
    [ToolboxItem(true)]
    [Description("带有新拟态样式的按钮控件")]
    public class ButtonPanel : Panel
    {
        private ElementHost _host;
        private ButtonControl _wpfControl;

        /// <summary>
        /// 当按钮被点击时触发的事件
        /// </summary>
        [Category("Action"), Description("当用户点击按钮时触发（抛出 oldValue, newValue）")]
        public new event ButtonClickEventHandler Click;

        public ButtonPanel()
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
            _wpfControl = new ButtonControl();
            _host.Child = _wpfControl;

            this.Controls.Add(_host);

            // 订阅事件
            _wpfControl.Click += WpfControl_Click;

            // 订阅宿主大小改变以刷新阴影区域，防止被裁切
            this.SizeChanged += (s, e) => { _host.Invalidate(); };
        }

        private void WpfControl_Click(bool oldValue, bool newValue)
        {
            Click?.Invoke(oldValue, newValue);
        }

        #region 给 LabVIEW 或外部代码暴露的属性与方法

        /// <summary>
        /// 获取或设置按钮文本
        /// </summary>
        [Category("Appearance"), Description("按钮显示的文本")]
        public string LabelText
        {
            get => _wpfControl.LabelText;
            set => _wpfControl.LabelText = value;
        }

        [Category("Behavior"), Description("动作模式支持：按下切换、抬起切换包、脉冲与保持等")]
        public ButtonActionBehavior Behavior
        {
            get => _wpfControl.Behavior;
            set => _wpfControl.Behavior = value;
        }

        [Category("Data"), Description("按钮的当前激活状态（布尔量）")]
        public bool Value
        {
            get => _wpfControl.Value;
            set => _wpfControl.Value = value;
        }

        /// <summary>
        /// 显示或隐藏文本
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
                    _wpfControl.Click -= WpfControl_Click;
                }
                _host?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
