using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using RetroLinker.Models;
using RetroLinker.Models.Avalonia;

namespace RetroLinker.Views;

public partial class MoreRaOptionsView : UserControl
{
    // Avalonia Props
    public static readonly DirectProperty<MoreRaOptionsView, string?> NewEnvVar_KeyProperty =
        AvaloniaProperty.RegisterDirect<MoreRaOptionsView, string?>(
            nameof(NewEnvVar_Key),
            view => view.NewEnvVar_Key,
            (view, value) => view.NewEnvVar_Key = value);
    public string? NewEnvVar_Key { get; set => SetAndRaise(NewEnvVar_KeyProperty, ref field, value); }
    
    public static readonly DirectProperty<MoreRaOptionsView, string?> NewEnvVar_ValueProperty =
        AvaloniaProperty.RegisterDirect<MoreRaOptionsView, string?>(
            nameof(NewEnvVar_Value),
            view => view.NewEnvVar_Value,
            (view, value) => view.NewEnvVar_Value = value);
    public string? NewEnvVar_Value { get; set => SetAndRaise(NewEnvVar_ValueProperty, ref field, value); }

    public static readonly DirectProperty<MoreRaOptionsView, bool> NewEnvVar_ValidProperty =
        AvaloniaProperty.RegisterDirect<MoreRaOptionsView, bool>(
            nameof(NewEnvVar_Valid),
            view => view.NewEnvVar_Valid,
            (view, value) => view.NewEnvVar_Valid = value);
    [MemberNotNullWhen(returnValue: true, nameof(NewEnvVar_Key))]
    public bool NewEnvVar_Valid { get; set => SetAndRaise(NewEnvVar_ValidProperty, ref field, value); }
    
    // Normal Props
    public IUserLinkParameters LinkParameters { get; }
    public Settings CurrentSettings { get; }
    
    public MoreRaOptionsView()
    {
        // Constructor for Designer
        InitializeComponent();
        _popUpWindow = new PopUpWindow(isDesigner: true);
        LinkParameters = new UserLinkBuilder();
        CurrentSettings = new Settings();
        /* TO ADD
         * -N, --nodevice=PORT          Disconnects controller device connected to PORT (1 to 16).
           -A, --dualanalog=PORT        Connect a DualAnalog controller to PORT (1 to 16).
           -d, --device=PORT:ID         Connect a generic device into PORT of the device (1 to 16).
                                        Format is PORT:ID, where ID is a number corresponding to the particular device.
           -M, --sram-mode=MODE         SRAM handling mode. MODE can be:
                                        'noload-nosave', 'noload-save', 'load-nosave' or 'load-save'.
                                        Note: 'noload-save' implies that save files *WILL BE OVERWRITTEN*.
           --set-shader=PATH            Path to a shader (preset) that will be loaded each time content is loaded.
                                        Effectively overrides automatic shader presets.
                                        An empty argument "" will disable automatic shader presets.
           -e, --entryslot=NUMBER       Slot from which to load an entry state.
         */
        DataContext = this;
    }

    public MoreRaOptionsView(PopUpWindow popUpWindow, MoreOptPopUpHelper moreOptParams)
    {
        InitializeComponent();
        _popUpWindow = popUpWindow;
        LinkParameters = moreOptParams.LinkObject;
        CurrentSettings = moreOptParams.SettingsObject;
        DataContext = this;
    }
    
    // Fields
    private PopUpWindow _popUpWindow;

    private void LockControls(bool value) => ((DockPanel?)Content)?.IsEnabled = !value;
    
    // LogFile
    private void SetRaLogFile(string? logFile) => LinkParameters.RaLogFile = logFile;
    
    private async void BtnRaLogFile_OnClickAsync()
    {
        try
        {
            LockControls(true);
            var currentFile = LinkParameters.RaLogFile ?? string.Empty;
            var file = await FileDialogOps.OpenFileAsync(OpenOpts.Other, _popUpWindow, currentFile);
            if (string.IsNullOrEmpty(file)) return;
            SetRaLogFile(file);
        }
        catch (System.Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    private void BtnRaLogFile_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        switch (button.CommandParameter)
        {
            case TextBox:
                BtnRaLogFile_OnClickAsync();
                break;
            case TextBoxActions action:
                if (action == TextBoxActions.Clear) 
                    LinkParameters.RaLogFile = null;
                break;
        }
    }

    private void ClearNewEnvVars() {
        NewEnvVar_Key = null;
        NewEnvVar_Value = null;
    }

    private async void BtnEnvVars_OnClickAsync()
    {
        try
        {
            LockControls(true);
            var folder = await FileDialogOps.OpenFolderAsync(OpenFolderOpts.Other, _popUpWindow);
            if (string.IsNullOrEmpty(folder)) return;
            NewEnvVar_Value = folder;
        }
        catch (System.Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    private void BtnEnvVars_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        switch (button.CommandParameter)
        {
            case TextBoxActions txtAction:
                switch (txtAction)
                {
                    case TextBoxActions.Browse:
                        BtnEnvVars_OnClickAsync();
                        break;
                    case TextBoxActions.Clear:
                        ClearNewEnvVars();
                        break;
                }
                break;
            case UserListActions lstAction:
                switch (lstAction)
                {
                    case UserListActions.Add:
                        if (!NewEnvVar_Valid) return;
                        LinkParameters.AddEnvironmentVariable(new EnvironmentVariable(NewEnvVar_Key, NewEnvVar_Value ?? string.Empty));
                        ClearNewEnvVars();
                        break;
                    case UserListActions.Clear:
                        LinkParameters.ClearEnvironmentVariables();
                        ClearNewEnvVars();
                        break;
                }
                break;
        }
    }

    private void BtnExit_OnClick(object? sender, RoutedEventArgs e) => _popUpWindow.Close();
}