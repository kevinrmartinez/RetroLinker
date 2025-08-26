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
using RetroLinker.Models.LinuxClasses;

namespace RetroLinker.Views;

public partial class RenameEntryView : UserControl
{
    public RenameEntryView()
    {
        // Constructor for Designer
        InitializeComponent();
        _popUpWindow = new PopUpWindow(true);
        GivenPath = "designer.txt";
        GivenName = NamePlaceHolder;
        CurrentCore = "mesen";
        Outputs = new List<ShortcutterOutput>();
    }
    
    public RenameEntryView(PopUpWindow parentWindow)
    {
        // Constructor for Designer
        InitializeComponent();
        _popUpWindow = parentWindow;
        GivenPath = "designer.txt";
        GivenName = NamePlaceHolder;
        CurrentCore = "mesen";
        Outputs = new List<ShortcutterOutput>();
    }
    
    public RenameEntryView(PopUpWindow parentWindow, string givenPath, string? givenCore, List<ShortcutterOutput> outputs)
    {
        InitializeComponent();
        _popUpWindow = parentWindow;
        GivenPath = givenPath;
        GivenName = FileOps.GetFileNameNoExtFromPath(GivenPath);
        CurrentCore = givenCore;
        Outputs = outputs;
        
        CustomFilename = false;
        txtFriendlyName.Text = GivenName;
        lblExt.Content = FileOps.GetOutputExt(false);
    }
    
    // FIELDS
    private readonly PopUpWindow _popUpWindow;
    private string GivenPath;
    private string GivenName;
    private string? CurrentCore;
    private bool CustomFilename;
    private ShortcutterOutput NewName = new();
    private List<ShortcutterOutput> Outputs;
    
    private const string NamePlaceHolder = LinDesktopEntry.NamePlaceHolder;
    

    #region Functions

    private void UpdateFilename()
    {
        if (CustomFilename) return;
        txtFileName.Text = LinDesktopEntry.StdDesktopEntry(txtFriendlyName.Text, CurrentCore);
    }

    private List<ShortcutterOutput> ResolveOutput()
    {
        if (Outputs.Count > 0) Outputs[0] = NewName;
        else Outputs.Add(NewName);
        return Outputs;
    }

    private bool AreBoxesEmpty() => string.IsNullOrWhiteSpace(txtFriendlyName.Text) || string.IsNullOrWhiteSpace(txtFileName.Text);

    private void LockButton(bool isLock) => btnNameApply.IsEnabled = !isLock;

    #endregion


    #region Controls

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
        CustomFilename = chkCustomFilename.IsChecked.GetValueOrDefault();
        txtFileName.IsReadOnly = !CustomFilename;
        UpdateFilename();
    }
    
    private void BtnClear_OnClick(object? sender, RoutedEventArgs e)
    {
        chkCustomFilename.IsChecked = false;
        txtFriendlyName.Text = GivenName;
    }

    private void BtnNameApply_OnClick(object? sender, RoutedEventArgs e)
    {
        var friendlyName = txtFriendlyName.Text!;
        var fileName = (CustomFilename) 
            ? txtFileName.Text! 
            : LinDesktopEntry.StdDesktopEntry(friendlyName + FileOps.GetOutputExt(false), CurrentCore);
        var newPath = FileOps.GetDirAndCombine(GivenPath, fileName);
        NewName = new ShortcutterOutput(newPath, friendlyName, fileName);
        NewName.CustomEntryName = CustomFilename;
        _popUpWindow.Close(ResolveOutput());
    }
    #endregion


    #region ViewEvents

    // On Close
    private void View_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (NewName.ValidOutput) return;
        if (GivenName == NamePlaceHolder) return;
        NewName = new ShortcutterOutput(GivenPath, CurrentCore);
        _popUpWindow.Close(ResolveOutput());
    }

    #endregion
}