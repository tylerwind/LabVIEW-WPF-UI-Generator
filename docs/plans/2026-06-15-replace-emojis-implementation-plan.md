# Replace Emojis with Safe ASCII Placeholders Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Remove all non-ASCII emoji characters (`"🚀"`, `"🌟"`, `"🎯"`, `"🔘"`) from default parameters, templates, UI views, fallbacks, and `.style.json` configs, replacing them with safe ASCII alternatives like `"Logo"` and `"Icon"` to prevent LabVIEW encoding crashes and display errors.

**Architecture:** We will replace default emoji definitions in code-behind templates, properties, template generator fallbacks, design UI textboxes, and template JSON configurations, then compile the designer and run programmatic validation tests.

**Tech Stack:** C# (.NET 4.0), WPF, MSBuild, PowerShell

---

### Task 1: Update Designer and Control Templates

**Files:**
- Modify: [ControlStyle.cs](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/ControlDesigner/Models/ControlStyle.cs)
- Modify: [TemplateEngine.cs](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/ControlDesigner/Services/TemplateEngine.cs)
- Modify: [MainWindow.xaml.cs](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/ControlDesigner/MainWindow.xaml.cs)
- Modify: [MainWindow.xaml](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/ControlDesigner/MainWindow.xaml)
- Modify: [SidebarControl.xaml.cs](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/ExportTemplate/SidebarControl.xaml.cs)
- Modify: [TopbarControl.xaml.cs](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/ExportTemplate/TopbarControl.xaml.cs)

**Step 1: Write a verification script to detect emoji usages**
Create a scratch PowerShell script `scratch/verify_no_emojis.ps1` that checks the files and reports if they contain `🚀`, `🌟`, `🎯`, or `🔘`.
It should fail (exit 1) if any are found in target lines.

**Step 2: Run the script to verify failures**
Run the script to see that it detects the existing emojis.

**Step 3: Modify C# files and XAML template to replace emojis**
- In [ControlStyle.cs](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/ControlDesigner/Models/ControlStyle.cs):
  - Change `_sidebarLogoIconText = "🚀"` to `_sidebarLogoIconText = "Logo"`
  - Change `_topbarLogoIconText = "🌟"` to `_topbarLogoIconText = "Logo"`
  - Change `_iconButtonIconText = "🎯"` to `_iconButtonIconText = "Icon"`
- In [TemplateEngine.cs](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/ControlDesigner/Services/TemplateEngine.cs):
  - Change fallback `"🚀"` to `"Logo"`
  - Change fallback `"🌟"` to `"Logo"`
  - Change fallback `"🔘"` to `"Icon"`
- In [MainWindow.xaml.cs](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/ControlDesigner/MainWindow.xaml.cs):
  - Change fallback `"🔘"` to `"Icon"`
- In [MainWindow.xaml](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/ControlDesigner/MainWindow.xaml):
  - Change `TxtSidebarLogoIcon` `Text="🚀"` to `Text="Logo"`
  - Change `TxtTopbarLogoIcon` `Text="🌟"` to `Text="Logo"`
  - Change `TxtIconButtonIconText` `Text="🎯"` to `Text="Icon"`
- In [SidebarControl.xaml.cs](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/ExportTemplate/SidebarControl.xaml.cs):
  - Change dependency property default `"🚀"` to `"Logo"`
  - Change fallback `"🚀"` to `"Logo"`
- In [TopbarControl.xaml.cs](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/ExportTemplate/TopbarControl.xaml.cs):
  - Change dependency property default `"🌟"` to `"Logo"`
  - Change fallback `"🌟"` to `"Logo"`

**Step 4: Run verification script to check that they are resolved**
Run `scratch/verify_no_emojis.ps1` to ensure no source file contains the emojis.

**Step 5: Commit**
Commit changes to local Git repository with Conventional Commits (中文).

---

### Task 2: Sync Updated Templates to bin/Release/ExportTemplate/

**Files:**
- Modify: `ControlDesigner/bin/Release/ExportTemplate/SidebarControl.xaml.cs`
- Modify: `ControlDesigner/bin/Release/ExportTemplate/TopbarControl.xaml.cs`

**Step 1: Copy modified files**
Copy `ExportTemplate/SidebarControl.xaml.cs` and `ExportTemplate/TopbarControl.xaml.cs` to `ControlDesigner/bin/Release/ExportTemplate/`.

**Step 2: Verify copy matches source**
Verify diff of target files to ensure they successfully synchronized.

---

### Task 3: Update Default JSON Style Configuration Files

**Files:**
- Modify: [MyControlAll.style.json](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/UI/Ui%20Xcontrol/Xcontrol/MyControlAll.style.json)
- Modify: [MyButton.style.json](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/UI/data/MyButton.style.json)
- Modify: [MyControlAll.style.json](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/UI/data/MyControlAll.style.json)
- Modify: [MyTopbar.style.json](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/UI/data/MyTopbar.style.json)
- Modify: [MyTreeList.style.json](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/UI/data/MyTreeList.style.json)
- Modify: [MyIconButton.style.json](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/UI/data/MyIconButton.style.json)
- Modify: [MyDataGrid.style.json](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/UI/data/MyDataGrid.style.json)
- Modify: [MyIconButton.style.json](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/UI/Ui2.0/IconButton/MyIconButton.style.json)
- Modify: [MyButton.style.json](file:///d:/Tyler/公众号/LabVIEW-WPF-UI-Generator-main-Tree/UI/Ui2.0/Button/MyButton.style.json)

**Step 1: Run search on JSON files for emojis**
Verify which JSON files contain `🚀`, `🌟`, `🎯`, or `🔘`.

**Step 2: Replace emojis with safe ASCII placeholders in all identified JSON files**
- `"SidebarLogoIconText": "🚀"` -> `"SidebarLogoIconText": "Logo"`
- `"TopbarLogoIconText": "🌟"` -> `"TopbarLogoIconText": "Logo"`
- `"IconButtonIconText": "🎯"` -> `"IconButtonIconText": "Icon"`

**Step 3: Run validation to verify no emojis remain in target JSONs**
Use the scratch verification script to scan JSON files as well.

---

### Task 4: Compilation and Build Verification

**Step 1: Build the ControlDesigner Project**
Compile via MSBuild:
`msbuild ControlDesigner/ControlDesigner.csproj /p:Configuration=Release`

**Step 2: Run test_export_all.ps1 to verify full export works**
Execute `powershell -ExecutionPolicy Bypass .\test_export_all.ps1` to generate testing DLLs and confirm everything is working dynamically.
