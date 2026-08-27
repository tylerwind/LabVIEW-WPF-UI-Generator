# LabVIEW WPF UI 控件 API 完整公开指导手册 (v3.3)

本手册涵盖了生成器向 .NET 控制面板（`Panel.cs` 包装器）暴露的所有主控 **属性 (Property)**、**方法 (Method)** 与 **事件 (Event)**。
> 标记 **[NEW] / [UPDATED]** 的项目为 v3.3 迭代中新增或强化的模块。

---

## 目录
1. [WpfPanelBase (通用基类面板)](#1-wpfpanelbase-通用基类面板)
2. [ToggleSwitch (拟态开关)](#2-toggleswitch-开关)
3. [Slider (拟态滑动杆)](#3-slider-滑杆)
4. [ProgressBar (积分进度条)](#4-progressbar-进度条)
5. [Led (指示灯)](#5-led-指示灯)
6. [Gauge (半圆仪表)](#6-gauge-半圆仪表)
7. [DataGrid (圆角数据网格)](#7-datagrid-数据网格)
8. [ComboBox (拟态下拉框)](#8-combobox-下拉框)
9. [TextInput (文本输入框)](#9-textinput-文本框)
10. [NumericDisplay (数值展示面板)](#10-numericdisplay-面板)
11. [Button (拟态动作按钮)](#11-button-按钮)
12. [IconButton (拟态图标按钮)](#12-iconbutton-图标按钮)
13. [Chart (高级多线进阶图表)](#13-chart-高级图表)
14. [Pie (环形饼图数据卡)](#14-pie-环形饼图)
15. [Tree (单列树形列表)](#15-tree-单列树形列表)
16. [TreeList (多列树形列表)](#16-treelist-多列树形列表)
17. [Sidebar (拟态侧边导航栏)](#17-sidebar-侧边导航栏)
18. [Topbar (拟态顶边导航栏)](#18-topbar-顶边导航栏)

---

## 1. WpfPanelBase (通用基类面板)
所有 17 类控件面板均继承自 `WpfPanelBase`。在 LabVIEW 中可使用 **"To More Generic Class"** 将任意控件引用转换为 `WpfPanelBase`，从而用一个通用 SubVI 统一进行主题热换与背景色管理。

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `LabelText` | `string` | 控件主体标签或标题文本 |
| **属性** | `BackgroundColorValue` | `int` | 底层容器背景色 (**LabVIEW RGB 32位数值**) |
| **属性** | `BackgroundColorHex` | `string` | 底层容器背景色 (**HEX 字符串，如 "#F0F2F5"**) |
| **方法** | `UpdateStyleFromJson` | `(string jsonPathOrText)` | **[NEW] 全域运行时换肤**：支持传入 JSON 样式文件绝对路径或 JSON 纯文本字符串，实时重绘 UI |
| **方法** | `SetBackgroundColor` | `(int colorValue)` | 直接以 LabVIEW RGB 32位数值设置背景色 |
| **方法** | `SetBackgroundColorHex` | `(string hexColor)` | 直接以 HEX 字符串设置背景色 |
| **方法** | `SetLabelTextUTF8` | `(byte[] bytes)` | **[NEW]** 通过 UTF-8 字节流设置标签文字（彻底杜绝中文字符乱码） |
| **方法** | `SetLabelVisible` | `(bool visible)` | 动态显示或隐藏标签区域 |

---

## 2. ToggleSwitch (开关)
- **命名空间**: `WpfTextInput.ToggleSwitchPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `IsOn` | `bool` | 当前开关状态 (开启=True / 关闭=False) |
| **属性** | `LabelText` | `string` | 绑定标签文字 |
| **属性** | `ActiveColor` | `string` | 开启时轨道颜色 (**HEX 字符串：例如 "#FF0000"**) |
| **属性** | `InactiveColor` | `string` | 关闭时轨道颜色 (**HEX 字符串**) |
| **属性** | `ActiveColorValue` | `int` | 开启时轨道颜色 (**数字格式，标准 RGB**) |
| **属性** | `InactiveColorValue`| `int` | 关闭时轨道颜色 (**数字格式，标准 RGB**) |
| **方法** | `SetLabelVisible` | `(bool visible)`| 隐藏或展示标签框 |
| **事件** | `ValueChanged` | `(bool oldVal, bool newVal)`| 开关状态发生翻转时触发事件 |

---

## 3. Slider (滑杆)
- **命名空间**: `WpfSlider.SliderPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `Value` | `double` | 滑动杆当前绝对数值 |
| **属性** | `Minimum` | `double` | 最小值范围 |
| **属性** | `Maximum` | `double` | 最大值范围 |
| **属性** | `TickFrequency` | `double` | 步进刻度增幅 |
| **属性** | `IsSnapToTickEnabled` | `bool` | 是否在拖拽间自动对齐至步进点 |
| **属性** | `LabelText` | `string` | 标签名称 |
| **属性** | `StartColor` | `string` | 渐变起点颜色 (**HEX**) |
| **属性** | `EndColor` | `string` | 渐变终点颜色 (**HEX**) |
| **属性** | `StartColorValue` | `int` | 渐变起点颜色 (**数字格式，标准 RGB**) |
| **属性** | `EndColorValue` | `int` | 渐变终点颜色 (**数字格式，标准 RGB**) |
| **方法** | `SetLabelVisible` | `(bool visible)`| 隐藏或展示标签名称 |
| **方法** | `SetValueVisible` | `(bool visible)`| 隐藏或展示滑块右侧浮动的当前数值文字 |
| **事件** | `ValueChanged` | `(double oldVal, double newVal)`| 滑杆数值改变时回传实时数据 |

---

## 4. ProgressBar (进度条)
- **命名空间**: `WpfTextInput.ProgressBarPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `Value` | `double` | 进度条当前推进数值 |
| **属性** | `Minimum` | `double` | 进度条起始基准值 (0%) |
| **属性** | `Maximum` | `double` | 进度条饱和封顶值 (100%) |
| **属性** | `ShowPercentage` | `bool` | 是否在进度柱右端浮显百分比比例 (`xx%`) |
| **属性** | `LabelText` | `string` | 进度条描述标签 |
| **属性** | `StartColor` | `string` | 轨道起点颜色值 (**HEX**) |
| **属性** | `EndColor` | `string` | 轨道终点颜色值 (**HEX**) |
| **属性** | `StartColorValue` | `int` | 轨道起点颜色 (**数字，标准 RGB**) |
| **属性** | `EndColorValue` | `int` | 轨道终点颜色 (**数字，标准 RGB**) |
| **方法** | `SetLabelVisible` | `(bool visible)`| 隐藏或展示顶层标签 |

---

## 5. Led (指示灯)
- **命名空间**: `WpfTextInput.LedPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `IsOn` | `bool` | 指示灯状态（亮起=True / 熄灭=False） |
| **属性** | `LabelText` | `string` | 关联文本名称 |
| **属性** | `ActiveColor` | `string` | 亮灯时发光色彩 (**HEX**) |
| **属性** | `ActiveColorValue` | `int` | 亮灯时发光色彩 (**数字，标准 RGB**) |
| **方法** | `SetLabelVisible` | `(bool visible)`| 开启或隐藏指示灯的文字占位 |

---

## 6. Gauge (半圆/环形仪表)
- **命名空间**: `WpfGauge.GaugePanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `Value` | `double` | 表盘当前数值推进 |
| **属性** | `Minimum` | `double` | 刻度下限值 |
| **属性** | `Maximum` | `double` | 刻度上限值 |
| **属性** | `LabelText` | `string` | 内部二级标头 |
| **属性** | `DescText`  | `string` | 底部居中的细粒度解释文本 |
| **属性** | `StartColor` | `string` | 外围进度圈起点颜色 (**HEX**) |
| **属性** | `EndColor`   | `string` | 外围进度圈终点颜色 (**HEX**) |
| **属性** | `StartColorValue`| `int` | 外围环线起点颜色 (**数字，标准 RGB**) |
| **属性** | `EndColorValue`  | `int` | 外围环线终点颜色 (**数字，标准 RGB**) |
| **方法** | `SetLabelVisible` | `(bool visible)`| 隐藏或展示标签占位 |
| **方法** | `SetRange` | `(double min, double max)`| 统一设置仪表量程 |
| **方法** | `SetValue` | `(double value)` | 动态更新当前仪表读数 |

---

## 7. DataGrid (数据网格)
- **命名空间**: `WpfDataGrid.DataGridPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `LabelText` | `string` | 表格头部大文本标签 |
| **属性** | `ShowHeader`| `bool` | 控制表头可见性 |
| **属性** | `RowHeight` | `double` | 单行高度 |
| **属性** | `HeaderColor` | `string` | 表头背景色 (**HEX**) |
| **属性** | `HeaderColorValue`| `int` | 表头背景色 (**数字，标准 RGB**) |
| **属性** | `ItemsSource` | `object` | 直达 WPF DataGrid 核心数据源 |
| **方法** | `BindDataTable`| `(DataTable dt)`| 直接绑定 DataTable 数据集 |
| **方法** | `SetHeaders`  | `(string[] titles)`| 设置完整的列名表头 |
| **方法** | `SetData`     | `(string[,] data)`| 二维字符串数组批量全量填充 |
| **方法** | `AddRow`      | `(string[] row)` $\rightarrow$ `int` | 向表格追加单行数据，返回新增行索引 |
| **方法** | `UpdateCell`  | `(int row, int col, string val)` | **[NEW] 单元格局部刷新**：精确定位更新单个单元格，极大降低渲染负载 |
| **方法** | `FormatBadge` | `(string text, int colorValue)` $\rightarrow$ `string` | **[NEW] 状态徽章格式化**：将文字与 LabVIEW RGB 颜色组合为徽章语法字符串（如 `[#10B981]OK`） |
| **方法** | `Clear`       | `()`               | 清空表格数据 |
| **方法** | `GetHeaders`  | `()` $\rightarrow$ `string[]`| 获取当前的表头列表 |
| **方法** | `GetAllData`  | `()` $\rightarrow$ `string[,]`| 获取当前表格的全部二维文本数据 |

---

## 8. ComboBox (下拉框)
- **命名空间**: `WpfComboBox.ComboBoxPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `LabelText` | `string` | 标签名称 |
| **属性** | `SelectedIndex`| `int` | 当前选中项的索引序号（从 0 开始） |
| **属性** | `TextValue` | `string` | 当前选中项的文本内容 |
| **属性** | `Items` | `string[]` | 整体下拉条目数组 |
| **方法** | `AddItem` | `(string item)` | 动态追加单一下拉选项 |
| **方法** | `ClearItems` | `()` | 清空全部下拉选项 |
| **方法** | `SetLabelVisible` | `(bool visible)`| 隐藏或展示标签区域 |
| **事件** | `SelectionChanged` | `(int index, string text)` | 当用户切换选中项时抛出事件通知 |

---

## 9. TextInput (文本框)
- **命名空间**: `WpfTextInput.TextInputPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `LabelText` | `string` | 面板常驻标题项 |
| **属性** | `Text` | `string` | 文本框内容字符串 |
| **方法** | `Write` | `(string t)` | 写入/覆写文本内容 |
| **方法** | `Read` | `()` $\rightarrow$ `string` | 读取当前文本内容 |
| **方法** | `Clear` | `()` | 清空文本框 |
| **方法** | `SetLabelVisible`| `(bool visible)` | 隐藏或展示标签 |
| **方法** | `SetScrollBarVisible`| `(bool visible)` | 开启或关闭多行滚动条支持 |
| **方法** | `SetReadOnly` | `(bool readOnly)` | 配置是否处于只读模式 |

---

## 10. NumericDisplay (数值展示面板)
- **命名空间**: `WpfTextInput.NumericDisplayPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `LabelText` | `string` | 左上方标题说明 |
| **属性** | `ValueStr` | `string` | 主数值呈现文本（如 `123.45`） |
| **属性** | `Unit` | `string` | 工程单位（如 `V`, `mA`, `℃`） |
| **属性** | `ValueFontSize`| `double` | 数值文字字号大小 |
| **属性** | `UnitFontSize` | `double` | 单位文字字号大小 |
| **方法** | `WriteDouble` | `(double v, string fmt)`| 写入浮点数，`fmt` 为格式字符串（默认 "F2"） |
| **方法** | `WriteString` | `(string rawText)`| 直接写入自定义字符串 |
| **方法** | `SetLabelVisible`| `(bool visible)` | 隐藏或展示标签文本 |
| **方法** | `SetUnitVisible` | `(bool visible)` | 隐藏或展示单位文字 |
| **方法** | `SetValueFontSize`| `(double size)`| 动态设置数值文字字号 |
| **方法** | `SetUnitFontSize` | `(double size)`| 动态设置单位文字字号 |
| **方法** | `SetFontSizes` | `(double valSize, double unitSize)`| 同时设置数值与单位字号 |

---

## 11. Button (动作按钮)
- **命名空间**: `WpfButton.ButtonPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `LabelText` | `string` | 按钮表面居中显示的文字 |
| **属性** | `Behavior`  | `Enum` | 动作机制（按下切换、释放复位、脉冲等）|
| **属性** | `Value`     | `bool` | 按钮的逻辑输出布尔值 |
| **方法** | `SetLabelVisible`| `(bool visible)` | 隐藏或展示按钮文字 |
| **事件** | `Click`     | `(bool oldVal, bool newVal)`| 当点击触发时驱动的事件响应节点 |

---

## 12. IconButton (拟态图标按钮)
- **命名空间**: `WpfIconButton.IconButtonPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `LabelText` | `string` | 图标旁伴随显示的文本 |
| **属性** | `IconPath`  | `string` | 本地矢量或位图图标路径 |
| **属性** | `Value`     | `bool` | 按钮逻辑输出布尔值 |
| **方法** | `SetLabelVisible`| `(bool visible)` | 隐藏或展示伴随文字 |
| **事件** | `Click`     | `(bool oldVal, bool newVal)`| 触发点击事件响应 |

---

## 13. Chart (高级多线图表)
- **命名空间**: `WpfChart.ChartPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `YMin` / `YMax` | `double` | 纵坐标下限 / 上限阈值 |
| **属性** | `AutoScaleY`  | `bool` | 开启基于数据极值的自动纵坐标缩放 |
| **属性** | `ShowGridLines`| `bool` | 网格参考线可见性 |
| **属性** | `ShowLegends`  | `bool` | 底部图例提示条可见性 |
| **属性** | `ShowSeriesCards`| `bool` | 是否弹出左侧多曲线最新观测值折跃卡片 |
| **属性** | `MaxPoints`    | `int`  | 滑动窗口支持的最大采样点数 |
| **方法** | `SetupSeries`  | `(string[] labels, int[] colors)`| 批量定义曲线名称与发光线色（接收 LabVIEW RGB 数值） |
| **方法** | `AppendPoint`  | `(string name, double v)`| 追加单条曲线的单个最新采样点 |
| **方法** | `AppendBatch`  | `(double[] values)`| 高速多通道并在同一时间节拍点注入数值 |
| **方法** | `SetXLabels`   | `(string[] labels)`| 覆写 X 轴时间/步长文本标签 |
| **方法** | `ClearSeries`  | `()`               | 清空所有曲线历史数据 |

---

## 14. Pie (环形饼图)
- **命名空间**: `WpfPie.PiePanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `LabelText` | `string` | 中心主百分比或主标题文本 |
| **属性** | `DescText`  | `string` | 主标题下方的说明性小字 |
| **属性** | `ShowSeriesCards`| `bool` | 是否展开侧边多色分色统计卡片 |
| **属性** | `SeriesNames`  | `string[]` | 提取当前已知的所有扇区分组名称 |
| **方法** | `AddSeries`   | `(string name, double val, int color)`| 添加单个扇区（`color` 接收 LabVIEW RGB 数值） |
| **方法** | `SetSeries`   | `(string[] names, double[] vals, int[] colors)`| 批量覆写全部扇区占比与颜色 |
| **方法** | `SetValue`    | `(string name, double val)`| 动态更新指定扇区的数值 |
| **方法** | `ClearSeries` | `()`               | 清空全部扇区数据 |

---

## 15. Tree (单列树形列表)
- **命名空间**: `WpfTree.TreePanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `SelectedId` | `string` | 当前选中的节点 ID |
| **属性** | `LabelText` | `string` | 顶部标题文本 |
| **方法** | `AddNode` | `(string id, string text, string parentId, string iconPath, bool isExpanded, bool hasDummyChild, bool showCheckBox)` | 添加单列树节点 |
| **方法** | `AddNodeUTF8` | `(string id, byte[] textBytes, string parentId, ...)` | **[NEW]** 以 UTF-8 字节流添加节点，杜绝中文乱码 |
| **方法** | `RemoveNode` | `(string id)` | 移除指定节点及其所有子节点 |
| **方法** | `ClearNodes` | `()` | 清空整棵树 |
| **方法** | `UpdateNodeText` | `(string id, string text)` | 更新指定节点的显示文本 |
| **方法** | `UpdateNodeTextUTF8` | `(string id, byte[] textBytes)` | **[NEW]** 以 UTF-8 字节流更新节点文本 |
| **方法** | `GetNodeText` | `(string id)` $\rightarrow$ `string` | **[NEW]** 获取指定节点名称 |
| **方法** | `GetNodeTextUTF8` | `(string id)` $\rightarrow$ `byte[]` | **[NEW]** 以 UTF-8 字节流获取指定节点名称 |
| **方法** | `GetParentNodeId` | `(string id)` $\rightarrow$ `string` | **[NEW]** 获取指定节点的父节点 ID（根节点返回空字符串） |
| **方法** | `GetParentNode` | `(string id)` $\rightarrow$ `TreeNode` | **[NEW]** 获取指定节点的父级节点对象引用 |
| **方法** | `ExpandAll` / `CollapseAll` | `()` | 全量展开或折叠树的所有层级 |
| **事件** | `NodeSelected` | `(string id, string text)` | 节点选中事件 |
| **事件** | `NodeDoubleClicked` | `(string id, string text)` | 节点双击事件 |
| **事件** | `NodeChecked` | `(string id, bool isChecked)` | 复选框勾选状态改变事件 |
| **事件** | `NodeExpanding` | `(string id)` | 节点展开事件（用于异步懒加载） |
| **事件** | `NodeMenuClicked` | `(string id, string menuKey)` | 节点右键菜单项点击事件 |

---

## 16. TreeList (多列树形列表)
- **命名空间**: `WpfTreeList.TreeListPanel`
- **特点**：融合树形折叠展开与 DataGrid 多列排布，支持动态列宽、独立列文本以及父节点双向回溯。

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `SelectedId` | `string` | 当前选中的节点 ID |
| **属性** | `LabelText` | `string` | 顶部大标题 |
| **方法** | `SetColumns` | `(string[] headers, double[] widths)` | **[NEW]** 动态初始化多列表头与列宽（首列为树形层级列） |
| **方法** | `AddNode` | `(string id, string[] columnTexts, string parentId, string iconPath, bool isExpanded, bool hasDummyChild, bool showCheckBox)` | **[NEW]** 添加多列节点（传入各列文本数组） |
| **方法** | `AddNodeUTF8` | `(string id, byte[][] columnBytes, string parentId, ...)` | **[NEW]** 以 UTF-8 字节流二维数组添加多列节点 |
| **方法** | `RemoveNode` | `(string id)` | 移除指定节点及其下属子分支 |
| **方法** | `ClearNodes` | `()` | 清空整个多列树 |
| **方法** | `UpdateNodeColumnTexts` | `(string id, string[] columnTexts)` | **[NEW]** 更新指定节点的多列文本数组 |
| **方法** | `GetNodeColumnTexts` | `(string id)` $\rightarrow$ `string[]` | **[NEW] 深度提取**：获取指定节点全部列的文本字符串数组 |
| **方法** | `GetNodeColumnTextsUTF8` | `(string id)` $\rightarrow$ `byte[][]` | **[NEW]** 以 UTF-8 字节流数组获取节点全部列文本 |
| **方法** | `GetNodeColumnText` | `(string id, int colIndex)` $\rightarrow$ `string` | **[NEW]** 获取指定节点指定列索引处的文本 |
| **方法** | `GetParentNodeId` | `(string id)` $\rightarrow$ `string` | **[NEW] 向上溯源**：获取指定节点的父节点 ID（顶层根节点返回空） |
| **方法** | `GetParentNode` | `(string id)` $\rightarrow$ `TreeListNode` | **[NEW]** 获取指定节点的父级节点对象引用 |
| **方法** | `GetParentNodeColumnTexts` | `(string id)` $\rightarrow$ `string[]` | **[NEW] 一步回溯**：直接获取指定节点之父节点的全部列文本内容 |
| **方法** | `ExpandAll` / `CollapseAll` | `()` | 全量展开或折叠整棵多列树 |
| **事件** | `NodeSelected` | `(string id, string[] columnTexts)` | 节点选中事件（回传各列完整数据） |
| **事件** | `NodeDoubleClicked` | `(string id, string[] columnTexts)` | 节点双击事件 |
| **事件** | `NodeChecked` | `(string id, bool isChecked)` | 复选框勾选改变事件 |
| **事件** | `NodeExpanding` | `(string id)` | 节点展开事件（懒加载） |
| **事件** | `NodeMenuClicked` | `(string id, string menuKey)` | 节点右键菜单项点击事件 |

---

## 17. Sidebar (拟态侧边导航栏)
- **命名空间**: `WpfSidebar.SidebarPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `SelectedIndex`| `int` | 当前激活选中的导航栏目序号 |
| **属性** | `LogoText` | `string` | 顶部 Logo 标识伴随文本 |
| **属性** | `LogoIconPath`| `string` | 顶部 Logo 图标路径 |
| **方法** | `SetNavItems` | `(string[] labels, string[] iconPaths)`| 批量定义导航条目列表与图标 |
| **方法** | `SetNavItemsUTF8` | `(byte[][] labelsBytes, string[] iconPaths)`| 以 UTF-8 字节流定义导航条目 |
| **事件** | `NavChanged` | `(int index, string label)` | 导航项切换时触发事件通知 |

---

## 18. Topbar (拟态顶边导航栏)
- **命名空间**: `WpfTopbar.TopbarPanel`

| 暴露类型 | 名称 | 传参/类型 | 描述说明 |
| :--- | :--- | :--- | :--- |
| **属性** | `SelectedIndex`| `int` | 当前激活选中的顶栏标签序号 |
| **属性** | `LogoText` | `string` | 顶栏左侧 Logo 文本 |
| **属性** | `LogoIconPath`| `string` | 顶栏左侧 Logo 图标路径 |
| **方法** | `SetNavItems` | `(string[] labels, string[] iconPaths)`| 批量定义横向导航标签与图标 |
| **方法** | `SetNavItemsUTF8` | `(byte[][] labelsBytes, string[] iconPaths)`| 以 UTF-8 字节流定义顶栏标签 |
| **事件** | `NavChanged` | `(int index, string label)` | 顶栏导航切换时触发事件通知 |
