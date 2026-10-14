/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using LinuxDesktopEntry = RetroLinker.Models.Linux.DesktopEntry;

namespace RetroLinker.Views;

public partial class MainView : UserControl
{
    // Avalonia Properties
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

    // Window Object
    public MainWindow ParentWindow { get; }
    
    // Props
    public Settings Settings { get; private set; }
    public UserLinkBuilder BuildingLink { get; } = new();

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
    private int PreloadedIconsCount;
    private AvaloniaBitmap ICONimage = new(AvaloniaAssetLoader.Open(Operations.GetMissingImage())); // TODO: Test binding on this
    private IconsItems? IconItemSET;
    private bool LinkCustomName;

    // TODO: This has to be a enum or something, as it needs to include macOS. And move to App. (>=0.10)
    //  We can use [SupportedOSPlatform("xxx")] for methods exclusive to xxx platform,
    //  and [SupportedOSPlatformGuard("xxx")] to denote a condition that tells the compiler that the method is guarded
    // https://learn.microsoft.com/en-us/dotnet/standard/analyzers/platform-compat-analyzer
    // true = Windows; false = Linux.
    private readonly bool DesktopOS = System.OperatingSystem.IsWindows();
    
    
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
        catch (System.Exception ex) {
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
        _ = comboICONDir_Loaded();
            
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

    async Task comboICONDir_Loaded()
    {
        comboICONDir.Items.Clear();
        comboICONDir.Items.Add(resMainView.comboDefItem);
        comboICONDir.SelectedIndex++;

        var icons = await Task.Run<(List<string> list, string? error)>(() => FileOps.LoadIcons(DesktopOS));
        // REWRITE: This could be a try-catch
        if (string.IsNullOrEmpty(icons.error)) {
            foreach (var iconFile in icons.list)
                comboICONDir.Items.Add(iconFile);
            Logger.LogInfo("Icons list imported");
        }
        else {
            var message = $"{resMainView.popIconsError_Mess}\n\n{resMainView.popIconsError_Mess2}\n'{icons.error}'";
            var content = new PopUpGenericContent(message, resMainView.popIconsError_Title);
            _ = this.PopUpGenericMessageBox(content, GenericPopUpType.Error);
        }
        
        PreloadedIconsCount = comboICONDir.ItemCount;
        rdoIconDef.IsChecked = true;
    }

    void ApplyDragDropEvents()
    {
        // REWRITE: This can be done with separated border elements
        var borderTransition = new BrushTransition() {
            Property = BorderBrushProperty,
            Duration = System.TimeSpan.FromMilliseconds(250),
        };
        
        AvaloniaTemplatedControl[] templatedControls = [comboICONDir, txtRADir, txtROMDir, comboConfig, txtLINKDir];
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
        txtRADir.MaxLength = 255;
        // Based on current OS
        if (!DesktopOS) {
            if (string.IsNullOrEmpty(Settings.DEFRADir)) Settings.DEFRADir = App.RetroBin;
            txtRADir.IsReadOnly = false;
            // DefLinRAIcon = AvaloniaOps.DefLinRAIcon;
        }
        else IconItemSET = new();

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

    // Icon Boxes
    void FillIconSource(IImage memImage)
    {
        pic16.Source = memImage;
        pic32.Source = memImage;
        pic48.Source = memImage;
        pic64.Source = memImage;
        pic128.Source = memImage;
        pic256.Source = memImage;
    }
    
    void FillIconBoxes(string path) {
        ICONimage.Dispose();
        ICONimage = Operations.GetBitmap(path);
        FillIconSource(ICONimage);
    }
    
    void FillIconBoxes(AvaloniaBitmap bitmap) {
        ICONimage.Dispose();
        ICONimage = bitmap;
        FillIconSource(ICONimage);
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
        return (DesktopOS) ? fileNameNoExt + ext : LinuxDesktopEntry.StdDesktopEntry(fileNameNoExt, core) + ext;
    }
    
    string ValidateLINBin(string RAPath) {
        if (RAPath == txtRADir.Text) return RAPath;
        return string.IsNullOrWhiteSpace(txtRADir.Text) ? string.Empty : txtRADir.Text;
    }

    bool IsRenameInactive() => Settings is { AlwaysAskOutput: true, TileIcoPath: null };
    
    void LockControls(bool lockControls) => this.IsEnabled = !lockControls;
    
    #endregion

    #region Execution
    
    void ResetAfterExecute()
    {
        // if (BuildingLink.OutputPaths.Count == 0 && PreviousOutput.ValidOutput)
        //     BuildingLink.OutputPaths.Add(
        //         ShortcutterOutput.RebuildOutputWithFriendly(PreviousOutput, 
        //             DesktopOS, 
        //             string.Empty));
        
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
                var correctedFileName = LinuxDesktopEntry.StdDesktopEntry(friendlyName, linkBuilder.RaCore) 
                                      + FileOps.GetOutputExt(DesktopOS);
                outputPath = (correctedFileName == linkBuilder.OutputFileName) 
                    ? linkBuilder.OutputPath : FileOps.CombineMultipleInputs(linkBuilder.OutputDir, correctedFileName); 
            }
            else {
                var outputPathStr = FileOps.CombineMultipleInputs(Settings.DEFLinkOutput, userInput +  FileOps.GetOutputExt(DesktopOS));
                outputPath = LinuxDesktopEntry.RenameForDesktopEntry(outputPathStr, linkBuilder.RaCore,  out friendlyName);
            }
            linkBuilder.SetOutputPathAndFriendlyName(outputPath, friendlyName);
        }
    }

    // TODO: This should ONLY alter the icon paths, image conversion should be done later (0.9)
    private async Task HandleFinalIcon(UserLinkBuilder outputLink)
    {
        // 'outputLink.ICONfile' is set when the comboBox changes, this is done in case of conversions and copy
        void UpdateUserIcon(string newPath) {
            outputLink.IconPath = newPath;

            if (IconItemSET is not { ConversionRequired: true, comboIconIndex: { } index }) return;
            // This is for the sake of compatibility with my old code; it will be tighter when Binding is implemented 
            var newIconItem = new IconsItems(newPath, index);
            comboICONDir.SelectedIndex = 0;
            IconProc.IconItemsList[IconProc.IconItemsList.IndexOf(IconItemSET)] = newIconItem;
            comboICONDir.Items.RemoveAt(index);
            comboICONDir.Items.Insert(index, newIconItem.FilePath);
            comboICONDir.SelectedIndex = index;
        }
            
        if (comboICONDir.SelectedIndex == 0) outputLink.IconPath = string.Empty; // RA binary icon (Default)
        else
        {
            if (DesktopOS)
            {
                // If it's Windows, the images may need to be converted to .ico
                if (IconItemSET is { ConversionRequired: true })
                {
                    outputLink.TileIcoImagePath = IconItemSET.FilePath; // Pass the original image as the image for the Tile Icon
                    outputLink.IconPath = FileOps.SaveWinIco(IconItemSET);
                    if (!FileOps.IsFileWinPE(outputLink.IconPath))
                    {
                        string ROMIcoSavAUX = (string.IsNullOrEmpty(outputLink.RaRom))
                            ? outputLink.RaExecutable
                            : outputLink.RaRom;
                        if (ROMIcoSavAUX == Manager.Contentless) ROMIcoSavAUX = outputLink.RaCore;
                        if (Settings.IcoLinkName) UpdateUserIcon(FileOps.ChangeIcoNameToLinkName(outputLink));
                        var newPath = Settings.IcoSavPath switch
                        {
                            SettingsOps.IcoSavROM => FileOps.CpyIconToCustomSet(outputLink.IconPath, ROMIcoSavAUX),
                            SettingsOps.IcoSavRA => FileOps.CpyIconToCustomSet(outputLink.IconPath, outputLink.RaExecutable),
                            _ => FileOps.CpyIconToUsrSet(outputLink.IconPath)
                        };
                        UpdateUserIcon(newPath);
                    }
                }

                if (Settings.TileIcoPath is not null)
                {
                    var imagePath = string.Empty;
                    try {
                        imagePath = await IconProc.ResolveImageForTileico(outputLink.IconPath, outputLink.TileIcoImagePath, IconItemSET);
                        System.ArgumentException.ThrowIfNullOrEmpty(imagePath);
                    }
                    catch (System.Exception ex)
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
            if (Settings.TileIcoPath is null && Settings.CpyUserIcon) UpdateUserIcon(FileOps.CpyIconToUsrSet(outputLink.IconPath));
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
            await HandleFinalIcon(BuildingLink);

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
        catch (System.Exception e) {
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
        catch (System.Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
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
            LockControls(true);
            var opt = DesktopOS ? OpenOpts.WINico : OpenOpts.LINico;
            string currentFile = (comboICONDir.SelectedIndex >= PreloadedIconsCount)
                ? (string)comboICONDir.SelectedItem!
                : string.Empty;
            string file = await FileDialogOps.OpenFileAsync(opt, ParentWindow, currentFile);
            if (string.IsNullOrEmpty(file)) return;
            ICONDir_Set(file);
        }
        catch (System.Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
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
            BuildingLink.IconPath = IconItemSET.FilePath;
            
            if (IconItemSET.IconStream != null) {
                IconItemSET.IconStream.Position = 0;
                var bitmap = Operations.GetBitmap(IconItemSET.IconStream);
                FillIconBoxes(bitmap);
            }
            else {
                try {
                    FillIconBoxes(BuildingLink.IconPath);
                    panelIconNoImage.IsVisible = false;
                }
                catch {
                    AvaloniaBitmap bitmap = new(AvaloniaAssetLoader.Open(Operations.GetMissingImage()));
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

    [System.Obsolete("Replace with binding?")]
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
        catch (System.Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }
    
    void btnRADir_OnClick(object sender, RoutedEventArgs e) => btnRADir_ClickAsync();
    
    #endregion

    #region ROM Controls

    [System.Obsolete("Replace with binding?")]
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
        catch (System.Exception e) { await this.PopUpGenericError(e); }
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
            var newFile = LinuxDesktopEntry.StdDesktopEntry(BuildingLink.FriendlyName + FileOps.GetOutputExt(false), combo.Text);
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
        catch (System.Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }
    
    void btnCONFIGDir_OnClick(object sender, RoutedEventArgs e) => btnCONFIGDir_ClickAsync();
    
    [System.Obsolete("Replace with binding?")]
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

    private async void BtnMoreParams_OnClickAsync() 
    {
        try
        {
            LockControls(true);
            var param = new MoreOptPopUpHelper(BuildingLink, Settings);
            var popUp = new PopUpWindow(PopUpWindowType.MoreRaOptions, param);
            await popUp.ShowDialog(ParentWindow);
        }
        catch (System.Exception e) { await this.PopUpGenericError(e); }
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
                    if (!helper.ValidOutput) throw new System.Exception("Output path could not be resolved.");
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
                // var imposible = 1684 / (comboConfig.Items.Count - 1);
                // Logger.LogDebg(imposible);
                // var readLink = Models.WinClasses.WinShortcutter.ReadShortcut(BuildingLink.OutputPaths[0].FullPath);
            }
#endif
        }
        catch (System.Exception e) { await this.PopUpGenericError(e); }
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
                ? LinuxDesktopEntry.NamePlaceHolder
                : textBox.Text + FileOps.GetOutputExt(DesktopOS);
            var fullPath = FileOps.CombineMultipleInputs(Settings.DEFLinkOutput, properFileName);
            var helper = await ResolveRenamePopUp(fullPath, comboCore.Text);
            if (!helper.ValidOutput) return;
            
            BuildingLink.SetOutputPathAndFriendlyName(helper.NewPath, helper.NewFriendlyName);
            BuildingLink.CustomEntryName = LinkCustomName = helper.CustomEntry;
            textBox.Text = BuildingLink.FriendlyName;
            // FixedOutputName = BuildingLink.OutputFileName;
        }
        catch (System.Exception e) { await this.PopUpGenericError(e); }
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
        ICONimage.Dispose();
    }
    
    void View1_Unloaded(object sender, RoutedEventArgs e)
    {
        // Save changes outside SettingsOps
        SettingsOps.WriteSettings(Settings);
    }
}
