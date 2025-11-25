/*
    A .NET GUI application to help create desktop links of games running on RetroArch.
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
        public SettingsView3()
        {
            // Constructor for Designer
            InitializeComponent();
            ParentWindow = new SettingsWindow(true);
            IsDesigner = true;
        }
        
        public SettingsView3(SettingsWindow settingsWindow, bool OS)
        {
            InitializeComponent();
            ParentWindow = settingsWindow;
            DesktopOS = OS;
        }
        
        // Window Obj
        // private MainWindow appMainWindow;
        private SettingsWindow ParentWindow;

        // PROPS/STATICS
        private bool IsDesigner;
        private bool FirstTimeLoad = true;
        private bool DesktopOS;
        private int candidatesCount;

        private string StrAddCustomCopyPath = resSettingsWindow.strAddCustomCopyPath;
        private List<string> candidateCopiesPath = new();
        private List<string> defIcoSavPathList = new()
        {
            resSettingsWindow.strDefIcoSavPathItem0,
            resSettingsWindow.strDefIcoSavPathItem1,
            resSettingsWindow.strDefIcoSavPathItem2,
        };
        
        // LOAD
        private void View_OnLoaded(object? sender, RoutedEventArgs e)
        {
            if (IsDesigner) lsboxLinkCopies.Items.Insert(NextCopyItemIndex(), AddLinkCopyItem(FileOps.UserDesktop)); // For Designer
            if (FirstTimeLoad)
            {
                if (!DesktopOS)
                {
                    candidateCopiesPath.AddRange(SettingsOps.LinLinkPathCandidates);
                    panelWindowsOnlyControls.IsEnabled = false;
                }
                else candidateCopiesPath.AddRange(SettingsOps.WinLinkPathCandidates);
                candidatesCount = candidateCopiesPath.Count;

                foreach (var candidate in candidateCopiesPath)
                {
                    comboDEFLinkOutput.Items.Add(candidate);
                }
                foreach (var path in SettingsOps.LinkCopyPaths)
                {
                    lsboxLinkCopies.Items.Insert(NextCopyItemIndex(), AddLinkCopyItem(path));
                }
                
                candidateCopiesPath.Add(StrAddCustomCopyPath);
                comboaddLinkCopy.ItemsSource = candidateCopiesPath;
                comboaddLinkCopy.SelectedIndex = 0;
                comboUseDefaultIcoSavPath.ItemsSource = defIcoSavPathList;
                
                FirstTimeLoad = false;
            }
            //Settings
            ApplySettingsToControls();
            // if (ParentWindow.settings.UserAssetsPath == ParentWindow.settings.ConvICONPath) 
            // { chkUseUserAssets.IsChecked = true; }
        }
        
        // FUNCTIONS
        void ApplySettingsToControls()
        {
            chkAlwaysAskOutput.IsChecked = ParentWindow.settings.AlwaysAskOutput;
            panelDEFLinkOutput.IsEnabled = !ParentWindow.settings.AlwaysAskOutput;
            if (panelDEFLinkOutput.IsEnabled)
            {
                var DefPath = ParentWindow.settings.DEFLinkOutput;
                if (!comboDEFLinkOutput.Items.Contains(DefPath))
                    comboDEFLinkOutput.Items.Add(DefPath);
                comboDEFLinkOutput.SelectedItem = DefPath;
            }
            else comboDEFLinkOutput.SelectedIndex = 0;
            
            chkMakeLinkCopy.IsChecked = ParentWindow.settings.MakeLinkCopy;
            lsboxLinkCopies.IsEnabled = ParentWindow.settings.MakeLinkCopy;

            if (!DesktopOS) return;
            ValidateSavIcoPath(ParentWindow.settings.IcoSavPath);
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
                1 => resSettingsWindow.txtIcoSavPath1,
                2 => resSettingsWindow.txtIcoSavPath2,
                _ => ParentWindow.settings.UserAssetsPath
            };
            
            ParentWindow.settings.IcoSavPath = index switch
            {
                0 => ParentWindow.settings.UserAssetsPath,
                1 => SettingsOps.IcoSavROM,
                2 => SettingsOps.IcoSavRA,
                _ => ParentWindow.settings.UserAssetsPath
            };
        }
        
        void SetCustomSavIcoPath(string path)
        {
            chkUseDefaultIcoSavPath.IsChecked = false;
            comboUseDefaultIcoSavPath.IsEnabled = false;
            comboUseDefaultIcoSavPath.SelectedIndex = 0;
            panelWindowsOnlyControls2.IsEnabled = true;
            ParentWindow.settings.IcoSavPath = path;
        }

        int NextCopyItemIndex() => (lsboxLinkCopies.Items.Count < 2) ? 0 : lsboxLinkCopies.Items.Count - 2;
        
        ListBoxItem AddLinkCopyItem(string dir)
        {
            var newItem = new ListBoxItem();
            var gridControl = new Styles.LinkCopyItemGrid(dir);

            var trashButton = gridControl.NewItemTrash;
            trashButton!.Click += btnTrashCopyItem;
            
            newItem.Content = gridControl.NewItemGrid;
            return newItem;
        }
        
        
        // DEFAULT OUTPUT
        private void ChkAlwaysAskOutput_OnClick(object? sender, RoutedEventArgs e)
        {
            var chk = chkAlwaysAskOutput.IsChecked.GetValueOrDefault();
            ParentWindow.settings.AlwaysAskOutput = chk;
            panelDEFLinkOutput.IsEnabled = !chk;
        }
        
        private void ComboDEFLinkOutpu_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) => 
            ParentWindow.settings.DEFLinkOutput = (string)comboDEFLinkOutput.SelectedItem!;
        
        private async void BtnDefLinkOutput_OnClick(object? sender, RoutedEventArgs e)
        {
            string currentFolder = (string)comboDEFLinkOutput.SelectedItem!;
            string folder = await FileDialogOps.OpenFolderAsync(template:0, currentFolder, ParentWindow);
            if (string.IsNullOrWhiteSpace(folder)) return;
            // int customDirIndex = candidatesCount;
            if (comboDEFLinkOutput.Items.Count == candidatesCount) comboDEFLinkOutput.Items.Add(folder);
            else comboDEFLinkOutput.Items[candidatesCount] = folder;
            comboDEFLinkOutput.SelectedIndex = candidatesCount;
        }
        
        private void BtnclrDefLinkOutput_OnClick(object? sender, RoutedEventArgs e)
        {
            ParentWindow.settings.DEFLinkOutput = ParentWindow.DEFsettings.DEFLinkOutput;
            comboDEFLinkOutput.SelectedIndex = 0;
        }
        
        
        // LINK COPY
        void ChkMakeLinkCopy_IsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            var chk = chkMakeLinkCopy.IsChecked.GetValueOrDefault();
            ParentWindow.settings.MakeLinkCopy = chk;
            lsboxLinkCopies.IsEnabled = chk;
        }

        async void BtnAddLinkCopy_OnClick(object? sender, RoutedEventArgs e)
        {
            string currentItem = (string)comboaddLinkCopy.SelectedItem!;
            if (string.IsNullOrWhiteSpace(currentItem)) return;

            if (currentItem == StrAddCustomCopyPath)
            {
                string folder = await FileDialogOps.OpenFolderAsync(template:3, string.Empty, ParentWindow);
                if (!string.IsNullOrWhiteSpace(folder))
                {
                    lsboxLinkCopies.Items.Insert(NextCopyItemIndex(), AddLinkCopyItem(folder));
                    ParentWindow.SetLinkCopyPaths.Add(folder);
                }
            }
            else if (!ParentWindow.SetLinkCopyPaths.Contains(currentItem))
            {
                lsboxLinkCopies.Items.Insert(NextCopyItemIndex(), AddLinkCopyItem(currentItem));
                ParentWindow.SetLinkCopyPaths.Add(currentItem);
            }
            comboaddLinkCopy.SelectedIndex = 0;
        }
        
        void btnTrashCopyItem(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            var parentGrid = button.Parent as Grid;
            
            if (parentGrid!.Children[0] is not Label label) return;
            var path = (string)label.Content!;
            ParentWindow.SetLinkCopyPaths.Remove(path);
            
            var parentListBoxItem = parentGrid.Parent as ListBoxItem;
            lsboxLinkCopies.Items.Remove(parentListBoxItem);
        }

        
        // ICONS
        
        #region WINDOWS OS ONLY
        async void btnIcoSavPath_Click(object sender, RoutedEventArgs e)
        {
            string currentFolder = (string.IsNullOrEmpty(txtIcoSavPath.Text)) ? string.Empty : txtIcoSavPath.Text;
            string folder = await FileDialogOps.OpenFolderAsync(template:2, currentFolder, ParentWindow);
            if (string.IsNullOrWhiteSpace(folder)) return;
            txtIcoSavPath.Text = folder; 
            ParentWindow.settings.IcoSavPath = folder;
        }

        void btnclrIcoSavPath_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.settings.IcoSavPath = ParentWindow.DEFsettings.IcoSavPath;
            txtIcoSavPath.Text = ParentWindow.settings.IcoSavPath;
        }

        void chkUseUserAssets_Checked(object sender, RoutedEventArgs e)
        {
            // TODO: Obsolete?
            panelWindowsOnlyControls2.IsEnabled = !chkMakeLinkCopy.IsChecked.GetValueOrDefault();
            ParentWindow.settings.IcoSavPath = ParentWindow.settings.UserAssetsPath;
            txtIcoSavPath.Text = ParentWindow.settings.IcoSavPath;
        }
        
        private void IcoSavChecks_IsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            ParentWindow.settings.ExtractIco = chkExtractIco.IsChecked.GetValueOrDefault();
            ParentWindow.settings.IcoLinkName = chkIcoLinkName.IsChecked.GetValueOrDefault();
        }
        
        private void ChkUseDefaultIcoSavPath_IsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            if (!chkUseDefaultIcoSavPath.IsChecked.GetValueOrDefault()) SetCustomSavIcoPath(string.Empty);
            else SetDefaultSavIcoPath(0);
        }
        
        private void ComboUseDefaultIcoSavPath_DropDownClosed(object? sender, System.EventArgs e)
        {
            if (sender is not ComboBox combo) return;
            SetDefaultSavIcoPath((byte)combo.SelectedIndex);
        }
        #endregion
    }
}
