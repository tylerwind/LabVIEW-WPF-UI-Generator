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
                string msg = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}\n{2}\n\n", DateTime.Now, source, ex);
                File.AppendAllText(logPath, msg);
            }
            catch { }
        }
    }
}
