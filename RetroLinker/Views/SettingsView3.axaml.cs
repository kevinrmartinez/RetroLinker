/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023  Kevin Rafael Martinez Johnston

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RetroLinker.Models;
using RetroLinker.Models.Avalonia;
using RetroLinker.Translations;

namespace RetroLinker.Views
{
    public partial class SettingsView3 : UserControl
    {
        // Window Obj
        // private MainWindow appMainWindow;
        public SettingsWindow ParentWindow { get; }
        
        // PROPS/STATICS
        public bool DesktopOS { get; }
        
        private bool IsDesigner;
        private bool FirstTimeLoad = true;
        private int candidatesCount;

        private string StrAddCustomCopyPath = resSettingsWindow.strAddCustomCopyPath;
        private List<string> candidateCopiesPath = new();
        private List<string> defIcoSavPathList = new() {
            resSettingsWindow.strDefIcoSavPathItem0,
            resSettingsWindow.strDefIcoSavPathItem1,
            resSettingsWindow.strDefIcoSavPathItem2,
        };
        
        public SettingsView3()
        {
            // Constructor for Designer
            InitializeComponent();
            ParentWindow = new SettingsWindow(true);
            IsDesigner = true;
        }
        
        public SettingsView3(SettingsWindow settingsWindow, bool OS) {
            InitializeComponent();
            ParentWindow = settingsWindow;
            DesktopOS = OS;
        }
        
        // LOAD
        private void View_OnLoaded(object? sender, RoutedEventArgs e)
        {
            // TODO: Move to Binding, requiring to practically rewrite this whole view (>= 0.9)
            if (IsDesigner) lsboxLinkCopies.Items.Insert(NextCopyItemIndex(), AddLinkCopyItem(FileOps.UserDesktop)); // For Designer
            if (FirstTimeLoad)
            {
                if (!DesktopOS) candidateCopiesPath.AddRange(SettingsOps.LinLinkPathCandidates);
                else candidateCopiesPath.AddRange(SettingsOps.WinLinkPathCandidates);
                candidatesCount = candidateCopiesPath.Count;

                foreach (var candidate in candidateCopiesPath)
                    comboDEFLinkOutput.Items.Add(candidate);
                foreach (var path in SettingsOps.LinkCopyPaths)
                    lsboxLinkCopies.Items.Insert(NextCopyItemIndex(), AddLinkCopyItem(path));
                
                candidateCopiesPath.Add(StrAddCustomCopyPath);
                comboaddLinkCopy.ItemsSource = candidateCopiesPath;
                comboaddLinkCopy.SelectedIndex = 0;
                comboUseDefaultIcoSavPath.ItemsSource = defIcoSavPathList;
                
                FirstTimeLoad = false;
            }
            //Settings
            ApplySettingsToControls();
        }
        
        // FUNCTIONS
        void ApplySettingsToControls()
        {
            if (panelDEFLinkOutput.IsEnabled)
            {
                var DefPath = ParentWindow.NewSettings.DEFLinkOutput;
                if (!comboDEFLinkOutput.Items.Contains(DefPath))
                    comboDEFLinkOutput.Items.Add(DefPath);
                comboDEFLinkOutput.SelectedItem = DefPath;
            }
            else comboDEFLinkOutput.SelectedIndex = 0;

            if (!DesktopOS) return;
            ValidateSavIcoPath(ParentWindow.NewSettings.IcoSavPath);
        }

        void ValidateSavIcoPath(string wrkPath)
        {
            var UsrAssets = ParentWindow.NewSettings.UserAssetsPath;
            var AbsoUsrAssets = FileOps.GetAbsolutePath(UsrAssets);
            
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
                0 => ParentWindow.NewSettings.UserAssetsPath,
                1 => resSettingsWindow.txtIcoSavPath1,
                2 => resSettingsWindow.txtIcoSavPath2,
                _ => ParentWindow.NewSettings.UserAssetsPath
            };
            
            ParentWindow.NewSettings.IcoSavPath = index switch
            {
                0 => ParentWindow.NewSettings.UserAssetsPath,
                1 => SettingsOps.IcoSavROM,
                2 => SettingsOps.IcoSavRA,
                _ => ParentWindow.NewSettings.UserAssetsPath
            };
        }
        
        void SetCustomSavIcoPath(string path)
        {
            chkUseDefaultIcoSavPath.IsChecked = false;
            comboUseDefaultIcoSavPath.IsEnabled = false;
            comboUseDefaultIcoSavPath.SelectedIndex = 0;
            panelWindowsOnlyControls2.IsEnabled = true;
            ParentWindow.NewSettings.IcoSavPath = path;
            txtIcoSavPath.Text = path;
        }

        int NextCopyItemIndex() => (lsboxLinkCopies.Items.Count < 2) ? 0 : lsboxLinkCopies.Items.Count - 2;
        
        ListBoxItem AddLinkCopyItem(string dir)
        {
            var newItem = new ListBoxItem();
            var gridControl = new Styles.LinkCopyItemGrid(dir);

            var trashButton = gridControl.NewItemTrash;
            trashButton.Click += btnTrashCopyItem;
            
            newItem.Content = gridControl.NewItemGrid;
            return newItem;
        }

        private void LockControls(bool locked) {
            panelContent.IsEnabled = !locked;
            DataContext = null;
            DataContext = this;
        }
        
        
        // DEFAULT OUTPUT
        private void ComboDEFLinkOutpu_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) => 
            ParentWindow.NewSettings.DEFLinkOutput = (string)comboDEFLinkOutput.SelectedItem!;

        private async void BtnDefLinkOutput_ClickAsync()
        {
            try {
                LockControls(true);
                string currentFolder = (string)comboDEFLinkOutput.SelectedItem!;
                string folder = await FileDialogOps.OpenFolderAsync(OpenFolderOpts.UserAssets, ParentWindow, currentFolder);
                if (string.IsNullOrWhiteSpace(folder)) return;
                // int customDirIndex = candidatesCount;
                if (comboDEFLinkOutput.Items.Count == candidatesCount) comboDEFLinkOutput.Items.Add(folder);
                else comboDEFLinkOutput.Items[candidatesCount] = folder;
                comboDEFLinkOutput.SelectedIndex = candidatesCount;
            }
            catch (System.Exception e) { _ = this.PopUpGenericError(e); }
            finally { LockControls(false); }
        }

        private void BtnDefLinkOutput_OnClick(object? sender, RoutedEventArgs e) => BtnDefLinkOutput_ClickAsync();
        
        private void BtnclrDefLinkOutput_OnClick(object? sender, RoutedEventArgs e) {
            ParentWindow.NewSettings.DEFLinkOutput = ParentWindow.GetDefSettings().DEFLinkOutput;
            comboDEFLinkOutput.SelectedIndex = 0;
        }
        
        // LINK COPY
        private async void BtnAddLinkCopy_ClickAsync()
        {
            try {
                LockControls(true);
                string currentItem = (string)comboaddLinkCopy.SelectedItem!;
                if (string.IsNullOrWhiteSpace(currentItem)) return;

                if (currentItem == StrAddCustomCopyPath)
                {
                    string folder = await FileDialogOps.OpenFolderAsync(OpenFolderOpts.LinkCopy, ParentWindow);
                    if (!string.IsNullOrWhiteSpace(folder)) {
                        lsboxLinkCopies.Items.Insert(NextCopyItemIndex(), AddLinkCopyItem(folder));
                        ParentWindow.SetLinkCopyPaths.Add(folder);
                    }
                }
                else if (!ParentWindow.SetLinkCopyPaths.Contains(currentItem)) {
                    lsboxLinkCopies.Items.Insert(NextCopyItemIndex(), AddLinkCopyItem(currentItem));
                    ParentWindow.SetLinkCopyPaths.Add(currentItem);
                }
                // If the element already exist in the list, make the existing element light up, or shake or something 
                comboaddLinkCopy.SelectedIndex = 0;
            }
            catch (System.Exception e) { _ = this.PopUpGenericError(e); }
            finally  { LockControls(false); }
        }

        private void BtnAddLinkCopy_OnClick(object? sender, RoutedEventArgs e) => BtnAddLinkCopy_ClickAsync();
        
        private void btnTrashCopyItem(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            var parentGrid = button.Parent as Grid;
            
            if (parentGrid!.Children[0] is not Label label) return;
            var path = (string)label.Content!;
            ParentWindow.SetLinkCopyPaths.Remove(path);
            
            var parentListBoxItem = parentGrid.Parent as ListBoxItem;
            lsboxLinkCopies.Items.Remove(parentListBoxItem);
        }

        
        #region WINDOWS OS ONLY

        // ICONS
        private async void btnIcoSavPath_ClickAsync()
        {
            try {
                LockControls(true);
                string currentFolder = (string.IsNullOrEmpty(txtIcoSavPath.Text)) ? string.Empty : txtIcoSavPath.Text;
                string folder = await FileDialogOps.OpenFolderAsync(OpenFolderOpts.IcoOutput, ParentWindow, currentFolder);
                if (string.IsNullOrWhiteSpace(folder)) return;
                txtIcoSavPath.Text = folder; 
                ParentWindow.NewSettings.IcoSavPath = folder;
            }
            catch (System.Exception e) { _ = this.PopUpGenericError(e); }
            finally { LockControls(false); }
        }

        private void btnIcoSavPath_OnClick(object sender, RoutedEventArgs e) => btnIcoSavPath_ClickAsync();

        private void btnclrIcoSavPath_Click(object sender, RoutedEventArgs e) {
            ParentWindow.NewSettings.IcoSavPath = ParentWindow.GetDefSettings().IcoSavPath;
            txtIcoSavPath.Text = ParentWindow.NewSettings.IcoSavPath;
        }
        
        private void ChkUseDefaultIcoSavPath_IsCheckedChanged(object? sender, RoutedEventArgs e) {
            if (!chkUseDefaultIcoSavPath.IsChecked.GetValueOrDefault()) SetCustomSavIcoPath(string.Empty);
            else SetDefaultSavIcoPath(0);
        }
        
        private void ComboUseDefaultIcoSavPath_DropDownClosed(object? sender, System.EventArgs e) {
            
            if (sender is not ComboBox combo) return;
            SetDefaultSavIcoPath((byte)combo.SelectedIndex);
        }
        
        // TILE ICON
        private async void btnTileIcoBrowse_OnClickAsync(TextBox textBox)
        {
            try
            {
                LockControls(true);
                var dialogTitle = resAvaloniaOps.dlgFileTileCliexe;
                var currentFile = (string.IsNullOrWhiteSpace(textBox.Text)) ? FileOps.BaseDir : textBox.Text;
                var file = await FileDialogOps.OpenFileAsync(OpenOpts.RAexe, ParentWindow, currentFile, dialogTitle);
                if (string.IsNullOrWhiteSpace(file)) return;
                // txtUserAssets.Text = folder;
                ParentWindow.NewSettings.TileIcoPath = file;
            }
            catch (System.Exception e) { _ = this.PopUpGenericError(e); }
            finally { LockControls(false); }
        }
        
        // TODO: This is brilliant, now reimplement it across the settings :)
        private void btnTileIco_OnClick(object? sender, RoutedEventArgs e) {
            if (sender is not Button button) return;
            switch (button.CommandParameter)
            {
                case TextBox textBox:
                    btnTileIcoBrowse_OnClickAsync(textBox);
                    return;
                case TextBoxActions action:
                    ParentWindow.NewSettings.TileIcoPath = action switch {
                        TextBoxActions.Restore => ParentWindow.GetOldSettings().TileIcoPath,
                        _ => null
                    };
                    break;
            }

            DataContext = null;
            DataContext = this;
        }
        #endregion
    }
}
