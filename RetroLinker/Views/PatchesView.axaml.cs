using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using RetroLinker.Models;

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
        string tip = Translations.resMainExtras.chkNoPatch_Tip1 + "\n" + Translations.resMainExtras.chkNoPatch_Tip2;
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

    // TOP CONTROLS
    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        ParentWindow.ReturnToMainView(this);
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
        if (rdoNoPatch is not null) ControlsEnabled(!(bool)rdoNoPatch.IsChecked);
        var controlName = (e.Source as RadioButton).Name;
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
        
        if (string.IsNullOrEmpty(txtPatchPath.Text) && (bool)!rdoNoPatch.IsChecked)
        {
            var standardParams = new MessageBoxStandardParams()
            {
                WindowIcon = ParentWindow.Icon,
                MaxWidth = 550,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ContentTitle = Translations.resMainExtras.popNonSelected_Tittle,
                ContentMessage = Translations.resMainExtras.popNonSelected_Msg,
                Icon = MsBox.Avalonia.Enums.Icon.Info
            };
            var msBox = MessageBoxManager.GetMessageBoxStandard(standardParams);
            msBox.ShowWindowDialogAsync(ParentWindow);
            
            patchComm = string.Empty;
        }
        else switch ((bool)rdoNoPatch.IsChecked)
        {
            case true when !(bool)chkNoPatch.IsChecked:
                patchComm = string.Empty;
                break;
            case true when (bool)chkNoPatch.IsChecked:
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