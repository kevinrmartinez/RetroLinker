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
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using RetroLinker.Models;

namespace RetroLinker.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            // Constructor for Designer
            InitializeComponent();
            settings = new Settings();
            
            SetLinkCopyPaths = SettingsOps.LinkCopyPaths;
            CCTab1.Content = new SettingsView(this, DesktopOS);
            CCTab2.Content = new SettingsView2(this, DesktopOS);
            CCTab3.Content = new SettingsView3(this, DesktopOS);
        }

        public SettingsWindow(bool isDesigner)
        {
            InitializeComponent();
            settings = new Settings();
            SetLinkCopyPaths = SettingsOps.LinkCopyPaths;
        }
        
        public SettingsWindow(MainWindow mainWindow, Settings _settings)
        {
            InitializeComponent();
            // this.mainWindow = mainWindow;
            settings = _settings;
            
            SetLinkCopyPaths = SettingsOps.LinkCopyPaths;
            CCTab1.Content = new SettingsView(this, DesktopOS);
            CCTab2.Content = new SettingsView2(this, DesktopOS);
            CCTab3.Content = new SettingsView3(this, DesktopOS);
        }
        
        // Window Obj
        // private MainWindow mainWindow;

        // PROPS/STATICS
        public Settings settings { get; set; }
        public Settings DEFsettings { get; set; } = new();
        
        private bool DesktopOS = System.OperatingSystem.IsWindows();
        public List<string> SetLinkCopyPaths;

        
        #region Window/Dialog Controls
        void btnDISSettings_Click(object sender, RoutedEventArgs e) => CloseWindow(null);

        async void btnDEFSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxStandardParams msparams = new()
            {
                ContentTitle = Translations.resSettingsWindow.popDefaults_Title,
                ContentMessage = Translations.resSettingsWindow.popDefaults_Mess,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ButtonDefinitions = MsBox.Avalonia.Enums.ButtonEnum.OkCancel,
                EnterDefaultButton = MsBox.Avalonia.Enums.ClickEnum.Ok,
                EscDefaultButton = MsBox.Avalonia.Enums.ClickEnum.Cancel,

            };
            var msbox = MessageBoxManager.GetMessageBoxStandard(msparams);
            var result = await msbox.ShowWindowDialogAsync(this);
            if (result == MsBox.Avalonia.Enums.ButtonResult.Ok)
            {
                SettingsOps.PrevConfigs = new List<string>();
                SettingsOps.LinkCopyPaths = new List<string>();
                SettingsOps.WriteSettings(DEFsettings);
                CloseWindow(DEFsettings);
            }
                
        }

        void btnSAVESettings_Click(object sender, RoutedEventArgs e)
        {
            if (SetLinkCopyPaths.Count < 1) settings.MakeLinkCopy = false;
            if (!settings.AlwaysAskOutput) settings.AlwaysAskOutput = string.IsNullOrEmpty(settings.DEFLinkOutput);
            SettingsOps.LinkCopyPaths = SetLinkCopyPaths;
            SettingsOps.WriteSettings(settings);
            CloseWindow(settings);
        }

        void CloseWindow(Settings? retSettings) => Close(retSettings);
        #endregion

        
    }
}
