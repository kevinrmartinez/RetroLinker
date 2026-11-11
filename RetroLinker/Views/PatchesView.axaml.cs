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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RetroLinker.Models.Avalonia;
using RetroLinker.Models.DesktopLink;
using RetroLinker.Models.IO;
using RetroLinker.Translations;

namespace RetroLinker.Views;

public partial class PatchesView : UserControl
{
    #region AvaloniaProperties

    public static readonly DirectProperty<PatchesView, bool> AutoSelectProperty =
        AvaloniaProperty.RegisterDirect<PatchesView, bool>(
            nameof(AutoSelect),
            view => view.AutoSelect,
            (view, value) => view.AutoSelect = value,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public bool AutoSelect { get; set => SetAndRaise(AutoSelectProperty, ref field, value); }

    public static readonly DirectProperty<PatchesView, RomPatchType> SelectedPatchTypeProperty =
        AvaloniaProperty.RegisterDirect<PatchesView, RomPatchType>(
            nameof(SelectedPatchType),
            view => view.SelectedPatchType,
            (view, value) => view.SelectedPatchType = value,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public RomPatchType SelectedPatchType
    {
        get;
        set => SetAndRaise(SelectedPatchTypeProperty, ref field, value);
    } = RomPatchType.None;

    public static readonly DirectProperty<PatchesView, string?> TxtPatchPathProperty =
        AvaloniaProperty.RegisterDirect<PatchesView, string?>(
            nameof(TxtPatchPath),
            view => view.TxtPatchPath,
            (view, value) => view.TxtPatchPath = value,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public string? TxtPatchPath { get; set => SetAndRaise(TxtPatchPathProperty, ref field, value); }

    #endregion

    // == Props ==
    public string CoreContent { get; }
    public static string ChkNoPatchTip => resMainExtras.chkNoPatch_Tip1 + "\n" + resMainExtras.chkNoPatch_Tip2;

    public PatchesView()
    {
        // Constructor for Designer
        InitializeComponent();

        _parentWindow = new MainWindow(true);
        CoreContent = "\"path/to/rom.bin\"";
    }

    public PatchesView(MainWindow mainWindow)
    {
        InitializeComponent();

        _parentWindow = mainWindow;
        IUserLinkParameters buildingLink = _parentWindow.PermaView?.BuildingLink ?? new UserLinkBuilder();
        CoreContent = buildingLink.RaRom;
        var patchArgs = buildingLink.RaRomPatch;
        if (patchArgs is null) return;
        SelectedPatchType = patchArgs.PatchType;
        if (SelectedPatchType is RomPatchType.None or RomPatchType.No_Patch) return;
        TxtPatchPath = patchArgs.PatchArg;
    }

    // == Window Object ==
    private readonly MainWindow _parentWindow;

    // == FIELDS ==

    // == FUNCTIONS ==
    private RomPatchType GetPatchTypeByExtension(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return RomPatchType.None;
        var fileExtension = FileOps.GetFileExtFromPath(filePath).Trim('.').ToLower();
        Enum.TryParse(typeof(RomPatchType), fileExtension, ignoreCase: true, out var result);
        return (RomPatchType?)result ?? RomPatchType.None;
    }

    private RomPatchArgs? CreateSoftPatchingArg(string patchFile, RomPatchType patchType)
    {
        if (patchType is RomPatchType.None) return null;
        var arg = patchType switch
        {
            RomPatchType.UPS or RomPatchType.BPS or
                RomPatchType.IPS or RomPatchType.XDelta => patchFile.ConditionallyQuoteString(),
            _ => string.Empty   // <= RomPatchType.No_Patch goes with an empty 'arg'
        };
        return new RomPatchArgs(patchType, arg);
    }

    // == PATCHES CONTROLS ==

    private void LockControls(bool locked) => gridContent.IsEnabled = !locked;

    public async void BrowsePatch_Async()
    {
        try
        {
            LockControls(true);
            var openOptions = PickerOpt.PatchOpenOptions(SelectedPatchType);
            string file = await FileDialogOps.OpenFileAsync(openOptions, _parentWindow);
            if (string.IsNullOrEmpty(file)) return;

            if (SelectedPatchType is RomPatchType.Any) SelectedPatchType = GetPatchTypeByExtension(file);
            TxtPatchPath = file;
        }
        catch (Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    // private void BtnPatchPath_OnClick(object? sender, RoutedEventArgs e) => BtnPatchPath_ClickAsycn();

    // == VIEW CONTROLS ==
    public void SavePatch()
    {
        var patchPath = TxtPatchPath ?? string.Empty;
        if (SelectedPatchType is RomPatchType.Any) SelectedPatchType = GetPatchTypeByExtension(patchPath);
        var patchComm = CreateSoftPatchingArg(patchPath, SelectedPatchType);

        if (string.IsNullOrWhiteSpace(patchPath)) {
            if (SelectedPatchType is not RomPatchType.None and not RomPatchType.No_Patch) {
                var msBoxContent = new PopUpGenericContent(resMainExtras.popNonSelected_Msg, resMainExtras.popNonSelected_Tittle);
                _ = this.PopUpGenericMessageBox(msBoxContent, GenericPopUpType.Info);
                patchComm = null;
            }
        }

        _parentWindow.ReturnToMainView(this, patchComm);
    }

    public void DiscPatch() => _parentWindow.ReturnToMainView();
}