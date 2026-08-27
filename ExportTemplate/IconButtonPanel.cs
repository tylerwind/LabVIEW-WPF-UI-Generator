using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WpfIconButton
{
    [ToolboxItem(true)]
    [Description("带有新拟态样式的图标按钮控件")]
    public class IconButtonPanel : WpfPanelBase
    {
        private ElementHost _host;
        private IconButtonControl _wpfControl;

        [Browsable(false)]
        public IconButtonControl WpfControl { get { return _wpfControl; } }

        [Category("Action"), Description("当用户点击按钮时触发（抛出 oldValue, newValue）")]
        public new event ButtonClickEventHandler Click;

        public IconButtonPanel()
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

            _wpfControl = new IconButtonControl();
            _host.Child = _wpfControl;

            this.Controls.Add(_host);

            // 订阅事件并转发
            _wpfControl.Click += (oldValue, newValue) => {
                if (Click != null) Click(oldValue, newValue);
            };

            this.SizeChanged += delegate(object s, EventArgs e) { 
                if (_host != null) _host.Invalidate(); 
            };
        }

        #region 给 LabVIEW 或外部代码暴露的属性与方法

        [Category("Appearance"), Description("按钮显示的文本")]
        public override string LabelText
        {
            get { return (string)InvokeOnUI(() => _wpfControl.LabelText); }
            set { InvokeOnUI(() => _wpfControl.LabelText = value); }
        }

        [Category("Appearance"), Description("按钮显示的字符图标")]
        public string IconText
        {
            get { return (string)InvokeOnUI(() => _wpfControl.IconText); }
            set { InvokeOnUI(() => _wpfControl.IconText = value); }
        }

        [Category("Appearance"), Description("按钮显示的图片图标路径")]
        public string IconPath
        {
            get { return (string)InvokeOnUI(() => _wpfControl.IconPath); }
            set { InvokeOnUI(() => _wpfControl.IconPath = value); }
        }

        [Category("Appearance"), Description("是否启用图片模式")]
        public bool UseImage
        {
            get { return (bool)InvokeOnUI(() => _wpfControl.UseImage); }
            set { InvokeOnUI(() => _wpfControl.UseImage = value); }
        }

        [Category("Behavior"), Description("动作模式支持：按下切换、抬起切换等")]
        public ButtonActionBehavior Behavior
        {
            get { return (ButtonActionBehavior)InvokeOnUI(() => _wpfControl.Behavior); }
            set { InvokeOnUI(() => _wpfControl.Behavior = value); }
        }

        [Category("Appearance"), Description("按钮圆角半径")]
        public double CornerRadius
        {
            get { return (double)InvokeOnUI(() => _wpfControl.CornerRadius); }
            set { InvokeOnUI(() => _wpfControl.CornerRadius = value); }
        }

        public void SetCornerRadius(double radius)
        {
            InvokeOnUI(() => _wpfControl.CornerRadius = radius);
        }

        [Category("Data"), Description("按钮的当前激活状态（布尔量）")]
        public bool Value
        {
            get { return (bool)InvokeOnUI(() => _wpfControl.Value); }
            set { InvokeOnUI(() => _wpfControl.Value = value); }
        }

        [Category("Appearance"), Description("图标高亮激活颜色 (HEX)")]
        public string ActiveColor
        {
            get { return (string)InvokeOnUI(() => _wpfControl.ActiveColor); }
            set { InvokeOnUI(() => _wpfControl.ActiveColor = value); }
        }

        [Category("Appearance"), Description("图标高亮激活颜色 (数字)")]
        public int ActiveColorValue
        {
            get
            {
                if (_wpfControl == null) return 0;
                try {
                    var hex = (string)InvokeOnUI(() => _wpfControl.ActiveColor);
                    var c = System.Drawing.ColorTranslator.FromHtml(hex);
                    return (c.R << 16) | (c.G << 8) | c.B;
                } catch { return 0; }
            }
            set
            {
                if (_wpfControl != null)
                {
                    string hex = string.Format("#{0:X6}", value & 0xFFFFFF);
                    InvokeOnUI(() => _wpfControl.ActiveColor = hex);
                }
            }
        }

        public override void SetLabelVisible(bool visible)
        {
            InvokeOnUI(() => _wpfControl.SetLabelVisible(visible));
        }

        public override void SetLabelTextUTF8(byte[] bytes)
        {
            if (bytes == null) return;
            try { LabelText = System.Text.Encoding.UTF8.GetString(bytes); } catch { }
        }

        #endregion

        #region UI 调度辅助

        private object InvokeOnUI(Func<object> func)
        {
            if (!_wpfControl.Dispatcher.CheckAccess())
                return _wpfControl.Dispatcher.Invoke(func);
            return func();
        }

        private void InvokeOnUI(Action action)
        {
            if (!_wpfControl.Dispatcher.CheckAccess())
                _wpfControl.Dispatcher.Invoke(action);
            else
                action();
        }

        #endregion

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_host != null) _host.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}




