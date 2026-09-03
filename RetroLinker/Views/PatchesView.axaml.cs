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
        var buildingLink = _parentWindow.PermaView?.BuildingLink ?? new Shortcutter();
        CoreContent = buildingLink.ROMdir;
        _patchString = buildingLink.PatchArg;
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

        rdoUPSPatch.Tag = CommandManager.UpsPatch;
        rdoBPSPatch.Tag = CommandManager.BpsPatch;
        rdoIPSPatch.Tag = CommandManager.IpsPatch;
        rdoXDPatch.Tag = CommandManager.XdPatch;
        rdoNoPatch.Tag = CommandManager.NoPatch;
        chkNoPatch.Tag = CommandManager.ExNoPatch;
        
        if (string.IsNullOrEmpty(_patchString)) return;
        try
        {
            var (file, patchType) = CommandManager.ResolveSoftPatchingArg(_patchString);
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
        fileExtension = fileExtension.Trim('.');
        return fileExtension switch
        {
            CommandManager.UpsExt => CommandManager.UpsPatch, 
            CommandManager.BpsExt => CommandManager.BpsPatch,
            CommandManager.IpsExt => CommandManager.IpsPatch, 
            CommandManager.XdExt => CommandManager.XdPatch,
            _ => CommandManager.UpsPatch
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
        SoftPatch selectedPatch = CommandManager.NoPatch;
        foreach (var radioButton in _patchRadioButtons)
        {
            if (radioButton.Tag is not SoftPatch softPatch) continue;
            if (!radioButton.IsChecked.GetValueOrDefault()) continue;
            selectedPatch = softPatch;
            break;
        }
        
        var patchPath = txtPatchPath.Text ?? string.Empty;
        if (chkNoPatch.IsChecked.GetValueOrDefault()) selectedPatch = CommandManager.ExNoPatch;
        if (rdoAutoPatch.IsChecked.GetValueOrDefault()) selectedPatch = GetPatchTypeByExtension(patchPath);
        patchComm = CommandManager.CreateSoftPatchingArg(patchPath, selectedPatch);
        
        if (string.IsNullOrEmpty(patchPath)) {
            if (!selectedPatch.Equals(CommandManager.NoPatch) && !selectedPatch.Equals(CommandManager.ExNoPatch)) {
                var msBoxContent = new PopUpGenericContent(resMainExtras.popNonSelected_Msg, resMainExtras.popNonSelected_Tittle);
                _ = this.PopUpGenericMessageBox(msBoxContent, GenericPopUpType.Info);
                patchComm = string.Empty;
            }
        }
        
        _parentWindow.ReturnToMainView(this, patchComm);
    }

    private void BtnDiscPatch_OnClick(object? sender, RoutedEventArgs e) => _parentWindow.ReturnToMainView();
}