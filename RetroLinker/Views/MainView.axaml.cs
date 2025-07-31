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
using RetroLinker.Models.LinuxClasses;
using RetroLinker.Translations;

using MessageBoxBottomResult = MsBox.Avalonia.Enums.ButtonResult;
using AvaloniaAssetLoader = Avalonia.Platform.AssetLoader;
using AvaloniaTemplatedControl = Avalonia.Controls.Primitives.TemplatedControl;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

namespace RetroLinker.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        // Constructor for Designer
        AvaloniaOps.DesignerMainViewPreConstruct(out settings);
        InitializeComponent();
        ParentWindow = new MainWindow(true);
        IsDesingner = true;
        stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
    }
    
    public MainView(MainWindow mainWindow)
    {
        AvaloniaOps.MainViewPreConstruct(mainWindow, out settings);
        InitializeComponent();
        ParentWindow = mainWindow;
        stopwatch = App.StopWatch;
        System.Diagnostics.Debug.WriteLine($"Execution time after MainView(): {stopwatch.ElapsedMilliseconds} ms", App.TimeTrace);
    }
    
    // Debug
    private System.Diagnostics.Stopwatch stopwatch;
    private bool IsDesingner = false;
    
    // Window Object
    private MainWindow ParentWindow;
    
    private bool FormFirstLoad = true;
    // private string DefLinRAIcon;
    private int PrevConfigsCount;
    private int PreloadedIconsCount;
    private byte CurrentTheme = 250;
    private Settings settings;
    private AvaloniaBitmap ICONimage = new(AvaloniaAssetLoader.Open(AvaloniaOps.GetNAimage()));
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
        if (FormFirstLoad)
        {
            AvaloniaOps.MainViewLoad();

#if DEBUG
            // TODO: Implement an Event for theme handling
            try
            {
                ParentWindow.RequestedThemeVariant = LoadThemeVariant();
            }
            catch (System.Exception e_theme) 
            {
                System.Diagnostics.Debug.WriteLine(e_theme, App.DebgTrace);
                TopLevel.GetTopLevel(this)!.RequestedThemeVariant = ThemeVariant.Light;
            }
#else
            ParentWindow.RequestedThemeVariant = LoadThemeVariant();
#endif
        
            // Based on current OS
            if (!DesktopOS)
            {
                if (string.IsNullOrEmpty(settings.DEFRADir)) settings.DEFRADir = App.RetroBin;
                txtRADir.IsReadOnly = false;
                // DefLinRAIcon = AvaloniaOps.DefLinRAIcon;
            }
            else IconItemSET = new();

            BuildingLink = new Shortcutter();
            txtLINKDir.PropertyChanged += TxtLINKDir_OnPropertyChanged;
            LoadDragDropEvents();
            ApplySettingsToControls();
            
            comboCore_Loaded(AvaloniaOps.GetCoresArray());
            comboConfig_Loaded();
            comboICONDir_Loaded(FileOps.LoadIcons(DesktopOS));
            
            // Arguments should only load when above controls are ready
            ApplyArgs();
            
            // TODO: Tutorial event for new users
            
            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"Execution time after View1_Loaded(): {stopwatch.ElapsedMilliseconds} ms", App.TimeTrace);
            FormFirstLoad = false;
        }
        else LoadNewSettings();
    }

    void comboCore_Loaded(string[] cores)
    {
        System.Diagnostics.Trace.WriteLine("Cores list imported.", App.InfoTrace);
        if (cores.Length < 1) lblNoCores.IsVisible = true;
        else comboCore.ItemsSource = cores;
     }

    void comboConfig_Loaded()
    {
        foreach (var config in FileOps.ConfigDir)
            comboConfig.Items.Add(config);
        
        comboConfig.SelectedIndex = 0;
    }

    void comboICONDir_Loaded(object[] iconsObject)
    {
        var iconsList = (List<string>)iconsObject[0];
        var hasError = (bool)iconsObject[1];
        var exception = (string)iconsObject[2];

        if (!hasError)
        {
            comboICONDir.Items.Add(resMainView.comboDefItem);
            foreach (var iconFile in iconsList)
                comboICONDir.Items.Add(iconFile);
            System.Diagnostics.Trace.WriteLine("Icons list imported", App.InfoTrace);

            PreloadedIconsCount = comboICONDir.ItemCount;
            comboICONDir.SelectedIndex++;
            rdoIconDef.IsChecked = true;
        }
        else
        {
            var popParams = new MessageBoxStandardParams()
            {
                ContentTitle = resMainView.popIconsError_Tittle,
                ContentMessage = $"{resMainView.popIconsError_Mess}\n\n{resMainView.popIconsError_Mess2}\n'{exception}'",
                Icon = MsBox.Avalonia.Enums.Icon.Error,
                ButtonDefinitions = MsBox.Avalonia.Enums.ButtonEnum.Ok
            };
            _ = MessageBoxPopUp(popParams);
        }
    }

    void LoadDragDropEvents()
    {
        var BorderTransition = new BrushTransition()
        {
            Property = BorderBrushProperty,
            Duration = System.TimeSpan.FromMilliseconds(250),
        };

        AvaloniaTemplatedControl[] templatedControls = [comboICONDir, txtRADir, txtROMDir, comboConfig, txtLINKDir];
        foreach (var control in templatedControls)
        {
            DragDrop.SetAllowDrop(control, true);
            if (control.Transitions != null) control.Transitions.Add(BorderTransition);
            else control.Transitions = new Transitions() { BorderTransition };
            control.AddHandler(DragDrop.DragEnterEvent, ControlBox_DragEnter);
            control.AddHandler(DragDrop.DragLeaveEvent, ControlBox_DragLeave);
            control.AddHandler(DragDrop.DropEvent, ControlBox_DropParse);
        }
    }
    #endregion

    #region Functions
    
    void LoadNewSettings()
    {
        // settings = FileOps.LoadCachedSettingsFO();
        ApplySettingsToControls();
        LoadLocalization();
    }
    
    ThemeVariant LoadThemeVariant()
    {
        ThemeVariant theme = settings.PreferedTheme switch
        {
            1 => ThemeVariant.Light,
            2 => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
        CurrentTheme = settings.PreferedTheme;
        System.Diagnostics.Trace.WriteLine($"The requested theme was: {theme}", App.InfoTrace);
        System.Diagnostics.Debug.WriteLine($"Index in byte: {settings.PreferedTheme}", App.DebgTrace);
        return theme;
    }

    void ApplySettingsToControls()
    {
        if (!string.IsNullOrEmpty(settings.DEFRADir)) txtRADir.Text = settings.DEFRADir;
        BuildingLink.RAdir = settings.DEFRADir;
        if (!IsDesingner && !ParentWindow.IsDesigner)
        {
            AvaloniaOps.SetROMTop(settings.DEFROMPath, ParentWindow);
            AvaloniaOps.SetDesktopStorageFolder(ParentWindow);
        }
        
        PrevConfigsCount = (settings.PrevConfig) ? SettingsOps.PrevConfigs.Count : -1;
        
        txtLINKDir.Watermark = "Super Mario Bros";
        txtLINKDir.Watermark += FileOps.GetOutputExt(DesktopOS);
        AllwaysAskOutputLink(settings.AlwaysAskOutput);
    }

    void ApplyArgs()
    {
        // TODO: Add argument loading support. 2 Cases:
        // 1. Opening existing links
        // 2. Starting from a ROM
        var args = App.Args;
        if (args is null)  return;
        if (args.Length == 0) System.Diagnostics.Debug.WriteLine("bleh", App.DebgTrace);
        else
        {
            foreach (var arg in args)
                System.Diagnostics.Debug.WriteLine(arg, App.DebgTrace);
        }
    }

    void LoadLocalization()
    {
        if (FormFirstLoad) return;
        ParentWindow.LocaleReload(settings);
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
            : FileOps.GetDefinedLinkPath(LinDesktopEntry.StdDesktopEntry(fileNameNoExt, core) + FileOps.GetOutputExt(DesktopOS), settings.DEFLinkOutput);
    }

    void LockForExecute(bool lockControls) => gridBODY.IsEnabled = !lockControls;

    void ResetAfterExecute()
    {
        if (BuildingLink.OutputPaths.Count == 0)
            BuildingLink.OutputPaths.Add(
                ShortcutterOutput.RebuildOutputWithFriendly(PreviousOutput, DesktopOS, string.Empty));
        
        LockForExecute(false);
    }

    void FillIconBoxes(string path)
    {
        ICONimage = AvaloniaOps.GetBitmap(path);
        FillIconSource(ICONimage);
    }
    
    void FillIconBoxes(AvaloniaBitmap bitmap)
    {
        ICONimage = bitmap;
        FillIconSource(ICONimage);
    }

    void FillIconSource(IImage memImage)
    {
        pic16.Source = memImage;
        pic32.Source = memImage;
        pic64.Source = memImage;
        pic128.Source = memImage;
    }
    
    async Task<MessageBoxBottomResult> MessageBoxPopUp(MessageBoxStandardParams standardParams)
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
        var popupWindow = new PopUpWindow(ParentWindow);
        popupWindow.RenamePopUp(givenPath, givenCore, outputs);
        return await popupWindow.ShowDialog<List<ShortcutterOutput>>(ParentWindow);
        // BuildingLink.OutputPaths = result;
    }

    string ValidateLINBin(string RAPath)
    {
        if (RAPath == txtRADir.Text) return RAPath;
        return string.IsNullOrWhiteSpace(txtRADir.Text) ? string.Empty : txtRADir.Text;
    }
    
    public void UpdateLinkFromOutside(MainWindow.ViewsTypes viewType, string[] argStrings)
    {
        switch (viewType)
        {
            case MainWindow.ViewsTypes.PatchesView:
                BuildingLink.PatchArg = argStrings[0];
                break;
            case MainWindow.ViewsTypes.SubsysView:
                // Subsystem loading
                break;
            case MainWindow.ViewsTypes.AppendView:
                // Append config
                break;
        }
    }
    #endregion


    // TOP CONTROLS
    async void btnSettings_Click(object sender, RoutedEventArgs e)
    {
        var settingWindow = new SettingsWindow(ParentWindow, settings); 
        var settingReturn =  await settingWindow.ShowDialog<Settings?>(ParentWindow);
        settings = (settingReturn is not null) ? FileOps.SetNewSettings(settingReturn) : FileOps.LoadCachedSettingsFO();
        LoadNewSettings();
    }
    
    private void ButtonAbout_OnClick(object? sender, RoutedEventArgs e)
    {
        var aboutWindow = new AboutWindow();
        aboutWindow.ShowDialog(ParentWindow);
    }

    #region Icon Controls

    void ICONDir_Set(string filePath)
    {
        int newIndex = comboICONDir.ItemCount;
        const int indexNotFound = -1;
        int existingItem = IconProc.IconItemsList.IndexOf(IconProc.IconItemsList.Find(Item => Item.FilePath == filePath)!);
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

    async void btnICONDir_Click(object sender, RoutedEventArgs e)
    {
        PickerOpt.OpenOpts opt;
        opt = DesktopOS ? PickerOpt.OpenOpts.WINico : PickerOpt.OpenOpts.LINico;

        string currentFile = (comboICONDir.SelectedIndex >= PreloadedIconsCount)
            ? (string)comboICONDir.SelectedItem!
            : string.Empty;
        string file = await AvaloniaOps.OpenFileAsync(opt, currentFile, ParentWindow);
        if (string.IsNullOrEmpty(file)) return;
        ICONDir_Set(file);
    }

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
                var bitmap = AvaloniaOps.GetBitmap(IconItemSET.IconStream);
                FillIconBoxes(bitmap);
            }
            else {
                try{
                    FillIconBoxes(BuildingLink.ICONfile); 
                    panelIconNoImage.IsVisible = false;
                }
                catch {
                    AvaloniaBitmap bitmap = new(AvaloniaAssetLoader.Open(AvaloniaOps.GetNAimage()));
                    FillIconBoxes(bitmap);
                    panelIconNoImage.IsVisible = true;
                } 
            }
        }
        else {   
            AvaloniaBitmap bitmap = new(AvaloniaAssetLoader.Open(AvaloniaOps.GetDefaultIcon()));
            FillIconBoxes(bitmap);
        }
    }
    #endregion
    
    #region RADirectory Controls

    void RADirSet(string filePath)
    {
        BuildingLink.RAdir = filePath;
        txtRADir.Text = filePath;
    }
    
    async void btnRADir_Click(object sender, RoutedEventArgs e)
    {
        PickerOpt.OpenOpts opt;
        string currentFile = string.Empty;
        if (DesktopOS)
        {
            opt = PickerOpt.OpenOpts.RAexe;
            currentFile = (string.IsNullOrEmpty(txtRADir.Text)) ? string.Empty : txtRADir.Text;
        }
        else { opt = PickerOpt.OpenOpts.RAbin; }
        string file = await AvaloniaOps.OpenFileAsync(opt, currentFile, ParentWindow);
        if (string.IsNullOrEmpty(file)) return;
        RADirSet(file);
    }
    #endregion

    #region ROM Controls

    void ROMDir_Set(string filePath)
    {
        BuildingLink.ROMdir = filePath;
        txtROMDir.Text = filePath;
    }
    
    void chkContentless_CheckedChanged(object sender, RoutedEventArgs e)
    {
        panelROMDirControl.IsEnabled = !chkContentless.IsChecked.GetValueOrDefault();
    }

    async void btnROMDir_Click(object sender, RoutedEventArgs e)
    {
        string currentFile = (string.IsNullOrEmpty(txtROMDir.Text)) ? string.Empty : txtROMDir.Text;
        string file = await AvaloniaOps.OpenFileAsync(PickerOpt.OpenOpts.RAroms, currentFile, ParentWindow);
        if (string.IsNullOrEmpty(file)) return;
        ROMDir_Set(file);
    }
    
    void BtnPatches_OnClick(object? sender, RoutedEventArgs e)
    {
        ParentWindow.ChangeOut(MainWindow.ViewsTypes.PatchesView, [BuildingLink.PatchArg]);
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
            newFile = LinDesktopEntry.StdDesktopEntry(BuildingLink.OutputPaths[0].FriendlyName + FileOps.GetOutputExt(false), comboCore.Text);
            outputDir = FileOps.GetDirFromPath(BuildingLink.OutputPaths[0].FullPath);
            if (string.IsNullOrWhiteSpace(outputDir)) outputDir = FileOps.BaseDir;
            txtLINKDir.Text = FileOps.CombineDirAndFile(outputDir, newFile);
        }
        else
        {
            newFile = LinDesktopEntry.StdDesktopEntry(txtLINKDir.Text + FileOps.GetOutputExt(false), comboCore.Text);
            outputDir = settings.DEFLinkOutput;
            lblLinkDefinedDir.Text = FileOps.CombineDirAndFile(outputDir, newFile);
        }
    }
    
    void btnSubSys_Click(object sender, RoutedEventArgs e)
    {
        // TODO    
    }
    #endregion

    #region Config Controls

    void comboConfig_Set(string filePath)
    {
        if (!comboConfig.Items.Contains(filePath))
        {
            comboConfig.Items.Add(filePath);
            if (settings.PrevConfig) SettingsOps.PrevConfigs.Add(filePath);
        }
        comboConfig.SelectedItem = filePath;
    }

    async void btnCONFIGDir_Click(object sender, RoutedEventArgs e)
    {
        string currentFile = (comboConfig.SelectedIndex > 0) ? (string)comboConfig.SelectedItem! : string.Empty;
        var file = await AvaloniaOps.OpenFileAsync(PickerOpt.OpenOpts.RAcfg, currentFile, ParentWindow);
        if (string.IsNullOrEmpty(file)) return;
        comboConfig_Set(file);
    }
    
    void comboConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        BuildingLink.CONFfile = comboConfig.SelectedIndex switch
        {
            -1 => null,
            0 => string.Empty,
            _ => (string)comboConfig.SelectedItem!
        };
    }
    
    void btnAppendConfig_Click(object sender, RoutedEventArgs e)
    {
        // TODO
    }
    #endregion

    #region LinkPath Controls

    async void LINKDir_Set(string filePath)
    {
        LinkCustomName = false;
        if (!DesktopOS)
        {
            BuildingLink.OutputPaths = await ResolveRenamePopUp(filePath, comboCore.Text, BuildingLink.OutputPaths);
            LinkCustomName = BuildingLink.OutputPaths[0].CustomEntryName;
            filePath = BuildingLink.OutputPaths[0].FullPath;
        }
        txtLINKDir.Text = filePath;
    }
    
    private void BtnMoreParams_OnClick(object? sender, RoutedEventArgs e)
    {
        System.Diagnostics.Trace.WriteLine("COMING SOON");
    }
    
    async void btnLINKDir_Click(object sender, RoutedEventArgs e)
    {
        PickerOpt.SaveOpts opt;
        opt = (DesktopOS) ? PickerOpt.SaveOpts.WINlnk : PickerOpt.SaveOpts.LINdesktop;
        string currentFile = (string.IsNullOrEmpty(txtLINKDir.Text)) ? string.Empty : txtLINKDir.Text;
        string file = await AvaloniaOps.SaveFileAsync(opt, currentFile, ParentWindow);
        if (!string.IsNullOrEmpty(file)) {
            LINKDir_Set(file);
        }
#if DEBUG
        else
        {
            System.Diagnostics.Debug.WriteLine("Running on debug...", App.DebgTrace);

            // var readLink = Models.WinClasses.WinShortcutter.ReadShortcut(BuildingLink.OutputPaths[0].FullPath);
            // System.Diagnostics.Debug.WriteLine(readLink[0], App.DebgTrace);
            // Testing.LinShortcutTest(DesktopOS);
            // var bitm = FileOps.IconExtractTest(); FillIconBoxes(bitm);
        }
#endif
    }
    
    private async void BtnLINKRename_OnClick(object? sender, RoutedEventArgs e)
    {
        LinkCustomName = false;
        var fullPath = FileOps.CombineDirAndFile(
            settings.DEFLinkOutput, 
            (string.IsNullOrWhiteSpace(txtLINKDir.Text) ? LinDesktopEntry.NamePlaceHolder : txtLINKDir.Text)
        );
        BuildingLink.OutputPaths = await ResolveRenamePopUp(fullPath, comboCore.Text, BuildingLink.OutputPaths);
        if (BuildingLink.OutputPaths.Count == 0) return;
        
        LinkCustomName = BuildingLink.OutputPaths[0].CustomEntryName;
        txtLINKDir.Text = BuildingLink.OutputPaths[0].FriendlyName;
        lblLinkDefinedDir.Text = BuildingLink.OutputPaths[0].FullPath;
    }

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
    void btnEXECUTE_Click(object sender, RoutedEventArgs e)
    {
        var OutputLink = new Shortcutter(BuildingLink);
        BuildingLink.OutputPaths = new();
        var msbox_params = new MessageBoxStandardParams();

        // Controls Lock
        LockForExecute(true);
        
        // Checkboxes!
        OutputLink.VerboseB = chkVerb.IsChecked.GetValueOrDefault();
        OutputLink.FullscreenB = chkFull.IsChecked.GetValueOrDefault();
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
        // TODO: Move the conversion to after the required fields check
        else
        {
            // If it's Windows OS, the images may have to be converted to .ico
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

        // REQUIRED FIELDS VALIDATION!
        if ((!string.IsNullOrEmpty(OutputLink.RAdir))
            && (!string.IsNullOrEmpty(OutputLink.ROMdir))
            && (!string.IsNullOrEmpty(OutputLink.ROMcore))
            && (OutputLink.OutputPaths[0].ValidOutput))
        {
            System.Diagnostics.Debug.WriteLine("All fields for link creation have benn accepted.", App.DebgTrace);
            
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
                    msbox_params.ContentMessage = resMainView.popSingleOutput1_Mess;
                    msbox_params.ContentTitle = resGeneric.genSucces;
                    msbox_params.Icon = MsBox.Avalonia.Enums.Icon.Success;
                }   
                else
                {
                    msbox_params.ContentHeader = resMainView.popSingleOutput0_Head; 
                    msbox_params.ContentTitle = resGeneric.genError;
                    msbox_params.ContentMessage = $"{resMainView.popSingleOutput0_Mess} \n {opResult[0].eMesseage}";
                    msbox_params.Icon = MsBox.Avalonia.Enums.Icon.Error;
                }
            }
            // Multiple Shortcut
            else
            {
                bool hasErrors = false;
                foreach (var r in opResult)
                {
                    if (r.Error) hasErrors = true;
                    break;
                }

                if (!hasErrors)
                {
                    msbox_params.ContentMessage = resMainView.popMultiOutput1_Mess; 
                    msbox_params.ContentTitle = resGeneric.genSucces;
                    msbox_params.Icon = MsBox.Avalonia.Enums.Icon.Success;
                }
                else
                {
                    msbox_params.ContentHeader = resMainView.popMultiOutput0_Head;
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
                    msbox_params.ContentTitle = resGeneric.genWarning;
                    msbox_params.Icon = (successCount > 0) ? MsBox.Avalonia.Enums.Icon.Warning : MsBox.Avalonia.Enums.Icon.Error;
                    msbox_params.ContentMessage = content;
                }
            }
        }
        else
        {
            msbox_params.ContentMessage = resMainView.popMissReq_Mess; 
            msbox_params.ContentTitle = resMainView.popMissReq_Title; 
            msbox_params.Icon = MsBox.Avalonia.Enums.Icon.Forbidden;
        }
        // The collection of IFs before fills 'msbox_params', then it's used to Pop Up a MessageBox
        _ = MessageBoxPopUp(msbox_params);
        ResetAfterExecute();
    }

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
        var filesEnum = e.Data.GetFiles();
        var text = e.Data.GetText();
        if (filesEnum is not null)
        {
            var files = new List<IStorageItem>(filesEnum);
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() => ControlBox_DropResult(ControlBox_HandleDrop(tc, files[0].Path.LocalPath), tc));
            return;
        }
        else if (!string.IsNullOrWhiteSpace(text))
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
