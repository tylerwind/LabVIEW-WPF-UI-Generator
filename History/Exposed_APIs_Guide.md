# LabVIEW WPF UI 控件 API 完整公开指导手册 (v2.5)

本手册涵盖了生成器向 .NET 控制面板（`Panel.cs` 包装器）暴露的所有主控 **属性 (Property)**、**方法 (Method)** 与 **事件 (Event)**。
> 标记 **[NEW] / [UPDATED]** 的项目为近期迭代中补齐或修复的模块。

---

## 目录
1. [ToggleSwitch (拟态开关)](#1-toggleswitch-开关)
2. [Slider (拟态滑动杆)](#2-slider-滑杆)
3. [ProgressBar (积分进度条)](#3-progressbar-进度条)
4. [Led (指示灯)](#4-led-指示灯)
5. [Gauge (半圆仪表)](#5-gauge-半圆仪表)
6. [DataGrid (圆角数据网格)](#6-datagrid-数据网格)
7. [ComboBox (拟态下拉框)](#7-combobox-下拉框)
8. [TextInput (文本输入框)](#8-textinput-文本框)
9. [NumericDisplay (数值展示面板)](#9-numericdisplay-面板)
10. [Button (拟态动作按钮)](#10-button-按钮)
11. [Chart (高级多线进阶图表)](#11-chart-高级图表)
12. [Pie (环形饼图数据卡)](#12-pie-环形饼图)

---

## 1. ToggleSwitch (开关)
- **命名空间**: `WpfTextInput.ToggleSwitchPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `IsOn` | `bool` | 当前开关的状态 (开启=True / 关闭=False) |
| **属性** | `LabelText` | `string` | 左上方显示的绑定标签文字 |
| **属性** | `ActiveColor` | `string` | 开启时背部轨道的颜色值 (**HEX HEX 字符串：例如 "#FF0000"**) |
| **属性** | `InactiveColor` | `string` | 封闭时背部轨道的颜色值 (**HEX 字符串**) |
| **属性** | `ActiveColorValue` | `int` | **[NEW]** 开启时的轨道颜色 (**数字格式，标准 RGB**) |
| **属性** | `InactiveColorValue`| `int` | **[NEW]** 关闭时的轨道颜色 (**数字格式，标准 RGB**) |
| **方法** | `SetLabelVisible` | `(bool visible)`| **[NEW]** 隐藏或展示左上角的标签框 |
| **事件** | `ValueChanged` | `(bool old, bool n)`| 当状态从开关之间倾倒拨动时抛出的触发响应节点 |

---

## 2. Slider (滑杆)
- **命名空间**: `WpfSlider.SliderPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `Value` | `double` | 滑动杆的当前滑块绝对数值 |
| **属性** | `Minimum` | `double` | 滑块行程的底部/左端最小值范围 |
| **属性** | `Maximum` | `double` | 滑块行程的顶部/右端最大值范围 |
| **属性** | `TickFrequency` | `double` | 滑动过程的最小标刻增幅（步进值） |
| **属性** | `IsSnapToTickEnabled` | `bool` | 是否在拖拽间自动对齐至步进阻力点 |
| **属性** | `LabelText` | `string` | 标签名称 |
| **属性** | `StartColor` | `string` | 渐变起点颜色 (**HEX**) |
| **属性** | `EndColor` | `string` | 渐变终点颜色 (**HEX**) |
| **属性** | `StartColorValue` | `int` | **[NEW]** 渐变起点颜色 (**数字状态，标准 RGB**) |
| **属性** | `EndColorValue` | `int` | **[NEW]** 渐变终点颜色 (**数字状态，标准 RGB**) |
| **方法** | `SetLabelVisible` | `(bool visible)`| **[NEW]** 隐藏或展示标签名称 |
| **方法** | `SetValueVisible` | `(bool visible)`| 隐藏或展示滑块右侧浮动的当前数值文字 |
| **事件** | `ValueChanged` | `(double o, double n)`| 当滑杆产生微调时向 LabVIEW 回传实时节点 |

---

## 3. ProgressBar (进度条)
- **命名空间**: `WpfTextInput.ProgressBarPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `Value` | `double` | 进度条当前的推进数值 |
| **属性** | `Minimum` | `double` | 进度条 0% 指向的起始基准值 |
| **属性** | `Maximum` | `double` | 进度条 100% 指向的饱和封顶值 |
| **属性** | `ShowPercentage` | `bool` | 是否在进度柱最右端浮显百分比比例 (`xx%`) |
| **属性** | `LabelText` | `string` | 进度条描述标签 |
| **属性** | `StartColor` | `string` | 起手点轨道颜色值 (**HEX**) |
| **属性** | `EndColor` | `string` | 封顶点轨道颜色值 (**HEX**) |
| **属性** | `StartColorValue` | `int` | **[NEW]** 轨道起点色彩通量 (**数字，标准 RGB**) |
| **属性** | `EndColorValue` | `int` | **[NEW]** 轨道终点色彩通量 (**数字，标准 RGB**) |
| **方法** | `SetLabelVisible` | `(bool visible)`| **[NEW]** 隐藏或展示顶层的标签名称 |

---

## 4. Led (指示灯)
- **命名空间**: `WpfTextInput.LedPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `IsOn` | `bool` | 指示灯状态（亮起 / 熄灭状态） |
| **属性** | `LabelText` | `string` | 指示灯关联文本名称 |
| **属性** | `ActiveColor` | `string` | 亮灯时发亮的极化球体色彩 (**HEX**) |
| **属性** | `ActiveColorValue` | `int` | **[NEW]** 亮灯时的发亮核心色彩 (**数字，标准 RGB**) |
| **方法** | `SetLabelVisible` | `(bool visible)`| **[NEW]** 开启或彻底抹除指示灯的文字占位 |

---

## 5. Gauge (半圆仪表)
- **命名空间**: `WpfGauge.GaugePanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `Value` | `double` | 仪表中圈表盘当前的数值推进 |
| **属性** | `Minimum` | `double` | 最左翼起点对应的缩放标刻 |
| **属性** | `Maximum` | `double` | 最右翼终点对应的缩放标刻 |
| **属性** | `LabelText` | `string` | 圆形内部填充的二级标头 |
| **属性** | `DescText`  | `string` | 圆形底部居中的细粒度解释文本 |
| **属性** | `StartColor` | `string` | 外围进度圈左起主色 (**HEX**) |
| **属性** | `EndColor`   | `string` | 外围进度圈右束主色 (**HEX**) |
| **属性** | `StartColorValue`| `int` | **[NEW]** 外围环线起点 (**数字，标准 RGB**) |
| **属性** | `EndColorValue`  | `int` | **[NEW]** 外围环线终点 (**数字，标准 RGB**) |
| **方法** | `SetLabelVisible` | `(bool visible)`| **[NEW]** 擦除标签占位 |
| **方法** | `SetRange` | `(double min, max)`| 统一同步外壳最大值与最小值设防线 |
| **方法** | `SetValue` | `(double value)` | 单线填充最新弧弦值 |

---

## 6. DataGrid (数据网格)
- **命名空间**: `WpfDataGrid.DataGridPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `LabelText` | `string` | 控制器的头部大文本标签 |
| **属性** | `ShowHeader`| `bool` | 控制列名（表头项）的高亮可见性 |
| **属性** | `RowHeight` | `double` | 单行高度（影响横向紧致性） |
| **属性** | `HeaderColor` | `string` | 列头顶柱着力背景色 (**HEX**) |
| **属性** | `HeaderColorValue`| `int` | **[NEW]** 圆角网格表头背景 (**数字，标准 RGB**) |
| **属性** | `ItemsSource` | `object` | 直达 WPF DataGrid Core 的数据塞口（避坑用） |
| **方法** | `BindDataTable`| `(DataTable dt)`| 直接吸收托管 WinForm 默认的二维关联数据集 |
| **方法** | `SetHeaders`  | `(string[] titles)`| 设置完整的列名标签束组 |
| **方法** | `SetData`     | `(string[,] data)`| 通配多维字符串数组的一体化拉通填充 |
| **方法** | `AddRow`      | `(string[] row)`  | 向页尾独立叠加单条字符行 |
| **方法** | `Clear`       | `()`               | 洗空表格中目前附着的缓存条目 |
| **方法** | `GetHeaders`  | `()` $\rightarrow$ `string[]`| 获取当前的头部字段镜像副本 |
| **方法** | `GetAllData`  | `()` $\rightarrow$ `string[,]`| 获取目前展现的全局二维映射表格底层数据 |

---

## 7. ComboBox (下拉框)
- **命名空间**: `WpfComboBox.ComboBoxPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `LabelText` | `string` | 浮悬名称 |
| **属性** | `SelectedIndex`| `int` | 被拉取的项的对应数值数组索引标号 |
| **属性** | `TextValue` | `string` | 被选中那一行所呈现的最终文本流 |
| **属性** | `Items` | `string[]` | 全额覆盖并自动展开的整体下拉条目数组序列 |
| **方法** | `AddItem` | `(string item)` | 动态叠加单独下拉行项 |
| **方法** | `ClearItems` | `()` | 全额清除下拉弹窗中所拥有的条目 |
| **方法** | `SetLabelVisible` | `(bool visible)`| **[NEW]** 擦除下拉组件对应的 label 外置区目 |

---

## 8. TextInput (文本框)
- **命名空间**: `WpfTextInput.TextInputPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `LabelText` | `string` | 面板常驻标题项 |
| **属性** | `Text` | `string` | 主动拉取或预先设置在内建 input 的字符串 |
| **方法** | `Write` | `(string t)` | 覆写灌入主存文本 |
| **方法** | `Read` | `()` $\rightarrow$ `string` | 返回只读的当前内部字符链路副本 |
| **方法** | `Clear` | `()` | 抹除非法字符，使呈现为空格布局 |
| **方法** | `SetLabelVisible`| `(bool visible)` | **[NEW]** 面板层压隐藏标签 |
| **方法** | `SetScrollBarVisible`| `(bool visible)` | 打开允许横纵滚轮条的支撑（多行） |
| **方法** | `SetReadOnly` | `(bool readOnly)` | 配置其进入静止非交互只读模式 |

---

## 9. NumericDisplay (面板)
- **命名空间**: `WpfTextInput.NumericDisplayPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `LabelText` | `string` | 左上方大标头说明 |
| **属性** | `ValueStr` | `string` | 主数值呈现带（带格式，如 `123.45`） |
| **属性** | `Unit` | `string` | 用于绑定对应刻线的工程变量单位 |
| **方法** | `WriteDouble` | `(double v, string fmt)`| 通配数值，`fmt` 决定后缀点位（默认 "F2"） |
| **方法** | `WriteString` | `(string rawText)`| 脱离格式管控的直达字符通配板注入 |
| **方法** | `SetLabelVisible`| `(bool visible)` | **[NEW]** 去掉最核心的控制栏层文本提示区 |
| **方法** | `SetUnitVisible` | `(bool visible)` | 抹除最右端追加的单位附和刻印 |

---

## 10. Button (按钮)
- **命名空间**: `WpfButton.ButtonPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `LabelText` | `string` | 悬挂于按钮表面中心呈立的动作指引文本 |
| **属性** | `Behavior`  | `Enum` | 动作机制（按下切换、释放复位、脉冲、自锁等）|
| **属性** | `Value`     | `bool` | 按钮的逻辑输出电极（配合 Behavior 工作） |
| **方法** | `SetLabelVisible`| `(bool visible)` | **[NEW]** 隐藏或浮显中间文字锚点 |
| **事件** | `Click`     | `(bool o, bool n)`| 当点击从按到弹升时驱动的事件连环闸阀 |

---

## 11. Chart (高级图表)
- **命名空间**: `WpfChart.ChartPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `YMin` / `YMax` | `double` | 控制其核心纵坐标下端 / 顶端界限值分水线 |
| **属性** | `AutoScaleY`  | `bool` | 激活基于波峰波谷数据的自顺应纵柱拉伸校准 |
| **属性** | `ShowGridLines`| `bool` | 主副辅助线层压（网格点阵可见性） |
| **属性** | `ShowLegends`  | `bool` | 关闭整个下沉式小系列多线图例提示条 |
| **属性** | `ShowSeriesCards`| `bool` | 是否弹出左边缘的多曲线最新观测值数值折跃卡片 |
| **属性** | `MaxPoints`    | `int`  | 在横向流体缓步滑动窗口中，预分配支撑的颗粒极限|
| **方法** | `SetupSeries`  | `(string[] labels, int[] colors)`| **高级拉通**：一次批量定义各名称曲线对应的发光线带层，色彩接收 LabVIEW 常量 |
| **方法** | `AppendPoint`  | `(string name, double v)`| 追加最新刻下的颗粒脉冲采样点进曲线 |
| **方法** | `AppendBatch`  | `(double[] values)`| **高速并轨**：在相同节拍点位向全部 setup 定义下的连线注氧进值 |
| **方法** | `SetXLabels`   | `(string[] labels)`| 覆写并绑定其底部坐标的非数据物理轴衬标签行 |
| **方法** | `ClearSeries`  | `()`               | 洗空所有的绘图叠加层缓充序列 |

---

## 12. Pie (环形饼图)
- **命名空间**: `WpfPie.PiePanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `LabelText` | `string` | 最中央填充的主百分数头部锚标文本 |
| **属性** | `DescText`  | `string` | 浮悬在主数字正下翼的基础数据解读小字 |
| **属性** | `ShowSeriesCards`| `bool` | 开关卡片式悬浮于侧翼展开的多色分色总账报表 |
| **属性** | `SeriesNames`  | `string[]` | 提取当前已知并已经入板的所有饼翼子目批次名 |
| **方法** | `AddSeries`   | `(string, v, int color)`| 浮盈式累加饼柱。其中 `color` 指向 LabVIEW 选色盒数值 |
| **方法** | `SetSeries`   | `(string[], v[], colors[])`| **批量校准**：整体覆写全部扇页条形多边比例支点关系 |
| **方法** | `SetValue`    | `(string s, double v)`| 针对单一名叫 `s` 的叶片执行微量的动态补值重画 |
| **方法** | `ClearSeries` | `()`               | 全盘炸毁并收回所有切线，使中间只悬停背景百分量 |
