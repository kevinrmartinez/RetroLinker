/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2024  Kevin Rafael Martinez Johnston

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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RetroLinker.Models;
using RetroLinker.Models.Avalonia;
using RetroLinker.Models.CommandLine;
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
        _patchString = string.Empty;
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
        _patchString = buildingLink.RaRomPatch ?? string.Empty;
        _patchRadioButtons.AddRange([rdoUPSPatch, rdoBPSPatch, rdoIPSPatch, rdoXDPatch, rdoNoPatch]);
        CompleteSetup();
        DataContext = this;
    }
    
    // == Window Object ==
    private readonly MainWindow _parentWindow;
    
    // == FIELDS ==
    private PatchOpts _patchOpts;
    private readonly string _patchString;
    private readonly List<RadioButton> _patchRadioButtons = new();
    
    
    // == LOAD EVENTS ==
    private void CompleteSetup()
    {
        string tip = resMainExtras.chkNoPatch_Tip1 + "\n" + resMainExtras.chkNoPatch_Tip2;
        ToolTip.SetTip(chkNoPatch, tip);
        rdoNoPatch.IsChecked = true;    // For proper behavior, rdoNoPatch must always change states on loading

        rdoUPSPatch.Tag = Manager.UpsPatch;
        rdoBPSPatch.Tag = Manager.BpsPatch;
        rdoIPSPatch.Tag = Manager.IpsPatch;
        rdoXDPatch.Tag = Manager.XdPatch;
        rdoNoPatch.Tag = Manager.NoPatch;
        chkNoPatch.Tag = Manager.ExNoPatch;
        
        if (string.IsNullOrEmpty(_patchString)) return;
        try
        {
            var (file, patchType) = Manager.ResolveSoftPatchingArg(_patchString);
            switch (patchType.PatchType)
            {
                case ROMPatchType.NoPatch:
                    break;
                case ROMPatchType.ExNoPatch:
                    chkNoPatch.IsChecked = true;
                    break;
                default:
                    var rdo = _patchRadioButtons.Find(rdo => ReferenceEquals(rdo.Tag, patchType));
                    if (rdo is not null) {
                        rdo.IsChecked = true;
                        txtPatchPath.Text = file;
                    }
                    break;
            }
        }
        catch (System.ArgumentException ex) {
            Logger.LogErro(ex);
            rdoNoPatch.IsChecked = true;
        }
    }
    
    // == FUNCTIONS ==
    private SoftPatch GetPatchTypeByExtension(string filePath)
    {
        var fileExtension = FileOps.GetFileExtFromPath(filePath);
        fileExtension = fileExtension.Trim('.').ToLower();
        return fileExtension switch
        {
            Manager.UpsExt => Manager.UpsPatch, 
            Manager.BpsExt => Manager.BpsPatch,
            Manager.IpsExt => Manager.IpsPatch, 
            Manager.XdExt => Manager.XdPatch,
            _ => Manager.UpsPatch
        };
    }
    
    // == PATCHES CONTROLS ==
    
    private void LockControls(bool locked) => gridContent.ShowGridLines = !locked;
    
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
        var softPatch = radioButton.Tag as SoftPatch;
        _patchOpts = softPatch?.PatchType switch
        {
            ROMPatchType.UPS => PatchOpts.UPS,
            ROMPatchType.BPS => PatchOpts.BPS,
            ROMPatchType.IPS => PatchOpts.IPS,
            ROMPatchType.XDelta => PatchOpts.XD,
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
        string patchComm;
        SoftPatch selectedPatch = Manager.NoPatch;
        foreach (var radioButton in _patchRadioButtons)
        {
            if (radioButton.Tag is not SoftPatch softPatch) continue;
            if (!radioButton.IsChecked.GetValueOrDefault()) continue;
            selectedPatch = softPatch;
            break;
        }
        
        var patchPath = txtPatchPath.Text ?? string.Empty;
        if (chkNoPatch.IsChecked.GetValueOrDefault()) selectedPatch = Manager.ExNoPatch;
        if (rdoAutoPatch.IsChecked.GetValueOrDefault()) selectedPatch = GetPatchTypeByExtension(patchPath);
        patchComm = Manager.CreateSoftPatchingArg(patchPath, selectedPatch);
        
        if (string.IsNullOrEmpty(patchPath)) {
            if (!selectedPatch.Equals(Manager.NoPatch) && !selectedPatch.Equals(Manager.ExNoPatch)) {
                var msBoxContent = new PopUpGenericContent(resMainExtras.popNonSelected_Msg, resMainExtras.popNonSelected_Tittle);
                _ = this.PopUpGenericMessageBox(msBoxContent, GenericPopUpType.Info);
                patchComm = string.Empty;
            }
        }
        
        _parentWindow.ReturnToMainView(this, patchComm);
    }

    private void BtnDiscPatch_OnClick(object? sender, RoutedEventArgs e) => _parentWindow.ReturnToMainView();
}