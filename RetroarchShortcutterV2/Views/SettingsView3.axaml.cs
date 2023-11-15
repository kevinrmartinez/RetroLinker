using Avalonia.Controls;

namespace RetroarchShortcutterV2.Views
{
    public partial class SettingsView3 : UserControl
    {
        public SettingsView3()
        { InitializeComponent(); }
        
        public SettingsView3(MainWindow mainWindow, SettingsWindow settingsWindow, bool OS)
        {
            InitializeComponent();
            AppMainWindow = mainWindow;
            ParentWindow = settingsWindow;
            DesktopOS = OS;
        }
        
        // Window Obj
        private MainWindow AppMainWindow;
        private SettingsWindow ParentWindow;

        // PROPS/STATICS
        private bool FirstTimeLoad = true;
        private bool DesktopOS;
    }
}
