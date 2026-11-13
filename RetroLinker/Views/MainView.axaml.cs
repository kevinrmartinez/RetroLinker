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
using RetroLinker.Models.Avalonia;
using RetroLinker.Models.CommandLine;
using RetroLinker.Models.DesktopLink;
using RetroLinker.Models.IO;
using RetroLinker.Models.Settings;
using RetroLinker.Translations;

using AvaloniaTemplatedControl = Avalonia.Controls.Primitives.TemplatedControl;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;
// ReSharper disable MemberCanBePrivate.Global

namespace RetroLinker.Views;

public partial class MainView : UserControl
{

    #region AvaloniaProperties

    // == Link Params ==
    public static readonly DirectProperty<MainView, bool> ContentlessRomProperty =
        AvaloniaProperty.RegisterDirect<MainView, bool>(
            nameof(ContentlessRom),
            view => view.ContentlessRom,
            (view, value) => view.ContentlessRom = value);

    public bool ContentlessRom
    {
        get;
        set {
            SetAndRaise(ContentlessRomProperty, ref field, value);
            if (value) {
                BuildingLink.RaRom = string.Empty;
                BuildingLink.RaRomPatch = null;
            }
        }
    }

    public static readonly DirectProperty<MainView, string> UserRaCoreProperty =
        AvaloniaProperty.RegisterDirect<MainView, string>(
            nameof(UserRaCore),
            view => view.UserRaCore,
            (view, value) => view.UserRaCore = value);

    public string UserRaCore
    {
        get => BuildingLink.RaCore;
        set
        {
            BuildingLink.RaCore = value;
            if (DesktopOS || string.IsNullOrWhiteSpace(BuildingLink.FriendlyName) ||
                BuildingLink.CustomEntryName.GetValueOrDefault()) return;
            OutputFriendlyName = BuildingLink.FriendlyName;
            RaisePropertyChanged(UserRaCoreProperty, string.Empty, value);
        }
    }

    public static readonly DirectProperty<MainView, ObservableConfigItem> ConfigSelectedItemProperty =
        AvaloniaProperty.RegisterDirect<MainView, ObservableConfigItem>(
            nameof(ConfigSelectedItem),
            view => view.ConfigSelectedItem,
            (view, value) => view.ConfigSelectedItem = value);

    public ObservableConfigItem ConfigSelectedItem {
        get;
        set {
            BuildingLink.RaConfig = (value.IsDefault) ? null : value.FilePath;
            SetAndRaise(ConfigSelectedItemProperty, ref field, value);
        }
    } = ObservableConfigItem.DefaultInstance;

    // == Output File ==
    public static readonly DirectProperty<MainView, string> OutputFullPathProperty =
        AvaloniaProperty.RegisterDirect<MainView, string>(
            nameof(OutputFullPath),
            view => view.OutputFullPath,
            (view, value) => view.OutputFullPath = value);

    public string OutputFullPath
    {
        get => (!IsRenameInactive() && string.IsNullOrWhiteSpace(BuildingLink.OutputPath))
            ? FixedOutputDir : BuildingLink.OutputPath;
        set {
            if (IsRenameInactive()) BuildingLink.SetOutputPath(value);
            RaisePropertyChanged(OutputFullPathProperty, string.Empty, value);
        }
    }

    public static readonly DirectProperty<MainView, string> OutputFriendlyNameProperty =
        AvaloniaProperty.RegisterDirect<MainView, string>(
            nameof(OutputFriendlyName),
            view => view.OutputFriendlyName,
            (view, value) => view.OutputFriendlyName = value);

    public string OutputFriendlyName
    {
        get => BuildingLink.FriendlyName;
        set {
            if (!IsRenameInactive() || !DesktopOS) BuildingLink.UpdateOutputPathFromFriendlyName(value, FixedOutputDir);
            RaisePropertyChanged(OutputFullPathProperty, string.Empty, string.Empty);
            RaisePropertyChanged(OutputFriendlyNameProperty, string.Empty, value);
        }
    }

    public static readonly DirectProperty<MainView, bool> TileIconEnabledProperty =
        AvaloniaProperty.RegisterDirect<MainView, bool>(
            nameof(TileIconEnabled),
            view => view.TileIconEnabled,
            (view, value) => view.TileIconEnabled = value);

    public bool TileIconEnabled
    {
        get;
        private set {
            SetAndRaise(TileIconEnabledProperty, ref field, value);
            UpdateAndNotifyFixedOutputDir();
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
        set {
            SetAndRaise(TileIconAllUsersProperty, ref field, value);
            UpdateAndNotifyFixedOutputDir();
        }
    }

    /* 
     * This one bellow gets raised by 'UpdateAndNotifyFixedOutputDir()' so the control gets updated.
     * My alternatives are subscribing to each observable, or to the view's OnPropertyChanged,
     * and in the end those are the same
     */
    public static readonly DirectProperty<MainView, string> FixedOutputDirProperty =
        AvaloniaProperty.RegisterDirect<MainView, string>(
            nameof(FixedOutputDir),
            view => view.FixedOutputDir);

    public string FixedOutputDir => (!TileIconEnabled) ? Settings.DEFLinkOutput : FileOps.TileIconOutDirs[TileIconAllUsers];

    // == Icon ==
    public static readonly DirectProperty<MainView, ObservableIconItem> IconSelectedItemProperty =
        AvaloniaProperty.RegisterDirect<MainView, ObservableIconItem>(
            nameof(IconSelectedItem),
            view => view.IconSelectedItem,
            (view, value) => view.IconSelectedItem = value);

    public ObservableIconItem IconSelectedItem
    {
        get;
        set {
            // if (value is null) return;
            if (!IconItems.Contains(value)) {
                Logger.LogWarn($"IconItem '{value} [{value?.FilePath}]' was set as '{nameof(IconSelectedItem)}', " +
                               $"but it's not in the {nameof(IconItems)} list.");
                return;
            }
            SetAndRaise(IconSelectedItemProperty, ref field, value);
        }
    } = ObservableIconItem.DefaultInstance;

    public static readonly DirectProperty<MainView, bool> CustomIconSourceProperty =
        AvaloniaProperty.RegisterDirect<MainView, bool>(
            nameof(IconCustomSource),
            view => view.IconCustomSource,
            (view, value) => view.IconCustomSource = value);

    public bool IconCustomSource
    {
        get;
        set {
            if (!value) IconSelectedItem = ObservableIconItem.DefaultInstance;
            SetAndRaise(CustomIconSourceProperty, ref field, value);
        }
    }

    public static readonly DirectProperty<MainView, AvaloniaBitmap?> IconImageProperty =
        AvaloniaProperty.RegisterDirect<MainView, AvaloniaBitmap?>(
            nameof(IconImage),
            view => view.IconImage,
            (view, value) => view.IconImage = value);

    public AvaloniaBitmap? IconImage { get; set => SetAndRaise(IconImageProperty, ref field, value); }

    #endregion

    // Window Object
    public MainWindow ParentWindow { get; }

    // Props
    public static string OutputDescLabel => (App.PlatformOs) ? resMainView.lblDesc_Win : resMainView.lblDesc_Lin;
    public static string OutputDescTip => (App.PlatformOs) ? resMainView.tipDesc_Win : resMainView.tipDesc_Lin;
    public Settings Settings { get; private set; }
    public UserLinkBuilder BuildingLink { get; } = new();
    public ObservableCollection<ObservableConfigItem> ConfigItems { get; } = [ObservableConfigItem.DefaultInstance];
    public ObservableCollection<ObservableIconItem> IconItems { get; } = [ObservableIconItem.DefaultInstance];


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
    public const string RomPlaceholder = "Super_Tilt_Bro_(E).nes";
    public const string RenamePlaceholder = "Super Tilt Bro";
    public static readonly string RaExecPlaceholder = (App.PlatformOs) ? App.RetroBin + FileOps.WinPeExtExe : App.RetroBin;
    public static readonly string OutputPlaceholder = RenamePlaceholder + FileOps.GetOutputExt(App.PlatformOs);
    private readonly bool _isDesingner;   // Debug
    private string OldDefRaExec = string.Empty;
    // private string DefLinRAIcon;

    // TODO: This has to be a enum or something, as it needs to include macOS. And move to App. (>=0.10)
    //  We can use [SupportedOSPlatform("xxx")] for methods exclusive to xxx platform,
    //  and [SupportedOSPlatformGuard("xxx")] to denote a condition that tells the compiler that the method is guarded
    // https://learn.microsoft.com/en-us/dotnet/standard/analyzers/platform-compat-analyzer
    // true = Windows; false = Linux.
    private readonly bool DesktopOS = App.PlatformOs;


    #region LOAD OPS

    private void CompleteSetup()
    {
        BuildingLink.RestoreRemembered(Settings.UserChoices);
        if (!DesktopOS) {
            Settings.TileIcoPath = null; // Disable 'TileIcoPath' when is not Windows
            Settings.DEFRADir = App.RetroBin;
        }

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

        ApplyDragDropEvents();
        ApplySettingsToControls();

        Prepare_comboConfig();
        _ = Prepare_comboCore();
        _ = Prepare_comboIcon();

        // Arguments should only load when above controls are ready
        ApplyArgs();

        DataContext = this;

        // TODO_MAYBE: Tutorial event for new users
    }


    private async Task Prepare_comboCore()
    {
        var cores = await Operations.GetCoresArray();
        if (cores.Length < 1) ToolTip.SetTip(comboCore, resMainView.lblNoCores);
        else comboCore.ItemsSource = cores;
        Logger.LogInfo($"{cores.Length} cores were imported.");
     }

    private void Prepare_comboConfig() {
        foreach (var config in FileOps.ConfigDir)
            ConfigItems.Add(new ObservableConfigItem(config));
    }

    private async Task Prepare_comboIcon()
    {
        try
        {
            var iconsList = await Task.Run(FileOps.LoadIcons);
            foreach (var iconFile in iconsList)
                IconItems.Add(ObservableIconItem.FromIIconItem(iconFile));
            Logger.LogInfo("Icons list imported");
        }
        catch (Exception e)
        {
            Logger.LogErro(e);
            var message = $"{resMainView.popIconsError_Mess}\n\n{resMainView.popIconsError_Mess2}\n'{e.Message}'";
            var content = new PopUpGenericContent(message, resMainView.popIconsError_Title);
            _ = this.PopUpGenericMessageBox(content, GenericPopUpType.Error);
        }
        IconSelectedItem.ResetBitmap(); // <= Default entry gets its icon data
    }

    private void ApplyDragDropEvents()
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

    private void UpdateAndNotifyFixedOutputDir() {
        OutputFriendlyName = OutputFriendlyName; // Updates the output path, if relevant
        RaisePropertyChanged(FixedOutputDirProperty, string.Empty, string.Empty);
    }

    // Controls Modifiers

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
        if (!OldDefRaExec.Equals(Settings.DEFRADir) && BuildingLink.RaExecutable.Equals(OldDefRaExec)) {
            BuildingLink.RaExecutable = Settings.DEFRADir;
            OldDefRaExec = Settings.DEFRADir;
        }

        if (!_isDesingner && !ParentWindow.IsDesigner) {
            Operations.SetROMParentStorageFolder(Settings.DEFROMPath, ParentWindow);
            Operations.SetIconParentStorageFolder(Settings.IconParentPath, ParentWindow);
            Operations.SetDesktopStorageFolder(ParentWindow);
        }
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
        btnLINKRename.IsVisible = !DesktopOS;
        panelTileIcon.IsVisible = tileIconEnabled;
        // FixedOutputDir = (!tileIconEnabled) ? Settings.DEFLinkOutput : FileOps.TileIconOutDirs[BuildingLink.TileIcoAllUsers];
        // UpdateAndNotifyFixedOutputDir();
    }

    // Pop-ups
    async Task<RenamePopUpHelper> ResolveRenamePopUp(string givenPath, string givenName, string givenCore) {
        var renameParams = new RenamePopUpHelper(givenPath, givenName, givenCore, BuildingLink.CustomEntryName ?? false);
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

    bool IsRenameInactive() => Settings is { AlwaysAskOutput: true, TileIcoPath: null };

    void LockControls(bool lockControls) => this.IsEnabled = !lockControls;

    #endregion

    #region Execution

    void ResetAfterExecute() {
        if (BuildingLink.RaRom == Manager.Contentless) ContentlessRom = true;
        BuildingLink.TileIcoImagePath = null;
        LockControls(false);
    }

    // private void HandleLinkOutput(string userInput, UserLinkBuilder linkBuilder)

    private async Task SetFinalIconPath(UserLinkBuilder outputLink)
    {
        if (IconSelectedItem.Equals(ObservableIconItem.DefaultInstance)) outputLink.IconPath = string.Empty; // RA binary icon (Default)
        else
        {
            outputLink.IconPath = IconSelectedItem.FilePath;    // It's set to the selected icon file by default
            if (DesktopOS)
            {
                // If it's Windows, the images may need to be converted to .ico
                if (IconSelectedItem.ConversionRequired) {
                    outputLink.TileIcoImagePath = IconSelectedItem.FilePath; // Pass the original image as the image for the Tile Icon
                    outputLink.IconPath = FileOps.GetConvertedIcoPath(outputLink, outputLink.RaRom, IconSelectedItem);
                }

                if (Settings.TileIcoPath is not null)
                {
                    var imagePath = string.Empty;
                    try
                    {
                        imagePath = await IconProc.ResolveImageForTileico(outputLink.IconPath, outputLink.TileIcoImagePath, IconSelectedItem);
                        ArgumentException.ThrowIfNullOrEmpty(imagePath);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogErro(ex);
                        var content = new PopUpGenericContent(resMainView.wrnIcoToImage_mess,
                            null, resMainView.wrnIcoToImage_header);
                        await this.PopUpGenericMessageBox(content, GenericPopUpType.Warning);
                        // var ms = await this.PopUpGenericMessageBox(content, GenericPopUpType.Warning);
                        // Logger.LogDebg(ms.ToString("G"));
                    }
                    outputLink.TileIcoImagePath = imagePath;
                }
            }
            // If it's Linux, no conversion is required

            // In case of 'CpyUserIcon = true', but it'll be ignored if Tileico is in use
            if (Settings is { CpyUserIcon: true, TileIcoPath: null } && !IconSelectedItem.ConversionRequired)
                outputLink.IconPath = FileOps.RenameIconToUsrSett(outputLink.IconPath);
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
                return (onlyResult.ResultSuccess)
                    ? (new PopUpGenericContent(resMainView.popSingleOutput1_Mess), GenericPopUpType.Success)
                    : (new PopUpGenericContent($"{resMainView.popSingleOutput0_Mess}\n{onlyResult.ExMessage}",
                        resMainView.popSingleOutput0_Head), GenericPopUpType.Error);
            // Multiple Shortcuts created
            default:
            {
                bool hasErrors = false;
                foreach (var r in results) {
                    if (r.ResultSuccess) continue;
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
                        content = string.Concat(content, resMainView.popLinkFailure); // <= Depends on success
                        content = string.Concat(content, "\n");
                        if (!R.ResultSuccess)
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
        try
        {
            LockControls(true);

            // Validate there's an executable (Linux)
            // BuildingLink.RaExecutable = ValidateLINBin(BuildingLink.RaExecutable);

            // Validating contentless or not
            BuildingLink.RaRom = (ContentlessRom) ? Manager.Contentless : BuildingLink.RaRom;

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

                if (DesktopOS && IconSelectedItem.ConversionRequired)
                {
                    // TODO: test again
                    if (!FileOps.IsFileWinPE(outputLink.IconPath))
                    {
                        var wrkIconItem = IconSelectedItem;
                        var icoBytes = (FileOps.IsExtWinPE(IconSelectedItem.FileExt))
                            ? (await wrkIconItem.GetImageStream()).ToArray()
                            : IconProc.VirtuallyCreateIco(await wrkIconItem.GetImageStream());
                        await FileOps.WriteAllBytesToFileAsync(outputLink.IconPath, icoBytes);

                        var index = IconItems.IndexOf(wrkIconItem);
                        if (index >= 0)
                        {
                            IconSelectedItem = IconItems[Index.Start];
                            IconItems.RemoveAt(index);
                            wrkIconItem.Dispose();
                            wrkIconItem = new ObservableIconItem(outputLink.IconPath, conversionRequired: false);
                            IconItems.Insert(index, wrkIconItem);
                            IconSelectedItem = wrkIconItem;
                        }
                    }
                }

                // Create Shortcuts
                var opsResult = await outputLink.BuildShortcut();
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
        try
        {
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
            var newIcon = new ObservableIconItem(filePath);
            IconItems.Add(newIcon);
            IconSelectedItem = newIcon;
        }
        else {
            IconSelectedItem = existing;
            IconSelectedItem.ResetBitmap();
        }
    }

    async void btnICONDir_ClickAsync()
    {
        try
        {
            LockControls(true);
            var opt = DesktopOS ? OpenOpts.WINico : OpenOpts.LINico;
            var currentFile = IconSelectedItem.FilePath;
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

    void RaExecutable_Set(string filePath) {
        BuildingLink.RaExecutable = filePath;
    }

    async void btnRADir_ClickAsync()
    {
        try
        {
            LockControls(true);
            OpenOpts opt;
            string currentFile = string.Empty;
            if (DesktopOS) {
                opt = OpenOpts.WinExe;
                currentFile = BuildingLink.RaExecutable;
            }
            else { opt = OpenOpts.LinBin; }
            var dlgTitle = resAvaloniaOps.dlgFileRAexe;
            var file = await FileDialogOps.OpenFileAsync(opt, ParentWindow, currentFile, dlgTitle);
            if (string.IsNullOrEmpty(file)) return;
            RaExecutable_Set(file);
        }
        catch (Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    void btnRADir_OnClick(object sender, RoutedEventArgs e) => btnRADir_ClickAsync();

    #endregion

    #region ROM Controls

    void ROMDir_Set(string filePath) {
        BuildingLink.RaRom = filePath;
    }

    async void btnROMDir_ClickAsync()
    {
        try
        {
            LockControls(true);
            string currentFile = (string.IsNullOrEmpty(BuildingLink.RaRom)) ? string.Empty : BuildingLink.RaRom;
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

    void btnSubSys_OnClick(object sender, RoutedEventArgs e) {
        ParentWindow.ChangeOut(MainViewTypes.SubsysView);
    }

    #endregion

    #region Config Controls

    void comboConfig_Set(string filePath)
    {
        var input = new ObservableConfigItem(filePath);
        if (!ConfigItems.Contains(input)) {
            ConfigItems.Add(input);
            if (Settings.PrevConfig) SettingsOps.PrevConfigs.Add(filePath);
        }
        ConfigSelectedItem = input;
    }

    async void btnCONFIGDir_ClickAsync()
    {
        try
        {
            LockControls(true);
            var currentFile = (ConfigSelectedItem.IsDefault) ? string.Empty : ConfigSelectedItem.FilePath;
            var file = await FileDialogOps.OpenFileAsync(OpenOpts.RAcfg, ParentWindow, currentFile);
            if (string.IsNullOrEmpty(file)) return;
            comboConfig_Set(file);
        }
        catch (Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    void btnCONFIGDir_OnClick(object sender, RoutedEventArgs e) => btnCONFIGDir_ClickAsync();

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
            var currentFile = (string.IsNullOrEmpty(textBox.Text)) ? string.Empty : textBox.Text;
            var file = await FileDialogOps.SaveFileAsync(opt, currentFile, ParentWindow);
            if (!string.IsNullOrEmpty(file))
            {
                if (!DesktopOS)
                {
                    var helper = await ResolveRenamePopUp(file, string.Empty, BuildingLink.RaCore);
                    if (!helper.ValidOutput) throw new Exception("Output path could not be resolved.");
                    BuildingLink.SetOutputPathAndFriendlyName(helper.NewPath, helper.NewFriendlyName);
                    BuildingLink.CustomEntryName = helper.CustomEntry;
                }
                else BuildingLink.SetOutputPath(file);
                UpdateAndNotifyFixedOutputDir();
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
        try
        {
            LockControls(true);
            RenamePopUpHelper helper;
            if (BuildingLink.CustomEntryName ?? false)
                helper = await ResolveRenamePopUp(OutputFullPath, OutputFriendlyName, BuildingLink.RaCore);
            else
            {
                var validInput = (string.IsNullOrWhiteSpace(textBox.Text)) ? null : textBox.Text;
                var properFileName = (validInput is null)
                    ? Models.Linux.DesktopEntryOps.NamePlaceHolder
                    : textBox.Text + FileOps.GetOutputExt(DesktopOS);
                var fullPath = FileOps.CombineMultipleInputs(Settings.DEFLinkOutput, properFileName);
                helper = await ResolveRenamePopUp(fullPath, validInput ?? string.Empty, BuildingLink.RaCore);
            }
            if (!helper.ValidOutput) return;

            // if (helper.NewFriendlyName.Equals(Models.Linux.DesktopEntryOps.NamePlaceHolder)) helper.NewFriendlyName = string.Empty;
            BuildingLink.SetOutputPathAndFriendlyName(helper.NewPath, helper.NewFriendlyName);
            BuildingLink.CustomEntryName = helper.CustomEntry;
            UpdateAndNotifyFixedOutputDir();
        }
        catch (Exception e) { await this.PopUpGenericError(e); }
        finally { LockControls(false); }
    }

    void BtnLINKRename_OnClick(object? sender, RoutedEventArgs e) {
        if (sender is not Button btn) return;
        if (btn.CommandParameter is not TextBox txt) return;
        BtnLINKRename_ClickAsync(txt);
    }

    #endregion


    // EXECUTE
    void btnEXECUTE_OnClick(object sender, RoutedEventArgs e) => RunExecution();


    #region Genric Envents

    void ControlBox_DragEnter(object? sender, DragEventArgs e) {
        if (sender is not AvaloniaTemplatedControl tc) return;
        tc.BorderBrush = Brushes.Aqua;
    }

    void ControlBox_DragLeave(object? sender, DragEventArgs e) {
        if (sender is not AvaloniaTemplatedControl tc) return;
        ResetBorderBrush(tc);
    }

    void ControlBox_DropParse(object? sender, DragEventArgs e)
    {
        if (sender is not AvaloniaTemplatedControl tc) return;
        var filesEnum = e.DataTransfer.TryGetFiles();
        var text = e.DataTransfer.TryGetText();
        if (filesEnum is { Length: > 0 }) {
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
        Avalonia.Threading.Dispatcher.UIThread.Invoke(() => ControlBox_DropResult(accepted: false, tc));
    }

    bool ControlBox_HandleDrop(AvaloniaTemplatedControl sender, string droppedPath)
    {
        if (sender.Equals(comboIcon)) {
            if (!FileOps.IsFileAnIcon(droppedPath, DesktopOS, out _)) return false;
            IconPath_Set(droppedPath);
        }
        else if (sender.Equals(txtRADir)) RaExecutable_Set(droppedPath);
        else if (sender.Equals(txtROMDir)) ROMDir_Set(droppedPath);
        else if (sender.Equals(comboConfig))
        {
            if (!FileOps.IsConfigFile(droppedPath, out _)) return false;
            comboConfig_Set(droppedPath);
        }
        // TODO: else if (sender.Equals(txtLINKDir)) later...

        return true;
    }

    bool ControlBox_HandlePaste(AvaloniaTemplatedControl sender, string pastedPath)
    {
        // if (pastedPath.Length > 255) return false;
        if (sender.Equals(comboIcon)) {
            if (!FileOps.IsFileAnIcon(pastedPath, DesktopOS, out _)) return false;
            IconPath_Set(pastedPath);
        }
        else if (sender.Equals(txtRADir)) RaExecutable_Set(pastedPath);
        else if (sender.Equals(txtROMDir)) ROMDir_Set(pastedPath);
        else if (sender.Equals(comboConfig)) {
            if (!FileOps.IsConfigFile(pastedPath, out _)) return false;
            comboConfig_Set(pastedPath);
        }
        // TODO: else if (sender.Equals(txtLINKDir)) later...

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
        IconSelectedItem = ObservableIconItem.DefaultInstance;
        foreach (var iconItem in IconItems) iconItem.Dispose();
        IconItems.Clear();
    }

    void View1_Unloaded(object sender, RoutedEventArgs e) {
        // Save changes outside SettingsOps
        SettingsOps.WriteSettings(Settings);
    }
}
