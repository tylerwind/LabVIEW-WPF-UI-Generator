using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace WpfTextInput
{
    /// <summary>
    /// LabVIEW 入口类 — 管理 WPF 控件的生命周期和线程
    /// 在 LabVIEW 中通过 .NET Constructor Node 创建此类的实例
    /// </summary>
    public class TextInputHost : IDisposable
    {
        private Thread _uiThread;
        private Dispatcher _dispatcher;
        private Window _hostWindow;
        private TextInputControl _control;
        private readonly ManualResetEventSlim _ready = new ManualResetEventSlim(false);
        private bool _disposed;

        #region 事件

        /// <summary>
        /// 文本值变更事件 — LabVIEW 可通过 Register Event Callback 注册
        /// </summary>
        public event ValueChangedHandler ValueChanged;

        #endregion

        #region 属性

        /// <summary>
        /// 获取或设置标签文字
        /// </summary>
        public string LabelText
        {
            get
            {
                if (_dispatcher == null) return string.Empty;
                return (string)_dispatcher.Invoke(new Func<string>(() => _control.LabelText));
            }
            set
            {
                _dispatcher?.Invoke(new Action(() => { _control.LabelText = value; }));
            }
        }

        /// <summary>
        /// 获取或设置窗口标题
        /// </summary>
        public string Title
        {
            get
            {
                if (_dispatcher == null) return string.Empty;
                return (string)_dispatcher.Invoke(new Func<string>(() => _hostWindow.Title));
            }
            set
            {
                _dispatcher?.Invoke(new Action(() => { _hostWindow.Title = value; }));
            }
        }

        /// <summary>
        /// 获取或设置窗口宽度
        /// </summary>
        public double WindowWidth
        {
            get
            {
                if (_dispatcher == null) return 0;
                return (double)_dispatcher.Invoke(new Func<double>(() => _hostWindow.Width));
            }
            set
            {
                _dispatcher?.Invoke(new Action(() => { _hostWindow.Width = value; }));
            }
        }

        /// <summary>
        /// 获取或设置窗口高度
        /// </summary>
        public double WindowHeight
        {
            get
            {
                if (_dispatcher == null) return 0;
                return (double)_dispatcher.Invoke(new Func<double>(() => _hostWindow.Height));
            }
            set
            {
                _dispatcher?.Invoke(new Action(() => { _hostWindow.Height = value; }));
            }
        }

        #endregion

        /// <summary>
        /// 构造函数 — 启动 WPF STA 线程并创建控件
        /// </summary>
        public TextInputHost()
        {
            _uiThread = new Thread(UIThreadEntry)
            {
                IsBackground = true,
                Name = "WpfTextInput_STA"
            };
            _uiThread.SetApartmentState(ApartmentState.STA);
            _uiThread.Start();

            // 等待 UI 线程准备就绪（最多 10 秒）
            _ready.Wait(TimeSpan.FromSeconds(10));
        }

        /// <summary>
        /// 带标签参数的构造函数
        /// </summary>
        public TextInputHost(string labelText) : this()
        {
            LabelText = labelText;
        }

        #region 公共方法

        /// <summary>
        /// 写入文本到输入框
        /// </summary>
        public void Write(string text)
        {
            _dispatcher?.Invoke(new Action(() =>
            {
                _control.Text = text ?? string.Empty;
            }));
        }

        /// <summary>
        /// 读取输入框当前文本
        /// </summary>
        public string Read()
        {
            if (_dispatcher == null) return string.Empty;
            return (string)_dispatcher.Invoke(new Func<string>(() => _control.Text));
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        public void Show()
        {
            _dispatcher?.Invoke(new Action(() =>
            {
                _hostWindow.Show();
                _hostWindow.Activate();
            }));
        }

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        public void Hide()
        {
            _dispatcher?.Invoke(new Action(() =>
            {
                _hostWindow.Hide();
            }));
        }

        /// <summary>
        /// 设置窗口位置
        /// </summary>
        public void SetPosition(double left, double top)
        {
            _dispatcher?.Invoke(new Action(() =>
            {
                _hostWindow.Left = left;
                _hostWindow.Top = top;
            }));
        }

        /// <summary>
        /// 清空输入框文本
        /// </summary>
        public void Clear()
        {
            Write(string.Empty);
        }

        /// <summary>
        /// 设置输入框是否只读
        /// </summary>
        public void SetReadOnly(bool isReadOnly)
        {
            _dispatcher?.Invoke(new Action(() =>
            {
                _control.InputBox.IsReadOnly = isReadOnly;
            }));
        }

        #endregion

        #region 内部方法

        private void UIThreadEntry()
        {
            // 创建宿主窗口
            _hostWindow = new Window
            {
                Title = "文本输入",
                Width = 320,
                Height = 100,
                WindowStyle = WindowStyle.ToolWindow,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                Topmost = true,
                Background = System.Windows.Media.Brushes.WhiteSmoke,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            // 创建控件
            _control = new TextInputControl
            {
                Margin = new Thickness(8)
            };

            // 注册值变更事件转发
            _control.ValueChanged += OnControlValueChanged;

            _hostWindow.Content = _control;

            // 保存 Dispatcher
            _dispatcher = Dispatcher.CurrentDispatcher;

            // 窗口关闭时仅隐藏，不销毁
            _hostWindow.Closing += (s, e) =>
            {
                if (!_disposed)
                {
                    e.Cancel = true;
                    _hostWindow.Hide();
                }
            };

            // 标记就绪
            _ready.Set();

            // 运行消息循环
            Dispatcher.Run();
        }

        private void OnControlValueChanged(string oldValue, string newValue)
        {
            // 将事件从 UI 线程转发到调用者
            ValueChanged?.Invoke(oldValue, newValue);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_dispatcher != null)
            {
                _dispatcher.InvokeShutdown();
            }

            _ready.Dispose();
        }

        #endregion
    }
}
