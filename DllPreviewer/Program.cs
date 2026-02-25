using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace DllPreviewer
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 从命令行或文件对话框获取 DLL 路径
            string dllPath = null;
            if (args.Length > 0 && File.Exists(args[0]))
            {
                dllPath = args[0];
            }
            else
            {
                var dlg = new OpenFileDialog
                {
                    Title = "选择导出的 DLL 文件",
                    Filter = "DLL 文件|*.dll",
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                };
                if (dlg.ShowDialog() == DialogResult.OK)
                    dllPath = dlg.FileName;
            }

            if (string.IsNullOrEmpty(dllPath) || !File.Exists(dllPath))
            {
                MessageBox.Show("未选择有效的 DLL 文件", "提示");
                return;
            }

            try
            {
                // 加载导出的 DLL
                var asm = Assembly.LoadFrom(dllPath);
                var panelType = asm.GetType("WpfTextInput.TextInputPanel");
                if (panelType == null)
                {
                    MessageBox.Show("在 DLL 中未找到 WpfTextInput.TextInputPanel 类型", "错误");
                    return;
                }

                var panel = (Control)Activator.CreateInstance(panelType);

                // 创建预览窗口
                var form = new Form
                {
                    Text = "DLL 样式预览 - " + Path.GetFileName(dllPath),
                    Width = 600,
                    Height = 300,
                    StartPosition = FormStartPosition.CenterScreen,
                    BackColor = System.Drawing.Color.FromArgb(230, 230, 230),
                };

                panel.Dock = DockStyle.Fill;
                panel.Margin = new Padding(20);
                form.Controls.Add(panel);

                Application.Run(form);
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载 DLL 失败:\n\n" + ex.ToString(), "错误");
            }
        }
    }
}
