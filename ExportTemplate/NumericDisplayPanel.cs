using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace WpfTextInput
{
    /// <summary>
    /// 用于 LabVIEW .NET 容器的数值显示 WinForms 包装控件
    /// </summary>
    public class NumericDisplayPanel : WpfPanelBase
    {
        private ElementHost _elementHost;
        private NumericDisplayControl _wpfControl;

        #region LabVIEW 可见的事件

        // 数值显示本身通常不产生用户输入事件，为了扩展可以留空或增加 Click 事件

        #endregion

        #region LabVIEW 可见的属性

        /// <summary>
        /// 获取或设置标签文字
        /// </summary>
        [Browsable(true)]
        [Category("NumericDisplay")]
        [Description("标签名称")]
        public override string LabelText
        {
            get { return _wpfControl != null ? _wpfControl.LabelText : string.Empty; }
            set { if (_wpfControl != null) _wpfControl.LabelText = value; }
        }

        /// <summary>
        /// 获取或设置显示的数值字符串
        /// </summary>
        [Browsable(true)]
        [Category("NumericDisplay")]
        [Description("数值内容（字符串格式）")]
        public string ValueStr
        {
            get { return _wpfControl != null ? _wpfControl.Value : string.Empty; }
            set { if (_wpfControl != null) _wpfControl.Value = value; }
        }

        /// <summary>
        /// 获取或设置单位
        /// </summary>
        [Browsable(true)]
        [Category("NumericDisplay")]
        [Description("单位（如 V, mA, ℃）")]
        public string Unit
        {
            get { return _wpfControl != null ? _wpfControl.Unit : string.Empty; }
            set { if (_wpfControl != null) _wpfControl.Unit = value; }
        }

        /// <summary>
        /// 获取或设置数值文本显示大小
        /// </summary>
        [Browsable(true)]
        [Category("NumericDisplay")]
        [Description("数值文本字号大小")]
        public double ValueFontSize
        {
            get { return _wpfControl != null ? _wpfControl.ValueFontSize : 24.0; }
            set { if (_wpfControl != null) _wpfControl.ValueFontSize = value; }
        }

        /// <summary>
        /// 获取或设置单位文本显示大小
        /// </summary>
        [Browsable(true)]
        [Category("NumericDisplay")]
        [Description("单位文本字号大小")]
        public double UnitFontSize
        {
            get { return _wpfControl != null ? _wpfControl.UnitFontSize : 14.0; }
            set { if (_wpfControl != null) _wpfControl.UnitFontSize = value; }
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
        public new BorderStyle BorderStyle { get { return base.BorderStyle; } set { base.BorderStyle = value; } }

        #endregion

        public NumericDisplayPanel()
        {
            InitializeWpfControl();
        }

        #region LabVIEW 可见的方法

        /// <summary>
        /// 以 Double 格式写入数值（带自定义格式字符串）
        /// format 示例: "F2" 保留两位小数
        /// </summary>
        public void WriteDouble(double value, string format = "F2")
        {
            if (_wpfControl != null)
                _wpfControl.Value = value.ToString(format);
        }

        /// <summary>
        /// 直接写入字符串数值
        /// </summary>
        public void WriteString(string value)
        {
            if (_wpfControl != null)
                _wpfControl.Value = value ?? string.Empty;
        }

        /// <summary>
        /// 清空数值显示
        /// </summary>
        public void Clear()
        {
            WriteString("");
        }

        /// <summary>
        /// 设置标签是否显示
        /// </summary>
        public override void SetLabelVisible(bool visible)
        {
            if (_wpfControl != null)
                _wpfControl.SetLabelVisible(visible);
        }

        /// <summary>
        /// 设置单位是否显示
        /// </summary>
        public void SetUnitVisible(bool visible)
        {
            if (_wpfControl != null)
                _wpfControl.SetUnitVisible(visible);
        }

        /// <summary>
        /// 写入字符串数值 (UTF8 字节流方案，解决乱码)
        /// </summary>
        public void WriteStringUTF8(byte[] bytes)
        {
            if (bytes == null) return;
            try { WriteString(System.Text.Encoding.UTF8.GetString(bytes)); } catch { }
        }

        /// <summary>
        /// 设置标签文字 (UTF8 字节流方案，解决乱码)
        /// </summary>
        public override void SetLabelTextUTF8(byte[] bytes)
        {
            if (bytes == null) return;
            try { LabelText = System.Text.Encoding.UTF8.GetString(bytes); } catch { }
        }

        /// <summary>
        /// 设置单位文字 (UTF8 字节流方案，解决乱码)
        /// </summary>
        public void SetUnitUTF8(byte[] bytes)
        {
            if (bytes == null) return;
            try { Unit = System.Text.Encoding.UTF8.GetString(bytes); } catch { }
        }

        /// <summary>
        /// 动态设置数值文本字号大小
        /// </summary>
        public void SetValueFontSize(double size)
        {
            if (_wpfControl != null)
                _wpfControl.ValueFontSize = size;
        }

        /// <summary>
        /// 动态设置单位文本字号大小
        /// </summary>
        public void SetUnitFontSize(double size)
        {
            if (_wpfControl != null)
                _wpfControl.UnitFontSize = size;
        }

        /// <summary>
        /// 同时设置数值与单位的字号大小
        /// </summary>
        public void SetFontSizes(double valueFontSize, double unitFontSize)
        {
            if (_wpfControl != null)
            {
                _wpfControl.ValueFontSize = valueFontSize;
                _wpfControl.UnitFontSize = unitFontSize;
            }
        }

        #endregion

        #region 内部方法

        private void InitializeWpfControl()
        {
            // 背景色由导出模板自动填充
            try {
                this.BackColor = System.Drawing.ColorTranslator.FromHtml("{{ControlBackground}}");
            } catch {
                this.BackColor = System.Drawing.Color.White;
            }

            _wpfControl = new NumericDisplayControl();

            _elementHost = new ElementHost
            {
                Dock = DockStyle.Fill,
                BackColorTransparent = true,
                Child = _wpfControl
            };

            this.Controls.Add(_elementHost);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_elementHost != null)
                {
                    _elementHost.Dispose();
                    _elementHost = null;
                }
            }
            base.Dispose(disposing);
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
    }
}
