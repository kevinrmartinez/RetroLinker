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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using RetroLinker.Models;
using RetroLinker.Models.Avalonia;
using RetroLinker.Models.Generic;
using RetroLinker.Translations;

using AvaloniaAssetLoader = Avalonia.Platform.AssetLoader;
using AvaloniaTemplatedControl = Avalonia.Controls.Primitives.TemplatedControl;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;
using LinuxDesktopEntry = RetroLinker.Models.Linux.DesktopEntry;

namespace RetroLinker.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        // Constructor for Designer
        InitializeComponent();
        ParentWindow = new MainWindow(this);
        IsDesingner = true;
        Settings =  new Settings();
        CompleteSetup();

        PatchArg = "--ups=\"path/to/rom.bin\"";
        SubsysArg = "--subsystem=abc \"path/to/rom.bin\"";
        CONFappend = "--appendconfig=\"/path/to/config1.conf|/path/to/config2.conf|/path/to/config3.conf\"";
    }
    
    public MainView(MainWindow mainWindow)
    {
        InitializeComponent();
        ParentWindow = mainWindow;
        Settings = mainWindow.Settings;
        CompleteSetup();
    }
    
    // Debug
    private bool IsDesingner;
    
    // Window Object
    public MainWindow ParentWindow { get; }
    
    // Props
    public Settings Settings { get; private set; }
    public Shortcutter BuildingLink { get; } = new();
    public string PatchArg {
        get;
        private set => field = SetPatchArg(value);
    } = string.Empty;
    public string SubsysArg {
        get;
        set => field = SetSubsysArg(value);
    } = string.Empty;
    public string CONFappend {
        get;
        set => field = SetCONFappend(value);
    } = string.Empty;
    public bool TileIconAllUsers {
        get; 
        set => field = SetTileIconAllUsers(value);
    }
    
    public string? FixedOutputDir { get; private set; }
    public string? FixedOutputName { get; private set; }

    // Fields
    // private bool FormFirstLoad = true;
    // private string DefLinRAIcon;
    private int PrevConfigsCount;
    private int PreloadedIconsCount;
    // private byte CurrentTheme = 250;
    private AvaloniaBitmap ICONimage = new(AvaloniaAssetLoader.Open(Operations.GetNAimage()));
    private IconsItems? IconItemSET;
    // private Shortcutter BuildingLink = new();
    private bool LinkCustomName;
    private ShortcutterOutput PreviousOutput = new();

    // TODO: This has to be a enum or something, as it needs to include MacOS. And move to App. (>=0.10)
    //  We can use [SupportedOSPlatform("xxx")] for methods exclusive to xxx platform,
    //  and [SupportedOSPlatformGuard("xxx")] to denote a condition that tells the compiler that the method is guarded
    // https://learn.microsoft.com/en-us/dotnet/standard/analyzers/platform-compat-analyzer
    // true = Windows; false = Linux.
    private readonly bool DesktopOS = System.OperatingSystem.IsWindows();
    
    
    #region LOAD EVENTS
    void CompleteSetup()
    {
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
        
        UpdateContext();
            
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
    
    private string SetPatchArg(string value)
    {
        BuildingLink.PatchArg = value;
        var newValue = string.Empty;
        if (!string.IsNullOrWhiteSpace(value))
        {
            var (path, patch) = CommandManager.ResolveSoftPatchingArg(BuildingLink.PatchArg);
            if (patch.PatchType is ROMPatchType.ExNoPatch) path = patch.Option;
            newValue = path;
        }
        noticePatchPresent.Text = newValue;
        return newValue;
    }

    private string SetSubsysArg(string value)
    {
        noticeSubsystemPresent.Text = (!string.IsNullOrWhiteSpace(value)) 
            ? CommandManager.GetArgumentNoOption(value) 
            : string.Empty;
        BuildingLink.SubsysArg = value;
        return value;
    }

    private string SetCONFappend(string value)
    {
        BuildingLink.CONFappend = value;
        noticeAppendPresent.Text = (!string.IsNullOrWhiteSpace(value)) 
            ? CommandManager.GetArgumentNoOption(value) 
            : string.Empty;
        return value;
    }

    private bool SetTileIconAllUsers(bool value) {
        BuildingLink.TileIcoAllUsers = value;
        FixedOutputDir = FileOps.TileIconOutDirs[value];
        return value;
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
        if (!string.IsNullOrEmpty(Settings.DEFRADir)) txtRADir.Text = Settings.DEFRADir;
        BuildingLink.RAdir = Settings.DEFRADir;
        if (!IsDesingner && !ParentWindow.IsDesigner) {
            Operations.SetROMTop(Settings.DEFROMPath, ParentWindow);
            Operations.SetDesktopStorageFolder(ParentWindow);
        }
        
        PrevConfigsCount = (Settings.PrevConfig) ? SettingsOps.PrevConfigs.Count : -1;
        
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

    void LoadLocalization() {
        ParentWindow.LocaleReload(Settings.LanguageLocale);
    }

    void OutputControlsMutations()
    {
        bool askOutput = Settings.AlwaysAskOutput;
        bool tileIconEnabled = Settings.TileIcoPath is not null;
        panelLINKRename.IsVisible = !askOutput || tileIconEnabled;
        btnLINKRename.IsVisible = !DesktopOS;
        panelTileIcon.IsVisible = tileIconEnabled;
        FixedOutputDir = (!tileIconEnabled) ? Settings.DEFLinkOutput : FileOps.TileIconOutDirs[BuildingLink.TileIcoAllUsers];
    }

    void UpdateContext() {
        DataContext = null;
        DataContext = this;
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
        ICONimage = Operations.GetBitmap(path);
        FillIconSource(ICONimage);
    }
    
    void FillIconBoxes(AvaloniaBitmap bitmap) {
        ICONimage = bitmap;
        FillIconSource(ICONimage);
    }
    
    // Pop-ups
    async Task<List<ShortcutterOutput>> ResolveRenamePopUp(string givenPath, string? givenCore, List<ShortcutterOutput> outputs) {
        var popupWindow = new PopUpWindow();
        popupWindow.RenamePopUp(givenPath, givenCore, outputs);
        return await popupWindow.ShowDialog<List<ShortcutterOutput>?>(ParentWindow) ?? new List<ShortcutterOutput>();
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
    
    void LockControls(bool lockControls) {
        gridBODY.IsEnabled = !lockControls;
        UpdateContext();
    }
    
    #endregion

    #region Execution
    
    void ResetAfterExecute()
    {
        if (BuildingLink.OutputPaths.Count == 0 && PreviousOutput.ValidOutput)
            BuildingLink.OutputPaths.Add(
                ShortcutterOutput.RebuildOutputWithFriendly(PreviousOutput, 
                    DesktopOS, 
                    string.Empty));
        
        LockControls(false);
    }

    async void RunExecution()
    {
        try {
            LockControls(true);
            var outputLink = (Shortcutter)BuildingLink.Clone();
            outputLink.OutputPaths = new List<ShortcutterOutput>(BuildingLink.OutputPaths);
            BuildingLink.OutputPaths.Clear();
            
            // Controls Lock
            LockControls(true);
            // Avalonia.Threading.Dispatcher.UIThread.Invoke(() => LockForExecute(true), DispatcherPriority.Normal);
            
            // Checkboxes!
            outputLink.VerboseB = chkVerb.IsChecked.GetValueOrDefault();
            outputLink.FullscreenB = chkFull.IsChecked.GetValueOrDefault();
            outputLink.MenuOnErrorB = chkMenuOnError.IsChecked.GetValueOrDefault();
            outputLink.AccessibilityB = chkAccessi.IsChecked.GetValueOrDefault();

            // Validating contentless or not
            outputLink.ROMdir = (chkContentless.IsChecked.GetValueOrDefault()) ? CommandManager.contentless : outputLink.ROMdir;

            // Validate there's an executable (Linux)
            outputLink.RAdir = ValidateLINBin(outputLink.RAdir);

            // Validate there is a core
            outputLink.ROMcore = (string.IsNullOrWhiteSpace(comboCore.Text)) ? string.Empty : comboCore.Text;

            // TODO: Move to a separated method
            // Link handling
            var linkDir = (IsRenameInactive()) ? txtLINKDir.Text : txtLINKRename.Text;
            if (!string.IsNullOrWhiteSpace(linkDir))
            {
                ShortcutterOutput outputPath;
                if (DesktopOS)
                {
                    var outDirectory = (Settings.TileIcoPath is null)
                        ? Settings.DEFLinkOutput
                        : FileOps.TileIconOutDirs[outputLink.TileIcoAllUsers];
                    var outputPathStr = (IsRenameInactive())
                        ? linkDir
                        : FileOps.GetDefinedLinkPath(linkDir + FileOps.GetOutputExt(DesktopOS), outDirectory);
                    outputPath = new ShortcutterOutput(outputPathStr);
                }
                else
                {
                    if ((outputLink.OutputPaths.Count > 0) && outputLink.OutputPaths.First().CustomEntryName) {
                        outputPath = outputLink.OutputPaths.First();
                    }
                    else
                    {
                        if (!Settings.AlwaysAskOutput) {
                            var outputPathStr = FileOps.GetDefinedLinkPath(linkDir + FileOps.GetOutputExt(DesktopOS),
                                Settings.DEFLinkOutput);
                            outputPath = new ShortcutterOutput(outputPathStr, outputLink.ROMcore);
                        }
                        else outputPath = ShortcutterOutput.RebuildOutputWithFriendly(outputLink.OutputPaths.First(), DesktopOS, outputLink.ROMcore);
                    }
                }

                if (outputLink.OutputPaths.Count == 0) outputLink.OutputPaths.Add(outputPath);
                else if (outputLink.OutputPaths.First().FullPath != outputPath.FullPath)
                {
                    // This is just to not use fixed array positions...
                    // Alternative: OutputLink.OutputPaths[0] = outputPath;
                    var oldOutputIndex = outputLink.OutputPaths.IndexOf(outputLink.OutputPaths.First());
                    outputLink.OutputPaths[oldOutputIndex] = outputPath;
                }
            }
            
            // Include a link description, if any
            outputLink.Desc = txtDesc.Text;

            // TODO: Move to a separated method
            // Icons handling
            void UpdateUserIcon(string newPath) {
                outputLink.ICONfile = newPath;
                
                if (IconItemSET is { ConversionRequired: true, comboIconIndex: { } index })
                {
                    // This is for the sake of compatibility with my old code; it will be tighter when Binding is implemented 
                    var newIconItem = new IconsItems(newPath, index);
                    comboICONDir.SelectedIndex = 0;
                    IconProc.IconItemsList[IconProc.IconItemsList.IndexOf(IconItemSET)] = newIconItem;
                    comboICONDir.Items.RemoveAt(index);
                    comboICONDir.Items.Insert(index, newIconItem.FilePath);
                    comboICONDir.SelectedIndex = index;
                }
            }
            
            if (comboICONDir.SelectedIndex == 0) outputLink.ICONfile = string.Empty; // RA binary icon (Default)
            else
            {
                if (DesktopOS)
                {
                    // If it's Windows, the images may need to be converted to .ico
                    if (IconItemSET is { ConversionRequired: true })
                    {
                        outputLink.TileIcoImage = IconItemSET.FilePath;     // Pass the original image as the image for the Tile Icon
                        outputLink.ICONfile = FileOps.SaveWinIco(IconItemSET);
                        if (!FileOps.IsFileWinPE(outputLink.ICONfile))
                        {
                            string ROMIcoSavAUX = (string.IsNullOrEmpty(outputLink.ROMdir)) ? outputLink.RAdir : outputLink.ROMdir;
                            if (ROMIcoSavAUX == CommandManager.contentless) ROMIcoSavAUX = outputLink.ROMcore;
                            if (Settings.IcoLinkName) UpdateUserIcon(FileOps.ChangeIcoNameToLinkName(outputLink));
                            var newPath = Settings.IcoSavPath switch
                            {
                                SettingsOps.IcoSavROM => FileOps.CpyIconToCustomSet(outputLink.ICONfile, ROMIcoSavAUX),
                                SettingsOps.IcoSavRA => FileOps.CpyIconToCustomSet(outputLink.ICONfile, outputLink.RAdir),
                                _ => FileOps.CpyIconToUsrSet(outputLink.ICONfile)
                            };
                            UpdateUserIcon(newPath);
                        }
                    }
                    else if (IconItemSET is { ConversionRequired: false } && Settings.TileIcoPath is not null)
                    {
                        var imageFromIco = string.Empty;
                        try {
                            var image = IconProc.ReverseImageConvert(IconItemSET.FilePath);
                            imageFromIco = FileOps.WriteImageToTemp(image, IconItemSET.FileName);
                        }
                        catch (System.Exception ex) {
                            var content = new PopUpGenericContent(resMainView.wrnIcoToImage_mess, 
                                null, resMainView.wrnIcoToImage_header);
                            var ms = await this.PopUpGenericMessageBox(content, GenericPopUpType.Warning);
                            Logger.LogDebg(ms.ToString("G"));
                            Logger.LogErro(ex);
                        }
                        finally { outputLink.TileIcoImage = imageFromIco; }
                    }
                }
                // If it's Linux, no conversion is required

                // In case of 'CpyUserIcon = true'
                if (Settings.CpyUserIcon) UpdateUserIcon(FileOps.CpyIconToUsrSet(outputLink.ICONfile));
            }

            // REQUIRED FIELDS CHECKS
            // var msboxParams = new MessageBoxStandardParams();
            PopUpGenericContent popUpContent;
            GenericPopUpType popUpType;
            var outputIsValid = false;
            if (outputLink.OutputPaths.Count > 0)
                if (outputLink.OutputPaths.First().ValidOutput) outputIsValid = true;
            if ((!string.IsNullOrEmpty(outputLink.RAdir))
                && (!string.IsNullOrEmpty(outputLink.ROMdir))
                && (!string.IsNullOrEmpty(outputLink.ROMcore))
                && (outputIsValid))
            {
                Logger.LogDebg("All fields for link creation have been accepted.");
                
                // Check for overwriting
                if (!IsRenameInactive()) {
                    // If the user selects no, the execution is canceled
                    var overwriteResult = await OverwriteFilePopUp(outputLink.OutputPaths.First().FullPath);
                    if (!overwriteResult) {
                        ResetAfterExecute();
                        return;
                    }
                }
                
                // Double quotes for directories that are parameters ->
                // -> for the ROM file
                if (!chkContentless.IsChecked.GetValueOrDefault()) 
                { outputLink.ROMdir = Utils.PutPathBetweenQuotes(outputLink.ROMdir); }

                // -> for the config file
                if (!string.IsNullOrEmpty(outputLink.CONFfile)) 
                { outputLink.CONFfile = Utils.PutPathBetweenQuotes(outputLink.CONFfile); }

                // Link Copies handling
                if (Settings.MakeLinkCopy)
                    outputLink.OutputPaths.AddRange(FileOps.GetLinkCopyPaths(SettingsOps.LinkCopyPaths, outputLink.OutputPaths.First()));
                PreviousOutput = outputLink.OutputPaths.First();
                
                // Create Shortcuts
                List<ShortcutterResult> opResult = await outputLink.BuildShortcut(DesktopOS);
                // TODO: Move to a separated method, maybe
                // Single Shortcut created
                if (opResult.Count == 1)
                {
                    (popUpContent, popUpType) = (!opResult.First().Error) 
                        ? (new PopUpGenericContent(resMainView.popSingleOutput1_Mess), GenericPopUpType.Success)
                        : (new PopUpGenericContent($"{resMainView.popSingleOutput0_Mess}\n{opResult.First().ExMessage}",
                            resMainView.popSingleOutput0_Head), GenericPopUpType.Error);
                }
                // Multiple Shortcuts created
                else
                {
                    bool hasErrors = false;
                    foreach (var r in opResult) {
                        if (r.Error) hasErrors = true;
                        break;
                    }

                    if (!hasErrors) {
                        popUpContent = new(resMainView.popMultiOutput1_Mess, resGeneric.genSucces);
                        popUpType = GenericPopUpType.Success;
                    }
                    else
                    {
                        int successCount = 0;
                        string content = string.Empty;
                        foreach (var R in opResult)
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
                        popUpContent = new(content, null, resMainView.popMultiOutput0_Head);
                        popUpType = (successCount > 0) ? GenericPopUpType.Warning : GenericPopUpType.Error;
                    }
                }
            }
            else {
                popUpContent = new(resMainView.popMissReq_Mess, resMainView.popMissReq_Title);
                popUpType = GenericPopUpType.Warning;
            }
            // The collection of IFs fills 'msbox_params', then it's used to Pop-Up a MessageBox
            _ = this.PopUpGenericMessageBox(popUpContent, popUpType);
            ResetAfterExecute();
        }
        catch (System.Exception e) {
            _ = this.PopUpGenericError(e, null, resMainView.popSingleOutput0_Head);
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
        catch (System.Exception e) { _ = this.PopUpGenericError(e); }
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
        catch (System.Exception e) { _ = this.PopUpGenericError(e); }
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
            LockControls(true);
            OpenOpts opt;
            string currentFile = string.Empty;
            if (DesktopOS) {
                opt = OpenOpts.RAexe;
                currentFile = (string.IsNullOrEmpty(txtRADir.Text)) ? string.Empty : txtRADir.Text;
            }
            else { opt = OpenOpts.RAbin; }
            string file = await FileDialogOps.OpenFileAsync(opt, ParentWindow, currentFile);
            if (string.IsNullOrEmpty(file)) return;
            RADirSet(file);
        }
        catch (System.Exception e) { _ = this.PopUpGenericError(e); }
        finally { LockControls(false); }
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
            LockControls(true);
            string currentFile = (string.IsNullOrEmpty(txtROMDir.Text)) ? string.Empty : txtROMDir.Text;
            string file = await FileDialogOps.OpenFileAsync(OpenOpts.RAroms, ParentWindow, currentFile);
            if (string.IsNullOrEmpty(file)) return;
            ROMDir_Set(file);
        }
        catch (System.Exception e) { _ = this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }
    
    void btnROMDir_OnClick(object sender, RoutedEventArgs e) => btnROMDir_ClickAsync();
    
    void BtnPatches_OnClick(object? sender, RoutedEventArgs e) {
        ParentWindow.ChangeOut(MainViewTypes.PatchesView);
    }
    #endregion

    #region RACore Controls
    
    private void ComboCore_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not AutoCompleteBox combo) return;
        if (string.IsNullOrWhiteSpace(txtLINKRename.Text) || DesktopOS || LinkCustomName) return;
        
        string newFile;
        string? outputDir;
        if (IsRenameInactive())
        {
            newFile = LinuxDesktopEntry.StdDesktopEntry(BuildingLink.OutputPaths.First().FriendlyName + FileOps.GetOutputExt(false), combo.Text);
            outputDir = FileOps.GetDirFromPath(BuildingLink.OutputPaths.First().FullPath);
            if (string.IsNullOrWhiteSpace(outputDir)) outputDir = FileOps.BaseDir;
            txtLINKRename.Text = FileOps.CombineMultipleInputs(outputDir, newFile);
        }
        else FixedOutputName = UpdateFixedLinkName(txtLINKRename.Text, combo.Text);
        UpdateContext();
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
        catch (System.Exception e) { _ = this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }
    
    void btnCONFIGDir_OnClick(object sender, RoutedEventArgs e) => btnCONFIGDir_ClickAsync();
    
    void comboConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        BuildingLink.CONFfile = comboConfig.SelectedIndex switch {
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
    private void BtnMoreParams_OnClick(object? sender, RoutedEventArgs e) {
        Logger.LogDebg("COMING SOON");
    }

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
                    BuildingLink.OutputPaths = await ResolveRenamePopUp(file, comboCore.Text, BuildingLink.OutputPaths);
                    LinkCustomName = BuildingLink.OutputPaths.First().CustomEntryName;
                    file = BuildingLink.OutputPaths.First().FullPath;
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
        catch (System.Exception e) { _ = this.PopUpGenericError(e); }
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
            BuildingLink.OutputPaths = await ResolveRenamePopUp(fullPath, comboCore.Text, BuildingLink.OutputPaths);
            if (BuildingLink.OutputPaths.Count == 0) return;
        
            LinkCustomName = BuildingLink.OutputPaths.First().CustomEntryName;
            textBox.Text = BuildingLink.OutputPaths.First().FriendlyName;
            FixedOutputName = BuildingLink.OutputPaths.First().FileName;
        }
        catch (System.Exception e) { _ = this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    void BtnLINKRename_OnClick(object? sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        if (btn.CommandParameter is not TextBox txt) return;
        BtnLINKRename_ClickAsync(txt);
    } 

    void txtLINKDir_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        if (IsRenameInactive()) return;
        if (BuildingLink.OutputPaths.Count > 0)
            if (BuildingLink.OutputPaths.First().CustomEntryName) return;
        FixedOutputName = (!string.IsNullOrWhiteSpace(textBox.Text)) 
            ? UpdateFixedLinkName(textBox.Text, comboCore.Text) : null;
        UpdateContext();
    }
    
    private void swtTileIcoAllUsers_OnClick(object? sender, RoutedEventArgs e) => UpdateContext();
    
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
        if (filesEnum is not null && filesEnum.Any()) {
            var files = new List<IStorageItem>(filesEnum);
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() => 
                ControlBox_DropResult(ControlBox_HandleDrop(tc, files.First().Path.LocalPath), tc)
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
    void View1_Unloaded(object sender, RoutedEventArgs e)
    {
        // Save new configs
        var cachedSettings = SettingsOps.GetCachedSettings();
        if ( 
            ((PrevConfigsCount != SettingsOps.PrevConfigs.Count) && (PrevConfigsCount > -1)) 
            || !Settings.Equals(cachedSettings)
        ) SettingsOps.WriteSettings(Settings);
    }

    public void DisposeResources() {
        ICONimage.Dispose();
    }
}
