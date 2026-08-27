# Support Raw JSON Style String in UpdateStyleFromJson Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Modify the `UpdateStyleFromJson` method in the 6 WPF control panels to accept and parse either a JSON file path or a raw JSON string directly.

**Architecture:** Add automatic detection of input type: if the trimmed string starts with `{` and ends with `}`, treat it as raw JSON; otherwise, treat it as a file path.

**Tech Stack:** C# (.NET 4.0), PowerShell (for TDD scripts).

---

## User Review Required

No breaking changes. The modification is fully backwards compatible.

## Open Questions

None.

## Proposed Changes

### Task 1: TDD Failure Setup (Red Light)

**Files:**
- Modify: [test_tdd_style.ps1](file:///d:/Tyler/%E5%85%AC%E4%BC%97%E5%8F%B7/LabVIEW-WPF-UI-Generator-main-Tree/test_tdd_style.ps1#L147)

**Step 1: Write the failing test**
Change the argument passed to `UpdateStyleFromJson` in the reflection check from `$jsonPath` to `$testStyleJson` (the raw JSON string).
In [test_tdd_style.ps1](file:///d:/Tyler/%E5%85%AC%E4%BC%97%E5%8F%B7/LabVIEW-WPF-UI-Generator-main-Tree/test_tdd_style.ps1):
```diff
-    $method.Invoke($panelInstance, @($jsonPath))
+    $method.Invoke($panelInstance, @($testStyleJson))
```

**Step 2: Run test to verify it fails**
Run: `powershell -File .\test_tdd_style.ps1`
Expected: FAIL with "Assertion Failed: Expected FontSize 24.0, but got 14" (because the style is not parsed as a file path and ignored).

---

### Task 2: Implement Path/Text Auto-Detection in ExportTemplate

**Files:**
- Modify: [ButtonPanel.cs](file:///d:/Tyler/%E5%85%AC%E4%BC%97%E5%8F%B7/LabVIEW-WPF-UI-Generator-main-Tree/ExportTemplate/ButtonPanel.cs#L131-L155)
- Modify: [LedPanel.cs](file:///d:/Tyler/%E5%85%AC%E4%BC%97%E5%8F%B7/LabVIEW-WPF-UI-Generator-main-Tree/ExportTemplate/LedPanel.cs#L112-L136)
- Modify: [ProgressBarPanel.cs](file:///d:/Tyler/%E5%85%AC%E4%BC%97%E5%8F%B7/LabVIEW-WPF-UI-Generator-main-Tree/ExportTemplate/ProgressBarPanel.cs#L166-L190)
- Modify: [SliderPanel.cs](file:///d:/Tyler/%E5%85%AC%E4%BC%97%E5%8F%B7/LabVIEW-WPF-UI-Generator-main-Tree/ExportTemplate/SliderPanel.cs#L249-L273)
- Modify: [TextInputPanel.cs](file:///d:/Tyler/%E5%85%AC%E4%BC%97%E5%8F%B7/LabVIEW-WPF-UI-Generator-main-Tree/ExportTemplate/TextInputPanel.cs#L227-L251)
- Modify: [ToggleSwitchPanel.cs](file:///d:/Tyler/%E5%85%AC%E4%BC%97%E5%8F%B7/LabVIEW-WPF-UI-Generator-main-Tree/ExportTemplate/ToggleSwitchPanel.cs#L145-L169)

**Step 1: Modify the panels' `UpdateStyleFromJson` method**
Update the logic in each class:
```csharp
        public void UpdateStyleFromJson(string jsonPath)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonPath))
                    return;

                string json = null;
                string trimmed = jsonPath.Trim();
                if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
                {
                    json = trimmed;
                }
                else if (System.IO.File.Exists(jsonPath))
                {
                    json = System.IO.File.ReadAllText(jsonPath, System.Text.Encoding.UTF8);
                }

                if (string.IsNullOrEmpty(json))
                    return;

                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var dict = serializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(json);
                if (dict != null)
                {
                    ApplyStyleDictionary(dict);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    System.IO.File.AppendAllText("StyleUpdateError.txt",
                        DateTime.Now.ToString() + " : " + ex.Message + "\n" + ex.StackTrace + "\n");
                }
                catch { }
            }
        }
```

**Step 2: Run test to verify it passes (Green Light)**
Run: `powershell -File .\test_tdd_style.ps1`
Expected: PASS with "TDD GREEN - Target FontSize successfully updated to 24.0!"

**Step 3: Commit the task changes**
Stage and commit changes to `ExportTemplate` and `test_tdd_style.ps1`.

---

### Task 3: Sync changes to bin/Release/ExportTemplate

**Files:**
- Modify: [ButtonPanel.cs](file:///d:/Tyler/%E5%85%AC%E4%BC%97%E5%8F%B7/LabVIEW-WPF-UI-Generator-main-Tree/ControlDesigner/bin/Release/ExportTemplate/ButtonPanel.cs#L131-L155)
- Modify: [LedPanel.cs](file:///d:/Tyler/%E5%85%AC%E4%BC%97%E5%8F%B7/LabVIEW-WPF-UI-Generator-main-Tree/ControlDesigner/bin/Release/ExportTemplate/LedPanel.cs#L112-L136)
- Modify: [ProgressBarPanel.cs](file:///d:/Tyler/%E5%85%AC%E4%BC%97%E5%8F%B7/LabVIEW-WPF-UI-Generator-main-Tree/ControlDesigner/bin/Release/ExportTemplate/ProgressBarPanel.cs#L166-L190)
- Modify: [SliderPanel.cs](file:///d:/Tyler/%E5%85%AC%E4%BC%97%E5%8F%B7/LabVIEW-WPF-UI-Generator-main-Tree/ControlDesigner/bin/Release/ExportTemplate/SliderPanel.cs#L249-L273)
- Modify: [TextInputPanel.cs](file:///d:/Tyler/%E5%85%AC%E4%BC%97%E5%8F%B7/LabVIEW-WPF-UI-Generator-main-Tree/ControlDesigner/bin/Release/ExportTemplate/TextInputPanel.cs#L227-L251)
- Modify: [ToggleSwitchPanel.cs](file:///d:/Tyler/%E5%85%AC%E4%BC%97%E5%8F%B7/LabVIEW-WPF-UI-Generator-main-Tree/ControlDesigner/bin/Release/ExportTemplate/ToggleSwitchPanel.cs#L145-L169)

**Step 1: Apply the same modifications to the build-template copy**
Copy the modified methods to the release-packaged versions of the files under `ControlDesigner/bin/Release/ExportTemplate/`.

**Step 2: Commit**
Stage and commit sync changes.

---

## Verification Plan

### Automated Tests
- Run `powershell -File .\test_tdd_style.ps1` to verify compilation and dynamic styling from raw JSON string works correctly.
