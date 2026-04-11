# TreeControl Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implement a commercial-grade TreeView WPF control for LabVIEW featuring lazy-loading events (`NodeExpanding`), batch configuration via CheckBoxes, and a modern Glass UI (no tree lines, full-width selection).

**Architecture:**
The implementation spans the ControlDesigner UI (for preview/configuration), the TemplateEngine (for generating customized C# files), and the core Templates (`TreeControl.xaml.template`, `TreeControl.xaml.cs`, `TreePanel.cs`). 

**Tech Stack:** C# 5.0, .NET Framework 4.0, WPF, XAML.

---

### Task 1: Extend Enums & Models
**Files:**
- Modify: `d:\Tyler\公众号\LabVIEW-WPF-UI-Generator-main\ControlDesigner\Models\Enums.cs`
- Modify: `d:\Tyler\公众号\LabVIEW-WPF-UI-Generator-main\ControlDesigner\Models\ControlStyle.cs`

**Step 1:** Add `TreeDisplay` to `ControlType` enum in `Enums.cs`.
**Step 2:** Add specific style properties (e.g. `TreeItemHeight`, `TreeIndentSize`) to `ControlStyle.cs`.

### Task 2: Update Template Engine & UI
**Files:**
- Modify: `d:\Tyler\公众号\LabVIEW-WPF-UI-Generator-main\ControlDesigner\Services\TemplateEngine.cs`
- Modify: `d:\Tyler\公众号\LabVIEW-WPF-UI-Generator-main\ControlDesigner\MainWindow.xaml`
- Modify: `d:\Tyler\公众号\LabVIEW-WPF-UI-Generator-main\ControlDesigner\MainWindow.xaml.cs`

**Step 1:** Update `TemplateEngine.GenerateProject` and `GenerateAllProject` to handle `TreeDisplay` generating `TreeControl.xaml`, `TreeControl.xaml.cs`, and `TreePanel.cs`.
**Step 2:** Add a Tree button to the `MainWindow.xaml` palette and wire it up to `SetType(ControlType.TreeDisplay)`.

### Task 3: Create XAML Control Template
**Files:**
- Create: `d:\Tyler\公众号\LabVIEW-WPF-UI-Generator-main\ExportTemplate\TreeControl.xaml.template`

**Step 1:** Implement a minimalist `TreeView` without default lines.
**Step 2:** Provide `TreeViewItem` style matching Glass UI: transparent unselected, full-width `AccentColor` background or `FontColor Opacity 0.12` when selected, animated expanding arrow, and conditionally bound `CheckBox`.

### Task 4: Create C# Code-Behind (API & Data Structure)
**Files:**
- Create: `d:\Tyler\公众号\LabVIEW-WPF-UI-Generator-main\ExportTemplate\TreeControl.xaml.cs`

**Step 1:** Define `TreeNode` class with `Id`, `ParentId`, `Text`, `IsExpanded`, `IsChecked`, `HasChildrenDummy`, and `Children` (ObservableCollection).
**Step 2:** Implement root `ItemsCollection` and logic for `AddNode`, `RemoveNode`, `ClearNodes`, `SetNodeChecked`, `GetCheckedNodes()`.
**Step 3:** Implement logic to throw events when a dummy node is expanded (`OnNodeExpanding`).

### Task 5: Create LabVIEW Panel Wrapper
**Files:**
- Create: `d:\Tyler\公众号\LabVIEW-WPF-UI-Generator-main\ExportTemplate\TreePanel.cs`

**Step 1:** Wrap `TreeControl` and expose generic unmanaged DLL API functions compatible with LabVIEW.
**Step 2:** Forward events (`NodeSelected`, `NodeExpanding`) to standard LabVIEW callbacks.

### Task 6: Verification & Export
**Verification Plan. Step 1:** Compile the solution via Visual Studio or MSBuild.
**Step 2:** Run `ControlDesigner.exe`.
**Step 3:** Create a TreeControl, customize colors, and click "Export".
**Step 4:** Verify the output DLL contains the expected classes.
