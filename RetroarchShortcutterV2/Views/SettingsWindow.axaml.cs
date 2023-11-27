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
        public List<string> SetLinkCopyPaths { get; set; }

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
            SetLinkCopyPaths = SettingsOps.LinkCopyPaths;
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
                SettingsOps.PrevConfigs = new List<string>();
                SettingsOps.LinkCopyPaths = new List<string>();
                SettingsOps.WriteSettingsFile(DEFsettings);
                CloseView();
            }
                
        }

        void btnSAVESettings_Click(object sender, RoutedEventArgs e)
        {
            if (SetLinkCopyPaths.Count == 0) settings.MakeLinkCopy = false;
            SettingsOps.WriteSettingsFile(settings);
            SettingsOps.LinkCopyPaths = SetLinkCopyPaths;
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
