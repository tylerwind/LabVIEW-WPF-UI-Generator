using System;
using System.Runtime.InteropServices;

namespace WpfTextInput
{
    /// <summary>
    /// 值变更事件委托 — LabVIEW 兼容的简单委托类型
    /// </summary>
    public delegate void ValueChangedHandler(string oldValue, string newValue, byte[] newValueUTF8);

    /// <summary>
    /// 值变更事件参数
    /// </summary>
    [ComVisible(true)]
    public class ValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 变更前的值
        /// </summary>
        public string OldValue { get; private set; }

        /// <summary>
        /// 变更后的值
        /// </summary>
        public string NewValue { get; private set; }

        public ValueChangedEventArgs(string oldValue, string newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
