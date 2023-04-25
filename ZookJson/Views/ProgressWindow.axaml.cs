using Avalonia.Controls;
using Avalonia.Threading;
using System.Security.Cryptography;

namespace ZookJson.Views
{
    public partial class ProgressWindow : Window
    {
        private bool _active = false;
        private readonly Window parent;
        public bool Active { 
            set
            {
                _active = value;
                Dispatcher.UIThread.InvokeAsync(()=> ProgRing.IsActive = value, DispatcherPriority.DataBind);
                if (value == true)
                    Dispatcher.UIThread.InvokeAsync(() => ShowDialog(parent), DispatcherPriority.DataBind);
                else
                    Dispatcher.UIThread.InvokeAsync(() => Hide(), DispatcherPriority.DataBind);
            } get { return _active; } }
        public ProgressWindow() : this(null) { }
        public ProgressWindow(Window? parent)
        {
            this.parent = parent;
            InitializeComponent();
        }
    }
}
