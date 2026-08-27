using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace {{Namespace}}
{
    /// <summary>
    /// WPF 控件面板公共接口 (供 LabVIEW 多态调用)
    /// </summary>
    public interface IWpfControlPanel
    {
        string LabelText { get; set; }
        int BackgroundColorValue { get; set; }
        string BackgroundColorHex { get; set; }
        void SetLabelVisible(bool visible);
        void SetLabelTextUTF8(byte[] bytes);
        void SetBackgroundColor(int colorValue);
        void SetBackgroundColorHex(string hexColor);
        void UpdateStyleFromJson(string jsonPathOrText);
        void ApplyStyleDictionary(Dictionary<string, object> style);
    }

    /// <summary>
    /// 所有 WPF 控件宿主面板的统一基类 (继承自 System.Windows.Forms.Panel)
    /// 在 LabVIEW 中可通过 "To More Generic Class" 转换为此类型，从而只用一个 SubVI 即可统一控制所有控件
    /// </summary>
    [System.Runtime.InteropServices.ComVisible(true)]
    [ToolboxItem(true)]
    [Description("WPF 控件通用基类面板")]
    public class WpfPanelBase : Panel, IWpfControlPanel
    {
        public WpfPanelBase()
        {
        }

        /// <summary>
        /// 获取或设置控件的主体文本/标签
        /// </summary>
        [Category("Appearance"), Description("控件显示的标签或文本")]
        public virtual string LabelText
        {
            get { return string.Empty; }
            set { }
        }

        /// <summary>
        /// 获取或设置最底层背景色数值 (LabVIEW RGB 数值格式，如 0xFFFFFF)
        /// </summary>
        [Category("Appearance"), Description("获取或设置最底层背景色数值 (RGB)")]
        public virtual int BackgroundColorValue
        {
            get
            {
                return (this.BackColor.R << 16) | (this.BackColor.G << 8) | this.BackColor.B;
            }
            set
            {
                this.BackColor = Color.FromArgb(255, (value >> 16) & 0xFF, (value >> 8) & 0xFF, value & 0xFF);
            }
        }

        /// <summary>
        /// 获取或设置最底层背景色 HEX 字符串 (例如 \"#F0F2F5\")
        /// </summary>
        [Category("Appearance"), Description("获取或设置最底层背景色 HEX 字符串")]
        public virtual string BackgroundColorHex
        {
            get
            {
                return string.Format("#{0:X2}{1:X2}{2:X2}", this.BackColor.R, this.BackColor.G, this.BackColor.B);
            }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                try { this.BackColor = ColorTranslator.FromHtml(value.StartsWith("#") ? value : "#" + value); } catch { }
            }
        }

        /// <summary>
        /// 动态设置标签文本的可见性
        /// </summary>
        public virtual void SetLabelVisible(bool visible)
        {
        }

        /// <summary>
        /// 通过 UTF-8 字节流设置标签文字 (解决 LabVIEW 编码乱码)
        /// </summary>
        public virtual void SetLabelTextUTF8(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return;
            try { LabelText = System.Text.Encoding.UTF8.GetString(bytes); } catch { }
        }

        /// <summary>
        /// 直接设置最底层背景色数值 (LabVIEW RGB 数值格式)
        /// </summary>
        public virtual void SetBackgroundColor(int colorValue)
        {
            this.BackgroundColorValue = colorValue;
        }

        /// <summary>
        /// 直接设置最底层背景色 HEX 字符串 (例如 \"#F0F2F5\")
        /// </summary>
        public virtual void SetBackgroundColorHex(string hexColor)
        {
            this.BackgroundColorHex = hexColor;
        }

        /// <summary>
        /// 根据指定的 JSON 样式文件路径或 JSON 字符串实时重绘 UI
        /// </summary>
        public virtual void UpdateStyleFromJson(string jsonPathOrText)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonPathOrText))
                    return;

                string json = null;
                string trimmed = jsonPathOrText.Trim();
                if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
                {
                    json = trimmed;
                }
                else if (System.IO.File.Exists(jsonPathOrText))
                {
                    json = System.IO.File.ReadAllText(jsonPathOrText, System.Text.Encoding.UTF8);
                }

                if (string.IsNullOrEmpty(json))
                    return;

                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var dict = serializer.Deserialize<Dictionary<string, object>>(json);
                if (dict != null)
                {
                    ApplyStyleDictionary(dict);
                }
            }
            catch { }
        }

        /// <summary>
        /// 当 WinForms 面板背景色改变时，自动同步更新内部嵌入的 WPF 根控件背景色
        /// </summary>
        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            SyncWpfBackground(this.BackColor);
        }

        /// <summary>
        /// 当内部 ElementHost 控件挂载到面板时，即刻执行一次背景色同步
        /// </summary>
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            SyncWpfBackground(this.BackColor);
        }

        /// <summary>
        /// 同步更新内部嵌入的 WPF 控件的根背景色，消除容器外边距的色差
        /// </summary>
        public virtual void SyncWpfBackground(Color color)
        {
            try
            {
                byte a = color.A;
                byte r = color.R;
                byte g = color.G;
                byte b = color.B;
                bool isTrans = (color == Color.Transparent || a == 0);

                foreach (Control ctrl in this.Controls)
                {
                    var host = ctrl as System.Windows.Forms.Integration.ElementHost;
                    if (host != null && host.Child != null)
                    {
                        var wpfElem = host.Child as System.Windows.Controls.Control;
                        if (wpfElem != null)
                        {
                            if (wpfElem.Dispatcher.CheckAccess())
                            {
                                try
                                {
                                    wpfElem.Background = isTrans ? System.Windows.Media.Brushes.Transparent : 
                                        new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(a, r, g, b));
                                }
                                catch { }
                            }
                            else
                            {
                                wpfElem.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    try
                                    {
                                        wpfElem.Background = isTrans ? System.Windows.Media.Brushes.Transparent : 
                                            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(a, r, g, b));
                                    }
                                    catch { }
                                }));
                            }
                        }
                    }
                }
            }
            catch { }
        }

        #region 运行时风格重绘

        /// <summary>
        /// 在内存中直接传入样式字典以重载样式
        /// </summary>
        public virtual void ApplyStyleDictionary(Dictionary<string, object> style)
        {
            if (style == null) return;
            try
            {
                // 1. 重写 Panel 自体 BackColor
                if (style.ContainsKey("ControlBackground"))
                {
                    string ctrlBg = style["ControlBackground"] as string;
                    if (!string.IsNullOrEmpty(ctrlBg))
                    {
                        var newColor = ColorTranslator.FromHtml(ctrlBg.StartsWith("#") ? ctrlBg : "#" + ctrlBg);
                        this.BackColor = newColor;
                        SyncWpfBackground(newColor);
                        this.Invalidate();
                    }
                }
            }
            catch { }
        }

        #endregion
    }
}
