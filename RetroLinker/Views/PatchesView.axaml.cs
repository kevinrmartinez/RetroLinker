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
    }
    
    public PatchesView(MainWindow mainWindow, string patchString)
    {
        InitializeComponent();
        ParentWindow = mainWindow;
        PatchString = patchString;
    }
    
    // Window Object
    private MainWindow ParentWindow;
    
    // FIELDS
    // TODO: Migrate argument parts to Commander class
    public const string NoPatch = "--no-patch"; 
    private const string UPatch = "--ups=";
    private const string BPatch = "--bps=";
    private const string IPatch = "--ips=";
    private const string XPatch = "--xdelta=";
    private string PatchString;
    private PickerOpt.PatchOpts PatchOpts;
    
    
    // LOAD EVENTS
    private void PatchView_OnLoaded(object? sender, RoutedEventArgs e)
    {
        string tip = resMainExtras.chkNoPatch_Tip1 + "\n" + resMainExtras.chkNoPatch_Tip2;
        ToolTip.SetTip(chkNoPatch, tip);
        rdoNoPatch.IsChecked = true;    // For proper behavior, rdoNoPatch must always change states on loading
        if (!string.IsNullOrEmpty(PatchString))
        {
            if (PatchString == NoPatch) chkNoPatch.IsChecked = true;
            else
            {
                // rdoNoPatch.IsChecked = false;
                var patchParts = PatchString.Split('=');
                switch (patchParts[0] + '=')
                {
                    case UPatch:
                        rdoUPSPatch.IsChecked = true;
                        break;
                    case BPatch:
                        rdoBPSPatch.IsChecked = true;
                        break;
                    case IPatch:
                        rdoIPSPatch.IsChecked = true;
                        break;
                    case XPatch:
                        rdoXDPatch.IsChecked = true;
                        break;
                    default:
                        rdoUPSPatch.IsChecked = true;
                        break;
                }
                
                txtPatchPath.Text = Utils.ReverseFixUnusualPaths(patchParts[1]);
            }
        }
    }
    
    // PATCHES CONTROLS
    private void ControlsEnabled(bool enable)
    {
        txtPatchPath.IsEnabled = enable;
        btnPatchPath.IsEnabled = enable;
        chkNoPatch.IsChecked = false;
        chkNoPatch.IsEnabled = !enable;
    }
    
    private void ToggleButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (rdoNoPatch is not null) ControlsEnabled(!rdoNoPatch.IsChecked.GetValueOrDefault());
        if (e.Source is not RadioButton radioButton) return;
        var controlName = radioButton.Name;
        // TODO: Use control 'Tag'
        PatchOpts = controlName switch
        {
            "rdoUPSPatch" => PickerOpt.PatchOpts.UPS,
            "rdoBPSPatch" => PickerOpt.PatchOpts.BPS,
            "rdoIPSPatch" => PickerOpt.PatchOpts.IPS,
            "rdoXDPatch" => PickerOpt.PatchOpts.XD,
            _ => PickerOpt.PatchOpts.UPS
        };
        
    }

    private async void BtnPatchPath_OnClick(object? sender, RoutedEventArgs e)
    {
        var openOptions = PickerOpt.PatchOpenOptions(PatchOpts);
        string file = await AvaloniaOps.OpenFileAsync(openOptions, ParentWindow);
        if (!string.IsNullOrEmpty(file)) txtPatchPath.Text = file;
    }

    // BOTTOM CONTROLS
    private void BtnSavePatch_OnClick(object? sender, RoutedEventArgs e)
    {
        string patchComm;
        
        if (string.IsNullOrEmpty(txtPatchPath.Text) && !rdoNoPatch.IsChecked.GetValueOrDefault())
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
        else switch (rdoNoPatch.IsChecked.GetValueOrDefault())
        {
            case true when !chkNoPatch.IsChecked.GetValueOrDefault():
                patchComm = string.Empty;
                break;
            case true when chkNoPatch.IsChecked.GetValueOrDefault():
                patchComm = NoPatch;
                break;
            default:
                PatchString = PatchOpts switch
                {
                    PickerOpt.PatchOpts.UPS => UPatch,
                    PickerOpt.PatchOpts.BPS => BPatch,
                    PickerOpt.PatchOpts.IPS => IPatch,
                    PickerOpt.PatchOpts.XD => XPatch,
                    _ => UPatch
                };
                patchComm = string.Concat(PatchString, txtPatchPath.Text);
                break;
        }

        // ParentWindow.BuildingLink.PatchArg = patchComm;
        ParentWindow.ReturnToMainView(this, patchComm);
    }

    private void BtnDiscPatch_OnClick(object? sender, RoutedEventArgs e) => ParentWindow.ReturnToMainView(this);
}