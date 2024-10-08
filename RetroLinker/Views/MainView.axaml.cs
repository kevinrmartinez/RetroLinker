﻿/*
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
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using RetroLinker.Models;
using RetroLinker.Models.Icons;
using RetroLinker.Models.LinFunc;
using RetroLinker.Models.WinFuncImport;
using RetroLinker.Translations;

namespace RetroLinker.Views;

public partial class MainView : UserControl
{
    System.TimeSpan timeSpan;
    public MainView()
    {
        // Constructor for Designer
        settings = AvaloniaOps.DesignerMainViewPreConstruct();
        InitializeComponent();
        ParentWindow = new MainWindow(true);
    }
    
    public MainView(MainWindow mainWindow)
    {
        settings = AvaloniaOps.MainViewPreConstruct();
        InitializeComponent();
        ParentWindow = mainWindow;
        timeSpan = System.DateTime.Now - App.LaunchTime;
        System.Diagnostics.Debug.WriteLine($"Execution time after MainView(): {timeSpan.ToString()}", App.TimeTrace);
    }

    // Window Object
    private MainWindow ParentWindow;
    
    private bool FormFirstLoad = true;
    // private string DefLinRAIcon;
    private int PrevConfigsCount;
    private int DLLErrorCount = 0;
    private int PreloadedIconsCount;
    private byte CurrentTheme = 250;
    private Settings settings;
    private Bitmap ICONimage;
    private IconsItems IconItemSET;
    private Shortcutter BuildingLink;
    private bool LinkCustomName;
    private static Shortcutter OutputLink;

    // true = Windows. false = Linux.
    private readonly bool DesktopOS = System.OperatingSystem.IsWindows();
    
    
    #region LOAD EVENTS
    void View1_Loaded(object sender, RoutedEventArgs e)
    {
        if (FormFirstLoad)
        {
            AvaloniaOps.MainViewLoad(DesktopOS);

#if DEBUG
            // TODO: Implement an Event for theme handling
            try
            {
                ParentWindow.RequestedThemeVariant = LoadThemeVariant();
            }
            catch (System.Exception theme_e) 
            {
                System.Diagnostics.Debug.WriteLine(theme_e);
                TopLevel.GetTopLevel(this).RequestedThemeVariant = ThemeVariant.Light;
            }
#else
            ParentWindow.RequestedThemeVariant = LoadThemeVariant();
#endif
        
            // Based on current OS
            if (!DesktopOS)
            {
                if (string.IsNullOrEmpty(settings.DEFRADir)) 
                { settings.DEFRADir = App.RetroBin; }
                txtRADir.IsReadOnly = false;
                // DefLinRAIcon = AvaloniaOps.DefLinRAIcon;
            }
            else
            {
                WinFuncImport();
                IconItemSET = new();
            }

            // BuildingLink = ParentWindow.BuildingLink;
            BuildingLink = new Shortcutter();
            txtLINKDir.PropertyChanged += TxtLINKDir_OnPropertyChanged;
            ApplySettingsToControls();
            comboCore_Loaded(AvaloniaOps.GetCoresArray());
            comboConfig_Loaded();
            comboICONDir_Loaded(AvaloniaOps.GetIconList());
            
            ApplyArgs();
            
            // TODO: Tutorial event for new users
            
            System.DateTime now = System.DateTime.Now;
            timeSpan = now - App.LaunchTime;
            System.Diagnostics.Debug.WriteLine($"Execution time after View1_Loaded(): {timeSpan}", App.TimeTrace);
            FormFirstLoad = false;
        }
        else
        { LoadNewSettings(); }
    }

    async void comboCore_Loaded(Task<string[]> coresTask)
    {
        var cores = await coresTask;
        System.Diagnostics.Trace.WriteLine("Cores list imported.", App.InfoTrace);
        if (cores.Length < 1) { lblNoCores.IsVisible = true; }
        else { comboCore.ItemsSource = cores; }
     }

    void comboConfig_Loaded()
    {
        foreach (var config in FileOps.ConfigDir)
        {
            comboConfig.Items.Add(config);
        }
        comboConfig.SelectedIndex = 0;
    }

    async void comboICONDir_Loaded(Task<object[]> iconsTask)
    {
        var iconsObject = await iconsTask;
        var iconsList = (List<string>)iconsObject[0];
        var hasError = (bool)iconsObject[1];
        var exception = (string)iconsObject[2];

        if (!hasError)
        {
            comboICONDir.Items.Add(resMainView.comboDefItem);
            System.Diagnostics.Trace.WriteLine("Icons list imported", App.InfoTrace);
            foreach (var iconFile in iconsList)
            {
                comboICONDir.Items.Add(iconFile);
            }

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
            MessageBoxPopUp(popParams);
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
        if (!string.IsNullOrEmpty(settings.DEFRADir))
        { txtRADir.Text = settings.DEFRADir; }
        BuildingLink.RAdir = settings.DEFRADir;
        AvaloniaOps.SetROMTop(settings.DEFROMPath, ParentWindow);
        AvaloniaOps.SetDesktopStorageFolder(ParentWindow);
        
        PrevConfigsCount = (settings.PrevConfig) ? SettingsOps.PrevConfigs.Count : -1;

        txtLINKDir.Watermark = "Super Mario Bros";
        txtLINKDir.Watermark += (DesktopOS) ? FileOps.WinLinkExt : FileOps.LinLinkExt;
        AllwaysAskOutputLink(settings.AllwaysAskOutput);
    }

    void ApplyArgs()
    {
        // TODO: Add argument loading support. 2 Cases: Opening existing links; Starting from a ROM. 
        System.Diagnostics.Debug.WriteLine("bleh", App.DebgTrace);
    }

    void LoadLocalization()
    {
        if (FormFirstLoad) return;
        ParentWindow.LocaleReload(settings);
    }

    void WinFuncImport()
    {
        try
        {
            FuncLoader.ImportWinFunc(); 
            IconProc.StartImport();
        }
        catch (System.Exception eMain)
        {
            System.Diagnostics.Trace.WriteLine($"The importing of {FuncLoader.WinOnlyLib} has failed!", App.ErroTrace);
            System.Diagnostics.Debug.WriteLine($"In MainView, the element {eMain.Source} had returned the error:\n {eMain.Message}", App.ErroTrace);
            lock (this)
            { WinFuncImportFail(eMain); }
        }
    }

    async Task WinFuncImportFail(System.Exception eMain)
    {
        string retry_btn = resGeneric.btnRetry;
        string abort_btn = resGeneric.btnAbort;
        ButtonDefinition[] diagBtns;
        if (DLLErrorCount < 6)
        {
            diagBtns = new[]
            {
                new ButtonDefinition {Name = retry_btn},
                new ButtonDefinition {Name = abort_btn, IsCancel = true, IsDefault = true}
            };
        }
        else 
        { 
            diagBtns = new[] 
            { new ButtonDefinition { Name = abort_btn, IsCancel = true, IsDefault = true } };
        }

        var msbParams = new MessageBoxCustomParams()
        {
            MaxWidth = 550,
            ShowInCenter = true,
            Icon = MsBox.Avalonia.Enums.Icon.Error,
            ContentTitle = resGeneric.genFatalError,
            ContentHeader = string.Format(resMainView.dllErrorHead, FuncLoader.WinOnlyLib),
            ContentMessage = $"{resMainView.dllErrorMess}\n{eMain.Message}\n\n{resMainView.dllErrorMess2}",
            ButtonDefinitions = diagBtns
        };

        var diagResult = await MessageBoxPopUp(msbParams);
        if (diagResult == abort_btn)
        {
            ParentWindow.Close();
        }
        else if (diagResult == retry_btn)
        {
            DLLErrorCount++;
            WinFuncImport();
        }
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

    string UpdateLinkLabel(string fileNameNoExt, string core)
    { 
        return (DesktopOS)
            ? FileOps.GetDefinedLinkPath(fileNameNoExt + FileOps.GetOutputExt(DesktopOS), settings.DEFLinkOutput) 
            : FileOps.GetDefinedLinkPath(LinDesktopEntry.StdDesktopEntry(fileNameNoExt, core) + FileOps.GetOutputExt(DesktopOS), settings.DEFLinkOutput);
        
    }

    string UpdateLinkLabelWithCore(string fullPath)
    {
        var fileDir = FileOps.GetDirFromPath(fullPath);
        var fileName = FileOps.GetFileNameFromPath(fullPath);
        var nameSplited = fileName.Split('.');
        nameSplited[1] = (string.IsNullOrWhiteSpace(comboCore.Text)) ? "[CORE]" : comboCore.Text;
        var newName = string.Empty;
        for (int i = 0; i < nameSplited.Length; i++)
        {
            newName += nameSplited[i];
            if (i != nameSplited.Length - 1) newName += '.';
        }

        return FileOps.CombineDirAndFile(fileDir, newName);
    }

    void LockForExecute(bool lockControls) => gridBODY.IsEnabled = !lockControls;

    void ResetAfterExecute()
    {
        //LinkCustomName = false;
        BuildingLink.OutputPaths.Add(
                ShortcutterOutput.RebuildOutputWithFriendly(OutputLink.OutputPaths[0], DesktopOS, string.Empty)
        );
        LockForExecute(false);
    }

    void FillIconBoxes(string path)
    {
        ICONimage = AvaloniaOps.GetBitmap(path);
        FillIconSource(ICONimage);
    }
    
    void FillIconBoxes(Bitmap bitmap)
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
    
    async Task<MsBox.Avalonia.Enums.ButtonResult> MessageBoxPopUp(MessageBoxStandardParams standardParams)
    {
        standardParams.WindowIcon = ParentWindow.Icon;
        standardParams.MaxWidth = 550;
        standardParams.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var msBox = MessageBoxManager.GetMessageBoxStandard(standardParams);
        var result = await msBox.ShowWindowDialogAsync(ParentWindow);
        return result;
    }
    
    async Task<string> MessageBoxPopUp(MessageBoxCustomParams customParams)
    {
        customParams.WindowIcon = ParentWindow.Icon;
        customParams.MaxWidth = 600;
        customParams.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var msBox = MessageBoxManager.GetMessageBoxCustom(customParams);
        var result = await msBox.ShowWindowDialogAsync(ParentWindow);
        return result;
    }

    async Task<bool> ResolveRenamePopUp(string givenPath, string givenCore, List<ShortcutterOutput> outputs)
    {
        var popupWindow = new PopUpWindow(ParentWindow);
        popupWindow.RenamePopUp(givenPath, givenCore, outputs);
        var result = await popupWindow.ShowDialog<List<ShortcutterOutput>>(ParentWindow);
        BuildingLink.OutputPaths = result;
        LinkCustomName = BuildingLink.OutputPaths[0].CustomEntryName;
        return true;
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

    // ShortcutterOutput getShortcutterOutput(string filePath, string core) => (DesktopOS)
    //     ? new ShortcutterOutput(filePath)
    //     : new ShortcutterOutput(filePath, core);
    #endregion


    // TOP CONTROLS
    async void btnSettings_Click(object sender, RoutedEventArgs e)
    {
        var settingWindow = new SettingsWindow(ParentWindow, settings); 
        var settingReturn =  await settingWindow.ShowDialog<Settings?>(ParentWindow);
        if (settingReturn is not null)
        {
            settings = settingReturn;
            FileOps.SetNewSettings(settings);
        }
        else settings = FileOps.LoadCachedSettingsFO();
        LoadNewSettings();
    }

    #region Icon Controls
    
    void rdoIcon_CheckedChanged(object sender, RoutedEventArgs e)
    {
        if ((bool)rdoIconDef.IsChecked)
        {
            comboICONDir.SelectedIndex = 0;
            gridIconControl.IsEnabled = false;
        }
        else { gridIconControl.IsEnabled = true; }
    }

    async void btnICONDir_Click(object sender, RoutedEventArgs e)
    {
        PickerOpt.OpenOpts opt;
        if (DesktopOS) { opt = PickerOpt.OpenOpts.WINico; }
        else { opt = PickerOpt.OpenOpts.LINico; }

        string currentFile = (comboICONDir.SelectedIndex >= PreloadedIconsCount)
            ? (string)comboICONDir.SelectedItem
            : string.Empty;
        string file = await AvaloniaOps.OpenFileAsync(opt, currentFile, ParentWindow);
        if (!string.IsNullOrEmpty(file))
        {
            int newIndex = comboICONDir.ItemCount;
            const int indexNotFound = -1;
            int existingItem = IconProc.IconItemsList.IndexOf(IconProc.IconItemsList.Find(Item => Item.FilePath == file));
            if (existingItem == indexNotFound)
            {
                comboICONDir.Items.Add(file);
                IconProc.BuildIconItem(file, newIndex, DesktopOS);
            }
            else { newIndex = (int)IconProc.IconItemsList[existingItem].comboIconIndex; }

            comboICONDir.SelectedIndex = newIndex;
        }
    }

    // Solution of SelectionChangedEventArgs thanks to snurre @ stackoverflow.com
    void comboICONDir_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {   
        int selectedIndex = comboICONDir.SelectedIndex;
        panelIconNoImage.IsVisible = false;
        if (selectedIndex > 0)
        {   // Fill the PictureBoxes with the icons provided by the user
            IconItemSET = IconProc.IconItemsList.Find(Item => Item.comboIconIndex == selectedIndex);
            BuildingLink.ICONfile = IconItemSET.FilePath;
            
            if (IconItemSET.IconStream != null)
            {
                IconItemSET.IconStream.Position = 0;
                var bitm = AvaloniaOps.GetBitmap(IconItemSET.IconStream);
                FillIconBoxes(bitm);
            }
            //else if (!DesktopOS && FileOps.IsVectorImage(item))
            else
            {
                try 
                { FillIconBoxes(BuildingLink.ICONfile); }
                catch 
                {
                    Bitmap bitm = new(AssetLoader.Open(AvaloniaOps.GetNAimage()));
                    FillIconBoxes(bitm);
                    panelIconNoImage.IsVisible = true;
                } 
            }
        }
        else
        {   // Fill the PictureBoxes with the default icon (Index 0)
            Bitmap bitm = new(AssetLoader.Open(AvaloniaOps.GetDefaultIcon()));
            FillIconBoxes(bitm);
        }
    }
    #endregion
    
    #region RADirectory Controls
    
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
        BuildingLink.RAdir = file;
        txtRADir.Text = file;
    }
    #endregion

    #region ROM Controls
    
    void chkContentless_CheckedChanged(object sender, RoutedEventArgs e)
    {
        panelROMDirControl.IsEnabled = !(bool)chkContentless.IsChecked;
    }

    async void btnROMDir_Click(object sender, RoutedEventArgs e)
    {
        string currentFile = (string.IsNullOrEmpty(txtROMDir.Text)) ? string.Empty : txtROMDir.Text;
        string file = await AvaloniaOps.OpenFileAsync(PickerOpt.OpenOpts.RAroms, currentFile, ParentWindow);
        if (!string.IsNullOrEmpty(file))
        {
            BuildingLink.ROMdir = file;
            // OutputLink.ROMname = file;
            txtROMDir.Text = file;
        }
    }
    
    private void BtnPatches_OnClick(object? sender, RoutedEventArgs e)
    {
        ParentWindow.ChangeOut(MainWindow.ViewsTypes.PatchesView, [BuildingLink.PatchArg]);
    }
    #endregion

    #region RACore Controls
    
    private void ComboCore_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtLINKDir.Text) || DesktopOS || LinkCustomName) return;
        
        string newFile;
        string outputDir;
        if (settings.AllwaysAskOutput)
        {
            newFile = LinDesktopEntry.StdDesktopEntry(BuildingLink.OutputPaths[0].FriendlyName + FileOps.LinLinkExt, comboCore.Text);
            outputDir = FileOps.GetDirFromPath(BuildingLink.OutputPaths[0].FullPath);
            txtLINKDir.Text = FileOps.CombineDirAndFile(outputDir, newFile);
        }
        else
        {
            newFile = LinDesktopEntry.StdDesktopEntry(txtLINKDir.Text + FileOps.LinLinkExt, comboCore.Text);
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
    
    void comboConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        switch (comboConfig.SelectedIndex)
        {
            case -1:
                //comboConfig.SelectedIndex = 0;
                break;
            case 0:
                BuildingLink.CONFfile = string.Empty;
                break;
            default:
                BuildingLink.CONFfile = comboConfig.SelectedItem.ToString();
                break;
        }
    }

    async void btnCONFIGDir_Click(object sender, RoutedEventArgs e)
    {
        string currentFile = (comboConfig.SelectedIndex > 0) ? (string)comboConfig.SelectedItem : string.Empty;
        var file = await AvaloniaOps.OpenFileAsync(PickerOpt.OpenOpts.RAcfg, currentFile, ParentWindow);
        if (!string.IsNullOrEmpty(file))
        {
            if (!comboConfig.Items.Contains(file))
            {
                comboConfig.Items.Add(file);
                if (settings.PrevConfig) { SettingsOps.PrevConfigs.Add(file); }
            }
            comboConfig.SelectedItem = file;
        }
    }
    
    void btnAppendConfig_Click(object sender, RoutedEventArgs e)
    {
        // TODO
    }
    #endregion

    #region LinkPath Controls
    
    private void BtnMoreParams_OnClick(object? sender, RoutedEventArgs e)
    {
        System.Diagnostics.Trace.WriteLine("COMING SOON");
    }
    
    async void btnLINKDir_Click(object sender, RoutedEventArgs e)
    {
        LinkCustomName = false;
        PickerOpt.SaveOpts opt;
        opt = (DesktopOS) ? PickerOpt.SaveOpts.WINlnk : PickerOpt.SaveOpts.LINdesktop;
        string currentFile = (string.IsNullOrEmpty(txtLINKDir.Text)) ? string.Empty : txtLINKDir.Text;
        string file = await AvaloniaOps.SaveFileAsync(opt, currentFile, ParentWindow);
        if (!string.IsNullOrEmpty(file))
        {
            if (!DesktopOS)
            {
                await ResolveRenamePopUp(file, comboCore.Text, BuildingLink.OutputPaths);
                file = BuildingLink.OutputPaths[0].FullPath;
            }
            txtLINKDir.Text = file;
        }
#if DEBUG
        else
        {
            System.Diagnostics.Debug.WriteLine("Running on debug...", App.DebgTrace);
            
            // Testing.LinShortcutTest(DesktopOS);
            // var bitm = FileOps.IconExtractTest(); FillIconBoxes(bitm);
        }
#endif
    }
    
    private async void BtnLINKRename_OnClick(object? sender, RoutedEventArgs e)
    {
        LinkCustomName = false;
        const string NamePlaceHolder = LinDesktopEntry.NamePlaceHolder;
        var fullPath = FileOps.CombineDirAndFile(
            settings.DEFLinkOutput, 
            (string.IsNullOrWhiteSpace(txtLINKDir.Text) ? NamePlaceHolder : txtLINKDir.Text)
        );
        await ResolveRenamePopUp(fullPath, comboCore.Text, BuildingLink.OutputPaths);
        
        if (BuildingLink.OutputPaths.Count == 0) return;
        txtLINKDir.Text = BuildingLink.OutputPaths[0].FriendlyName;
        lblLinkDefinedDir.Text = BuildingLink.OutputPaths[0].FullPath;
    }

    void txtLINKDir_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (settings.AllwaysAskOutput) return;
        if (BuildingLink.OutputPaths.Count > 0)
            if (BuildingLink.OutputPaths[0].CustomEntryName) return;
        lblLinkDefinedDir.Text = (!string.IsNullOrWhiteSpace(txtLINKDir.Text)) 
            ? UpdateLinkLabel(txtLINKDir.Text, comboCore.Text)
            : settings.DEFLinkOutput;
    }
    
    private void TxtLINKDir_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "IsReadOnly") txtLINKDir.Text = string.Empty;
    }
    #endregion

    
    // EXECUTE
    void btnEXECUTE_Click(object sender, RoutedEventArgs e)
    {
        OutputLink = new Shortcutter(BuildingLink);
        BuildingLink.OutputPaths = new();
        var msbox_params = new MessageBoxStandardParams();

        // Controls Lock
        LockForExecute(true);
        
        // Checkboxes!
        OutputLink.VerboseB = (bool)chkVerb.IsChecked;
        OutputLink.FullscreenB = (bool)chkFull.IsChecked;
        OutputLink.AccessibilityB = (bool)chkAccessi.IsChecked;

        // Validating contentless or not
        OutputLink.ROMdir = ((bool)chkContentless.IsChecked) ? Commander.contentless : OutputLink.ROMdir;

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
                var outputPathStr = !settings.AllwaysAskOutput 
                    ? FileOps.GetDefinedLinkPath(txtLINKDir.Text + FileOps.GetOutputExt(DesktopOS), settings.DEFLinkOutput) 
                    : (txtLINKDir.Text);
                outputPath = new ShortcutterOutput(outputPathStr);
            }
            else
            {
                if (OutputLink.OutputPaths[0] is not null && OutputLink.OutputPaths[0].CustomEntryName)
                    outputPath = OutputLink.OutputPaths[0];
                else
                {
                    if (!settings.AllwaysAskOutput)
                    {
                        var outputPathStr = FileOps.GetDefinedLinkPath(txtLINKDir.Text + FileOps.GetOutputExt(DesktopOS),
                            settings.DEFLinkOutput);
                        outputPath = new ShortcutterOutput(outputPathStr, OutputLink.ROMcore);
                    }
                    else outputPath = ShortcutterOutput.RebuildOutputWithFriendly(OutputLink.OutputPaths[0], DesktopOS, OutputLink.ROMcore);
                }
            }

            if (OutputLink.OutputPaths.Count == 0)
                OutputLink.OutputPaths.Add(outputPath);
            else
            {
                if (OutputLink.OutputPaths[0].FullPath != outputPath.FullPath)
                    OutputLink.OutputPaths[0] = outputPath;
            }
        }
        
        // Include a link description, if any
        OutputLink.Desc = (string.IsNullOrWhiteSpace(txtDesc.Text)) ? string.Empty : txtDesc.Text;

        // Icons handling
        // RA binary icon (Default)
        if (comboICONDir.SelectedIndex == 0)
        { OutputLink.ICONfile = string.Empty; }
        // TODO: Consider moving this part after the required fields check
        else
        {
            // If it's Windows OS, the images may have to be converted to .ico
            if (IconItemSET.ConvertionRequiered)
            {
                OutputLink.ICONfile = FileOps.SaveWinIco(IconItemSET);
                string ROMIcoSavAUX = (string.IsNullOrEmpty(OutputLink.ROMdir)) ? OutputLink.RAdir : OutputLink.ROMdir; 
                OutputLink.ICONfile = settings.IcoSavPath switch
                {
                    SettingsOps.IcoSavROM => FileOps.CpyIconToCustomSet(OutputLink.ICONfile, ROMIcoSavAUX),
                    SettingsOps.IcoSavRA => FileOps.CpyIconToCustomSet(OutputLink.ICONfile, OutputLink.RAdir),
                    _ => FileOps.CpyIconToUsrSet(OutputLink.ICONfile)
                };
                if (settings.IcoLinkName) OutputLink.ICONfile = FileOps.ChangeIcoNameToLinkName(OutputLink);
            }

            // In case of 'CpyUserIcon = true'
            if (settings.CpyUserIcon) OutputLink.ICONfile = FileOps.CpyIconToUsrSet(OutputLink.ICONfile);
        }

        // REQUIRED FIELDS VALIDATION!
        if ((!string.IsNullOrEmpty(OutputLink.RAdir))
            && (!string.IsNullOrEmpty(OutputLink.ROMdir))
            && (!string.IsNullOrEmpty(OutputLink.ROMcore))
            && (OutputLink.OutputPaths[0].ValidOutput))
        {
            System.Diagnostics.Debug.WriteLine("All fields for link creation have benn accepted.", App.InfoTrace);
            
            // Double quotes for directories that are parameters ->
            // -> for the ROM file
            if (!(bool)chkContentless.IsChecked) 
            { OutputLink.ROMdir = Utils.FixUnusualPaths(OutputLink.ROMdir); }

            // -> for the config file
            if (!string.IsNullOrEmpty(OutputLink.CONFfile)) 
            { OutputLink.CONFfile = Utils.FixUnusualPaths(OutputLink.CONFfile); }

            // Link Copies handling
            if (settings.MakeLinkCopy)
            { OutputLink.OutputPaths.AddRange(FileOps.GetLinkCopyPaths(SettingsOps.LinkCopyPaths, OutputLink.OutputPaths[0]));}

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
                    MessageBoxPopUp(msbox_params);
                }   
                else
                {
                    msbox_params.ContentHeader = resMainView.popSingleOutput0_Head; 
                    msbox_params.ContentTitle = resGeneric.genError;
                    msbox_params.ContentMessage = $"{resMainView.popSingleOutput0_Mess} \n {opResult[0].eMesseage}";
                    msbox_params.Icon = MsBox.Avalonia.Enums.Icon.Error; 
                    MessageBoxPopUp(msbox_params);
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
                    MessageBoxPopUp(msbox_params);
                }
                else
                {
                    msbox_params.ContentHeader = resMainView.popMultiOutput0_Head;
                    int successCount = 0;
                    int errorCount = 0;
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
                            errorCount++;
                        }
                        else { successCount++; }
                    }
                    msbox_params.ContentTitle = resGeneric.genWarning;
                    msbox_params.Icon = (successCount > 0) ? MsBox.Avalonia.Enums.Icon.Warning : MsBox.Avalonia.Enums.Icon.Error;
                    msbox_params.ContentMessage = content;
                    MessageBoxPopUp(msbox_params);
                }
            }
        }
        else
        {
            msbox_params.ContentMessage = resMainView.popMissReq_Mess; 
            msbox_params.ContentTitle = resMainView.popMissReq_Title; 
            msbox_params.Icon = MsBox.Avalonia.Enums.Icon.Forbidden;
            MessageBoxPopUp(msbox_params);
        }
        ResetAfterExecute();
    }

    // CLOSING
    void View1_Unloaded(object sender, RoutedEventArgs e)
    {
        var cachedSettings = SettingsOps.GetCachedSettings();
        bool settingsChanged = !settings.Equals(cachedSettings);
        if ( ((PrevConfigsCount != SettingsOps.PrevConfigs.Count) && (PrevConfigsCount > -1)) || settingsChanged) 
        { SettingsOps.WriteSettings(settings); }  
    }
}
