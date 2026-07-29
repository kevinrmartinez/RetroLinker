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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia.Dto;
using RetroLinker.Models;
using RetroLinker.Models.Avalonia;

namespace RetroLinker.Views
{
    public partial class SettingsWindow : Window
    {
        // Window Obj
        // private MainWindow mainWindow;

        // PROPS/STATICS
        public Settings settings { get; set; }
        public Settings DEFsettings { get; set; } = new();
        public DispatcherTimer? BindTimer { get; private set; }
        
        ContentControl[] _tabs;
        private bool DesktopOS = System.OperatingSystem.IsWindows();
        public List<string> SetLinkCopyPaths;
        
        public SettingsWindow()
        {
            // Constructor for Designer
            InitializeComponent();
            settings = new Settings();
            _tabs = [CCTab1, CCTab2, CCTab3];
            SetLinkCopyPaths = SettingsOps.LinkCopyPaths;
            CCTab1.Content = new SettingsView(this, DesktopOS);
            CCTab2.Content = new SettingsView2(this, DesktopOS);
            CCTab3.Content = new SettingsView3(this, DesktopOS);
        }

        public SettingsWindow(bool isDesigner)
        {
            InitializeComponent();
            settings = new Settings();
            _tabs = [CCTab1, CCTab2, CCTab3];
            SetLinkCopyPaths = SettingsOps.LinkCopyPaths;
        }
        
        public SettingsWindow(MainWindow mainWindow, Settings _settings)
        {
            InitializeComponent();
            // this.mainWindow = mainWindow;
            settings = _settings;
            _tabs = [CCTab1, CCTab2, CCTab3];
            SetLinkCopyPaths = SettingsOps.LinkCopyPaths;
            CCTab1.Content = new SettingsView(this, DesktopOS);
            CCTab2.Content = new SettingsView2(this, DesktopOS);
            CCTab3.Content = new SettingsView3(this, DesktopOS);
        }
        
        
        private void UpdateContext(object? sender, System.EventArgs? e)
        {
            foreach (var tab in _tabs) {
                if (tab.Content is not UserControl view) continue;
                view.DataContext = null;
                view.DataContext = view;
            }
        }
        
        public void UpdateContextFromOutside() => UpdateContext(null, null);
        
        private void Window_OnLoaded(object? sender, RoutedEventArgs e)
        {
            BindTimer = new() {
                Interval = System.TimeSpan.FromMilliseconds(300)
            };
            BindTimer.Tick += UpdateContext;
            BindTimer.Start();
        }

        private void Window_OnUnloaded(object? sender, RoutedEventArgs e)
        {
            if (BindTimer is null) return;
            BindTimer.Stop();
            BindTimer = null;
        }
        
        #region Window/Dialog Controls
        void btnDISSettings_OnClick(object sender, RoutedEventArgs e) => CloseWindow(null);

        async void btnDEFSettings_Click()
        {
            try
            {
                MessageBoxStandardParams mbParams = new()
                {
                    ContentTitle = Translations.resSettingsWindow.popDefaults_Title,
                    ContentMessage = Translations.resSettingsWindow.popDefaults_Mess,
                    Icon = MsBox.Avalonia.Enums.Icon.Question,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ButtonDefinitions = MsBox.Avalonia.Enums.ButtonEnum.OkCancel,
                    EnterDefaultButton = MsBox.Avalonia.Enums.ClickEnum.Ok,
                    EscDefaultButton = MsBox.Avalonia.Enums.ClickEnum.Cancel,

                };
                var result = await this.PopUpMessageBox<MsBox.Avalonia.Enums.ButtonResult>(mbParams);
                if (result != MsBox.Avalonia.Enums.ButtonResult.Ok) return;
                SettingsOps.PrevConfigs = new List<string>();
                SettingsOps.LinkCopyPaths = new List<string>();
                SettingsOps.WriteSettings(DEFsettings);
                CloseWindow(DEFsettings);
            }
            catch (System.Exception e) { _ = this.PopUpGenericError(e); }
        }

        void btnDEFSettings_Click(object sender, RoutedEventArgs e) => btnDEFSettings_Click();

        void btnSAVESettings_OnClick(object sender, RoutedEventArgs e)
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
