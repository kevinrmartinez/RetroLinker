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

using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using RetroLinker.Models;
using RetroLinker.Models.Avalonia;
using RetroLinker.Models.CommandLine;
using RetroLinker.Models.DesktopLink;
using RetroLinker.Models.IO;
using RetroLinker.Models.Settings;
using RetroLinker.Translations;

using AvaloniaAssetLoader = Avalonia.Platform.AssetLoader;
using AvaloniaTemplatedControl = Avalonia.Controls.Primitives.TemplatedControl;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

using static RetroLinker.Models.Linux.DesktopEntryOps;

namespace RetroLinker.Views;

public partial class MainView : UserControl
{

    #region AvaloniaProperties

    public static readonly DirectProperty<MainView, ViewableIconItem> SelectedIconProperty =
        AvaloniaProperty.RegisterDirect<MainView, ViewableIconItem>(
            nameof(SelectedIcon),
            view => view.SelectedIcon,
            (view, value) => view.SelectedIcon = value);
    
    public ViewableIconItem SelectedIcon { get; set => SetAndRaise(SelectedIconProperty, ref field, value); } = ViewableIconItem.DefaultInstance;

    public static readonly DirectProperty<MainView, bool> CustomIconSourceProperty =
        AvaloniaProperty.RegisterDirect<MainView, bool>(
            nameof(CustomIconSource),
            view => view.CustomIconSource,
            (view, value) => view.CustomIconSource = value);
    
    public bool CustomIconSource
    {
        get;
        set {
            if (!value) SelectedIcon = ViewableIconItem.DefaultInstance;
            SetAndRaise(CustomIconSourceProperty, ref field, value);
        }
    }
    
    public static readonly DirectProperty<MainView, AvaloniaBitmap?> IconImageProperty =
        AvaloniaProperty.RegisterDirect<MainView, AvaloniaBitmap?>(
            nameof(IconImage),
            view => view.IconImage,
            (view, value) => view.IconImage = value);

    public AvaloniaBitmap? IconImage { get; set => SetAndRaise(IconImageProperty, ref field, value); }
    
    public static readonly DirectProperty<MainView, bool> TileIconEnabledProperty =
        AvaloniaProperty.RegisterDirect<MainView, bool>(
            nameof(TileIconEnabled),
            view => view.TileIconAllUsers,
            (view, value) => view.TileIconAllUsers = value);

    public bool TileIconEnabled
    {
        get;
        private set {
            SetAndRaise(TileIconEnabledProperty, ref field, value);
            RaisePropertyChanged(FixedOutputDirProperty, null,null);
        }
    }

    public static readonly DirectProperty<MainView, bool> TileIconAllUsersProperty =
        AvaloniaProperty.RegisterDirect<MainView, bool>(
            nameof(TileIconAllUsers),
            view => view.TileIconAllUsers,
            (view, value) => view.TileIconAllUsers = value);
    public bool TileIconAllUsers
    {
        get;
        set 
        {
            BuildingLink.TileIcoAllUsers = value;   // <= TODO: Replace with Binding
            SetAndRaise(TileIconAllUsersProperty, ref field, value);
            RaisePropertyChanged(FixedOutputDirProperty, null, null);
        }
    }

    /* 
     * This one bellow gets raised by 'TileIconEnabled' and 'TileIconAllUsers' so the control gets updated.
     * My alternatives are subscribing to each observable, or to the view's OnPropertyChanged,
     * and in the end those are the same
     */
    public static readonly DirectProperty<MainView, string?> FixedOutputDirProperty =
        AvaloniaProperty.RegisterDirect<MainView, string?>(
            nameof(FixedOutputDir),
            view => (!view.TileIconEnabled) 
                ? view.Settings.DEFLinkOutput : FileOps.TileIconOutDirs[view.TileIconAllUsers]);

    public string? FixedOutputDir { get; } = null;

    #endregion

    // Window Object
    public MainWindow ParentWindow { get; }
    
    // Props
    public Settings Settings { get; private set; }
    public UserLinkBuilder BuildingLink { get; } = new();
    public ObservableCollection<ViewableIconItem> IconItems { get; } = [ViewableIconItem.DefaultInstance];

    [Obsolete("Replace with Binding of BuildingLink.OutputFileName")]
    public string? FixedOutputName { get; private set; }
    
    
    public MainView()
    {
        // Constructor for Designer
        InitializeComponent();
        ParentWindow = new MainWindow(this);
        _isDesingner = true;
        Settings =  new Settings();
        // Settings.TileIcoPath = "mmmmmmm";
        BuildingLink.RaRomPatch = new RomPatchArgs(RomPatchType.UPS, "\"path/to/romPatch.ups\"");
        BuildingLink.RaCoreSubsystem = new SubsystemArgs("abc", new List<string>() { "\"path/to/rom_a.bin\"", "\"path/to/rom_b.bin\"" });
        BuildingLink.RaAppendedConfig = new ObservableAppendedConfigs() { "/path/to/config1.conf", "/path/to/config2.conf", "/path/to/config3.conf" };
        CompleteSetup();
    }
    
    public MainView(MainWindow mainWindow)
    {
        InitializeComponent();
        ParentWindow = mainWindow;
        Settings = mainWindow.Settings;
        CompleteSetup();
    }
    
    
    // Fields
    private readonly bool _isDesingner;   // Debug
    // private string DefLinRAIcon;
    private bool LinkCustomName;

    // TODO: This has to be a enum or something, as it needs to include macOS. And move to App. (>=0.10)
    //  We can use [SupportedOSPlatform("xxx")] for methods exclusive to xxx platform,
    //  and [SupportedOSPlatformGuard("xxx")] to denote a condition that tells the compiler that the method is guarded
    // https://learn.microsoft.com/en-us/dotnet/standard/analyzers/platform-compat-analyzer
    // true = Windows; false = Linux.
    private readonly bool DesktopOS = OperatingSystem.IsWindows();
    
    
    #region LOAD OPS
    void CompleteSetup()
    {
        BuildingLink.RestoreRemembered(Settings.UserChoices);
        if (!DesktopOS) Settings.TileIcoPath = null; // Disable 'TileIcoPath' when is not Windows
        
        // Implement an Event for theme handling
#if DEBUG
        try {
            ParentWindow.RequestedThemeVariant = LoadThemeVariant();
        }
        catch (Exception ex) {
            Logger.LogDebg(ex);
            TopLevel.GetTopLevel(this)?.RequestedThemeVariant = ThemeVariant.Light;
        }
#else
            ParentWindow.RequestedThemeVariant = LoadThemeVariant();
#endif
            
        SetViewPreSettings();
        
        ApplyDragDropEvents();
        ApplySettingsToControls();
        
        comboConfig_Loaded();
        _ = comboCore_Loaded();
        _ = comboIcon_Loaded();
            
        // Arguments should only load when above controls are ready
        ApplyArgs();

        DataContext = this;

        // TODO_MAYBE: Tutorial event for new users
    }
    

    async Task comboCore_Loaded()
    {
        var cores = await Operations.GetCoresArray();
        if (cores.Length < 1) ToolTip.SetTip(comboCore, resMainView.lblNoCores);
        else comboCore.ItemsSource = cores;
        Logger.LogInfo($"{cores.Length} cores were imported.");
     }

    void comboConfig_Loaded()
    {
        comboConfig.Items.Clear();
        foreach (var config in FileOps.ConfigDir)
            comboConfig.Items.Add(config);
        
        comboConfig.SelectedIndex = 0;
    }

    async Task comboIcon_Loaded()
    {
        try
        {
            var iconsList = await Task.Run(FileOps.LoadIcons);
            foreach (var iconFile in iconsList)
                IconItems.Add(ViewableIconItem.FromIIconItem(iconFile));
            Logger.LogInfo("Icons list imported");
        }
        catch (Exception e)
        {
            Logger.LogErro(e);
            var message = $"{resMainView.popIconsError_Mess}\n\n{resMainView.popIconsError_Mess2}\n'{e.Message}'";
            var content = new PopUpGenericContent(message, resMainView.popIconsError_Title);
            _ = this.PopUpGenericMessageBox(content, GenericPopUpType.Error);
        }
    }

    void ApplyDragDropEvents()
    {
        // REWRITE: This can be done with separated border elements
        var borderTransition = new BrushTransition() {
            Property = BorderBrushProperty,
            Duration = TimeSpan.FromMilliseconds(250),
        };
        
        AvaloniaTemplatedControl[] templatedControls = [comboIcon, txtRADir, txtROMDir, comboConfig, txtLINKDir];
        foreach (var control in templatedControls)
        {
            if (control.Transitions is { } transitions) transitions.Add(borderTransition);
            else control.Transitions = [ borderTransition ];
            DragDrop.SetAllowDrop(control, true);
            DragDrop.AddDragOverHandler(control, ControlBox_DragEnter);
            DragDrop.AddDragLeaveHandler(control, ControlBox_DragLeave);
            DragDrop.AddDropHandler(control, ControlBox_DropParse);
            // https://docs.avaloniaui.net/docs/how-to/drag-and-drop-how-to
        }
    }
    #endregion

    #region Functions
    
    // Props Function
    private void SetSettings(Settings settings) {
        // TODO: This is a temporal workaround, this should be eliminated when a global Settings is implemented
        Settings = settings;
        ParentWindow.SetSettings(settings);
    }

    // Controls Modifiers
    void SetViewPreSettings()
    {
        // Based on current OS
        if (!DesktopOS) {
            if (string.IsNullOrEmpty(Settings.DEFRADir)) Settings.DEFRADir = App.RetroBin;
            txtRADir.IsReadOnly = false;
            // DefLinRAIcon = AvaloniaOps.DefLinRAIcon;
        }

        txtROMDir.IsReadOnly = true;
        comboConfig.IsTextSearchEnabled = false;
    }
    
    void LoadNewSettings() {
        ApplySettingsToControls();
        LoadLocalization();
    }
    
    ThemeVariant LoadThemeVariant()
    {
        ThemeVariant theme = Settings.ChosenTheme switch
        {
            1 => ThemeVariant.Light,
            2 => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
        // CurrentTheme = settings.PreferedTheme;
        Logger.LogInfo($"The requested theme was: {theme}");
        Logger.LogDebg($"Index in byte: {Settings.ChosenTheme}");
        return theme;
    }

    void ApplySettingsToControls()
    {
        // TODO: If done after first load, these line bellow could override whatever the user has set
        if (!string.IsNullOrEmpty(Settings.DEFRADir)) txtRADir.Text = Settings.DEFRADir;
        BuildingLink.RaExecutable = Settings.DEFRADir;
        if (!_isDesingner && !ParentWindow.IsDesigner) {
            Operations.SetROMParentStorageFolder(Settings.DEFROMPath, ParentWindow);
            Operations.SetIconParentStorageFolder(Settings.IconParentPath, ParentWindow);
            Operations.SetDesktopStorageFolder(ParentWindow);
        }
        
        txtLINKDir.PlaceholderText = "Super Mario Bros";
        txtLINKDir.PlaceholderText += FileOps.GetOutputExt(DesktopOS);
        OutputControlsMutations();
    }

    void ApplyArgs()
    {
        // TODO: (0.9) Add argument loading support. 2 Cases:
        // 1. Opening existing links
        // 2. Starting from a ROM
        var args = App.Args;
        if (args is null)  return;
        if (args.Length == 0) Logger.LogDebg("bleh");
        else {
            foreach (var arg in args)
                Logger.LogDebg(arg);
        }
    }

    void LoadLocalization() => ParentWindow.LocaleReload(Settings.LanguageLocale);

    void OutputControlsMutations()
    {
        // REWRITE: can be changed to Bindings when 'Settings' gets rewritten 
        bool askOutput = Settings.AlwaysAskOutput;
        bool tileIconEnabled = Settings.TileIcoPath is not null;
        TileIconEnabled = tileIconEnabled;
        panelLINKRename.IsVisible = !askOutput || tileIconEnabled;
        btnLINKRename.IsVisible = !DesktopOS;   // <= what?
        panelTileIcon.IsVisible = tileIconEnabled;
        // FixedOutputDir = (!tileIconEnabled) ? Settings.DEFLinkOutput : FileOps.TileIconOutDirs[BuildingLink.TileIcoAllUsers];
    }
    
    // Pop-ups
    async Task<RenamePopUpHelper> ResolveRenamePopUp(string givenPath, string? givenCore) {
        var renameParams = new RenamePopUpHelper(givenPath, givenCore);
        var popupWindow = new PopUpWindow(PopUpWindowType.RenameLinux, renameParams);
        return await popupWindow.ShowDialog<RenamePopUpHelper?>(ParentWindow) ?? renameParams;
    }
    
    async Task<bool> OverwriteFilePopUp(string pathToFile)
    {
        var msg = string.Format(resMainView.popOverwrite_Mess, pathToFile);
        var content = new PopUpGenericContent(msg, resMainView.popOverwrite_Title);
        if (FileOps.PathAlreadyExists(pathToFile)) {
            var result = await this.PopUpGenericMessageBox(content, GenericPopUpType.Question);
            return result switch { 
                MsBox.Avalonia.Enums.ButtonResult.No => false,
                _ => true
            };
        }
        else return true;
    }
    
    // External/Call-back
    
    
    // Other Functions
    string UpdateFixedLinkName(string fileNameNoExt, string? core) {
        var ext = FileOps.GetOutputExt(DesktopOS);
        return (DesktopOS) ? fileNameNoExt + ext : StdDesktopEntry(fileNameNoExt, core) + ext;
    }
    
    string ValidateLINBin(string RAPath) {
        if (RAPath == txtRADir.Text) return RAPath;
        return string.IsNullOrWhiteSpace(txtRADir.Text) ? string.Empty : txtRADir.Text;
    }

    bool IsRenameInactive() => Settings is { AlwaysAskOutput: true, TileIcoPath: null };
    
    void LockControls(bool lockControls) => this.IsEnabled = !lockControls;
    
    #endregion

    #region Execution
    
    void ResetAfterExecute() {
        BuildingLink.TileIcoImagePath = null;
        LockControls(false);
    }

    private void HandleLinkOutput(string userInput, UserLinkBuilder linkBuilder)
    {
        if (DesktopOS)
        {
            var outDirectory = (Settings.TileIcoPath is null)
                ? Settings.DEFLinkOutput
                : FileOps.TileIconOutDirs[linkBuilder.TileIcoAllUsers];
            var outputPathStr = (IsRenameInactive())
                ? userInput
                : FileOps.CombineMultipleInputs(outDirectory, userInput +  FileOps.GetOutputExt(DesktopOS));
            linkBuilder.SetOutputPath(outputPathStr);
        }
        else
        {
            if (linkBuilder.CustomEntryName ?? false) return;

            string outputPath;
            string friendlyName;
            if (Settings.AlwaysAskOutput) {
                friendlyName = linkBuilder.FriendlyName;
                var correctedFileName = StdDesktopEntry(friendlyName, linkBuilder.RaCore) 
                                        + FileOps.GetOutputExt(DesktopOS);
                outputPath = (correctedFileName == linkBuilder.OutputFileName) 
                    ? linkBuilder.OutputPath : FileOps.CombineMultipleInputs(linkBuilder.OutputDir, correctedFileName); 
            }
            else {
                var outputPathStr = FileOps.CombineMultipleInputs(Settings.DEFLinkOutput, userInput +  FileOps.GetOutputExt(DesktopOS));
                outputPath = RenameForDesktopEntry(outputPathStr, linkBuilder.RaCore,  out friendlyName);
            }
            linkBuilder.SetOutputPathAndFriendlyName(outputPath, friendlyName);
        }
    }
    
    private async Task SetFinalIconPath(UserLinkBuilder outputLink)
    {    
        if (SelectedIcon.Equals(ViewableIconItem.DefaultInstance)) outputLink.IconPath = string.Empty; // RA binary icon (Default)
        else
        {
            outputLink.IconPath = SelectedIcon.FilePath;    // It's set to the selected icon file by default 
            if (DesktopOS)
            {
                // If it's Windows, the images may need to be converted to .ico
                if (SelectedIcon.ConversionRequired)
                {
                    outputLink.TileIcoImagePath = SelectedIcon.FilePath; // Pass the original image as the image for the Tile Icon
                    outputLink.IconPath = FileOps.GetConvertedIcoPath(SelectedIcon);
                    if (!FileOps.IsFileWinPE(outputLink.IconPath))
                    {
                        var romIcoSave = (string.IsNullOrEmpty(outputLink.RaRom) || outputLink.RaRom == Manager.Contentless)
                            ? outputLink.RaExecutable
                            : outputLink.RaRom;
                        if (Settings.IcoLinkName) {
                            var dir = FileOps.ForceGetDirFromPath(outputLink.IconPath);
                            var newName = FileOps.ChangeFileExtension(outputLink.OutputFileName, FileOps.WinIcoExt);
                            outputLink.IconPath = FileOps.CombineMultipleInputs(dir, newName);
                        }
                        outputLink.IconPath = Settings.IcoSavPath switch
                        {
                            SettingsOps.IcoSavROM => FileOps.RenameIconToCustomSett(outputLink.IconPath, romIcoSave),
                            SettingsOps.IcoSavRA => FileOps.RenameIconToCustomSett(outputLink.IconPath, outputLink.RaExecutable),
                            _ => FileOps.RenameIconToUsrSett(outputLink.IconPath)
                        };
                    }
                }

                if (Settings.TileIcoPath is not null)
                {
                    var imagePath = string.Empty;
                    try {
                        imagePath = await IconProc.ResolveImageForTileico(outputLink.IconPath, outputLink.TileIcoImagePath, SelectedIcon);
                        ArgumentException.ThrowIfNullOrEmpty(imagePath);
                    }
                    catch (Exception ex)
                    {
                        var content = new PopUpGenericContent(resMainView.wrnIcoToImage_mess, 
                            null, resMainView.wrnIcoToImage_header);
                        var ms = await this.PopUpGenericMessageBox(content, GenericPopUpType.Warning);
                        Logger.LogDebg(ms.ToString("G"));
                        Logger.LogErro(ex);
                    }
                    outputLink.TileIcoImagePath = imagePath;
                }
            }
            // If it's Linux, no conversion is required

            // In case of 'CpyUserIcon = true', but it'll be ignored if Tileico is in use 
            if (Settings.TileIcoPath is null && Settings.CpyUserIcon) outputLink.IconPath = FileOps.RenameIconToUsrSett(outputLink.IconPath);
        }
    }

    private (PopUpGenericContent, GenericPopUpType) HandleExecutionReturn(List<LinkWriterResult> results)
    {
        switch (results.Count)
        {
            // Very erroneous behavior
            case 0:
                return (new PopUpGenericContent(resMainView.popNoOutput_Mess), GenericPopUpType.Error);
            // Single Shortcut created
            case 1:
                var onlyResult = results[Index.Start];
                return (!onlyResult.Error) 
                    ? (new PopUpGenericContent(resMainView.popSingleOutput1_Mess), GenericPopUpType.Success)
                    : (new PopUpGenericContent($"{resMainView.popSingleOutput0_Mess}\n{onlyResult.ExMessage}",
                        resMainView.popSingleOutput0_Head), GenericPopUpType.Error);
            // Multiple Shortcuts created
            default:
            {
                bool hasErrors = false;
                foreach (var r in results) {
                    if (!r.Error) continue;
                    hasErrors = true;
                    break;
                }

                if (!hasErrors) {
                    return (new(resMainView.popMultiOutput1_Mess, resGeneric.genSucces),
                        GenericPopUpType.Success);
                }
                else
                {
                    int successCount = 0;
                    string content = string.Empty;
                    foreach (var R in results)
                    {
                        string output = R.OutputPath + ": ";
                        content = string.Concat(content, output);
                        content = string.Concat(content, R.Message);
                        content = string.Concat(content, "\n");
                        if (R.Error)
                        {
                            content = string.Concat(content, $"=> \"{R.ExMessage}\" <=");
                            content = string.Concat(content, "\n");
                        }
                        else successCount++;
                    }
                    return (new PopUpGenericContent(content, null, resMainView.popMultiOutput0_Head),
                        (successCount > 0) ? GenericPopUpType.Warning : GenericPopUpType.Error);
                }
            }
        }
    }

    async void RunExecution()
    {
        try {
            LockControls(true);
            
            // Validate there's an executable (Linux)
            BuildingLink.RaExecutable = ValidateLINBin(BuildingLink.RaExecutable);
            
            // Validating contentless or not
            BuildingLink.RaRom = (chkContentless.IsChecked.GetValueOrDefault()) ? Manager.Contentless : BuildingLink.RaRom;
            
            // Validate there is a core
            BuildingLink.RaCore = (string.IsNullOrWhiteSpace(comboCore.Text)) ? string.Empty : comboCore.Text;
            
            // Link handling
            var linkDir = (IsRenameInactive()) ? txtLINKDir.Text : txtLINKRename.Text;
            if (!string.IsNullOrWhiteSpace(linkDir)) HandleLinkOutput(linkDir, BuildingLink);
            
            // Include a link description, if any
            // outputLink.Desc = txtDesc.Text;
            
            // Icons handling
            await SetFinalIconPath(BuildingLink);

            // REQUIRED FIELDS CHECKS
            PopUpGenericContent popUpContent;
            GenericPopUpType popUpType;
            if ((!string.IsNullOrEmpty(BuildingLink.RaExecutable))
                && (!string.IsNullOrEmpty(BuildingLink.RaRom))
                && (!string.IsNullOrEmpty(BuildingLink.RaCore))
                && (!string.IsNullOrEmpty(BuildingLink.OutputPath)))
            {
                Logger.LogDebg("All fields for link creation have been accepted.");
                var outputLink = new LinkWriter(BuildingLink);

                // Check for overwriting
                if (!IsRenameInactive()) {
                    // If the user selects no, the execution is canceled
                    var overwriteResult = await OverwriteFilePopUp(outputLink.OutputPath);
                    if (!overwriteResult) {
                        ResetAfterExecute();
                        return;
                    }
                }

                if (DesktopOS && SelectedIcon.ConversionRequired)
                {
                    if (!FileOps.IsFileWinPE(outputLink.IconPath))
                    {
                        var wrkIconItem = SelectedIcon;
                        var icoBytes = (FileOps.IsExtWinPE(SelectedIcon.FileExt)) 
                            ? (await wrkIconItem.GetImageStream()).ToArray()
                            : IconProc.VirtuallyCreateIco(await wrkIconItem.GetImageStream());
                        await FileOps.WriteAllBytesToFileAsync(outputLink.IconPath, icoBytes);

                        var index = IconItems.IndexOf(wrkIconItem);
                        if (index >= 0)
                        {
                            SelectedIcon = IconItems[Index.Start];
                            IconItems.RemoveAt(index);
                            wrkIconItem.Dispose();
                            wrkIconItem = new ViewableIconItem(outputLink.IconPath, conversionRequired: false);
                            IconItems.Insert(index, wrkIconItem);
                            SelectedIcon = wrkIconItem;
                        }
                    }
                }

                // Create Shortcuts
                var opsResult = await outputLink.BuildShortcut(DesktopOS);
                (popUpContent, popUpType) = HandleExecutionReturn(opsResult);
            }
            else {
                popUpContent = new(resMainView.popMissReq_Mess, resMainView.popMissReq_Title);
                popUpType = GenericPopUpType.Warning;
            }
            // The collection of IFs fills 'msbox_params', then it's used to Pop-Up a MessageBox
            await this.PopUpGenericMessageBox(popUpContent, popUpType);
            ResetAfterExecute();
        }
        catch (Exception e) {
            await this.PopUpGenericError(e, null, resMainView.popSingleOutput0_Head);
        }
        finally { LockControls(false); }
    }

    #endregion


    // TOP CONTROLS
    async void btnSettings_ClickAsync()
    {
        try {
            LockControls(true);
            var settingWindow = new SettingsWindow(ParentWindow, Settings); 
            var settingReturn =  await settingWindow.ShowDialog<Settings?>(ParentWindow);
            SetSettings((settingReturn is not null) ? FileOps.SetNewSettings(settingReturn) : FileOps.LoadCachedSettingsFO());
            LoadNewSettings();
        }
        catch (Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
    } 

    void btnSettings_OnClick(object sender, RoutedEventArgs e) => btnSettings_ClickAsync();

    private void ButtonAbout_OnClick(object? sender, RoutedEventArgs e) {
        var aboutWindow = new AboutWindow();
        aboutWindow.ShowDialog(ParentWindow);
    }

    #region Icon Controls

    void IconPath_Set(string filePath) {
        var existing = IconItems.FirstOrDefault(i => i.FilePath == filePath);
        if (existing is null) {
            var newIcon = new ViewableIconItem(filePath);
            IconItems.Add(newIcon);
            SelectedIcon = newIcon;
        }
        else {
            SelectedIcon = existing;
            SelectedIcon.ResetBitmap();
        }
    }

    async void btnICONDir_ClickAsync()
    {
        try {
            LockControls(true);
            var opt = DesktopOS ? OpenOpts.WINico : OpenOpts.LINico;
            var currentFile = SelectedIcon.FilePath;
            var file = await FileDialogOps.OpenFileAsync(opt, ParentWindow, currentFile);
            if (string.IsNullOrEmpty(file)) return;
            IconPath_Set(file);
        }
        catch (Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    void btnICONDir_OnClick(object sender, RoutedEventArgs e) => btnICONDir_ClickAsync();

    #endregion
    
    #region RADirectory Controls

    [Obsolete("Replace with binding?")]
    void RADirSet(string filePath) {
        BuildingLink.RaExecutable = filePath;
        txtRADir.Text = filePath;
    }

    async void btnRADir_ClickAsync()
    {
        try {
            LockControls(true);
            OpenOpts opt;
            string currentFile = string.Empty;
            if (DesktopOS) {
                opt = OpenOpts.WinExe;
                currentFile = (string.IsNullOrEmpty(txtRADir.Text)) ? string.Empty : txtRADir.Text;
            }
            else { opt = OpenOpts.LinBin; }
            var dlgTitle = resAvaloniaOps.dlgFileRAexe;
            string file = await FileDialogOps.OpenFileAsync(opt, ParentWindow, currentFile, dlgTitle);
            if (string.IsNullOrEmpty(file)) return;
            RADirSet(file);
        }
        catch (Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }
    
    void btnRADir_OnClick(object sender, RoutedEventArgs e) => btnRADir_ClickAsync();
    
    #endregion

    #region ROM Controls

    [Obsolete("Replace with binding?")]
    void ROMDir_Set(string filePath) {
        BuildingLink.RaRom = filePath;
        txtROMDir.Text = filePath;
    }
    
    void chkContentless_CheckedChanged(object sender, RoutedEventArgs e) {
        panelROMDirControl.IsEnabled = !chkContentless.IsChecked.GetValueOrDefault();
    }

    async void btnROMDir_ClickAsync()
    {
        try {
            LockControls(true);
            string currentFile = (string.IsNullOrEmpty(txtROMDir.Text)) ? string.Empty : txtROMDir.Text;
            string file = await FileDialogOps.OpenFileAsync(OpenOpts.RAroms, ParentWindow, currentFile);
            if (string.IsNullOrEmpty(file)) return;
            ROMDir_Set(file);
        }
        catch (Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }
    
    void btnROMDir_OnClick(object sender, RoutedEventArgs e) => btnROMDir_ClickAsync();
    
    void BtnPatches_OnClick(object? sender, RoutedEventArgs e) {
        ParentWindow.ChangeOut(MainViewTypes.PatchesView);
    }
    #endregion

    #region RACore Controls
    
    [Obsolete("Replace with Binding with BuildingLink.Core, and subscribe to its OnPropertyChanged")]
    private void ComboCore_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not AutoCompleteBox combo) return;
        if (string.IsNullOrWhiteSpace(txtLINKRename.Text) || DesktopOS || LinkCustomName) return;
        
        if (IsRenameInactive())
        {
            var newFile = StdDesktopEntry(BuildingLink.FriendlyName + FileOps.GetOutputExt(false), combo.Text);
            var outputDir = FileOps.GetDirFromPath(BuildingLink.OutputPath);
            if (string.IsNullOrWhiteSpace(outputDir)) outputDir = FileOps.BaseDir;
            txtLINKRename.Text = FileOps.CombineMultipleInputs(outputDir, newFile);
        }
        else FixedOutputName = UpdateFixedLinkName(txtLINKRename.Text, combo.Text);
    }
    
    void btnSubSys_OnClick(object sender, RoutedEventArgs e) {
        ParentWindow.ChangeOut(MainViewTypes.SubsysView);
    }
    
    #endregion

    #region Config Controls

    void comboConfig_Set(string filePath)
    {
        if (!comboConfig.Items.Contains(filePath)) {
            comboConfig.Items.Add(filePath);
            if (Settings.PrevConfig) SettingsOps.PrevConfigs.Add(filePath);
        }
        comboConfig.SelectedItem = filePath;
    }

    async void btnCONFIGDir_ClickAsync()
    {
        try {
            LockControls(true);
            string currentFile = (comboConfig.SelectedIndex > 0) ? (string)comboConfig.SelectedItem! : string.Empty;
            var file = await FileDialogOps.OpenFileAsync(OpenOpts.RAcfg, ParentWindow, currentFile);
            if (string.IsNullOrEmpty(file)) return;
            comboConfig_Set(file);
        }
        catch (Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }
    
    void btnCONFIGDir_OnClick(object sender, RoutedEventArgs e) => btnCONFIGDir_ClickAsync();
    
    [Obsolete("Replace with binding?")]
    void comboConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        BuildingLink.RaConfig = comboConfig.SelectedIndex switch {
            -1 => null,
            0 => string.Empty,
            _ => (string)comboConfig.SelectedItem!
        };
    }
    
    void btnAppendConfig_OnClick(object sender, RoutedEventArgs e) {
        ParentWindow.ChangeOut(MainViewTypes.AppendView);
    }
    #endregion

    #region LinkPath Controls

    /*
     * TODO
     *  Bind txtLINKDir to BuildingLink.OutputPath
     *  Bind txtLINKRename to BuildingLink.FriendlyName
     *  Have comboCore update BuildingLink.OutputFileName when relevant
     */
    
    private async void BtnMoreParams_OnClickAsync() 
    {
        try
        {
            LockControls(true);
            var param = new MoreOptPopUpHelper(BuildingLink, Settings);
            var popUp = new PopUpWindow(PopUpWindowType.MoreRaOptions, param);
            await popUp.ShowDialog(ParentWindow);
        }
        catch (Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }
    
    private void BtnMoreParams_OnClick(object? sender, RoutedEventArgs e) => BtnMoreParams_OnClickAsync();

    async void btnLINKDir_ClickAsync(TextBox textBox)
    {
        try
        {
            LockControls(true);
            var opt = (DesktopOS) ? SaveOpts.WINlnk : SaveOpts.LINdesktop;
            string currentFile = (string.IsNullOrEmpty(textBox.Text)) ? string.Empty : textBox.Text;
            string file = await FileDialogOps.SaveFileAsync(opt, currentFile, ParentWindow);
            if (!string.IsNullOrEmpty(file))
            {
                LinkCustomName = false;
                if (!DesktopOS)
                {
                    var helper = await ResolveRenamePopUp(file, comboCore.Text);
                    if (!helper.ValidOutput) throw new Exception("Output path could not be resolved.");
                    BuildingLink.SetOutputPathAndFriendlyName(helper.NewPath, helper.NewFriendlyName);
                    BuildingLink.CustomEntryName = LinkCustomName = helper.CustomEntry;
                    file = BuildingLink.OutputPath;
                }

                textBox.Text = file;
            }
#if DEBUG
            else
            {
                Logger.LogDebg("Running on debug...");
            }
#endif
        }
        catch (Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    void btnLINKDir_OnClick(object sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        if (btn.CommandParameter is not TextBox txt) return;
        btnLINKDir_ClickAsync(txt);
    }

    async void BtnLINKRename_ClickAsync(TextBox textBox)
    {
        try {
            LockControls(true);
            LinkCustomName = false;
            var properFileName = (string.IsNullOrWhiteSpace(textBox.Text))
                ? NamePlaceHolder
                : textBox.Text + FileOps.GetOutputExt(DesktopOS);
            var fullPath = FileOps.CombineMultipleInputs(Settings.DEFLinkOutput, properFileName);
            var helper = await ResolveRenamePopUp(fullPath, comboCore.Text);
            if (!helper.ValidOutput) return;
            
            BuildingLink.SetOutputPathAndFriendlyName(helper.NewPath, helper.NewFriendlyName);
            BuildingLink.CustomEntryName = LinkCustomName = helper.CustomEntry;
            textBox.Text = BuildingLink.FriendlyName;
            // FixedOutputName = BuildingLink.OutputFileName;
        }
        catch (Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    void BtnLINKRename_OnClick(object? sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        if (btn.CommandParameter is not TextBox txt) return;
        BtnLINKRename_ClickAsync(txt);
    } 

    [Obsolete("Maybe replace with some Binding, not sure")]
    void txtLINKDir_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        if (IsRenameInactive()) return;
        if (LinkCustomName) return;
        FixedOutputName = (!string.IsNullOrWhiteSpace(textBox.Text)) 
            ? UpdateFixedLinkName(textBox.Text, comboCore.Text) : null;
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
        ResetBorderBrush(tc);
    }

    void ControlBox_DropParse(object? sender, DragEventArgs e)
    {
        if (sender is not AvaloniaTemplatedControl tc) return;
        var filesEnum = e.DataTransfer.TryGetFiles();
        var text = e.DataTransfer.TryGetText();
        if (filesEnum is not null && filesEnum.Length > 0) {
            var file = filesEnum[Index.Start];
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() => 
                ControlBox_DropResult(ControlBox_HandleDrop(tc, file.Path.LocalPath), tc)
                );
            return;
        }
        if (!string.IsNullOrWhiteSpace(text)) {
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() => 
                ControlBox_DropResult(ControlBox_HandlePaste(tc, text), tc)
                );
            return;
        }
        Avalonia.Threading.Dispatcher.UIThread.Invoke(() => 
            ControlBox_DropResult(false, tc)
            );
    }
    
    bool ControlBox_HandleDrop(AvaloniaTemplatedControl sender, string droppedPath)
    {
        if (sender.Equals(comboIcon)) {
            if (!FileOps.IsFileAnIcon(droppedPath, DesktopOS, out _)) return false;
            IconPath_Set(droppedPath);
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
        // if (pastedPath.Length > 255) return false;
        if (sender.Equals(comboIcon))
        {
            if (!FileOps.IsFileAnIcon(pastedPath, DesktopOS, out _)) return false;
            IconPath_Set(pastedPath);
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
        await Task.Delay(1000);
        ResetBorderBrush(tc);
    }

    void ResetBorderBrush(AvaloniaTemplatedControl tc) {
        // 'tc.ClearValue(BorderBrushProperty);' should do this, but I'm having mixed results
        // tc.SetValue(BorderBrushProperty, AvaloniaProperty.UnsetValue);
        tc.BorderBrush = txtLINKRename.BorderBrush;
    }
    #endregion
    
    // CLOSING
    public void DisposeResources() {
        foreach (var iconItem in IconItems) iconItem.Dispose();
        IconItems.Clear();
        IconImage?.Dispose();
    }
    
    void View1_Unloaded(object sender, RoutedEventArgs e) {
        // Save changes outside SettingsOps
        SettingsOps.WriteSettings(Settings);
    }
}
