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
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using RetroLinker.Models;
using RetroLinker.Models.Icons;
using RetroLinker.Models.WinFuncImport;
using RetroLinker.Translations;

namespace RetroLinker.Views;

public partial class MainView : UserControl
{
    System.TimeSpan timeSpan;
    public MainView()
    { 
        InitializeComponent();
        timeSpan = System.DateTime.Now - App.LaunchTime;
        System.Diagnostics.Debug.WriteLine(string.Format("Tiempo de ejecuacion tras MainView(): {0}", timeSpan.ToString()), App.TimeTrace);
    }
    
    public MainView(MainWindow mainWindow)
    { 
        InitializeComponent();
        ParentWindow = mainWindow;
        timeSpan = System.DateTime.Now - App.LaunchTime;
        System.Diagnostics.Debug.WriteLine(string.Format("Tiempo de ejecuacion tras MainView(): {0}", timeSpan.ToString()), App.TimeTrace);
        // System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("es");
    }

    // Window Object
    private MainWindow ParentWindow;
    
    private bool FirstTimeLoad = true;
    private string DefLinRAIcon;
    private int PrevConfigsCount;
    private int retrycount = 0;
    private byte CurrentTheme = 250;
    private Shortcutter OutputLink = new();
    private Settings settings;
    private Bitmap ICONimage;
    private IconsItems IconItemSET;

    // true = Windows. false = Linux.
    // Esto es asumiendo que solo podra correr en Windows y Linux.
    private bool DesktopOS = System.OperatingSystem.IsWindows();
    
    // TODO: Implementar evento de manejo de tema
    #region LOAD EVENTS
    // LOADS
    void View1_Loaded(object sender, RoutedEventArgs e)
    {
        if (FirstTimeLoad)
        {
            System.Diagnostics.Trace.WriteLine($"OS actual: {System.Environment.OSVersion.VersionString}.", App.InfoTrace);
            SettingsOps.BuildConfFile();
            settings = FileOps.LoadSettingsFO();
            System.Diagnostics.Debug.WriteLine("Settings cargadas para la MainView.", App.InfoTrace);
            System.Diagnostics.Debug.WriteLine("Settings convertido a Base64:" + settings.GetBase64(), App.DebgTrace);
        
            // El designer de Avalonia se rompe en esta parte, buscar una manera alterna de realizar en DEGUB, o algo especifico de designer
            ParentWindow.RequestedThemeVariant = LoadThemeVariant();
        
            var cores_task = FileOps.LoadCores();
            var icon_task = FileOps.LoadIcons(DesktopOS);
        
            // Condicion de OS
            if (!DesktopOS)
            {
                if (string.IsNullOrEmpty(settings.DEFRADir)) 
                { settings.DEFRADir = App.RetroBin; }
                txtRADir.IsReadOnly = false;
                DefLinRAIcon = FileOps.GetRAIcons();
            }
            else
            {
                WinFuncImport();
                IconItemSET = new();
            }
            ApplySettingsToControls();
            comboCore_Loaded(cores_task);
            comboConfig_Loaded();
            comboICONDir_Loaded(icon_task);
            // TODO: Evento de tutorial para nuevos usuarios

            FirstTimeLoad = false;
            System.DateTime now = System.DateTime.Now;
            timeSpan = now - App.LaunchTime;
            System.Diagnostics.Debug.WriteLine($"Ejecuacion tras View1_Loaded(): {timeSpan}", App.TimeTrace);
        }
        else
        {
            LoadNewSettings();
        }
    }

    async void comboCore_Loaded(Task<string[]> cores_task)
    {
        var cores = await cores_task;
        System.Diagnostics.Debug.WriteLine("Lista de Cores importada.", App.InfoTrace);
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

    async void comboICONDir_Loaded(Task<List<string>> icon_task)
    {
        var icons_list = await icon_task;
        comboICONDir.Items.Add(resMainView.comboDefItem);
        System.Diagnostics.Debug.WriteLine("Lista de iconos importada", App.InfoTrace);
        foreach (var iconFile in icons_list)
        {
            comboICONDir.Items.Add(iconFile);
        }
        comboICONDir.SelectedIndex++;
        rdoIconDef.IsChecked = true;
    }
    #endregion

    #region Funciones
    // FUNCIONES
    void LoadNewSettings()
    {
        settings = FileOps.LoadCachedSettingsFO();
        ApplySettingsToControls();
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
        System.Diagnostics.Trace.WriteLine($"El tema solicitado fue: {theme}", App.InfoTrace);
        System.Diagnostics.Debug.WriteLine($"Codigo en byte: {settings.PreferedTheme}", App.DebgTrace);
        return theme;
    }

    void ApplySettingsToControls()
    {
        if (!string.IsNullOrEmpty(settings.DEFRADir))
        { txtRADir.Text = settings.DEFRADir; }
        OutputLink.RAdir = settings.DEFRADir;
        AvaloniaOps.SetROMPadre(settings.DEFROMPath, ParentWindow);
        
        PrevConfigsCount = (settings.PrevConfig) ? SettingsOps.PrevConfigs.Count : -1;

        txtLINKDir.Watermark = "Super Mario Bros";
        txtLINKDir.Watermark += (DesktopOS) ? FileOps.WinLinkExt : FileOps.LinLinkExt;
        if (settings.AllwaysAskOutput) { LinkCustomPathSetting(); }
        else { LinkLoadedPathSetting(); }
    }

    void WinFuncImport()
    {
        try { FuncLoader.ImportWinFunc(); IconProc.StartImport(); }
        catch (System.Exception eMain)
        {
            System.Diagnostics.Trace.WriteLine($"El importado de {FuncLoader.WinOnlyLib} ha fallado!", App.ErroTrace);
            System.Diagnostics.Debug.WriteLine($"En MainView, el elemento {eMain.Source} a retrornado el error {eMain.Message}", App.ErroTrace);
            lock (this)
            { WinFuncImportFail(eMain); }
        }
    }

    async Task WinFuncImportFail(System.Exception eMain)
    {
        string retry_btn = resMainView.btnRetry;
        string abort_btn = resMainView.btnAbort;
        ButtonDefinition[] diag_btns;
        if (retrycount < 6)
        {
            diag_btns = new[]
            {
                new ButtonDefinition {Name = retry_btn},
                new ButtonDefinition {Name = abort_btn, IsCancel = true, IsDefault = true}
            };
        }
        else 
        { 
            diag_btns = new[] 
            { new ButtonDefinition { Name = abort_btn, IsCancel = true, IsDefault = true } };
        }

        var msb_params = new MessageBoxCustomParams()
        {
            MaxWidth = 550,
            ShowInCenter = true,
            Icon = MsBox.Avalonia.Enums.Icon.Error,
            ContentTitle = resMainView.genFatalError,
            ContentHeader = string.Format(resMainView.dllErrorHead, FuncLoader.WinOnlyLib),
            ContentMessage = string.Format(resMainView.dllErrorMess, eMain.Message),
            ButtonDefinitions = diag_btns
        };

        var diag_result = await MessageBoxPopUp(msb_params);
        if (diag_result == abort_btn)
        {
            ParentWindow.Close();
        }
        else if (diag_result == retry_btn)
        {
            retrycount++;
            WinFuncImport();
        }
    }

    void LinkCustomPathSetting()
    {
        lblLinkDir.IsVisible = true;
        lblLinkName.IsVisible = false;
        txtLINKDir.IsReadOnly = true;
        btnLINKDir.IsEnabled = true;
        lblLinkDeskDir.IsVisible = false;
    }

    void LinkLoadedPathSetting()
    {
        lblLinkDir.IsVisible = false;
        lblLinkName.IsVisible = true;
        txtLINKDir.IsReadOnly = false;
        txtLINKDir.AcceptsReturn = false;
        btnLINKDir.IsEnabled = false;
        lblLinkDeskDir.IsVisible = true;
        lblLinkDeskDir.Content = settings.DEFLinkOutput;
    }

    void FillIconBoxes(string DIR)
    {
        ICONimage = AvaloniaOps.GetBitmap(DIR);
        pic16.Source = ICONimage;
        pic32.Source = ICONimage;
        pic64.Source = ICONimage;
        pic128.Source = ICONimage;
    }
    
    void FillIconBoxes(Bitmap bitmap)
    {
        ICONimage = bitmap;
        pic16.Source = ICONimage;
        pic32.Source = ICONimage;
        pic64.Source = ICONimage;
        pic128.Source = ICONimage;
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

    void ValidateLINBin()
    {
        if (OutputLink.RAdir == txtRADir.Text) return;
        OutputLink.RAdir = string.IsNullOrWhiteSpace(txtRADir.Text) ? string.Empty : txtRADir.Text;
    }
    #endregion


    // TOP CONTROLS
    async void btnSettings_Click(object sender, RoutedEventArgs e)
    {
        var settingWindow = new SettingsWindow(ParentWindow); 
        await settingWindow.ShowDialog(ParentWindow); 
        LoadNewSettings();
    }

    #region Icon Controls
    // Usuario llenando object del Icono
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
        
        string file = await AvaloniaOps.OpenFileAsync(opt, ParentWindow);
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

    // Solucion del SelectionChangedEventArgs gracias a snurre en stackoverflow.com
    void comboICONDir_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {   
        int selectedIndex = comboICONDir.SelectedIndex;
        panelIconNoImage.IsVisible = false;
        if (selectedIndex > 0)
        {   // llena los controles pic con los iconos provistos por el usuario
            IconItemSET = IconProc.IconItemsList.Find(Item => Item.comboIconIndex == selectedIndex);
            OutputLink.ICONfile = IconItemSET.FilePath;
            
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
                { FillIconBoxes(OutputLink.ICONfile); }
                catch 
                {
                    Bitmap bitm = new(AssetLoader.Open(AvaloniaOps.GetNAimage()));
                    FillIconBoxes(bitm);
                    panelIconNoImage.IsVisible = true;
                } 
            }
        }
        else
        {   // llena los controles pic con el icono default (Indice 0)
            Bitmap bitm = new(AssetLoader.Open(AvaloniaOps.GetDefaultIcon()));
            FillIconBoxes(bitm);
        }
    }
    #endregion

    // Usuario llenando object del link
    #region RADirectory Controls
    // RetroArch Directory
    async void btnRADir_Click(object sender, RoutedEventArgs e)
    {
        PickerOpt.OpenOpts opt;
        if (DesktopOS) { opt = PickerOpt.OpenOpts.RAexe; }        // FilePicker Option para .exe de Windows
        else { opt = PickerOpt.OpenOpts.RAbin; }                  // FilePicker Option para .AppImage de Windows
        string file = await AvaloniaOps.OpenFileAsync(opt, ParentWindow);
        if (!string.IsNullOrEmpty(file))
        {
            OutputLink.RAdir = file;
            txtRADir.Text = file;
        }
    }
    #endregion

    #region ROM Controls
    // ROM
    void chkContentless_CheckedChanged(object sender, RoutedEventArgs e)
    {
        panelROMDirControl.IsEnabled = !(bool)chkContentless.IsChecked;
    }

    async void btnROMDir_Click(object sender, RoutedEventArgs e)
    {
        string file = await AvaloniaOps.OpenFileAsync(PickerOpt.OpenOpts.RAroms, ParentWindow);
        if (!string.IsNullOrEmpty(file))
        {
            OutputLink.ROMdir = file;
            OutputLink.ROMfile = file;
            txtROMDir.Text = file;
        }
    }

    // TODO
    void btnPatches_Click(object sender, RoutedEventArgs e)
    {
        
    }
    #endregion

    #region RACore Controls
    // CORE
    void btnSubSys_Click(object sender, RoutedEventArgs e)
    {
        // TODO    
    }
    #endregion

    #region Config Controls
    // CONFIG
    void comboConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        switch (comboConfig.SelectedIndex)
        {
            case -1:
                //comboConfig.SelectedIndex = 0;
                break;
            case 0:
                OutputLink.CONFfile = string.Empty;
                break;
            default:
                OutputLink.CONFfile = comboConfig.SelectedItem.ToString();
                break;
        }
    }

    async void btnCONFIGDir_Click(object sender, RoutedEventArgs e)
    {
        var file = await AvaloniaOps.OpenFileAsync(PickerOpt.OpenOpts.RAcfg, ParentWindow);
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

    #region LinkDir Controls
    // Link Directory
    async void btnLINKDir_Click(object sender, RoutedEventArgs e)
    {
        if (!DesktopOS && settings.LinDesktopPopUp)
        {
            var msbox_params = new MessageBoxStandardParams()
            {
                ContentTitle = resMainView.LinPopUp_Title,
                ContentHeader = resMainView.LinPopUp_Head,
                ContentMessage = resMainView.LinPopUp_Mess,
                ButtonDefinitions = MsBox.Avalonia.Enums.ButtonEnum.OkCancel,
                Icon = MsBox.Avalonia.Enums.Icon.Folder
            };
            var boxResult = await MessageBoxPopUp(msbox_params);
            if (boxResult == MsBox.Avalonia.Enums.ButtonResult.Cancel) { settings.LinDesktopPopUp = false; }
        }
        
        PickerOpt.SaveOpts opt;
        opt = (DesktopOS) ? PickerOpt.SaveOpts.WINlnk : PickerOpt.SaveOpts.LINdesktop;
        string file = await AvaloniaOps.SaveFileAsync(template:opt, ParentWindow);
        if (!string.IsNullOrEmpty(file))
        {
            OutputLink.LNKdir = file;
            txtLINKDir.Text = OutputLink.LNKdir;
        }
#if DEBUG
        else
        {
            System.Diagnostics.Debug.WriteLine("Running on debug...");
            // Testing.LinShortcutTest(DesktopOS);
            // var bitm = FileOps.IconExtractTest(); FillIconBoxes(bitm);
        }
#endif
    }

    void txtLINKDir_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (settings.AllwaysAskOutput) return;
        lblLinkDeskDir.Content = !string.IsNullOrWhiteSpace(txtLINKDir.Text) ? FileOps.GetDefinedLinkPath(txtLINKDir.Text, settings.DEFLinkOutput, DesktopOS) 
                                                                             : FileOps.UserDesktop;
    }
    #endregion

    
    // EXECUTE
    // La accion ocurre aqui
    void btnEXECUTE_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement a control lock to prevent fields changing during operations
        bool ShortcutPosible;
        var msbox_params = new MessageBoxStandardParams();

        // CHECKS!
        OutputLink.VerboseB = (bool)chkVerb.IsChecked;
        OutputLink.FullscreenB = (bool)chkFull.IsChecked;
        OutputLink.AccessibilityB = (bool)chkAccessi.IsChecked;

        // Validando si sera contentless o no
        OutputLink.ROMdir = ((bool)chkContentless.IsChecked) ? Commander.contentless : OutputLink.ROMfile;

        // Validar que haya ejecutable (LINUX)
        ValidateLINBin();

        // Validando que haya un core
        OutputLink.ROMcore = (string.IsNullOrWhiteSpace(comboCore.Text)) ? string.Empty : comboCore.Text;

        // Manejo del link en caso de 'AllwaysAskOutput = false'
        if (!settings.AllwaysAskOutput && !string.IsNullOrWhiteSpace(txtLINKDir.Text))
        {
            OutputLink.LNKdir = FileOps.GetDefinedLinkPath(txtLINKDir.Text, settings.DEFLinkOutput, DesktopOS);
        }

        // Validando que haya descripcion o no
        OutputLink.Desc = (string.IsNullOrWhiteSpace(txtDesc.Text)) ? string.Empty : txtDesc.Text;

        // Manejo de iconos
        // Icono del ejecutable (Default)
        if (comboICONDir.SelectedIndex == 0)
        { OutputLink.ICONfile = string.Empty; }
        // TODO: Conciderar mover esta parte despues de la validacion
        else
        {
            // En caso de ser Winodws OS, hay que convertir las imagenes a .ico
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

            // En caso de tener que copiar el icono provisto por el usuario
            if (settings.CpyUserIcon)
            { OutputLink.ICONfile = FileOps.CpyIconToUsrSet(OutputLink.ICONfile); }
        }

        // REQUIERED FIELDS VALIDATION!
        if ((!string.IsNullOrEmpty(OutputLink.RAdir)) 
         && (!string.IsNullOrEmpty(OutputLink.ROMdir)) 
         && (!string.IsNullOrEmpty(OutputLink.ROMcore))
         && (!string.IsNullOrEmpty(OutputLink.LNKdir))
         )
        { ShortcutPosible = true; System.Diagnostics.Debug.WriteLine("Todos los campos para el shortcut han sido aceptados", App.InfoTrace); }
        else
        {
            ShortcutPosible = false;
            msbox_params.ContentMessage = resMainView.popMissReq_Mess; 
            msbox_params.ContentTitle = resMainView.popMissReq_Title; 
            msbox_params.Icon = MsBox.Avalonia.Enums.Icon.Forbidden;
            MessageBoxPopUp(msbox_params);
        }

        if (!ShortcutPosible) return;
        // Comillas para directorios que iran de parametros...
        // para el directorio de la ROM
        // TODO: Use the Contentless checkbox
        if (OutputLink.ROMdir != Commander.contentless) 
        { OutputLink.ROMdir = Utils.FixUnusualDirectories(OutputLink.ROMdir); }

        // para el archivo config
        if (!string.IsNullOrEmpty(OutputLink.CONFfile)) 
        { OutputLink.CONFfile = Utils.FixUnusualDirectories(OutputLink.CONFfile); }

        // Manejo de Link Copies
#if DEBUG
        // SettingsOps.LinkCopyPaths = new List<string>()
        // {
        //     System.IO.Path.Combine(FileOps.UserDesktop, "testing", "test1"),
        //     System.IO.Path.Combine(FileOps.UserDesktop, "testing", "test2"),
        // };
#endif
        if (settings.MakeLinkCopy)
        {
            //TODO: Move operation to FileOps
            OutputLink.LNKcpy = new string[SettingsOps.LinkCopyPaths.Count];
            for (int i = 0; i < OutputLink.LNKcpy.Length; i++)
            {
                OutputLink.LNKcpy[i] = System.IO.Path.Combine(SettingsOps.LinkCopyPaths[i],
                    System.IO.Path.GetFileName(OutputLink.LNKdir));
            }
        }

        List<ShortcutterResult> opResult = Shortcutter.BuildShortcut(OutputLink, DesktopOS);
        // Single Shortcut
        if (opResult.Count == 1)
        {
            if (!opResult[0].Error)
            {
                msbox_params.ContentMessage = resMainView.popSingleOutput1_Mess; 
                msbox_params.ContentTitle = resMainView.genSucces;
                msbox_params.Icon = MsBox.Avalonia.Enums.Icon.Success;
                MessageBoxPopUp(msbox_params);
            }
            else
            {
                msbox_params.ContentHeader = resMainView.popSingleOutput0_Head; 
                msbox_params.ContentTitle = resMainView.genError;
                msbox_params.ContentMessage = string.Format(resMainView.popSingleOutput0_Head, opResult[0].eMesseage);
                msbox_params.Icon = MsBox.Avalonia.Enums.Icon.Error; 
                MessageBoxPopUp(msbox_params);
            }
        }
        // Multiple Shortcut
        else
        {
            bool hasErrors = opResult.Any(r => r.Error);

            if (!hasErrors)
            {
                msbox_params.ContentMessage = resMainView.popMultiOutput1_Mess; 
                msbox_params.ContentTitle = resMainView.genSucces;
                msbox_params.Icon = MsBox.Avalonia.Enums.Icon.Success;
                MessageBoxPopUp(msbox_params);
            }
            else
            {
                msbox_params.ContentHeader = resMainView.popMultiOutput0_Head;
                int successCount = 0;
                int errorCount = 0;
                string content = string.Empty;
                for (int i = 0; i < opResult.Count; i++)
                {
                    string outputPath = opResult[i].OutputDir + ": ";
                    content = string.Concat(content, outputPath);
                    content = string.Concat(content, opResult[i].Messeage);
                    content = string.Concat(content, "\n");
                    if (opResult[i].Error)
                    {
                        content = string.Concat(content, $"=> \"{opResult[i].eMesseage}\" <=");
                        content = string.Concat(content, "\n");
                        errorCount++;
                    }
                    else { successCount++; }
                }
                msbox_params.ContentTitle = resMainView.genWarning;
                msbox_params.Icon = (successCount > 0) ? MsBox.Avalonia.Enums.Icon.Warning : MsBox.Avalonia.Enums.Icon.Error;
                msbox_params.ContentMessage = content;
                MessageBoxPopUp(msbox_params);
            }
        }
    }

    // CLOSING
    void View1_Unloaded(object sender, RoutedEventArgs e)
    {
        var cachedSettings = SettingsOps.GetCachedSettings();
        bool settingsChanged = !settings.Equals(cachedSettings);
        if ( ((PrevConfigsCount != SettingsOps.PrevConfigs.Count) && (PrevConfigsCount > -1)) || settingsChanged) 
        { SettingsOps.WriteSettingsFile(settings); }  
    }

#if DEBUG
    void View2_Loaded(object sender, RoutedEventArgs e)
    {
        _ = sender.ToString();
    }

    async void testing1(object sender, RoutedEventArgs e)
    {
        Testing.FilePickerTesting(ParentWindow);
    }
#endif
}
