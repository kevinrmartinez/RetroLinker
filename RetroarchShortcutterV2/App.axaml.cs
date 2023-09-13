using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using RetroarchShortcutterV2.ViewModels;
using RetroarchShortcutterV2.Views;

namespace RetroarchShortcutterV2;

public partial class App : Application
{
    public static System.DateTime LaunchTime = System.DateTime.Now;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        System.TimeSpan launchTime = System.TimeSpan.FromTicks(LaunchTime.Ticks);
        System.Diagnostics.Trace.WriteLine($"AppLoadTime: {launchTime}", "[Time]");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            System.Diagnostics.Trace.WriteLine("Iniciando MainWindow", "[Info]");
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
