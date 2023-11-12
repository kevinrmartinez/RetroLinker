using Avalonia.Controls;

namespace RetroarchShortcutterV2.Views;

public partial class MainWindow : Window
{
    private MainView PermaView;
    public MainWindow()
    {
        InitializeComponent();
        PermaView = new MainView(this);
        MainCC1.Content = PermaView;
    }

    public enum Window1Views1
    {
        mainView, SettingsView1, SettingsView2
    }
    
    public enum Window1Views2
    {
        PatchesView
    }
    
    public void ChangeContent(Window1Views1 views)
    {
        
    }

    public void ReturnToMainView(UserControl view)
    {
        MainCC1.Content = PermaView;
        view = null;
    }
}
