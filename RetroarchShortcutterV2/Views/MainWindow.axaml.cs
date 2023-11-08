using Avalonia.Animation;
using Avalonia.Controls;

namespace RetroarchShortcutterV2.Views;

public partial class MainWindow : Window
{
    private MainView PermaView;
    private SettingsView Settings1;
    private SettingsView Settings2;
    public MainWindow()
    {
        InitializeComponent();
        PermaView = new MainView(this);
        CC1.Content = PermaView;
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
        
        Settings1 = new SettingsView(this);
        //Settings2 = new SettingsView();
        CC1.Content = views switch
        {
            Window1Views1.mainView => new MainView(),
            Window1Views1.SettingsView1 => Settings1,
            //Window1Views1.SettingsView2 => Settings2
        };
    }

    public void ReturnToMainView(UserControl view)
    {
        CC1.Content = PermaView;
        view = null;
    }
}
