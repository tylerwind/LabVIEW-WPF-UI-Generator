# LED Corner Radius Adaptation Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Modify the LED indicator (`LedControl`) so that its shape is determined by the global `CornerRadius` setting, allowing it to morph from a perfect circle to a square or rounded square.

**Architecture:** We will replace all `Ellipse` elements inside `LedControl` with `Border` elements and set their `CornerRadius` to `{{CornerRadius}}`. We will also update the code-behind `ApplyStyle` method to support dynamic updates of the LED shape at runtime.

**Tech Stack:** C# (.NET 4.0), WPF, XAML

---

### Task 1: Update LedControl XAML Template

**Files:**
- Modify: [LedControl.xaml.template](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/ExportTemplate/LedControl.xaml.template)

**Step 1: Replace Ellipses with Borders**
Replace all five `Ellipse` instances in `LedControl.xaml.template` with named `Border` elements:
- 凹槽底座 $\rightarrow$ `Border` named `BaseBorder` with `CornerRadius="{{CornerRadius}}"`
- 灯体 $\rightarrow$ `Border` named `LedBorder` with `CornerRadius="{{CornerRadius}}"` and `Background` set to `LedOffBrush` (which is a `SolidColorBrush`)
- 发光层 $\rightarrow$ `Border` named `LedGlow` with `CornerRadius="{{CornerRadius}}"`
- 高光反射 $\rightarrow$ `Border` named `ReflectBorder` with `CornerRadius="{{CornerRadius}}"`
- 外发光晕 $\rightarrow$ `Border` named `LedHalo` with `CornerRadius="{{CornerRadius}}"`

---

### Task 2: Update LedControl Code-Behind

**Files:**
- Modify: [LedControl.xaml.cs](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/ExportTemplate/LedControl.xaml.cs)

**Step 1: Add CornerRadius to ApplyStyle**
In the `ApplyStyle(Dictionary<string, object> style)` method, add logic to check if `style` contains `"CornerRadius"`.
If present:
- Parse `CornerRadius` as a double.
- Apply a new `CornerRadius(val)` to `BaseBorder`, `LedBorder`, `LedGlow`, `ReflectBorder`, and `LedHalo` if they are not null.

---

### Task 3: Sync and Rebuild ControlDesigner

**Files:**
- Create/Overwrite: `ControlDesigner/bin/Release/ExportTemplate/LedControl.xaml.template`
- Create/Overwrite: `ControlDesigner/bin/Release/ExportTemplate/LedControl.xaml.cs`

**Step 1: Copy modified files to Release copies**
Copy files using PowerShell.

**Step 2: Rebuild the project**
Run `msbuild` to compile the solution.

---

### Task 4: Compilation and Verification

**Step 1: Run verification export script**
Run `test_export_all.ps1` to ensure DLL collection compiles cleanly.
Check the generated XAML files to ensure they contain correct properties and compile successfully.
