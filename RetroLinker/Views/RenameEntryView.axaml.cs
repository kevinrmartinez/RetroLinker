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

using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RetroLinker.Models;
using RetroLinker.Models.LinFunc;

namespace RetroLinker.Views;

public partial class RenameEntryView : UserControl
{
    public RenameEntryView(PopUpWindow parentWindow, string givenPath, string givenCore)
    {
        InitializeComponent();
        _popUpWindow = parentWindow;
        GivenPath = givenPath;
        CurrentCore = givenCore;
    }
    
    // FIELDS
    private PopUpWindow _popUpWindow;
    private string GivenPath;
    private string GivenName;
    private string CurrentCore;
    private bool CustomFilename;
    private ShortcutterOutput NewName;
    
    // LOAD EVENTS
    private void View_OnLoaded(object? sender, RoutedEventArgs e)
    {
        CustomFilename = false;
        GivenName = FileOps.GetFileNameNoExtFromPath(GivenPath);
        txtFriendlyName.Text = GivenName;
        lblExt.Content = FileOps.LinLinkExt;
    }
    
    // FUNCTIONS
    private void UpdateFilename()
    {
        if (CustomFilename) return;
        txtFileName.Text = LinDesktopEntry.StdDesktopEntry(txtFriendlyName.Text, CurrentCore);
    }

    private void ResolveOutput()
    { 
        if (_popUpWindow.ParentWindow.BuildingLink.OutputPaths.Count > 0)
            _popUpWindow.ParentWindow.BuildingLink.OutputPaths[0] = NewName;
        else _popUpWindow.ParentWindow.BuildingLink.OutputPaths.Add(NewName);
    }

    private bool AreBoxesEmpty() => string.IsNullOrWhiteSpace(txtFriendlyName.Text) || string.IsNullOrWhiteSpace(txtFileName.Text);

    private void LockButton(bool isLock) => btnNameApply.IsEnabled = !isLock;
    
    // CONTROLS
    private void TxtFriendlyName_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        LockButton(AreBoxesEmpty());
        UpdateFilename();
    }
    
    private void TxtFileName_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!txtFileName.IsReadOnly) LockButton(AreBoxesEmpty());
    }
    
    private void ChkCustomFilename_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (chkCustomFilename is not null) CustomFilename = (bool)chkCustomFilename.IsChecked;
        txtFileName.IsReadOnly = !CustomFilename;
        lblExt.IsVisible = CustomFilename;
        UpdateFilename();
    }
    
    private void BtnClear_OnClick(object? sender, RoutedEventArgs e)
    {
        chkCustomFilename.IsChecked = false;
        txtFriendlyName.Text = GivenName;
    }

    private void BtnNameApply_OnClick(object? sender, RoutedEventArgs e)
    {
        var friendlyName = txtFriendlyName.Text;
        var fileName = txtFileName.Text + FileOps.LinLinkExt;
        var newPath = FileOps.GetDirAndCombine(GivenPath, fileName);
        NewName = new ShortcutterOutput(newPath, friendlyName, fileName);
        ResolveOutput();
        _popUpWindow.Close();
    }
    
    // CLOSING EVENTS
    private void View_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (NewName is not null) return;
        NewName = new ShortcutterOutput(GivenPath, CurrentCore);
        ResolveOutput();
    }
}