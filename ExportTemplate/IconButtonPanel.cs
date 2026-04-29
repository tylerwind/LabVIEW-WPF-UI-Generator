using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace {{Namespace}}
{
    public class IconButtonPanel : Panel
    {
        private ElementHost _host;
        private IconButtonControl _control;

        public event EventHandler Click;

        public IconButtonPanel()
        {
            _host = new ElementHost();
            _host.BackColorTransparent = true;
            _control = new IconButtonControl();
            _host.Child = _control;
            _host.Dock = DockStyle.Fill;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(_host);

            _control.Click += (s, e) => {
                if (Click != null) Click(this, EventArgs.Empty);
            };
        }

        public string LabelText
        {
            get { return (string)InvokeOnUI(() => _control.LabelText); }
            set { InvokeOnUI(() => _control.LabelText = value); }
        }

        public string IconText
        {
            get { return (string)InvokeOnUI(() => _control.IconText); }
            set { InvokeOnUI(() => _control.IconText = value); }
        }

        public string IconPath
        {
            get { return (string)InvokeOnUI(() => _control.IconPath); }
            set { InvokeOnUI(() => _control.IconPath = value); }
        }

        public bool UseImage
        {
            get { return (bool)InvokeOnUI(() => _control.UseImage); }
            set { InvokeOnUI(() => _control.UseImage = value); }
        }

        private object InvokeOnUI(Func<object> func)
        {
            if (!_control.Dispatcher.CheckAccess())
                return _control.Dispatcher.Invoke(func);
            return func();
        }

        private void InvokeOnUI(Action action)
        {
            if (!_control.Dispatcher.CheckAccess())
                _control.Dispatcher.Invoke(action);
            else
                action();
        }
    }
}
