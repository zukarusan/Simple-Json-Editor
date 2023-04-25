using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ZookJson.ViewModels;
using ZookJson.Views;

namespace ZookJson
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWin = new MainWindow();
                mainWin.DataContext = new MainWindowViewModel(mainWin);
                desktop.MainWindow = mainWin;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
