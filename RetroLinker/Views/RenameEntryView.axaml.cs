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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RetroLinker.Models.IO;
using RetroLinker.Models.Linux;

namespace RetroLinker.Views;

public partial class RenameEntryView : UserControl
{

    #region AvaloniaProperties

    public static readonly DirectProperty<RenameEntryView, string> TxtFriendlyNameProperty =
        AvaloniaProperty.RegisterDirect<RenameEntryView, string>(
            nameof(TxtFriendlyName),
            view => view.TxtFriendlyName,
            (view, value) => view.TxtFriendlyName = value,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public string TxtFriendlyName
    {
        get;
        set {
            SetAndRaise(TxtFriendlyNameProperty, ref field, value);
            if (ChkCustomFilename) return;
            TxtFileName = DesktopEntryOps.StdDesktopEntry(value, _helper.GivenCore);
        }
    } = string.Empty;

    public static readonly DirectProperty<RenameEntryView, string> TxtFileNameProperty =
        AvaloniaProperty.RegisterDirect<RenameEntryView, string>(
            nameof(TxtFileName),
            view => view.TxtFileName,
            (view, value) => view.TxtFileName = value,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public string TxtFileName {
        get;
        set => SetAndRaise(TxtFileNameProperty, ref field, value);
    } = "---";

    public static readonly DirectProperty<RenameEntryView, bool> ChkCustomFilenameProperty =
        AvaloniaProperty.RegisterDirect<RenameEntryView, bool>(
            nameof(ChkCustomFilename),
            view => view.ChkCustomFilename,
            (view, value) => view.ChkCustomFilename = value,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public bool ChkCustomFilename
    {
        get;
        set {
            _helper.CustomEntry = value;
            SetAndRaise(ChkCustomFilenameProperty, ref field, value);
            if (!value) TxtFriendlyName = TxtFriendlyName;
        }
    }

    #endregion

    // Static FIELDS
    public static readonly string EntryExt = FileOps.GetOutputExt(false);

    public RenameEntryView()
    {
        // Constructor for Designer
        InitializeComponent();
        _popUpWindow = new PopUpWindow(true);
        _helper = new RenamePopUpHelper("designer.txt", string.Empty, "mesen", customEntry: false);
        _correctedGivenName = _helper.GivenName;
        Restore();
    }

    public RenameEntryView(PopUpWindow parentWindow, RenamePopUpHelper popUpParams)
    {
        InitializeComponent();
        _popUpWindow = parentWindow;
        _helper = popUpParams;

        _correctedGivenName = DesktopEntryOps.ReverseDesktopEntryName(_helper.GivenName, stripExt: false, out var friendlyName)
            ? friendlyName : _helper.GivenName;
        _ogCustomEntry = _helper.CustomEntry;
        Restore();
    }

    // FIELDS
    private string ReadyFileName => TxtFileName + EntryExt;
    private readonly PopUpWindow _popUpWindow;
    private readonly RenamePopUpHelper _helper;
    private readonly string _correctedGivenName;
    private readonly bool _ogCustomEntry;

    private const string NamePlaceHolder = DesktopEntryOps.NamePlaceHolder;

    #region Controls

    public void Restore() {
        TxtFriendlyName = _correctedGivenName;
        ChkCustomFilename = _ogCustomEntry;
        if (ChkCustomFilename) TxtFileName = FileOps.GetFileNameFromPath(_helper.GivenPath);
    }

    public void ApplyRename()
    {
        var newDir = FileOps.ForceGetDirFromPath(_helper.GivenPath);
        _helper.NewPath = FileOps.CombineMultipleInputs(newDir, ReadyFileName);
        _helper.NewFriendlyName = TxtFriendlyName;
        _helper.ValidOutput = true;
        _popUpWindow.Close(_helper);
    }

    public void DoNotRename()
    {
        _helper.NewPath = _helper.GivenPath;
        _helper.NewFriendlyName = _correctedGivenName;
        ChkCustomFilename = true;
        _helper.ValidOutput = true;
        _popUpWindow.Close(_helper);
    }
    #endregion
}