# DataGrid Cell Badge and Dynamic Update Design Specification

This document details the design and architecture for adding dynamic cell background colors (via badge rendering) and cell-level content updates to the WPF `DataGrid` component used by the LabVIEW-WPF-UI-Generator.

## Design Goals
1. **Dynamic Styling**: Support coloring specific cells in the DataGrid (e.g., status badges for "Error" / "OK") without compromising global selection/hover styles.
2. **Smooth Scrolling**: Avoid virtualization rendering glitches common in custom DataGrid coloring.
3. **LabVIEW Native Integration**: Support setting cell background colors using standard LabVIEW color integers (U32/I32).
4. **Reactive Single-Cell Updating**: Provide a way to modify individual cells reactively without reloading the entire dataset.

## Architecture & Data Flow

### 1. Embedded Styling Syntax
We define an embedded text pattern:
```
[#COLOR_HEX_OR_NAME]TEXT
```
- Examples: `"[#FF5722]Error"`, `"[#2E8B57]OK"`, `"[#Coral]Warning"`.
- If a cell's string matches this pattern, the rendering engine displays `TEXT` wrapped inside a rounded colored `Border` (Badge) using the specified color.
- If a cell does not match the pattern, it renders as plain text with no background decoration.

### 2. Class: `CellBadgeConverter`
A C# value converter implementing `IValueConverter` that parses the string and extracts:
- `Visibility` (Visible for badge, Collapsed for normal text).
- `NormalVisibility` (Collapsed for badge, Visible for normal text).
- `Text` (Stripped of the `[#...]` metadata prefix).
- `Background` (Parsed `Brush` from the color metadata).

### 3. Dynamic Columns Interception
In `DataGridControl.xaml.cs`, we subscribe to the `AutoGeneratingColumn` event. When a text column is generated, we swap it with a custom `DataGridTemplateColumn` containing:
- A host `Grid`.
- A default `TextBlock` for plain text.
- A `Border` enclosing a `TextBlock` representing the Badge.
- Binding both elements to the `CellBadgeConverter` with appropriate converter parameters.

### 4. API Endpoints
Two public APIs are added to `DataGridControl` and wrapped in `DataGridPanel`:

```csharp
// Helper to format string in LabVIEW
public static string FormatBadge(string text, int colorValue);

// Dynamic cell update
public void UpdateCell(int rowIndex, int colIndex, string value);
```

---

## Verification Plan

### Automated Verification
- Verify the template compiles and builds successfully under MSBuild.

### Manual Verification
- Visual inspection of the generated control in the WPF control designer to ensure:
  - "Error" and "OK" cells render with beautiful rounded badges.
  - Hovering and selecting rows behaves correctly without conflicts.
  - Modifying text updates dynamically.
