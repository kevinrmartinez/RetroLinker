using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Projektanker.Icons.Avalonia;

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
        
        private List<string> ListTest = new List<string>()
        {
            "Primero",
            "Segundo",
            "Tercero"
        };
        
        // LOAD
        private void SettingsView3_1_OnLoaded1(object? sender, RoutedEventArgs e)
        {
            lsboxDebug1.IsVisible = false;
        }
        
        // FUNCIONES
        void refresh_comboaddLinkCopy()
        {
            comboaddLinkCopy.ItemsSource = null;
            comboaddLinkCopy.ItemsSource = ListTest;
        }
        
        ListBoxItem AddLinkCopyItem(string Dir)
        {
            var NewItem = new ListBoxItem();
            var gridControl = new Styles.LinkCopyItemGrid();
            Grid _grid = gridControl.GetNewCopyGrid(Dir);
            
            NewItem.Content = _grid;
            return NewItem;
        }
        
#if DEBUG
        private void SettingsView3_1_OnLoaded2(object? sender, RoutedEventArgs e)
        {
            for (int i = 0; i < ListTest.Count; i++)
            {
                int newItemIndex = (lsboxLinkCopies.Items.Count == 1) ? 0 : lsboxLinkCopies.Items.Count - 2;
                lsboxLinkCopies.Items.Insert(newItemIndex, (AddLinkCopyItem(ListTest[i])));
            }
            //refresh_comboaddLinkCopy();
        }
        
        private void BtnaddLinkCopy_OnClick2(object? sender, RoutedEventArgs e)
        {
            int newItemIndex = (lsboxLinkCopies.Items.Count == 1) ? 0 : lsboxLinkCopies.Items.Count - 2;
            lsboxLinkCopies.Items.Insert(newItemIndex, AddLinkCopyItem("Cuarto"));
            // ListTest.Add("Cuarto");
            // refresh_comboaddLinkCopy();
        }
#endif
    }
}
