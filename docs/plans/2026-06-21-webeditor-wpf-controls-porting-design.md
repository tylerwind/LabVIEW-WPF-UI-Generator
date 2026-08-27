# WPF UI 控件库全量迁移至 Web Editor 详细设计说明书

本文档记录了将 WPF 生成器中已设计的 18 种控件及其基于 `style.json` 的全局主题配置迁移至 Web Editor 的架构与实现方案。

---

## 1. 架构方案与全局主题系统 (Theme System)

在 Web Editor 中引入完整的全局样式控制系统。此样式系统对应 WPF 生成器中的 `ControlStyle.cs` 数据结构：

```typescript
export interface ThemeSettings {
  // === 基础面板与卡片布局 ===
  controlBackground: string;      // "#E3E6EC"
  gradientStart: string;          // "#EAEDF2"
  gradientMid: string;            // "#E0E3E9"
  gradientEnd: string;            // "#D8DCE3"
  borderColor: string;            // "#DDE0E6"
  borderThickness: number;        // 1
  cornerRadius: number;           // 12
  cardPadding: string;            // "12,8,12,6"

  // === 立体阴影与高光 ===
  shadowBlur: number;             // 10
  shadowDepth: number;            // 4
  shadowColor: string;            // "#A3A9B5"
  shadowOpacity: number;          // 0.5
  highlightColor: string;         // "#FFFFFF"
  highlightOpacity: number;       // 0.65

  // === 字体与文本渲染 ===
  fontFamily: string;             // "Segoe UI"
  fontSize: number;               // 14
  fontColor: string;              // "#3A3F50"
  caretColor: string;             // "#5A6070"
  labelColor: string;             // "#8A90A0"
  labelFontSize: number;          // 11

  // === 交互状态 ===
  focusBorderColor: string;       // "#B0B8C8"
  accentColor: string;            // "#7A8AA8"

  // === 控件专属配置项 ===
  ledOnColor: string;             // "#4CAF50"
  ledOffColor: string;            // "#808080"
  chartLineMode: number;          // 0=Smooth, 1=Linear, 2=Step
  chartTitle: string;
  chartSubtitle: string;
  chartLineWeight: number;
  chartFillOpacity: number;
  chartColor1: string;
  chartColor2: string;
  chartColor3: string;
  chartShowGridLines: boolean;
  chartPlotBackground: string;
  chartShowSeriesCards: boolean;
  
  dataGridRowHeight: number;
  dataGridHeaderBackground: string;
  dataGridBackground: string;
  dataGridAlternatingOpacity: number;
  dataGridGridLinesVisible: boolean;
  dataGridLabelText: string;
  dataGridShowHeader: boolean;

  gaugeColor1: string;
  gaugeColor2: string;

  sliderColor1: string;
  sliderColor2: string;

  progressColor1: string;
  progressColor2: string;

  comboBoxArrowColor: string;

  toggleColorOn: string;
  toggleColorOff: string;

  treeItemHeight: number;
  treeIndentSize: number;
  treeLabelText: string;
  treeBackground: string;
  treeShowCheckBox: boolean;

  sidebarLogoText: string;
  sidebarBackground: string;
  sidebarItemHeight: number;

  topbarLogoText: string;
  topbarBackground: string;
  topbarHeight: number;

  iconButtonText: string;
}
```

- **数据绑定形式**：在 React 主体应用中提供 `ThemeContext`。所有拟态控件读取此 Context 的配置以实时改变呈现效果（阴影、边框、高光等）。

---

## 2. 18 类控件库功能与渲染细则

1. **Button & IconButton**：常规和带图标按钮。图标加载支持本地路径，支持点击高亮反转。
2. **Led**：核心圆环通过 Canvas 或 SVG `<circle>` 绘制。
3. **Slider & ProgressBar**：Slider 使用原生 input range 进行隐式覆盖，外层使用 Neumorphic 阴影轨道填充。ProgressBar 为双色渐变进度指示器。
4. **TextInput & NumericInput & NumericDisplay**：TextInput 支持文字输入。NumericInput 支持数值增加/减少（微调器按钮交互）。NumericDisplay 用于只读展示。
5. **ComboBox**：新拟态下拉框，包含选项列表的浮动毛玻璃面板。
6. **ToggleSwitch**：圆滑开关滑块，使用 CSS transition 动画渲染推拉手感。
7. **Gauge**：环形进度仪表盘。使用 SVG 的 `stroke-dasharray` 来展示当前度数。
8. **Chart & Pie**：Chart 采用 Canvas 或 Chart.js（轻量实现）展示平滑曲线。Pie 渲染占比饼图。
9. **DataGrid**：Neumorphic 外框表格，具有行高定制与奇偶行斑马纹。
10. **Tree & TreeList**：层级节点展开折叠组件，配备独立复选框。
11. **Sidebar & Topbar**：用于提供外层侧边或顶部布局容器。

---

## 3. MQTT 通信与数据流动

1. **控制与修改器（发布侧）**：每次输入或按钮点击会立刻触发 `mqtt.publish(topic, value)`。
2. **指示器（订阅侧）**：自动解析 MQTT 的 payload 数据，若为数值则执行 Clamp 处理；若为 Tree/DataGrid 等复杂控件，则通过 `JSON.parse` 处理。对于格式不匹配的数据采用 `try-catch` 捕获以防崩溃。

---

## 4. 验证规划
- **单元测试**：针对全部新增控件在 `src/test/` 下创建相对应的 `.test.tsx` 单元测试，测试参数正确反射在组件上的效果。
- **打包验证**：运行 `npm run build` 和 `vitest run` 保证持续集成成功。
