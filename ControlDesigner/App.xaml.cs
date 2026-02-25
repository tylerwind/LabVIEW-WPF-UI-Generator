using System;
using System.IO;
using System.Windows;

namespace ControlDesigner
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // 全局异常捕获
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                LogError("UnhandledException", args.ExceptionObject as Exception);
            };
            DispatcherUnhandledException += (s, args) =>
            {
                LogError("DispatcherUnhandledException", args.Exception);
                MessageBox.Show(args.Exception.ToString(), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            base.OnStartup(e);
        }

        private void LogError(string source, Exception ex)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                string msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}\n{ex}\n\n";
                File.AppendAllText(logPath, msg);
            }
            catch { }
        }
    }
}
