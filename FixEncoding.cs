using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string dir = @"d:\Tyler\公众号\LabVIEW-WPF-UI-Generator\ExportTemplate";
        var files = Directory.GetFiles(dir, "*Panel.cs");
        foreach(var f in files)
        {
            string content = File.ReadAllText(f, System.Text.Encoding.Default);
            // Fix corrupted characters if any
            content = content.Replace("带有新拟态样式的下拉框控?", "带有新拟态样式的下拉框控件\"");
            content = content.Replace("用于?LabVIEW", "用于在 LabVIEW");
            content = content.Replace("容器面?", "容器面板");
            content = content.Replace("当用户选择更改时触?", "当用户选择更改时触发");
            
            // Reapply BackColor fix
            content = Regex.Replace(content, @"try\s*\{\s*this\.BackColor\s*=\s*.*?catch\s*\{\s*this\.BackColor\s*=\s*(?:System\.Drawing\.)?Color\.White;\s*\}", "this.BackColor = System.Drawing.Color.Transparent;", RegexOptions.Singleline);
            content = Regex.Replace(content, @"this\.BackColor\s*=\s*(?:System\.Drawing\.)?ColorTranslator\.FromHtml\(""\{\{ControlBackground\}\}""\);", "this.BackColor = System.Drawing.Color.Transparent;");
            content = Regex.Replace(content, @"this\.BackColor\s*=\s*(?:System\.Drawing\.)?Color\.White;", "");
            
            File.WriteAllText(f, content, System.Text.Encoding.UTF8);
        }
    }
}
