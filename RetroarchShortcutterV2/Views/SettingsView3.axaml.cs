using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
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

        private List<string> candidateCopiesPath = new();
        private List<string> defIcoSavPathList = new()
        {
            "Usar la carpeta UserAssets",
            "Usar la carpeta de la ROM",
            "Usar la carpeta de Retroarch",
        };
        
        // LOAD
        private void SettingsView3_1_OnLoaded(object? sender, RoutedEventArgs e)
        {
            lsboxDebug1.IsVisible = false;
            if (FirstTimeLoad)
            {
                if (!DesktopOS)
                {
                    candidateCopiesPath.AddRange(SettingsOps.LINLinkPathCandidates);
                    panelWindowsOnlyControls.IsEnabled = false;
                }
                else
                { candidateCopiesPath.AddRange(SettingsOps.WINLinkPathCandidates); }

                //candidateCopiesPath.Add(list);
                candidateCopiesPath.Add("Otro...");
                comboaddLinkCopy.ItemsSource = candidateCopiesPath;
                comboaddLinkCopy.SelectedIndex = 0;
                txtDEFLinkOutput.Watermark = FileOps.UserDesktop;
                comboUseDefaultIcoSavPath.ItemsSource = defIcoSavPathList;
                
                FirstTimeLoad = false;
            }
            //Settings
            ApplySettingsToControls();
            // if (ParentWindow.settings.UserAssetsPath == ParentWindow.settings.ConvICONPath) 
            // { chkUseUserAssets.IsChecked = true; }
        }
        
        // FUNCIONES
        void ApplySettingsToControls()
        {
            chkAlwaysAskOutput.IsChecked = ParentWindow.settings.AllwaysAskOutput;
            txtDEFLinkOutput.Text = ParentWindow.settings.DEFLinkOutput;
            chkMakeLinkCopy.IsChecked = ParentWindow.settings.MakeLinkCopy;
            lsboxLinkCopies.IsEnabled = ParentWindow.settings.MakeLinkCopy;
            if (DesktopOS) ValidateSavIcoPath(ParentWindow.settings.IcoSavPath);
            chkExtractIco.IsChecked = ParentWindow.settings.ExtractIco;
            chkIcoLinkName.IsChecked = ParentWindow.settings.IcoLinkName;
        }

        void ValidateSavIcoPath(string wrkPath)
        {
            var UsrAssets = ParentWindow.settings.UserAssetsPath;
            var AbsoUsrAssets = System.IO.Path.GetFullPath(ParentWindow.settings.UserAssetsPath);
            
            switch (wrkPath)
            {
                case SettingsOps.IcoSavROM:
                    SetDefaultSavIcoPath(1);
                    break;
                case SettingsOps.IcoSavRA:
                    SetDefaultSavIcoPath(2);
                    break;
                default:
                    if ((wrkPath == UsrAssets) || (wrkPath == AbsoUsrAssets))
                    { SetDefaultSavIcoPath(0); }
                    else
                    { SetCustomSavIcoPath(wrkPath);}
                    break;
            }
        }

        void SetDefaultSavIcoPath(byte index)
        {
            chkUseDefaultIcoSavPath.IsChecked = true;
            comboUseDefaultIcoSavPath.IsEnabled = true;
            comboUseDefaultIcoSavPath.SelectedIndex = index;
            panelWindowsOnlyControls2.IsEnabled = false;
            txtIcoSavPath.Text = index switch
            {
                0 => ParentWindow.settings.UserAssetsPath,
                1 => "Segun el ROM",
                2 => "Segun el ejecutable de Retroarch"
            };
            ParentWindow.settings.IcoSavPath = index switch
            {
                0 => ParentWindow.settings.UserAssetsPath,
                1 => SettingsOps.IcoSavROM,
                2 => SettingsOps.IcoSavRA
            };
        }
        
        void SetCustomSavIcoPath(string path)
        {
            chkUseDefaultIcoSavPath.IsChecked = false;
            comboUseDefaultIcoSavPath.IsEnabled = false;
            comboUseDefaultIcoSavPath.SelectedIndex = 0;
            panelWindowsOnlyControls2.IsEnabled = true;
            // TODO: Decidir si usar ruta absoluta o relativa
            ParentWindow.settings.IcoSavPath = path;
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
        
        
        // DEFAULT OUTPUT
        void ChkAlwaysAskOutput_IsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            var chk = sender as CheckBox;
            ParentWindow.settings.AllwaysAskOutput = (bool)chk.IsChecked;
            panelDEFLinkOutput.IsEnabled = !(bool)chk.IsChecked;
            // if ((bool)chk.IsChecked)
            // {
            //     panelDEFLinkOutput.IsEnabled = false;
            // }
            // else
            // {
            //     panelDEFLinkOutput.IsEnabled = true;
            // }
        }
        
        private async void BtnDefLinkOutput_OnClick(object? sender, RoutedEventArgs e)
        {
            string folder = await FileOps.OpenFolderAsync(template:0, ParentWindow);
            if (!string.IsNullOrWhiteSpace(folder))
            {
                txtDEFLinkOutput.Text = folder;
                ParentWindow.settings.DEFLinkOutput = folder;
            }
        }
        
        private void BtnclrDefLinkOutput_OnClick(object? sender, RoutedEventArgs e)
        {
            ParentWindow.settings.DEFLinkOutput = ParentWindow.DEFsettings.DEFLinkOutput;
            txtDEFLinkOutput.Text = ParentWindow.settings.DEFLinkOutput;
        }
        
        
        // LINK COPY
        void ChkMakeLinkCopy_IsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            var chk = sender as CheckBox;
            ParentWindow.settings.MakeLinkCopy = (bool)chk.IsChecked;
            lsboxLinkCopies.IsEnabled = (bool)chk.IsChecked;
            //lsboxLinkCopies.IsVisible = (bool)chk.IsChecked;
        }

        void BtnaddLinkCopy_OnClick(object? sender, RoutedEventArgs e)
        {
            string currentItem = comboaddLinkCopy.SelectedItem.ToString();
            if (string.IsNullOrEmpty(currentItem)) { }
            else
            {
                int newItemIndex = (lsboxLinkCopies.Items.Count == 1) ? 0 : lsboxLinkCopies.Items.Count - 2;
                lsboxLinkCopies.Items.Insert(newItemIndex, AddLinkCopyItem(currentItem));
                comboaddLinkCopy.SelectedIndex = 0;
            }
        }
        
        void btnTrashCopyItem(object sender, RoutedEventArgs e)
        {
            var parentGrid = (sender as Control).Parent as Grid;
            
            var path = (parentGrid.Children[0] as Label).Content.ToString();
            
            var parentListBoxItem = parentGrid.Parent as ListBoxItem;
            lsboxLinkCopies.Items.Remove(parentListBoxItem);
        }

        
        // ICONS
        
        #region WINDOWS OS ONLY
        async void btnIcoSavPath_Click(object sender, RoutedEventArgs e)
        {
            string folder = await FileOps.OpenFolderAsync(template:2, ParentWindow);
            if (!string.IsNullOrWhiteSpace(folder))
            {
                txtIcoSavPath.Text = folder; 
                ParentWindow.settings.IcoSavPath = folder;
            }
        }

        void btnclrIcoSavPath_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.settings.IcoSavPath = ParentWindow.DEFsettings.IcoSavPath;
            // TODO: Decidir si usar ruta relativa o absoluta
            txtIcoSavPath.Text = System.IO.Path.GetFullPath(ParentWindow.settings.IcoSavPath);
        }

        void chkUseUserAssets_Checked(object sender, RoutedEventArgs e)
        {
            panelWindowsOnlyControls2.IsEnabled = !(bool)chkMakeLinkCopy.IsChecked;
            ParentWindow.settings.IcoSavPath = ParentWindow.settings.UserAssetsPath;
            txtIcoSavPath.Text = System.IO.Path.GetFullPath(ParentWindow.settings.IcoSavPath);
        }
        
        private void IcoSavChecks_IsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            ParentWindow.settings.ExtractIco = (bool)chkExtractIco.IsChecked;
            ParentWindow.settings.IcoLinkName = (bool)chkIcoLinkName.IsChecked;
        }
        
        private void ChkUseDefaultIcoSavPath_IsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            var chk = sender as CheckBox;
            if (!(bool)chk.IsChecked)
            { SetCustomSavIcoPath(string.Empty); }
            else
            { SetDefaultSavIcoPath(0); }
        }
        
        private void ComboUseDefaultIcoSavPath_DropDownClosed(object? sender, System.EventArgs e)
        {
            var combo = sender as ComboBox;
            SetDefaultSavIcoPath((byte)combo.SelectedIndex);
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
