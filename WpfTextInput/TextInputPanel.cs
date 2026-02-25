using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WpfTextInput
{
    /// <summary>
    /// 用于 LabVIEW .NET 容器的 WinForms 控件
    /// </summary>
    public class TextInputPanel : UserControl
    {
        private ElementHost _elementHost;
        private TextInputControl _wpfControl;

        #region LabVIEW 可见的事件

        /// <summary>
        /// 文本值变更事件
        /// </summary>
        public event ValueChangedHandler ValueChanged;

        #endregion

        #region LabVIEW 可见的属性

        /// <summary>
        /// 获取或设置标签文字
        /// </summary>
        [Browsable(true)]
        [Category("TextInput")]
        [Description("标签名称")]
        public string LabelText
        {
            get { return _wpfControl != null ? _wpfControl.LabelText : string.Empty; }
            set { if (_wpfControl != null) _wpfControl.LabelText = value; }
        }

        /// <summary>
        /// 获取或设置文本内容
        /// </summary>
        [Browsable(true)]
        [Category("TextInput")]
        [Description("文本内容")]
        public new string Text
        {
            get { return _wpfControl != null ? _wpfControl.Text : string.Empty; }
            set { if (_wpfControl != null) _wpfControl.Text = value; }
        }

        #endregion

        #region 隐藏继承的属性（LabVIEW 不显示）

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new System.Drawing.Color BackColor { get { return base.BackColor; } set { base.BackColor = value; } }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new System.Drawing.Color ForeColor { get { return base.ForeColor; } set { base.ForeColor = value; } }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new System.Drawing.Font Font { get { return base.Font; } set { base.Font = value; } }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new System.Drawing.Image BackgroundImage { get { return base.BackgroundImage; } set { base.BackgroundImage = value; } }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Cursor Cursor { get { return base.Cursor; } set { base.Cursor = value; } }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new RightToLeft RightToLeft { get { return base.RightToLeft; } set { base.RightToLeft = value; } }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool AllowDrop { get { return base.AllowDrop; } set { base.AllowDrop = value; } }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new AutoValidate AutoValidate { get { return base.AutoValidate; } set { base.AutoValidate = value; } }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new BorderStyle BorderStyle { get { return base.BorderStyle; } set { base.BorderStyle = value; } }

        #endregion

        public TextInputPanel()
        {
            InitializeWpfControl();
        }

        #region LabVIEW 可见的方法

        /// <summary>
        /// 写入文本到输入框
        /// </summary>
        public void Write(string text)
        {
            if (_wpfControl != null)
                _wpfControl.Text = text ?? string.Empty;
        }

        /// <summary>
        /// 读取输入框当前文本
        /// </summary>
        public string Read()
        {
            return _wpfControl != null ? _wpfControl.Text : string.Empty;
        }

        /// <summary>
        /// 清空输入框
        /// </summary>
        public void Clear()
        {
            Write(string.Empty);
        }

        /// <summary>
        /// 设置标签是否显示
        /// </summary>
        public void SetLabelVisible(bool visible)
        {
            if (_wpfControl != null)
                _wpfControl.SetLabelVisible(visible);
        }

        /// <summary>
        /// 设置是否显示滚动条（启用后支持多行输入）
        /// </summary>
        public void SetScrollBarVisible(bool visible)
        {
            if (_wpfControl != null)
                _wpfControl.SetScrollBarVisible(visible);
        }

        /// <summary>
        /// 设置只读模式
        /// </summary>
        public void SetReadOnly(bool isReadOnly)
        {
            if (_wpfControl != null)
                _wpfControl.InputBox.IsReadOnly = isReadOnly;
        }

        #endregion

        #region 内部方法

        private void InitializeWpfControl()
        {
            this.BackColor = System.Drawing.Color.FromArgb(227, 230, 236);

            _wpfControl = new TextInputControl();
            _wpfControl.ValueChanged += OnWpfValueChanged;

            _elementHost = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = _wpfControl
            };

            this.Controls.Add(_elementHost);
        }

        private void OnWpfValueChanged(string oldValue, string newValue)
        {
            ValueChanged?.Invoke(oldValue, newValue);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_wpfControl != null)
                    _wpfControl.ValueChanged -= OnWpfValueChanged;
                if (_elementHost != null)
                {
                    _elementHost.Dispose();
                    _elementHost = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
