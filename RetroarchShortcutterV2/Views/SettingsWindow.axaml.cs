using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using RetroarchShortcutterV2.Models;

namespace RetroarchShortcutterV2.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }
        
        public SettingsWindow(MainWindow parentWindow)
        {
            // Este constructor existe para facilitar la creacion de esta ventana como un PopUp
            InitializeComponent();
            ParentWindow = parentWindow;
        }
        
        // Window Obj
        private MainWindow ParentWindow;

        // PROPS/STATICS
        private bool DesktopOS = System.OperatingSystem.IsWindows();
        private SettingsView SettingsView1;
        private SettingsView2 SettingsView21;
        private SettingsView3 SettingsView31;
        public Settings settings { get; set; }
        public Settings DEFsettings { get; } = new();

        #region LoadContent
        private void SettingsWindow1_OnLoaded(object? sender, RoutedEventArgs e)
        {
            LoadFromSettings();
            SettingsView1 = new SettingsView(ParentWindow, this, DesktopOS);
            SettingsView21 = new SettingsView2(ParentWindow, this, DesktopOS);
            SettingsView31 = new SettingsView3(ParentWindow, this, DesktopOS);
            CCTab1.Content = SettingsView1;
            CCTab2.Content = SettingsView21;
            CCTab3.Content = SettingsView31;
        }
        
        void LoadFromSettings()
        {
            settings = SettingsOps.GetCachedSettings();
        }
        #endregion
        
        
        #region Window/Dialog Controls
        // Window/Dialog Controls
        void btnDISSettings_Click(object sender, RoutedEventArgs e) => CloseView();

        async void btnDEFSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxStandardParams msparams = new()
            {
                ContentTitle = "Restaurar Defaults",
                ContentMessage = "�Est� seguro de restaurar la configuracion default?",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ButtonDefinitions = MsBox.Avalonia.Enums.ButtonEnum.OkCancel,
                EnterDefaultButton = MsBox.Avalonia.Enums.ClickEnum.Ok,
                EscDefaultButton = MsBox.Avalonia.Enums.ClickEnum.Cancel,

            };
            var msbox = MessageBoxManager.GetMessageBoxStandard(msparams);
            var result = await msbox.ShowWindowDialogAsync(ParentWindow);
            if (result == MsBox.Avalonia.Enums.ButtonResult.Ok)
            {
                //SettingsOps.PrevConfigs = new();
                SettingsOps.WriteSettingsFile(DEFsettings);
                CloseView();
            }
                
        }

        void btnCONSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!DesktopOS) { settings.DEFRADir = SettingsView21.txtDefRADir.Text; }
            // Set bools
            settings.PrevConfig = (bool)SettingsView1.chkPrevCONFIG.IsChecked;
            settings.AllwaysAskOutput = (bool)SettingsView31.chkAlwaysAskOutput.IsChecked;
            settings.CpyUserIcon = (bool)SettingsView1.chkCpyUserIcon.IsChecked;
            settings.ExtractIco = (bool)SettingsView31.chkExtractIco.IsChecked;
            
            SettingsOps.WriteSettingsFile(settings);
            CloseView();
        }

        void CloseView()
        {
            Close();
            //ParentWindow.ReturnToMainView(this);
        }
        #endregion

        
    }
}
