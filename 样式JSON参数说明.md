# WPF UI 控件生成器 — 样式 JSON 参数说明

本文档对通过 `style.json` 运行时配置进行动态风格重绘的所有开放参数进行了整理和详细说明。

---

## 1. 基础面板与卡片布局
| 参数名 | 类型 | 说明 | 示例 |
| :--- | :--- | :--- | :--- |
| **`ControlBackground`** | string | 控件容器的最底层背景色（HEX格式）。 | `"#E3E6EC"` |
| **`GradientStart`** | string | 控件主体背景三色渐变起止的**起点**颜色（HEX格式）。 | `"#EAEDF2"` |
| **`GradientMid`** | string | 控件主体背景三色渐变起止的**中点**颜色（HEX格式）。 | `"#E0E3E9"` |
| **`GradientEnd`** | string | 控件主体背景三色渐变起止的**终点**颜色（HEX格式）。 | `"#D8DCE3"` |
| **`BorderColor`** | string | 控件在默认状态下的边框颜色（HEX格式）。 | `"#DDE0E6"` |
| **`BorderThickness`** | double | 控件的边框粗细，单位：像素（px）。 | `1` |
| **`CornerRadius`** | double | 控件外框与边框的圆角半径。 | `12` |
| **`CardPadding`** | string | 控件内部元素与边缘的内边距，格式为 `"左,上,右,下"`。 | `"12,8,12,6"` |

---

## 2. 新拟态立体阴影与亮部高光
| 参数名 | 类型 | 说明 | 示例 |
| :--- | :--- | :--- | :--- |
| **`ShadowBlur`** | double | 暗部投影阴影的模糊半径（BlurRadius）。 | `10` |
| **`ShadowDepth`** | double | 暗部投影阴影的垂直偏移量/深度（ShadowDepth）。 | `4` |
| **`ShadowColor`** | string | 暗部投影阴影的颜色（HEX格式，通常为暗色）。 | `"#A3A9B5"` |
| **`ShadowOpacity`** | double | 暗部投影阴影的不透明度，范围为 `0.0` 到 `1.0`。 | `0.5` |
| **`HighlightColor`** | string | 亮部投影（高光阴影）及悬浮叠加高光层的颜色（通常为纯白 `"#FFFFFF"`）。 | `"#FFFFFF"` |
| **`HighlightOpacity`** | double | 亮部投影阴影的不透明度，范围为 `0.0` 到 `1.0`。 | `0.65` |

---

## 3. 字体与文本渲染
| 参数名 | 类型 | 说明 | 示例 |
| :--- | :--- | :--- | :--- |
| **`FontFamily`** | string | 内容/值区域文本的字体系列名称。 | `"Segoe UI"` / `"微软雅黑"` |
| **`FontSize`** | double | 内容/值区域文本的字号大小。 | `14` |
| **`FontColor`** | string | 内容/值区域文本的前景色/字体颜色（HEX格式）。 | `"#3A3F50"` |
| **`CaretColor`** | string | 文本输入控件中闪烁文本光标（CaretBrush）的颜色（HEX格式）。 | `"#5A6070"` |
| **`LabelColor`** | string | 控件外部/顶部外挂标签（LabelText）的文字颜色（HEX格式）。 | `"#8A90A0"` |
| **`LabelFontSize`** | double | 控件外部/顶部外挂标签（LabelText）的文字字号大小。 | `11` |

---

## 4. 交互状态
| 参数名 | 类型 | 说明 | 示例 |
| :--- | :--- | :--- | :--- |
| **`FocusBorderColor`** | string | 当输入控件被鼠标选中并激活聚焦时，外框的边框突显颜色（HEX格式）。 | `"#B0B8C8"` |
| **`AccentColor`** | string | 全局强调色，主要用于控制滑动杆滑块、选定点等激活态元素（HEX格式）。 | `"#7A8AA8"` |

---

## 5. LED 指示灯
| 参数名 | 类型 | 说明 | 示例 |
| :--- | :--- | :--- | :--- |
| **`LedOnColor`** | string | 指示灯处于 True（接通/点亮）状态下的辉光与主色（HEX格式）。 | `"#4CAF50"` |
| **`LedOffColor`** | string | 指示灯处于 False（断开/熄灭）状态下的底色（HEX格式）。 | `"#808080"` |

---

## 6. Chart 折线图表
| 参数名 | 类型 | 说明 | 示例 |
| :--- | :--- | :--- | :--- |
| **`ChartLineMode`** | int | 曲线渲染折线模式：`0` = 平滑贝塞尔曲线, `1` = 直连折线, `2` = 阶梯折线。 | `0` |
| **`ChartTitle`** | string | 图表顶部的中心主标题文本。 | `"实时曲线监控"` |
| **`ChartSubtitle`** | string | 图表顶部主标题下方的副标题文本。 | `"Multi-Series Analytics"` |
| **`ChartLineWeight`** | double | 曲线线条的渲染粗细。 | `2` |
| **`ChartFillOpacity`** | double | 曲线与底轴之间半透明阴影填充层的透明度。 | `0.2` |
| **`ChartColor1`** | string | 数据通道 1 的曲线前景色（HEX格式）。 | `"#1E90FF"` |
| **`ChartColor2`** | string | 数据通道 2 的曲线前景色（HEX格式）。 | `"#00FA9A"` |
| **`ChartColor3`** | string | 数据通道 3 的曲线前景色（HEX格式）。 | `"#FF4500"` |
| **`ChartShowGridLines`** | bool | 是否显示图表背景中的网格参考线。 | `true` |
| **`ChartPlotBackground`** | string | 曲线绘制核心区域的背景网格底色（HEX格式）。 | `"#08000000"` |
| **`ChartShowSeriesCards`** | bool | 是否在顶部中央显示图例（通道名卡片）图例栏。 | `true` |

---

## 7. DataGrid 数据表格
| 参数名 | 类型 | 说明 | 示例 |
| :--- | :--- | :--- | :--- |
| **`DataGridRowHeight`** | double | 表格中每行数据的高度（行高）。 | `40` |
| **`DataGridHeaderBackground`** | string | 表头列标题所在的背景底色（HEX格式）。 | `"#F8F9FB"` |
| **`DataGridBackground`** | string | 数据行区域的底色背景（HEX格式）。 | `"#FFFFFF"` |
| **`DataGridAlternatingOpacity`** | double | 奇偶行交替显示时，交替浅色背景的半透明底色透明度。 | `0.04` |
| **`DataGridGridLinesVisible`** | bool | 是否在单元格之间显示细网格边界线。 | `false` |
| **`DataGridLabelText`** | string | 表格上边栏标题的显示文本。 | `"数据表格"` |
| **`DataGridShowHeader`** | bool | 是否显示表格顶部的表头标题栏。 | `true` |

---

## 8. Gauge 仪表盘
| 参数名 | 类型 | 说明 | 示例 |
| :--- | :--- | :--- | :--- |
| **`GaugeColor1`** | string | 圆环进度量条渐变色的**起点**颜色（HEX格式）。 | `"#00BFFF"` |
| **`GaugeColor2`** | string | 圆环进度量条渐变色的**终点**颜色（HEX格式）。 | `"#00FA9A"` |

---

## 9. Slider 滑块 & ProgressBar 进度条
| 参数名 | 类型 | 说明 | 示例 |
| :--- | :--- | :--- | :--- |
| **`SliderColor1`** | string | 滑块已填充轨道的渐变色**起点**颜色（HEX格式）。 | `"#7A8AA8"` |
| **`SliderColor2`** | string | 滑块已填充轨道的渐变色**终点**颜色（HEX格式）。 | `"#4682B4"` |
| **`ProgressColor1`** | string | 进度条已完成部分的渐变色**起点**颜色（HEX格式）。 | `"#7A8AA8"` |
| **`ProgressColor2`** | string | 进度条已完成部分的渐变色**终点**颜色（HEX格式）。 | `"#4682B4"` |

---

## 10. ComboBox 下拉框
| 参数名 | 类型 | 说明 | 示例 |
| :--- | :--- | :--- | :--- |
| **`ComboBoxArrowColor`** | string | 下拉按钮指示用的三角箭头前景色（HEX格式）。 | `"#7A8AA8"` |

---

## 11. ToggleSwitch 开关
| 参数名 | 类型 | 说明 | 示例 |
| :--- | :--- | :--- | :--- |
| **`ActiveColor`** / **`ToggleColorOn`** | string | 开关处于 ON（开启）状态时轨道滑块的前景色（HEX格式）。 | `"#7A8AA8"` |
| **`InactiveColor`** / **`ToggleColorOff`** | string | 开关处于 OFF（关闭）状态时轨道轨道的底色背景色（HEX格式）。 | `"#C8CCD0"` |

---

## 12. Tree 树形列表
| 参数名 | 类型 | 说明 | 示例 |
| :--- | :--- | :--- | :--- |
| **`TreeItemHeight`** | double | 树节点每一层单行的高度。 | `36` |
| **`TreeIndentSize`** | double | 每一级子菜单相对于父菜单的左侧缩进像素。 | `24` |
| **`TreeLabelText`** | string | 树形控件顶部标题区域的显示文本。 | `"配置节点"` |
| **`TreeBackground`** | string | 树形结构容器的底色背景（HEX格式）。 | `"#FFFFFF"` |
| **`TreeShowCheckBox`** | bool | 树节点前面是否提供可以选择的复选框（CheckBox）。 | `true` |

---

## 13. Sidebar 侧边导航栏
| 参数名 | 类型 | 说明 | 示例 |
| :--- | :--- | :--- | :--- |
| **`SidebarLogoText`** | string | 侧边栏左上方 Logo 所要显示的品牌文字。 | `"WPF SIDEBAR"` |
| **`SidebarLogoImagePath`** | string | Logo 的图片文件路径。 | `""` |
| **`SidebarLogoUseImage`** | bool | 为 True 时强制展示图片，为 False 时仅展示文本文字。 | `false` |
| **`SidebarLogoMargin`** | string | Logo 位置的偏移内边距，格式 `"左,上,右,下"`。 | `"4,0,12,0"` |
| **`SidebarItemHeight`** | double | 各层级导航菜单单行项的高度。 | `40` |
| **`SidebarItemSpacing`** | double | 导航菜单项之间的垂直间距。 | `2` |
| **`SidebarBackground`** | string | 侧边栏整体所占用的背景底色（HEX格式）。 | `"#F0F2F5"` |

---

## 14. Topbar 顶部工具栏
| 参数名 | 类型 | 说明 | 示例 |
| :--- | :--- | :--- | :--- |
| **`TopbarLogoText`** | string | 顶部导航左上方 Logo 区域的显示文字。 | `"WPF TOPBAR"` |
| **`TopbarLogoImagePath`** | string | 顶部栏 Logo 图片文件所在的路径。 | `""` |
| **`TopbarLogoUseImage`** | bool | 是否启用图片格式作为顶部栏 Logo。 | `false` |
| **`TopbarHeight`** | double | 顶部工具栏的总体垂直高度。 | `60` |
| **`TopbarItemWidth`** | double | 单个顶部功能菜单按压项的宽度大小。 | `100` |
| **`TopbarBackground`** | string | 顶部工具栏整体区域的背景底色（HEX格式）。 | `"#E3E6EC"` |

---

## 15. IconButton 图标按钮
| 参数名 | 类型 | 说明 | 示例 |
| :--- | :--- | :--- | :--- |
| **`IconButtonText`** | string | 按钮展示的主体文本。 | `"图标按钮"` |
| **`IconButtonIconPath`** | string | 本地图标图片路径（如使用图片图标模式）。 | `""` |
| **`IconButtonUseImage`** | bool | 是否使用本地图片作为图标（True=图片, False=文字/矢量符号）。 | `false` |
