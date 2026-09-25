using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RetroLinker.Views;

public partial class MoreRaOptionsView : UserControl
{
    public MoreRaOptionsView()
    {
        // Constructor for Designer
        InitializeComponent();
        _popUpWindow = new PopUpWindow(isDesigner: true);
        /*
         * Fullscreen
         * Accessibility
         * Load Menu on Error
         * Verbose
         * LogFile
         *
         * Env Vars
         */
        
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
    }

    public MoreRaOptionsView(PopUpWindow popUpWindow, StdPopUpParams stdParams)
    {
        InitializeComponent();
        _popUpWindow = popUpWindow;
        // Handle IUserLinkParameters instead of the shortcut object itself
    }
    
    // Fields
    private PopUpWindow _popUpWindow;
}