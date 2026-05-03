/*
    A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023  Kevin Rafael Martinez Johnston

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
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using RetroLinker.Models;
using RetroLinker.Models.Avalonia;
using RetroLinker.Models.Generic;
using RetroLinker.Models.Linux;
using RetroLinker.Styles;
using RetroLinker.Translations;

using MessageBoxIcons = MsBox.Avalonia.Enums.Icon;
using MessageBoxButtons = MsBox.Avalonia.Enums.ButtonEnum;
using MessageBoxButtonResult = MsBox.Avalonia.Enums.ButtonResult;
using AvaloniaAssetLoader = Avalonia.Platform.AssetLoader;
using AvaloniaTemplatedControl = Avalonia.Controls.Primitives.TemplatedControl;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

namespace RetroLinker.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        // Constructor for Designer
        InitializeComponent();
        DataContext = this;
        ParentWindow = new MainWindow(this);
        IsDesingner = true;
        settings =  new Settings();

        PatchArg = "--ups=\"path/to/rom.bin\"";
        SubsysArg = "--subsystem abc \"path/to/rom.bin\"";
        CONFappend = "--appendconfig \"/path/to/config1.conf|/path/to/config2.conf|/path/to/config3.conf\"";
    }
    
    public MainView(MainWindow mainWindow)
    {
        InitializeComponent();
        DataContext = this;
        ParentWindow = mainWindow;
        settings = mainWindow.Settings;
    }
    
    // Debug
    private bool IsDesingner;
    
    // Window Object
    private MainWindow ParentWindow;
    
    // Props
    public string PatchArg
    {
        get;
        private set
        {
            BuildingLink.PatchArg = value;
            var newValue = string.Empty;
            if (!string.IsNullOrWhiteSpace(value))
            {
                var (path, patch) = Commander.ResolveSoftPatchingArg(BuildingLink.PatchArg);
                if (patch.PatchType is Commander.PatchType.ExNoPatch) path = patch.Argument;
                newValue = path;
            }
            noticePatchPresent.Text = newValue;
            field = newValue;
        }
    } = string.Empty;

    public string SubsysArg
    {
        get;
        set {
            BuildingLink.SubsysArg = value;
            noticeSubsystemPresent.Text = (!string.IsNullOrWhiteSpace(value)) 
                ? Commander.GetArgumentNoOption(value) 
                : string.Empty;
        }
    } = string.Empty;

    public string CONFappend
    {
        get;
        set {
            BuildingLink.CONFappend = value;
            noticeAppendPresent.Text = (!string.IsNullOrWhiteSpace(value)) 
                ? Commander.GetArgumentNoOption(value) 
                : string.Empty;
        }
    } = string.Empty;

    // Fields
    private bool FormFirstLoad = true;
    // private string DefLinRAIcon;
    private int PrevConfigsCount;
    private int PreloadedIconsCount;
    // private byte CurrentTheme = 250;
    private Settings settings;
    private AvaloniaBitmap ICONimage = new(AvaloniaAssetLoader.Open(Operations.GetNAimage()));
    private IconsItems? IconItemSET;
    private Shortcutter BuildingLink = new();
    private bool LinkCustomName;
    private ShortcutterOutput PreviousOutput = new();

    // TODO: This has to be a enum or something, as it needs to include MacOS. (>= 0.8)
    // true = Windows; false = Linux.
    private readonly bool DesktopOS = System.OperatingSystem.IsWindows();
    
    
    #region LOAD EVENTS
    void View1_Loaded(object sender, RoutedEventArgs e)
    {
        // TODO: Evaluate what can be moved from this event function, and move it
        // TODO: Implement an Event for theme handling
        if (FormFirstLoad)
        {
            // AvaloniaOps.MainViewLoad();

#if DEBUG
            try
            {
                ParentWindow.RequestedThemeVariant = LoadThemeVariant();
            }
            catch (System.Exception e_theme) 
            {
                App.Logger?.LogDebg(e_theme);
                TopLevel.GetTopLevel(this)!.RequestedThemeVariant = ThemeVariant.Light;
            }
#else
            ParentWindow.RequestedThemeVariant = LoadThemeVariant();
#endif
        
            
            SetViewPreSettings();

            BuildingLink = new Shortcutter();
            txtLINKDir.PropertyChanged += TxtLINKDir_OnPropertyChanged;
            LoadDragDropEvents();
            ApplySettingsToControls();
            
            comboCore_Loaded(ParentWindow.CoresList);
            comboConfig_Loaded();
            comboICONDir_Loaded(ParentWindow.IconsList);
            
            // Arguments should only load when above controls are ready
            ApplyArgs();
            
            // TODO_MAYBE: Tutorial event for new users
            
            FormFirstLoad = false;
        }
        else LoadNewSettings();
    }

    void comboCore_Loaded(string[] cores)
    {
        if (cores.Length < 1) lblNoCores.IsVisible = true;
        else comboCore.ItemsSource = cores;
        App.Logger?.LogInfo($"{cores.Length} cores were imported.");
     }

    void comboConfig_Loaded()
    {
        comboConfig.Items.Clear();
        foreach (var config in FileOps.ConfigDir)
            comboConfig.Items.Add(config);
        
        comboConfig.SelectedIndex = 0;
    }

    void comboICONDir_Loaded((List<string>, System.Exception?) iconsObject)
    {
        comboICONDir.Items.Clear();
        if (iconsObject.Item2 is null)
        {
            comboICONDir.Items.Add(resMainView.comboDefItem);
            foreach (var iconFile in iconsObject.Item1)
                comboICONDir.Items.Add(iconFile);
            App.Logger?.LogInfo("Icons list imported");

            PreloadedIconsCount = comboICONDir.ItemCount;
            comboICONDir.SelectedIndex++;
            rdoIconDef.IsChecked = true;
        }
        else
        {
            var popParams = new MessageBoxStandardParams()
            {
                ContentTitle = resMainView.popIconsError_Tittle,
                ContentMessage = $"{resMainView.popIconsError_Mess}\n\n{resMainView.popIconsError_Mess2}\n'{iconsObject.Item2.Message}'",
                Icon = MessageBoxIcons.Error,
                ButtonDefinitions = MessageBoxButtons.Ok
            };
            _ = MessageBoxPopUp(popParams);
        }
    }

    void LoadDragDropEvents()
    {
        var borderTransition = new BrushTransition()
        {
            Property = BorderBrushProperty,
            Duration = System.TimeSpan.FromMilliseconds(250),
        };

        AvaloniaTemplatedControl[] templatedControls = [comboICONDir, txtRADir, txtROMDir, comboConfig, txtLINKDir];
        foreach (var control in templatedControls)
        {
            DragDrop.SetAllowDrop(control, true);
            if (control.Transitions != null) control.Transitions.Add(borderTransition);
            else control.Transitions = [borderTransition];
            control.AddHandler(DragDrop.DragEnterEvent, ControlBox_DragEnter);
            control.AddHandler(DragDrop.DragLeaveEvent, ControlBox_DragLeave);
            control.AddHandler(DragDrop.DropEvent, ControlBox_DropParse);
        }
    }
    #endregion

    #region Functions

    // Controls Modifiers
    void SetViewPreSettings()
    {
        txtRADir.MaxLength = 255;
        // Based on current OS
        if (!DesktopOS) {
            if (string.IsNullOrEmpty(settings.DEFRADir)) settings.DEFRADir = App.RetroBin;
            txtRADir.IsReadOnly = false;
            // DefLinRAIcon = AvaloniaOps.DefLinRAIcon;
        }
        else IconItemSET = new();

        txtROMDir.IsReadOnly = true;
        comboConfig.IsTextSearchEnabled = false;
        txtLINKDir.IsReadOnly = true;
    }
    
    void LoadNewSettings()
    {
        // settings = FileOps.LoadCachedSettingsFO();
        ApplySettingsToControls();
        LoadLocalization();
    }
    
    ThemeVariant LoadThemeVariant()
    {
        ThemeVariant theme = settings.ChosenTheme switch
        {
            1 => ThemeVariant.Light,
            2 => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
        // CurrentTheme = settings.PreferedTheme;
        App.Logger?.LogInfo($"The requested theme was: {theme}");
        App.Logger?.LogDebg($"Index in byte: {settings.ChosenTheme}");
        return theme;
    }

    void ApplySettingsToControls()
    {
        if (!string.IsNullOrEmpty(settings.DEFRADir)) txtRADir.Text = settings.DEFRADir;
        BuildingLink.RAdir = settings.DEFRADir;
        if (!IsDesingner && !ParentWindow.IsDesigner) {
            Operations.SetROMTop(settings.DEFROMPath, ParentWindow);
            Operations.SetDesktopStorageFolder(ParentWindow);
        }
        
        PrevConfigsCount = (settings.PrevConfig) ? SettingsOps.PrevConfigs.Count : -1;
        
        txtLINKDir.Watermark = "Super Mario Bros";
        txtLINKDir.Watermark += FileOps.GetOutputExt(DesktopOS);
        AllwaysAskOutputLink(settings.AlwaysAskOutput);
    }

    void ApplyArgs()
    {
        // TODO: (0.9) Add argument loading support. 2 Cases:
        // 1. Opening existing links
        // 2. Starting from a ROM
        var args = App.Args;
        if (args is null)  return;
        if (args.Length == 0) App.Logger?.LogDebg("bleh");
        else {
            foreach (var arg in args)
                App.Logger?.LogDebg(arg);
        }
    }

    void LoadLocalization()
    {
        if (FormFirstLoad) return;
        ParentWindow.LocaleReload(settings.LanguageLocale);
    }

    void AllwaysAskOutputLink(bool ask)
    {
        lblLinkDir.IsVisible = ask;
        txtLINKDir.IsReadOnly = ask;
        txtLINKDir.AcceptsReturn = false;
        
        btnLINKDir.IsEnabled = ask;
        btnLINKRename.IsVisible = (!DesktopOS && !ask);
        lblLinkName.IsVisible = !ask;
        lblLinkDefinedDir.IsVisible = !ask;
        lblLinkDefinedDir.Text = settings.DEFLinkOutput;
    }

    string UpdateLinkLabel(string fileNameNoExt, string? core)
    { 
        return (DesktopOS)
            ? FileOps.GetDefinedLinkPath(fileNameNoExt + FileOps.GetOutputExt(DesktopOS), settings.DEFLinkOutput) 
            : FileOps.GetDefinedLinkPath(DesktopEntry.StdDesktopEntry(fileNameNoExt, core) + FileOps.GetOutputExt(DesktopOS), settings.DEFLinkOutput);
    }
    
    string ValidateLINBin(string RAPath)
    {
        if (RAPath == txtRADir.Text) return RAPath;
        return string.IsNullOrWhiteSpace(txtRADir.Text) ? string.Empty : txtRADir.Text;
    }

    // Icon Boxes
    void FillIconSource(IImage memImage)
    {
        pic16.Source = memImage;
        pic32.Source = memImage;
        pic64.Source = memImage;
        pic128.Source = memImage;
    }
    
    void FillIconBoxes(string path)
    {
        ICONimage = Operations.GetBitmap(path);
        FillIconSource(ICONimage);
    }
    
    void FillIconBoxes(AvaloniaBitmap bitmap)
    {
        ICONimage = bitmap;
        FillIconSource(ICONimage);
    }
    
    // Pop-ups
    async Task<MessageBoxButtonResult> MessageBoxPopUp(MessageBoxStandardParams standardParams)
    {
        if (ParentWindow.Icon is { } icon) standardParams.WindowIcon = icon;
        standardParams.MaxWidth = 550;
        standardParams.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var msBox = MessageBoxManager.GetMessageBoxStandard(standardParams);
        return await msBox.ShowWindowDialogAsync(ParentWindow);
    }
    
    async Task<string> MessageBoxPopUp(MessageBoxCustomParams customParams)
    {
        // Obsolete?
        if (ParentWindow.Icon is { } icon) customParams.WindowIcon = icon;
        customParams.MaxWidth = 600;
        customParams.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var msBox = MessageBoxManager.GetMessageBoxCustom(customParams);
        return await msBox.ShowWindowDialogAsync(ParentWindow);
    }

    async Task<List<ShortcutterOutput>> ResolveRenamePopUp(string givenPath, string? givenCore, List<ShortcutterOutput> outputs)
    {
        var popupWindow = new PopUpWindow();
        popupWindow.RenamePopUp(givenPath, givenCore, outputs);
        return await popupWindow.ShowDialog<List<ShortcutterOutput>>(ParentWindow);
    }
    
    async Task<bool> OverwriteFilePopUp(string pathToFile)
    {
        var stdParams = new MessageBoxStandardParams()
        {
            ContentTitle = "Overwrite file?",
            ContentMessage = $"The file '{pathToFile}' already exist, do you wish to overwrite it?",
            ButtonDefinitions = MessageBoxButtons.YesNo,
            Icon = MessageBoxIcons.Warning
        };
        if (FileOps.PathAlreadyExists(pathToFile)) {
            var result = await MessageBoxPopUp(stdParams);
            return result switch {
                MessageBoxButtonResult.No => false,
                _ => true
            };
        }
        else return true;
    }

    void GenericAsyncErrorPopup(System.Exception error)
    {
        // TODO: Move to Models.Avalonia.OtherDialogs.cs* (0.8)
        App.Logger?.LogErro(error);
        var stdParams = new MessageBoxStandardParams()
        {
            ContentHeader = resGeneric.popUnError_Head0, 
            ContentTitle = resGeneric.genError,
            ContentMessage = $"{resGeneric.popUnError_Mess0}\n{error.Message}",
            Icon = MessageBoxIcons.Error
        };
        _ = MessageBoxPopUp(stdParams);
    } 
    
    // External/Call-back
    public void UpdateLinkFromOutside(MainWindow.ViewsTypes viewType, string longArg)
    {
        switch (viewType)
        {
            case MainWindow.ViewsTypes.PatchesView:
                PatchArg = longArg;
                break;
            case MainWindow.ViewsTypes.SubsysView:
                SubsysArg = longArg;
                break;
            case MainWindow.ViewsTypes.AppendView:
                CONFappend =  longArg;
                break;
            default:
                App.Logger?.LogErro("Unexpected view type at UpdateLinkFromOutside: " + viewType);
                break;
        }
    }
    
    // Execution
    void LockForExecute(bool lockControls) => gridBODY.IsEnabled = !lockControls;

    void ResetAfterExecute()
    {
        if (BuildingLink.OutputPaths.Count == 0)
            BuildingLink.OutputPaths.Add(
                ShortcutterOutput.RebuildOutputWithFriendly(PreviousOutput, 
                    DesktopOS, 
                    string.Empty));
        
        LockForExecute(false);
    }

    async void RunExecution()
    {
        try {
            var OutputLink = new Shortcutter(BuildingLink);
            BuildingLink.OutputPaths = new();

            // Controls Lock
            LockForExecute(true);
            // Avalonia.Threading.Dispatcher.UIThread.Invoke(() => LockForExecute(true), DispatcherPriority.Normal);
            
            // Checkboxes!
            OutputLink.VerboseB = chkVerb.IsChecked.GetValueOrDefault();
            OutputLink.FullscreenB = chkFull.IsChecked.GetValueOrDefault();
            OutputLink.MenuOnErrorB = chkMenuOnError.IsChecked.GetValueOrDefault();
            OutputLink.AccessibilityB = chkAccessi.IsChecked.GetValueOrDefault();

            // Validating contentless or not
            OutputLink.ROMdir = (chkContentless.IsChecked.GetValueOrDefault()) ? Commander.contentless : OutputLink.ROMdir;

            // Validate theres an executable (Linux)
            OutputLink.RAdir = ValidateLINBin(OutputLink.RAdir);

            // Validate there is a core
            OutputLink.ROMcore = (string.IsNullOrWhiteSpace(comboCore.Text)) ? string.Empty : comboCore.Text;

            // Link handling
            if (!string.IsNullOrWhiteSpace(txtLINKDir.Text))
            {
                ShortcutterOutput outputPath;
                if (DesktopOS)
                {
                    var outputPathStr = (!settings.AlwaysAskOutput) 
                        ? FileOps.GetDefinedLinkPath(txtLINKDir.Text + FileOps.GetOutputExt(DesktopOS), settings.DEFLinkOutput) 
                        : txtLINKDir.Text;
                    outputPath = new ShortcutterOutput(outputPathStr);
                }
                else
                {
                    if ((OutputLink.OutputPaths.Count > 0) && OutputLink.OutputPaths[0].CustomEntryName)
                        outputPath = OutputLink.OutputPaths[0];
                    else
                    {
                        if (!settings.AlwaysAskOutput)
                        {
                            var outputPathStr = FileOps.GetDefinedLinkPath(txtLINKDir.Text + FileOps.GetOutputExt(DesktopOS),
                                settings.DEFLinkOutput);
                            outputPath = new ShortcutterOutput(outputPathStr, OutputLink.ROMcore);
                        }
                        else outputPath = ShortcutterOutput.RebuildOutputWithFriendly(OutputLink.OutputPaths[0], DesktopOS, OutputLink.ROMcore);
                    }
                }

                if (OutputLink.OutputPaths.Count == 0) OutputLink.OutputPaths.Add(outputPath);
                else if (OutputLink.OutputPaths[0].FullPath != outputPath.FullPath) OutputLink.OutputPaths[0] = outputPath;
            }
            
            // Include a link description, if any
            OutputLink.Desc = (string.IsNullOrWhiteSpace(txtDesc.Text)) ? string.Empty : txtDesc.Text;

            // Icons handling
            // RA binary icon (Default)
            if (comboICONDir.SelectedIndex == 0) OutputLink.ICONfile = string.Empty;
            else
            {
                // If it's Windows OS, the images may need to be converted to .ico
                if (IconItemSET!.ConversionRequired)
                {
                    OutputLink.ICONfile = FileOps.SaveWinIco(IconItemSET);
                    if (!FileOps.IsFileWinPE(OutputLink.ICONfile))
                    {
                        var ROMIcoSavAUX = (string.IsNullOrEmpty(OutputLink.ROMdir)) ? OutputLink.RAdir : OutputLink.ROMdir;
                        ROMIcoSavAUX = (ROMIcoSavAUX != Commander.contentless) ? ROMIcoSavAUX : OutputLink.ROMcore;
                        if (settings.IcoLinkName) OutputLink.ICONfile = FileOps.ChangeIcoNameToLinkName(OutputLink);
                        OutputLink.ICONfile = settings.IcoSavPath switch
                        {
                            SettingsOps.IcoSavROM => FileOps.CpyIconToCustomSet(OutputLink.ICONfile, ROMIcoSavAUX),
                            SettingsOps.IcoSavRA => FileOps.CpyIconToCustomSet(OutputLink.ICONfile, OutputLink.RAdir),
                            _ => FileOps.CpyIconToUsrSet(OutputLink.ICONfile)
                        };
                    }
                }

                // In case of 'CpyUserIcon = true'
                if (settings.CpyUserIcon) OutputLink.ICONfile = FileOps.CpyIconToUsrSet(OutputLink.ICONfile!);
            }

            // REQUIRED FIELDS CHECKS
            var msboxParams = new MessageBoxStandardParams();
            var outputIsValid = false;
            if (OutputLink.OutputPaths.Count > 0)
                if (OutputLink.OutputPaths[0].ValidOutput) outputIsValid = true;
            if ((!string.IsNullOrEmpty(OutputLink.RAdir))
                && (!string.IsNullOrEmpty(OutputLink.ROMdir))
                && (!string.IsNullOrEmpty(OutputLink.ROMcore))
                && (outputIsValid))
            {
                App.Logger?.LogDebg("All fields for link creation have been accepted.");
                
                // Check for overwriting
                if (!settings.AlwaysAskOutput) {
                    // If the user selects no, the execution process is canceled
                    var overwriteResult = await OverwriteFilePopUp(OutputLink.OutputPaths[0].FullPath);
                    if (!overwriteResult) {
                        ResetAfterExecute();
                        return;
                    }
                }
                
                // Double quotes for directories that are parameters ->
                // -> for the ROM file
                if (!chkContentless.IsChecked.GetValueOrDefault()) 
                { OutputLink.ROMdir = Utils.FixUnusualPaths(OutputLink.ROMdir); }

                // -> for the config file
                if (!string.IsNullOrEmpty(OutputLink.CONFfile)) 
                { OutputLink.CONFfile = Utils.FixUnusualPaths(OutputLink.CONFfile); }

                // Link Copies handling
                if (settings.MakeLinkCopy)
                    OutputLink.OutputPaths.AddRange(FileOps.GetLinkCopyPaths(SettingsOps.LinkCopyPaths, OutputLink.OutputPaths[0]));
                PreviousOutput = OutputLink.OutputPaths[0];
                
                // Create Shortcuts
                List<ShortcutterResult> opResult = Shortcutter.BuildShortcut(OutputLink, DesktopOS);
                // Single Shortcut
                if (opResult.Count == 1)
                {
                    if (!opResult[0].Error)
                    {
                        msboxParams.ContentMessage = resMainView.popSingleOutput1_Mess;
                        msboxParams.ContentTitle = resGeneric.genSucces;
                        msboxParams.Icon = MessageBoxIcons.Success;
                    }   
                    else
                    {
                        msboxParams.ContentHeader = resMainView.popSingleOutput0_Head; 
                        msboxParams.ContentTitle = resGeneric.genError;
                        msboxParams.ContentMessage = $"{resMainView.popSingleOutput0_Mess}\n{opResult[0].eMesseage}";
                        msboxParams.Icon = MessageBoxIcons.Error;
                    }
                }
                // Multiple Shortcut
                else
                {
                    bool hasErrors = false;
                    foreach (var r in opResult) {
                        if (r.Error) hasErrors = true;
                        break;
                    }

                    if (!hasErrors) {
                        msboxParams.ContentMessage = resMainView.popMultiOutput1_Mess; 
                        msboxParams.ContentTitle = resGeneric.genSucces;
                        msboxParams.Icon = MessageBoxIcons.Success;
                    }
                    else
                    {
                        msboxParams.ContentHeader = resMainView.popMultiOutput0_Head;
                        int successCount = 0;
                        string content = string.Empty;
                        foreach (var R in opResult)
                        {
                            string output = R.OutputPath + ": ";
                            content = string.Concat(content, output);
                            content = string.Concat(content, R.Messeage);
                            content = string.Concat(content, "\n");
                            if (R.Error)
                            {
                                content = string.Concat(content, $"=> \"{R.eMesseage}\" <=");
                                content = string.Concat(content, "\n");
                            }
                            else successCount++;
                        }
                        msboxParams.ContentTitle = resGeneric.genWarning;
                        msboxParams.Icon = (successCount > 0) ? MessageBoxIcons.Warning : MessageBoxIcons.Error;
                        msboxParams.ContentMessage = content;
                    }
                }
            }
            else
            {
                msboxParams.ContentMessage = resMainView.popMissReq_Mess; 
                msboxParams.ContentTitle = resMainView.popMissReq_Title; 
                msboxParams.Icon = MessageBoxIcons.Forbidden;
            }
            // The collection of IFs fills 'msbox_params', then it's used to Pop-Up a MessageBox
            _ = MessageBoxPopUp(msboxParams);
            ResetAfterExecute();
        }
        catch (System.Exception e) {
            App.Logger?.LogErro(e.Message);
            var erroParams = new MessageBoxStandardParams()
            {
                ContentHeader = resMainView.popSingleOutput0_Head, 
                ContentTitle = resGeneric.genError,
                ContentMessage = $"{resMainView.popSingleOutput0_Mess} \n {e.Message}",
                Icon = MessageBoxIcons.Error
            };
            _ = MessageBoxPopUp(erroParams);
        }
    }
    #endregion


    // TOP CONTROLS
    async void btnSettings_ClickAsync()
    {
        try {
            var settingWindow = new SettingsWindow(ParentWindow, settings); 
            var settingReturn =  await settingWindow.ShowDialog<Settings?>(ParentWindow);
            settings = (settingReturn is not null) ? FileOps.SetNewSettings(settingReturn) : FileOps.LoadCachedSettingsFO();
            LoadNewSettings();
        }
        catch (System.Exception e) { GenericAsyncErrorPopup(e); }
    } 
    
    void btnSettings_OnClick(object sender, RoutedEventArgs e) => btnSettings_ClickAsync();
    
    private void ButtonAbout_OnClick(object? sender, RoutedEventArgs e) {
        var aboutWindow = new AboutWindow();
        aboutWindow.ShowDialog(ParentWindow);
    }

    #region Icon Controls

    void ICONDir_Set(string filePath)
    {
        int newIndex = comboICONDir.ItemCount;
        const int indexNotFound = -1;
        int existingItem = IconProc.IconItemsList.IndexOf(IconProc.IconItemsList.Find(item => item.FilePath == filePath)!);
        if (existingItem == indexNotFound)
        {
            comboICONDir.Items.Add(filePath);
            IconProc.BuildIconItem(filePath, newIndex, DesktopOS);
        }
        else newIndex = IconProc.IconItemsList[existingItem].comboIconIndex.GetValueOrDefault();

        comboICONDir.SelectedIndex = newIndex;
    }
    
    void rdoIcon_CheckedChanged(object sender, RoutedEventArgs e)
    {
        if (rdoIconDef.IsChecked.GetValueOrDefault())
        {
            comboICONDir.SelectedIndex = 0;
            gridIconControl.IsEnabled = false;
        }
        else gridIconControl.IsEnabled = true;
    }

    async void btnICONDir_ClickAsync()
    {
        try {
            var opt = DesktopOS ? PickerOpt.OpenOpts.WINico : PickerOpt.OpenOpts.LINico;
            string currentFile = (comboICONDir.SelectedIndex >= PreloadedIconsCount)
                ? (string)comboICONDir.SelectedItem!
                : string.Empty;
            string file = await FileDialogOps.OpenFileAsync(opt, ParentWindow, currentFile);
            if (string.IsNullOrEmpty(file)) return;
            ICONDir_Set(file);
        }
        catch (System.Exception e) { GenericAsyncErrorPopup(e); }
    }
    
    void btnICONDir_OnClick(object sender, RoutedEventArgs e) => btnICONDir_ClickAsync();

    // Solution of SelectionChangedEventArgs thanks to snurre @ stackoverflow.com
    void comboICONDir_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!sender.Equals(comboICONDir)) return;
        int selectedIndex = comboICONDir.SelectedIndex;
        panelIconNoImage.IsVisible = false;
        if (selectedIndex > 0)
        {   // Fill the PictureBoxes with the icons provided by the user
            IconItemSET = IconProc.IconItemsList.Find(item => item.comboIconIndex == selectedIndex)!;
            BuildingLink.ICONfile = IconItemSET.FilePath;
            
            if (IconItemSET.IconStream != null) {
                IconItemSET.IconStream.Position = 0;
                var bitmap = Operations.GetBitmap(IconItemSET.IconStream);
                FillIconBoxes(bitmap);
            }
            else {
                try {
                    FillIconBoxes(BuildingLink.ICONfile); 
                    panelIconNoImage.IsVisible = false;
                }
                catch {
                    AvaloniaBitmap bitmap = new(AvaloniaAssetLoader.Open(Operations.GetNAimage()));
                    FillIconBoxes(bitmap);
                    panelIconNoImage.IsVisible = true;
                } 
            }
        }
        else {   
            AvaloniaBitmap bitmap = new(AvaloniaAssetLoader.Open(Operations.GetDefaultIcon()));
            FillIconBoxes(bitmap);
        }
    }
    #endregion
    
    #region RADirectory Controls

    void RADirSet(string filePath) {
        BuildingLink.RAdir = filePath;
        txtRADir.Text = filePath;
    }

    async void btnRADir_ClickAsync()
    {
        try {
            PickerOpt.OpenOpts opt;
            string currentFile = string.Empty;
            if (DesktopOS) {
                opt = PickerOpt.OpenOpts.RAexe;
                currentFile = (string.IsNullOrEmpty(txtRADir.Text)) ? string.Empty : txtRADir.Text;
            }
            else { opt = PickerOpt.OpenOpts.RAbin; }
            string file = await FileDialogOps.OpenFileAsync(opt, ParentWindow, currentFile);
            if (string.IsNullOrEmpty(file)) return;
            RADirSet(file);
        }
        catch (System.Exception e) { GenericAsyncErrorPopup(e); }
    }
    
    void btnRADir_OnClick(object sender, RoutedEventArgs e) => btnRADir_ClickAsync();
    #endregion

    #region ROM Controls

    void ROMDir_Set(string filePath) {
        BuildingLink.ROMdir = filePath;
        txtROMDir.Text = filePath;
    }
    
    void chkContentless_CheckedChanged(object sender, RoutedEventArgs e) {
        panelROMDirControl.IsEnabled = !chkContentless.IsChecked.GetValueOrDefault();
    }

    async void btnROMDir_ClickAsync()
    {
        try {
            string currentFile = (string.IsNullOrEmpty(txtROMDir.Text)) ? string.Empty : txtROMDir.Text;
            string file = await FileDialogOps.OpenFileAsync(PickerOpt.OpenOpts.RAroms, ParentWindow, currentFile);
            if (string.IsNullOrEmpty(file)) return;
            ROMDir_Set(file);
        }
        catch (System.Exception e) { GenericAsyncErrorPopup(e); }
    }
    
    void btnROMDir_OnClick(object sender, RoutedEventArgs e) => btnROMDir_ClickAsync();
    
    void BtnPatches_OnClick(object? sender, RoutedEventArgs e) {
        ParentWindow.ChangeOut(MainWindow.ViewsTypes.PatchesView, BuildingLink.PatchArg);
    }
    #endregion

    #region RACore Controls
    
    private void ComboCore_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtLINKDir.Text) || DesktopOS || LinkCustomName) return;
        
        string newFile;
        string? outputDir;
        if (settings.AlwaysAskOutput)
        {
            newFile = DesktopEntry.StdDesktopEntry(BuildingLink.OutputPaths[0].FriendlyName + FileOps.GetOutputExt(false), comboCore.Text);
            outputDir = FileOps.GetDirFromPath(BuildingLink.OutputPaths[0].FullPath);
            if (string.IsNullOrWhiteSpace(outputDir)) outputDir = FileOps.BaseDir;
            txtLINKDir.Text = FileOps.CombineDirAndFile(outputDir, newFile);
        }
        else
        {
            newFile = DesktopEntry.StdDesktopEntry(txtLINKDir.Text + FileOps.GetOutputExt(false), comboCore.Text);
            outputDir = settings.DEFLinkOutput;
            lblLinkDefinedDir.Text = FileOps.CombineDirAndFile(outputDir, newFile);
        }
    }
    
    void btnSubSys_OnClick(object sender, RoutedEventArgs e)
    {
        // BuildingLink.ROMcore, BuildingLink.ROMdir,
        var reqs = new SubsystemReq(
            comboCore.Text ?? string.Empty, 
            txtROMDir.Text ?? string.Empty, 
            BuildingLink.SubsysArg);
        ParentWindow.ChangeOut(MainWindow.ViewsTypes.SubsysView, reqs);
    }
    #endregion

    #region Config Controls

    void comboConfig_Set(string filePath)
    {
        if (!comboConfig.Items.Contains(filePath)) {
            comboConfig.Items.Add(filePath);
            if (settings.PrevConfig) SettingsOps.PrevConfigs.Add(filePath);
        }
        comboConfig.SelectedItem = filePath;
    }

    async void btnCONFIGDir_ClickAsync()
    {
        try {
            string currentFile = (comboConfig.SelectedIndex > 0) ? (string)comboConfig.SelectedItem! : string.Empty;
            var file = await FileDialogOps.OpenFileAsync(PickerOpt.OpenOpts.RAcfg, ParentWindow, currentFile);
            if (string.IsNullOrEmpty(file)) return;
            comboConfig_Set(file);
        }
        catch (System.Exception e) { GenericAsyncErrorPopup(e); }
    }
    
    void btnCONFIGDir_OnClick(object sender, RoutedEventArgs e) => btnCONFIGDir_ClickAsync();
    
    void comboConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        BuildingLink.CONFfile = comboConfig.SelectedIndex switch
        {
            -1 => null,
            0 => string.Empty,
            _ => (string)comboConfig.SelectedItem!
        };
    }
    
    void btnAppendConfig_OnClick(object sender, RoutedEventArgs e) {
        ParentWindow.ChangeOut(MainWindow.ViewsTypes.AppendView, BuildingLink.CONFappend);
    }
    #endregion

    #region LinkPath Controls
    private void BtnMoreParams_OnClick(object? sender, RoutedEventArgs e)
    {
        App.Logger?.LogDebg("COMING SOON");
    }

    async void btnLINKDir_ClickAsync()
    {
        try {
            var opt = (DesktopOS) ? PickerOpt.SaveOpts.WINlnk : PickerOpt.SaveOpts.LINdesktop;
            string currentFile = (string.IsNullOrEmpty(txtLINKDir.Text)) ? string.Empty : txtLINKDir.Text;
            string file = await FileDialogOps.SaveFileAsync(opt, currentFile, ParentWindow);
            if (!string.IsNullOrEmpty(file)) {
                LinkCustomName = false;
                if (!DesktopOS)
                {
                    BuildingLink.OutputPaths = await ResolveRenamePopUp(file, comboCore.Text, BuildingLink.OutputPaths);
                    LinkCustomName = BuildingLink.OutputPaths[0].CustomEntryName;
                    file = BuildingLink.OutputPaths[0].FullPath;
                }
                txtLINKDir.Text = file;
            }
#if DEBUG
            else {
                App.Logger?.LogDebg("Running on debug...");
                var imposible = 1684 / (comboConfig.Items.Count - 1);
                App.Logger?.LogDebg(imposible);
                // var readLink = Models.WinClasses.WinShortcutter.ReadShortcut(BuildingLink.OutputPaths[0].FullPath);
            }
#endif
        }
        catch (System.Exception e) { GenericAsyncErrorPopup(e); }
    }
    
    void btnLINKDir_OnClick(object sender, RoutedEventArgs e) => btnLINKDir_ClickAsync();

    async void BtnLINKRename_ClickAsync()
    {
        try {
            LinkCustomName = false;
            var fullPath = FileOps.CombineDirAndFile(
                settings.DEFLinkOutput, 
                (string.IsNullOrWhiteSpace(txtLINKDir.Text) ? DesktopEntry.NamePlaceHolder : txtLINKDir.Text)
            );
            BuildingLink.OutputPaths = await ResolveRenamePopUp(fullPath, comboCore.Text, BuildingLink.OutputPaths);
            if (BuildingLink.OutputPaths.Count == 0) return;
        
            LinkCustomName = BuildingLink.OutputPaths[0].CustomEntryName;
            txtLINKDir.Text = BuildingLink.OutputPaths[0].FriendlyName;
            lblLinkDefinedDir.Text = BuildingLink.OutputPaths[0].FullPath;
        }
        catch (System.Exception e) { GenericAsyncErrorPopup(e); }
    }
    
    void BtnLINKRename_OnClick(object? sender, RoutedEventArgs e) => BtnLINKRename_ClickAsync();

    void txtLINKDir_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (settings.AlwaysAskOutput) return;
        if (BuildingLink.OutputPaths.Count > 0)
            if (BuildingLink.OutputPaths[0].CustomEntryName) return;
        lblLinkDefinedDir.Text = (!string.IsNullOrWhiteSpace(txtLINKDir.Text)) 
            ? UpdateLinkLabel(txtLINKDir.Text, comboCore.Text)
            : settings.DEFLinkOutput;
    }
    
    void TxtLINKDir_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) 
    {
        if (sender is not TextBox textBox) return;
        if (e.Property.Name == "IsReadOnly") textBox.Text = string.Empty;
    }
    #endregion

    
    // EXECUTE
    void btnEXECUTE_OnClick(object sender, RoutedEventArgs e) => RunExecution();

    
    #region Genric Envents

    void ControlBox_DragEnter(object? sender, DragEventArgs e)
    {
        if (sender is not AvaloniaTemplatedControl tc) return;
        tc.BorderBrush = Brushes.Aqua;
    }
    
    void ControlBox_DragLeave(object? sender, DragEventArgs e)
    {
        if (sender is not AvaloniaTemplatedControl tc) return;
        tc.BorderBrush = txtDesc.BorderBrush;
    }

    void ControlBox_DropParse(object? sender, DragEventArgs e)
    {
        if (sender is not AvaloniaTemplatedControl tc) return;
        var filesEnum = e.DataTransfer.TryGetFiles();
        var text = e.DataTransfer.TryGetText();
        if (filesEnum is not null)
        {
            var files = new List<IStorageItem>(filesEnum);
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() => ControlBox_DropResult(ControlBox_HandleDrop(tc, files[0].Path.LocalPath), tc));
            return;
        }
        if (!string.IsNullOrWhiteSpace(text))
        {
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() => ControlBox_DropResult(ControlBox_HandlePaste(tc, text), tc));
            return;
        }
        Avalonia.Threading.Dispatcher.UIThread.Invoke(() => ControlBox_DropResult(false, tc));
    }
    
    bool ControlBox_HandleDrop(AvaloniaTemplatedControl sender, string droppedPath)
    {
        if (sender.Equals(comboICONDir))
        {
            if (!FileOps.IsFileAnIcon(droppedPath, DesktopOS, out _)) return false;
            ICONDir_Set(droppedPath);
        }
        else if (sender.Equals(txtRADir)) RADirSet(droppedPath);
        else if (sender.Equals(txtROMDir)) ROMDir_Set(droppedPath);
        else if (sender.Equals(comboConfig))
        {
            if (!FileOps.IsConfigFile(droppedPath, out _)) return false;
            comboConfig_Set(droppedPath);
        }
        // else if (sender.Equals(txtLINKDir)) later...

        return true;
    }

    bool ControlBox_HandlePaste(AvaloniaTemplatedControl sender, string pastedPath)
    {
        if (pastedPath.Length > 255) return false;
        if (sender.Equals(comboICONDir))
        {
            if (!FileOps.IsFileAnIcon(pastedPath, DesktopOS, out _)) return false;
            ICONDir_Set(pastedPath);
        }
        else if (sender.Equals(txtRADir)) RADirSet(pastedPath);
        else if (sender.Equals(txtROMDir)) ROMDir_Set(pastedPath);
        else if (sender.Equals(comboConfig)) 
        {
            if (!FileOps.IsConfigFile(pastedPath, out _)) return false;
            comboConfig_Set(pastedPath);
        }
        // else if (sender.Equals(txtLINKDir)) later...

        return true;
    }
    
    async Task ControlBox_DropResult(bool accepted, AvaloniaTemplatedControl tc)
    {
        var resultBrush = accepted ? Brushes.LimeGreen : Brushes.Crimson; 
        tc.BorderBrush = resultBrush;
        await Task.Delay(700);
        tc.BorderBrush = txtLINKDir.BorderBrush;
    }
    #endregion
    
    // CLOSING
    void View1_Unloaded(object sender, RoutedEventArgs e)
    {
        var cachedSettings = SettingsOps.GetCachedSettings();
        if ( 
            ((PrevConfigsCount != SettingsOps.PrevConfigs.Count) && (PrevConfigsCount > -1)) 
            || !settings.Equals(cachedSettings)
        ) SettingsOps.WriteSettings(settings);
    }
}
