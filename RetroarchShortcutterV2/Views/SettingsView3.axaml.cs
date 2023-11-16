using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Projektanker.Icons.Avalonia;
using RetroarchShortcutterV2.Models;

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
        
        // LOAD
        private void SettingsView3_1_OnLoaded(object? sender, RoutedEventArgs e)
        {
            lsboxDebug1.IsVisible = false;
            if (FirstTimeLoad)
            {
                if (!DesktopOS)
                { panelWindowsOnlyControls.IsEnabled = false; }

                FirstTimeLoad = false;
            }
            //Settings
            ApplySettingsToControls();
            if (ParentWindow.settings.UserAssetsPath == ParentWindow.settings.ConvICONPath) 
            { chkUseUserAssets.IsChecked = true; }
        }
        
        // FUNCIONES
        void ApplySettingsToControls()
        {
            
            txtIcoSavPath.Text = System.IO.Path.GetFullPath(ParentWindow.settings.ConvICONPath);
            chkExtractIco.IsChecked = ParentWindow.settings.ExtractIco;
        }
        
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
            _ = _grid.Children;

            var trashButtom = _grid.Children[1] as Button;
            trashButtom.AddHandler(Button.ClickEvent, btnTrashCopyItem);
            
            NewItem.Content = _grid;
            return NewItem;
        }

        void btnTrashCopyItem(object sender, RoutedEventArgs e)
        {
            var parentGrid = (sender as Control).Parent as Grid;
            
            var path = (parentGrid.Children[0] as Label).Content.ToString();
            
            var parentListBoxItem = parentGrid.Parent as ListBoxItem;
            lsboxLinkCopies.Items.Remove(parentListBoxItem);
        }
        
        private void BtnaddLinkCopy_OnClick(object? sender, RoutedEventArgs e)
        {
            string currentItem = comboaddLinkCopy.SelectedItem.ToString();
            if (string.IsNullOrEmpty(currentItem)) { }
            else
            {
                int newItemIndex = (lsboxLinkCopies.Items.Count == 1) ? 0 : lsboxLinkCopies.Items.Count - 2;
                lsboxLinkCopies.Items.Insert(newItemIndex, AddLinkCopyItem(currentItem));
            }
        }
        
        // ICONS

        #region WINDOWS OS ONLY
        async void btnIcoSavPath_Click(object sender, RoutedEventArgs e)
        {
            string folder = await FileOps.OpenFolderAsync(template:2, ParentWindow);
            // TODO: Resolver sin el uso de null
            if (folder != null)
            {
                txtIcoSavPath.Text = folder; 
                ParentWindow.settings.ConvICONPath = folder;
            }
        }

        void btnclrIcoSavPath_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.settings.ConvICONPath = FileOps.DefUserAssetsDir; 
            txtIcoSavPath.Text = System.IO.Path.GetFullPath(ParentWindow.settings.ConvICONPath);
        }

        void chkUseUserAssets_Checked(object sender, RoutedEventArgs e)
        {
            panelWindowsOnlyControls2.IsEnabled = !(bool)chkUseUserAssets.IsChecked;
            ParentWindow.settings.ConvICONPath = ParentWindow.settings.UserAssetsPath;
            txtIcoSavPath.Text = System.IO.Path.GetFullPath(ParentWindow.settings.ConvICONPath);
        }
        #endregion
        
        //UNLOAD
        private void SettingsView3_1_OnUnloaded(object? sender, RoutedEventArgs e)
        {
            _ = e.Source;
        }
        
#if DEBUG
        private List<string> ListTest = new()
        {
            "Primero",
            "Segundo",
            "Tercero"
        };
        
        private void SettingsView3_1_OnLoaded2(object? sender, RoutedEventArgs e)
        {
            for (int i = 0; i < ListTest.Count; i++)
            {
                int newItemIndex = (lsboxLinkCopies.Items.Count == 1) ? 0 : lsboxLinkCopies.Items.Count - 2;
                lsboxLinkCopies.Items.Insert(newItemIndex, (AddLinkCopyItem(ListTest[i])));
            }
            refresh_comboaddLinkCopy();
            comboaddLinkCopy.SelectedIndex++;
        }
        
        private void BtnaddLinkCopy_OnClick2(object? sender, RoutedEventArgs e)
        {
            int newItemIndex = (lsboxLinkCopies.Items.Count == 1) ? 0 : lsboxLinkCopies.Items.Count - 2;
            lsboxLinkCopies.Items.Insert(newItemIndex, AddLinkCopyItem("Cuarto"));
            // ListTest.Add("Cuarto");
            refresh_comboaddLinkCopy();
        }
#endif
    }
}
