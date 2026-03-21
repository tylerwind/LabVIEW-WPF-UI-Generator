namespace ControlDesigner.Models
{
    /// <summary>
    /// 支持的控件类型
    /// </summary>
    public enum ControlType
    {
        /// <summary>
        /// 文本输入框
        /// </summary>
        TextInput,
        
        /// <summary>
        /// 数值显示框
        /// </summary>
        NumericDisplay,
        /// <summary>
        /// 下拉框
        /// </summary>
        ComboBoxInput,

        /// <summary>
        /// 滑动杆
        /// </summary>
        SliderInput,
        
        /// <summary>
        /// 按钮
        /// </summary>
        ButtonInput,

        /// <summary>
        /// LED 指示灯
        /// </summary>
        LedIndicator,

        /// <summary>
        /// Toggle 开关
        /// </summary>
        ToggleSwitch,

        /// <summary>
        /// 进度条
        /// </summary>
        ProgressBarInput,

        /// <summary>
        /// 双重圆角平滑波形图
        /// </summary>
        ChartDisplay,
        /// <summary>
        /// 饼图
        /// </summary>
        PieDisplay,
        /// <summary>
        /// 仪表盘
        /// </summary>
        GaugeDisplay,
        /// <summary>
        /// 极简玻璃态数据表格
        /// </summary>
        DataGridDisplay
    }
}
