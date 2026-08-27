# WPF UI 控件库全量迁移至 Web Editor 实施计划

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 将 WPF 控件库中已设计的 18 种控件及其新拟态样式配置（基于 `style.json`）全量迁移移植至 Web Editor 中，并确保离线 HTML 网页能同步一键导出完整效果。

**Architecture:** 
1. 在 `canvas.ts` 中建立 `ThemeSettings` 全局接口，在 React 中引入全局 `ThemeContext`，使所有新拟态控件的主体颜色、阴影深度、高光不透明度等渲染行为完全由该配置项决定。
2. 扩展 Web Editor 的属性编辑栏，提供“全局主题配置”和“单个控件属性”的双重配置项。
3. 对全部 18 个控件进行 TDD 驱动的重构与新增，并升级单文件 HTML 导出模版，使其支持全量控件的一键式导出闭环。

**Tech Stack:** TypeScript, React 19, Vitest, SVG/Canvas, MQTT.js, esm.sh

---

### Task 1: 全局主题配置模型与右侧主题编辑面板 (Theme Settings Model & Editor Panel)

**Files:**
- Modify: `WebEditor/src/types/canvas.ts` (添加 ThemeSettings 类型定义)
- Modify: `WebEditor/src/hooks/useCanvasState.ts` (增加全局主题的 state 及 updateTheme 方法)
- Modify: `WebEditor/src/App.tsx` (在右侧属性栏新增“全局主题”选项卡与各表单项，并使用 ThemeContext 提供样式变量)
- Create: `WebEditor/src/test/theme.test.ts` (验证全局主题修改与状态传递)

**Step 1: 编写测试**
创建 `WebEditor/src/test/theme.test.ts`，写入如下测试：
```typescript
import { describe, it, expect } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useCanvasState } from '../hooks/useCanvasState';

describe('Global Theme State TDD', () => {
  it('should initialize with default neumorphic theme settings and allow updating', () => {
    const { result } = renderHook(() => useCanvasState());
    
    // @ts-ignore
    expect(result.current.theme.shadowBlur).toBe(10);
    // @ts-ignore
    expect(result.current.theme.cornerRadius).toBe(12);

    act(() => {
      // @ts-ignore
      result.current.updateTheme({ shadowBlur: 15, cornerRadius: 8 });
    });

    // @ts-ignore
    expect(result.current.theme.shadowBlur).toBe(15);
    // @ts-ignore
    expect(result.current.theme.cornerRadius).toBe(8);
  });
});
```

**Step 2: 运行测试并确认失败**
运行：`npm.cmd test src/test/theme.test.ts`
预期：FAIL（编译失败或 `result.current.theme` 未定义）

**Step 3: 编写最小实现代码**
修改 `WebEditor/src/types/canvas.ts`：
```typescript
export interface ThemeSettings {
  controlBackground: string;
  gradientStart: string;
  gradientMid: string;
  gradientEnd: string;
  borderColor: string;
  borderThickness: number;
  cornerRadius: number;
  cardPadding: string;
  shadowBlur: number;
  shadowDepth: number;
  shadowColor: string;
  shadowOpacity: number;
  highlightColor: string;
  highlightOpacity: number;
  fontFamily: string;
  fontSize: number;
  fontColor: string;
  caretColor: string;
  labelColor: string;
  labelFontSize: number;
  focusBorderColor: string;
  accentColor: string;
  ledOnColor: string;
  ledOffColor: string;
  toggleColorOn: string;
  toggleColorOff: string;
  // 其他特有配置项默认加载
  [key: string]: any;
}
```

修改 `WebEditor/src/hooks/useCanvasState.ts`：
```typescript
  const [theme, setTheme] = useState<ThemeSettings>({
    controlBackground: "#E3E6EC",
    gradientStart: "#EAEDF2",
    gradientMid: "#E0E3E9",
    gradientEnd: "#D8DCE3",
    borderColor: "#DDE0E6",
    borderThickness: 1,
    cornerRadius: 12,
    cardPadding: "12,8,12,6",
    shadowBlur: 10,
    shadowDepth: 4,
    shadowColor: "#A3A9B5",
    shadowOpacity: 0.5,
    highlightColor: "#FFFFFF",
    highlightOpacity: 0.65,
    fontFamily: "Segoe UI",
    fontSize: 14,
    fontColor: "#3A3F50",
    caretColor: "#5A6070",
    labelColor: "#8A90A0",
    labelFontSize: 11,
    focusBorderColor: "#B0B8C8",
    accentColor: "#7A8AA8",
    ledOnColor: "#4CAF50",
    ledOffColor: "#808080",
    toggleColorOn: "#7A8AA8",
    toggleColorOff: "#C8CCD0"
  });

  const updateTheme = (newSettings: Partial<ThemeSettings>) => {
    setTheme(prev => ({ ...prev, ...newSettings }));
  };
```
在 hook 导出项中加入 `theme` 和 `updateTheme`。

**Step 4: 运行测试并确认通过**
运行：`npm.cmd test src/test/theme.test.ts`
预期：PASS

**Step 5: 提交更改**
```bash
git add WebEditor/src/types/canvas.ts WebEditor/src/hooks/useCanvasState.ts WebEditor/src/test/theme.test.ts
git commit -m "feat(webeditor): 建立全局 Neumorphic 主题参数模型与状态绑定"
```

---

### Task 2: 移植 ToggleSwitch 与 TextInput 控件 (TDD)

**Files:**
- Create: `WebEditor/src/components/ToggleSwitch.tsx` (WPF 原版开关)
- Create: `WebEditor/src/components/TextInput.tsx` (文本输入控件)
- Create: `WebEditor/src/test/ToggleSwitch.test.tsx`
- Create: `WebEditor/src/test/TextInput.test.tsx`

**Step 1: 编写测试**
为 `ToggleSwitch` 和 `TextInput` 编写基本的订阅与发布测试，验证点击开关发布 `true`/`false`，输入文本发布字符串。

**Step 2: 确认测试失败**
运行测试套件，确认新增测试由于未找到模块或无功能实现而失败。

**Step 3: 最小代码实现**
依据 `style.json` 中的 `toggleColorOn`/`toggleColorOff` 颜色，实现胶囊切换动画的 `ToggleSwitch`；使用 Neumorphic 阴影实现聚焦高光的 `TextInput`。

**Step 4: 确认测试通过**
运行：`npm.cmd test`
预期：全部通过。

**Step 5: 提交更改**
```bash
git add WebEditor/src/components/ToggleSwitch.tsx WebEditor/src/components/TextInput.tsx
git commit -m "feat(webeditor): 移植 ToggleSwitch 与 TextInput 控件"
```

---

### Task 3: 移植 ProgressBar 与 Gauge 仪表盘控件 (TDD)

**Files:**
- Create: `WebEditor/src/components/ProgressBar.tsx` (双色渐变进度条)
- Create: `WebEditor/src/components/Gauge.tsx` (新拟态圆环仪表盘)
- Create: `WebEditor/src/test/ProgressBar.test.tsx`
- Create: `WebEditor/src/test/Gauge.test.tsx`

**Step 1: 编写测试**
测试在接收 MQTT 数值时，ProgressBar 改变进度宽度，Gauge 利用 SVG 旋转百分比。

**Step 2: 确认测试失败**
运行测试，断言由于未完成绘制逻辑而失败。

**Step 3: 最小代码实现**
实现 ProgressBar 绑定 `ProgressColor1` 和 `ProgressColor2` 渐变。使用 SVG 路径、`<circle>` 渲染 Gauge 仪表盘圆环，渐变绑定 `GaugeColor1` 和 `GaugeColor2`。

**Step 4: 确认测试通过**
运行：`npm.cmd test`

**Step 5: 提交**
```bash
git add WebEditor/src/components/ProgressBar.tsx WebEditor/src/components/Gauge.tsx
git commit -m "feat(webeditor): 移植 ProgressBar 与 Gauge 仪表盘控件"
```

---

### Task 4: 移植 Chart 与 Pie 图表控件 (TDD)

**Files:**
- Create: `WebEditor/src/components/Chart.tsx` (支持 Smooth 贝塞尔折线图)
- Create: `WebEditor/src/components/Pie.tsx` (饼图比例展示)
- Create: `WebEditor/src/test/Chart.test.tsx`

**Step 1: 编写测试**
验证 Chart 在接收数组 payload 时正确追加时序数据。

**Step 2: 确认测试失败**
运行测试，确认未完成时序数据处理导致失败。

**Step 3: 最小代码实现**
实现轻量 HTML5 Canvas 或 SVG 折线图，读取 `chartColor1` ... `chartColor3` 配置多通道前景色，由 `chartLineMode` 决定平滑度，网格底色对应 `chartPlotBackground`。实现 Pie 控件展示百分比占比。

**Step 4: 确认测试通过**
运行：`npm.cmd test`

**Step 5: 提交**
```bash
git commit -m "feat(webeditor): 移植 Chart 与 Pie 图表组件"
```

---

### Task 5: 移植 DataGrid, Tree 与 TreeList 复杂数据控件 (TDD)

**Files:**
- Create: `WebEditor/src/components/DataGrid.tsx` (数据表格)
- Create: `WebEditor/src/components/Tree.tsx` (树状列表)
- Create: `WebEditor/src/test/DataGrid.test.tsx`

**Step 1: 编写测试**
编写测试验证 DataGrid 能订阅结构化 JSON 数据并呈现行，Tree 支持展开折叠和选择复选框。

**Step 2: 确认测试失败**
运行测试，确认渲染失败。

**Step 3: 最小代码实现**
设计表格渲染绑定 `dataGridRowHeight`、`dataGridHeaderBackground` 与 `dataGridAlternatingOpacity`。实现树状列表节点递归绘制与左边距缩进 `treeIndentSize`。

**Step 4: 确认测试通过**
运行测试确认全绿。

**Step 5: 提交**
```bash
git commit -m "feat(webeditor): 移植 DataGrid、Tree 与 TreeList 控件"
```

---

### Task 6: 移植 Topbar, Sidebar 与 ComboBox, IconButton (TDD)

**Files:**
- Create: `WebEditor/src/components/Topbar.tsx`
- Create: `WebEditor/src/components/Sidebar.tsx`
- Modify: `WebEditor/src/components/Canvas.tsx` (在画布中集成全量 18 种控件的渲染逻辑)

**Step 1: 编写测试**
在 `Canvas.test.tsx` 中编写测试，断言 18 种控件皆可在画布中成功根据 type 进行挂载。

**Step 2: 确认测试失败**
运行测试，确认无法挂载报错。

**Step 3: 最小代码实现**
在 `Canvas.tsx` 的类型判断中，引入这 14 种新增控件。在 App.tsx 工具箱中增加这 14 种控件的添加按钮。

**Step 4: 确认测试通过**
运行：`npm.cmd test`
确保全部 100% 绿灯。

**Step 5: 提交**
```bash
git commit -m "feat(webeditor): 全量集成 Topbar, Sidebar 与 18 种控件到画布"
```

---

### Task 7: 升级离线 HTML 一键导出模版 (Export Template Upgrade)

**Files:**
- Modify: `WebEditor/src/utils/htmlTemplate.ts` (升级 generateDashboardHtml 注入全量 18 个控件与 style.json 全局变量)
- Modify: `WebEditor/src/test/htmlTemplate.test.ts` (更新测试断言)

**Step 1: 编写测试**
升级 `htmlTemplate.test.ts`，断言导出的大 HTML 包含所有 18 个控件组件代码片段，以及 `theme` 样式配置对象的渲染。

**Step 2: 确认测试失败**
运行测试并失败。

**Step 3: 最小代码实现**
重构 `htmlTemplate.ts`，将所有 18 个控件的 ES Module 代码以及 `React.createElement` 映射逻辑放入打包模板中。

**Step 4: 确认测试通过**
运行 `npm.cmd test` 确认 100% 通过。
运行 `npm.cmd run build` 确保完全零错误编译成功。

**Step 5: 提交**
```bash
git add WebEditor/src/utils/htmlTemplate.ts WebEditor/src/test/htmlTemplate.test.ts
git commit -m "feat(webeditor): 完成 18 种全量控件在离线一键导出模板中的升级与部署闭环"
```
