/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023-2026 Kevin Rafael Martinez Johnston

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

using Avalonia.Controls;
using Avalonia.Interactivity;
using RetroLinker.Models.IO;
using RetroLinker.Models.Linux;

namespace RetroLinker.Views;

public partial class RenameEntryView : UserControl
{
    public RenameEntryView()
    {
        // Constructor for Designer
        InitializeComponent();
        _popUpWindow = new PopUpWindow(true);
        _helper = new RenamePopUpHelper("designer.txt", string.Empty, "mesen", customEntry: false);
        _correctedGivenName = _helper.GivenName;
    }

    public RenameEntryView(PopUpWindow parentWindow, RenamePopUpHelper popUpParams)
    {
        InitializeComponent();
        _popUpWindow = parentWindow;
        _helper = popUpParams;

        _correctedGivenName = DesktopEntryOps.ReverseDesktopEntryName(_helper.GivenName, stripExt: false, out var friendlyName)
            ? friendlyName : _helper.GivenName;
        txtFriendlyName.Text = _correctedGivenName;
        chkCustomFilename.IsChecked = _helper.CustomEntry;
        if (_helper.CustomEntry) txtFileName.Text = FileOps.GetFileNameFromPath(_helper.GivenPath);
    }

    // FIELDS
    public static readonly string EntryExt = FileOps.GetOutputExt(false);
    private readonly PopUpWindow _popUpWindow;
    private readonly RenamePopUpHelper _helper;
    private readonly string _correctedGivenName;

    private const string NamePlaceHolder = DesktopEntryOps.NamePlaceHolder;


    #region Functions

    private void UpdateFilename() {
        if (_helper.CustomEntry) return;
        txtFileName.Text = DesktopEntryOps.StdDesktopEntry(txtFriendlyName.Text, _helper.GivenCore);
    }

    private bool AreBoxesEmpty()
        => string.IsNullOrWhiteSpace(txtFriendlyName.Text) || string.IsNullOrWhiteSpace(txtFileName.Text);

    private void LockButton(bool isLock) => btnNameApply.IsEnabled = !isLock;

    #endregion


    #region Controls

    private void TxtFriendlyName_OnTextChanged(object? sender, TextChangedEventArgs e) {
        LockButton(AreBoxesEmpty());
        UpdateFilename();
    }

    private void TxtFileName_OnTextChanged(object? sender, TextChangedEventArgs e) {
        if (sender is not TextBox textBox) return;
        if (!textBox.IsReadOnly) LockButton(AreBoxesEmpty());
    }

    private void ChkCustomFilename_OnIsCheckedChanged(object? sender, RoutedEventArgs e) {
        _helper.CustomEntry = chkCustomFilename.IsChecked.GetValueOrDefault();
        txtFileName.IsReadOnly = !_helper.CustomEntry;
        UpdateFilename();
    }

    private void BtnRestore_OnClick(object? sender, RoutedEventArgs e) {
        // TODO: Restore! Not default!
        chkCustomFilename.IsChecked = false;
        txtFriendlyName.Text = _correctedGivenName;
    }

    private void BtnNameApply_OnClick(object? sender, RoutedEventArgs e)
    {
        var friendlyName = txtFriendlyName.Text ?? NamePlaceHolder;
        var cusFileName = txtFileName.Text;
        if (!string.IsNullOrWhiteSpace(cusFileName)) cusFileName += EntryExt;
        var fileName = (_helper.CustomEntry)
            ? cusFileName ?? NamePlaceHolder
            : DesktopEntryOps.StdDesktopEntry(friendlyName + EntryExt, _helper.GivenCore);
        var newDir = FileOps.GetDirFromPath(_helper.GivenPath) ?? FileOps.BaseDir;
        var newPath = FileOps.CombineMultipleInputs(newDir, fileName);

        _helper.NewPath = newPath;
        _helper.NewFriendlyName = friendlyName;
        _helper.ValidOutput = true;
        _popUpWindow.Close(_helper);
    }
    #endregion


    #region ViewEvents

    // On Close
    private void View_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (_helper.ValidOutput) return;
        if (_correctedGivenName == NamePlaceHolder) return;

        // TODO: Try to restore before returning a default value
        var fileName = DesktopEntryOps.StdDesktopEntry(_correctedGivenName + EntryExt, _helper.GivenCore);
        var newDir = FileOps.GetDirFromPath(_helper.GivenPath) ?? FileOps.BaseDir;
        _helper.NewPath = FileOps.CombineMultipleInputs(newDir, fileName);
        _helper.NewFriendlyName = _correctedGivenName;
        _helper.ValidOutput = true;
        _popUpWindow.Close(_helper);
    }

    #endregion
}