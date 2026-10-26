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
    // == Props ==
    public string CoreContent { get; }
    
    public PatchesView()
    {
        // Constructor for Designer
        InitializeComponent();
        
        _parentWindow = new MainWindow(true);
        CoreContent = "\"path/to/rom.bin\"";
        _patchRadioButtons.AddRange([rdoUPSPatch, rdoBPSPatch, rdoIPSPatch, rdoXDPatch, rdoNoPatch]);
        CompleteSetup();
        DataContext = this;
    }
    
    public PatchesView(MainWindow mainWindow)
    {
        InitializeComponent();
        
        _parentWindow = mainWindow;
        IUserLinkParameters buildingLink = _parentWindow.PermaView?.BuildingLink ?? new UserLinkBuilder();
        CoreContent = buildingLink.RaRom;
        _patchArgs = buildingLink.RaRomPatch;
        _patchRadioButtons.AddRange([rdoUPSPatch, rdoBPSPatch, rdoIPSPatch, rdoXDPatch, rdoNoPatch]);
        CompleteSetup();
        DataContext = this;
    }
    
    // == Window Object ==
    private readonly MainWindow _parentWindow;
    
    // == FIELDS ==
    private PatchOpts _patchOpts;
    private readonly RomPatchArgs? _patchArgs;
    private readonly List<RadioButton> _patchRadioButtons = new();
    
    
    // == LOAD EVENTS ==
    private void CompleteSetup()
    {
        string tip = resMainExtras.chkNoPatch_Tip1 + "\n" + resMainExtras.chkNoPatch_Tip2;
        ToolTip.SetTip(chkNoPatch, tip);
        rdoNoPatch.IsChecked = true;    // For proper behavior, rdoNoPatch must always change states on loading

        rdoUPSPatch.Tag = RomPatchType.UPS;
        rdoBPSPatch.Tag = RomPatchType.BPS;
        rdoIPSPatch.Tag = RomPatchType.IPS;
        rdoXDPatch.Tag  = RomPatchType.XDelta;
        rdoNoPatch.Tag  = RomPatchType.None;
        chkNoPatch.Tag  = RomPatchType.No_Patch;
        
        if (_patchArgs is null) return;
        var file = _patchArgs.PatchArg;
        var patchType = _patchArgs.PatchType;
        switch (patchType)
        {
            case RomPatchType.None:
                break;
            case RomPatchType.No_Patch:
                chkNoPatch.IsChecked = true;
                break;
            default:
                var rdo = _patchRadioButtons.Find(rdo => Equals(rdo.Tag, patchType));
                if (rdo is not null) {
                    rdo.IsChecked = true;
                    txtPatchPath.Text = file;
                }
                break;
        }
    }
    
    // == FUNCTIONS ==
    private RomPatchType GetPatchTypeByExtension(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return RomPatchType.None;
        var fileExtension = FileOps.GetFileExtFromPath(filePath);
        fileExtension = fileExtension.Trim('.').ToLower();
        System.Enum.TryParse(typeof(RomPatchType), fileExtension, ignoreCase: true, out var result);
        return (RomPatchType?)result ?? RomPatchType.None;
    }
    
    private RomPatchArgs? CreateSoftPatchingArg(string patchFile, RomPatchType patchType)
    {
        if (patchType is RomPatchType.None) return null;
        var arg = patchType switch
        {
            RomPatchType.UPS or RomPatchType.BPS or RomPatchType.IPS or RomPatchType.XDelta 
                => Utils.WrapStringBetweenQuotes(patchFile),
            _ => string.Empty   // <= RomPatchType.No_Patch goes with an empty 'arg'
        };
        return new RomPatchArgs(patchType, arg);
    }
    
    // == PATCHES CONTROLS ==
    
    private void LockControls(bool locked) => gridContent.IsEnabled = !locked;
    
    [System.Obsolete("Replace with some binding")]
    private void ControlsEnabled(bool enable)
    {
        txtPatchPath.IsEnabled = enable;
        btnPatchPath.IsEnabled = enable;
        chkNoPatch.IsChecked = false;
        chkNoPatch.IsEnabled = !enable;
    }
    
    private void RadioButtonPatch_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (rdoNoPatch is not null) ControlsEnabled(!rdoNoPatch.IsChecked.GetValueOrDefault());
        if (e.Source is not RadioButton radioButton) return;
        // if (radioButton.Tag is not SoftPatch softPatch) return;
        var softPatch = radioButton.Tag as RomPatchType?;
        _patchOpts = softPatch switch
        {
            RomPatchType.UPS => PatchOpts.UPS,
            RomPatchType.BPS => PatchOpts.BPS,
            RomPatchType.IPS => PatchOpts.IPS,
            RomPatchType.XDelta => PatchOpts.XD,
            _ => PatchOpts.Auto
        };
    }

    private async void BtnPatchPath_ClickAsycn()
    {
        try {
            LockControls(true);
            var openOptions = PickerOpt.PatchOpenOptions(_patchOpts);
            string file = await FileDialogOps.OpenFileAsync(openOptions, _parentWindow);
            if (!string.IsNullOrEmpty(file)) txtPatchPath.Text = file;
        }
        catch (System.Exception e) { _ = this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    private void BtnPatchPath_OnClick(object? sender, RoutedEventArgs e) => BtnPatchPath_ClickAsycn();

    // == VIEW CONTROLS ==
    private void BtnSavePatch_OnClick(object? sender, RoutedEventArgs e)
    {
        RomPatchArgs? patchComm;
        RomPatchType selectedPatch = RomPatchType.None;
        foreach (var radioButton in _patchRadioButtons)
        {
            if (!radioButton.IsChecked.GetValueOrDefault()) continue;
            if (radioButton.Tag is not RomPatchType softPatch) continue;
            selectedPatch = softPatch;
            break;
        }
        
        var patchPath = txtPatchPath.Text ?? string.Empty;
        if (rdoAutoPatch.IsChecked.GetValueOrDefault()) selectedPatch = GetPatchTypeByExtension(patchPath);
        if (chkNoPatch.IsChecked.GetValueOrDefault()) selectedPatch = RomPatchType.No_Patch;
        patchComm = CreateSoftPatchingArg(patchPath, selectedPatch);
        
        if (string.IsNullOrEmpty(patchPath)) {
            if (selectedPatch is not RomPatchType.None and not RomPatchType.No_Patch) {
                var msBoxContent = new PopUpGenericContent(resMainExtras.popNonSelected_Msg, resMainExtras.popNonSelected_Tittle);
                _ = this.PopUpGenericMessageBox(msBoxContent, GenericPopUpType.Info);
                patchComm = null;
            }
        }
        
        _parentWindow.ReturnToMainView(this, patchComm);
    }

    private void BtnDiscPatch_OnClick(object? sender, RoutedEventArgs e) => _parentWindow.ReturnToMainView();
}