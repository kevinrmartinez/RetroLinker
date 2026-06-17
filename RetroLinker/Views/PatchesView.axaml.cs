/*
    A .NET GUI application to help create desktop links of games running on RetroArch.
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
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using RetroLinker.Models;
using RetroLinker.Models.Avalonia;
using RetroLinker.Translations;

namespace RetroLinker.Views;

public partial class PatchesView : UserControl
{
    public PatchesView()
    {
        // Constructor for Designer
        InitializeComponent();
        ParentWindow = new MainWindow(true);
        PatchString = string.Empty;
        patchRadioButtons.AddRange([rdoUPSPatch, rdoBPSPatch, rdoIPSPatch, rdoXDPatch, rdoNoPatch]);
    }
    
    public PatchesView(MainWindow mainWindow, string patchString)
    {
        InitializeComponent();
        ParentWindow = mainWindow;
        PatchString = patchString;
        patchRadioButtons.AddRange([rdoUPSPatch, rdoBPSPatch, rdoIPSPatch, rdoXDPatch, rdoNoPatch]);
    }
    
    // == Window Object ==
    private MainWindow ParentWindow;
    
    // == FIELDS ==
    private string PatchString;
    private PatchOpts PatchOpts;
    private List<RadioButton> patchRadioButtons = new();
    
    
    // == LOAD EVENTS ==
    private void PatchView_OnLoaded(object? sender, RoutedEventArgs e)
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
        
        if (string.IsNullOrEmpty(PatchString)) return;
        try
        {
            (var file, var patchType) = CommandManager.ResolveSoftPatchingArg(PatchString);
            switch (patchType.PatchType)
            {
                case ROMPatchType.NoPatch:
                    break;
                case ROMPatchType.ExNoPatch:
                    chkNoPatch.IsChecked = true;
                    break;
                default:
                    var rdo = patchRadioButtons.Find(rdo => ReferenceEquals(rdo.Tag, patchType));
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
        if (radioButton.Tag is not SoftPatch softPatch) return;
        PatchOpts = softPatch.PatchType switch
        {
            ROMPatchType.UPS => PatchOpts.UPS,
            ROMPatchType.BPS => PatchOpts.BPS,
            ROMPatchType.IPS => PatchOpts.IPS,
            ROMPatchType.XDelta => PatchOpts.XD,
            _ => PatchOpts.UPS
        };
        
    }

    private async void BtnPatchPath_ClickAsycn()
    {
        try {
            LockControls(true);
            var openOptions = PickerOpt.PatchOpenOptions(PatchOpts);
            string file = await FileDialogOps.OpenFileAsync(openOptions, ParentWindow);
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
        foreach (var radioButton in patchRadioButtons)
        {
            if (radioButton.Tag is not SoftPatch softPatch) continue;
            if (!radioButton.IsChecked.GetValueOrDefault()) continue;
            selectedPatch = softPatch;
            break;
        }
        
        if (string.IsNullOrEmpty(txtPatchPath.Text) && !selectedPatch.Equals(CommandManager.NoPatch)) {
            var msBoxContent = new PopUpGenericContent(resMainExtras.popNonSelected_Msg, resMainExtras.popNonSelected_Tittle);
            _ = this.PopUpGenericMessageBox(msBoxContent, GenericPopUpType.Info);
            patchComm = string.Empty;
        }
        else
        {
            if (chkNoPatch.IsChecked.GetValueOrDefault()) selectedPatch = CommandManager.ExNoPatch;
            patchComm = selectedPatch.Equals(CommandManager.NoPatch) switch {
                true => selectedPatch.Option,
                _ => CommandManager.CreateSoftPatchingArg(txtPatchPath.Text!, selectedPatch)
            };
        }
        
        ParentWindow.ReturnToMainView(this, patchComm);
    }

    private void BtnDiscPatch_OnClick(object? sender, RoutedEventArgs e) => ParentWindow.ReturnToMainView();
}