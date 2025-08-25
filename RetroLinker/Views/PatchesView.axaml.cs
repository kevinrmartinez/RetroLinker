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
    // TODO: Migrate argument parts to Commander class
    private string PatchString;
    private PickerOpt.PatchOpts PatchOpts;
    private List<RadioButton> patchRadioButtons = new();
    
    
    // == LOAD EVENTS ==
    private void PatchView_OnLoaded(object? sender, RoutedEventArgs e)
    {
        string tip = resMainExtras.chkNoPatch_Tip1 + "\n" + resMainExtras.chkNoPatch_Tip2;
        ToolTip.SetTip(chkNoPatch, tip);
        rdoNoPatch.IsChecked = true;    // For proper behavior, rdoNoPatch must always change states on loading

        rdoUPSPatch.Tag = Commander.UpsPatch;
        rdoBPSPatch.Tag = Commander.BpsPatch;
        rdoIPSPatch.Tag = Commander.IpsPatch;
        rdoXDPatch.Tag = Commander.XdPatch;
        rdoNoPatch.Tag = Commander.NoPatch;
        chkNoPatch.Tag = Commander.ExNoPatch;
        
        if (string.IsNullOrEmpty(PatchString)) return;
        try
        {
            (var file, var patchType) = Commander.ResolveSoftPatchingArg(PatchString);
            switch (patchType.PatchType)
            {
                case Commander.PatchType.NoPatch:
                    break;
                case Commander.PatchType.ExNoPatch:
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
        catch (System.ArgumentException argumentException)
        {
            System.Console.WriteLine(argumentException);
            // TODO
            rdoNoPatch.IsChecked = true;
        }
    }
    
    // == PATCHES CONTROLS ==
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
            Commander.PatchType.UPS => PickerOpt.PatchOpts.UPS,
            Commander.PatchType.BPS => PickerOpt.PatchOpts.BPS,
            Commander.PatchType.IPS => PickerOpt.PatchOpts.IPS,
            Commander.PatchType.XDelta => PickerOpt.PatchOpts.XD,
            _ => PickerOpt.PatchOpts.UPS
        };
        
    }

    private async void BtnPatchPath_OnClick(object? sender, RoutedEventArgs e)
    {
        var openOptions = PickerOpt.PatchOpenOptions(PatchOpts);
        string file = await AvaloniaOps.OpenFileAsync(openOptions, ParentWindow);
        if (!string.IsNullOrEmpty(file)) txtPatchPath.Text = file;
    }

    // == VIEW CONTROLS ==
    private void BtnSavePatch_OnClick(object? sender, RoutedEventArgs e)
    {
        string patchComm;
        SoftPatch selectedPatch = Commander.NoPatch;
        foreach (var radioButton in patchRadioButtons)
        {
            if (radioButton.Tag is not SoftPatch softPatch) continue;
            if (!radioButton.IsChecked.GetValueOrDefault()) continue;
            selectedPatch = softPatch;
            break;
        }
        
        if (string.IsNullOrEmpty(txtPatchPath.Text) && !selectedPatch.Equals(Commander.NoPatch))
        {
            var standardParams = new MessageBoxStandardParams()
            {
                WindowIcon = ParentWindow.Icon,
                MaxWidth = 550,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ContentTitle = resMainExtras.popNonSelected_Tittle,
                ContentMessage = resMainExtras.popNonSelected_Msg,
                Icon = MsBox.Avalonia.Enums.Icon.Info
            };
            var msBox = MessageBoxManager.GetMessageBoxStandard(standardParams);
            msBox.ShowWindowDialogAsync(ParentWindow);
            
            patchComm = string.Empty;
        }
        else
            patchComm = selectedPatch.Equals(Commander.NoPatch) switch
            {
                true => selectedPatch.Argument,
                _ => Commander.GetSoftPatchingArg(txtPatchPath.Text!, selectedPatch)
            };

        // ParentWindow.BuildingLink.PatchArg = patchComm;
        ParentWindow.ReturnToMainView(this, patchComm);
    }

    private void BtnDiscPatch_OnClick(object? sender, RoutedEventArgs e) => ParentWindow.ReturnToMainView(this);
}