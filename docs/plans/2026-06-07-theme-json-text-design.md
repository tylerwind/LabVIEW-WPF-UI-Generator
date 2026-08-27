# 控件主题 JSON 地址支持 JSON 文本输入设计方案

## 1. 背景与目标
目前 LabVIEW WPF UI 生成器导出的控件在运行时通过 `UpdateStyleFromJson(string jsonPath)` 接收样式配置文件路径并重绘 UI。为了提高灵活性、减少临时文件 I/O，并避免文件权限问题，我们需要将该接口升级为：**既支持传入 JSON 配置文件路径，也支持直接传入 JSON 格式的样式文本**。

## 2. 详细设计
在以下 6 个面板控件的 `UpdateStyleFromJson` 方法中加入自动识别逻辑：
- `ButtonPanel.cs`
- `LedPanel.cs`
- `ProgressBarPanel.cs`
- `SliderPanel.cs`
- `TextInputPanel.cs`
- `ToggleSwitchPanel.cs`

同时，必须保证：
1. `ExportTemplate` 目录与 `ControlDesigner\bin\Release\ExportTemplate` 目录下的对应源文件**完全同步更新**。
2. 原有文件路径逻辑完全向后兼容。

### 2.1 识别算法
对于传入的参数 `jsonPathOrText`：
1. 若为空，直接返回。
2. 去除首尾空白字符。
3. 若首字符为 `{` 且尾字符为 `}`，则视为 **JSON 文本**，直接解析。
4. 否则，视为 **文件路径**：
   - 检查文件是否存在。
   - 若存在，读取文件内容后解析。
   - 若不存在，直接返回。

### 2.2 伪代码实现
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
        else
        {
            if (System.IO.File.Exists(jsonPath))
            {
                json = System.IO.File.ReadAllText(jsonPath, System.Text.Encoding.UTF8);
            }
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
        // 异常记录逻辑...
    }
}
```

## 3. 测试与验证 (TDD)
1. **红灯阶段**：修改 `test_tdd_style.ps1`，使其在反射调用 `UpdateStyleFromJson` 时，直接传递 JSON 文本字符串而不是路径。运行该测试脚本，预期编译成功但在运行时因为无法将 JSON 文本作为文件路径而失败或属性未更新。
2. **绿灯阶段**：实现上述自动识别逻辑。运行测试脚本，预期能够完美解析并使测试通过（绿灯）。
3. **同步验证**：确保修改同步应用到 `ControlDesigner\bin\Release\ExportTemplate` 下的对应文件。
