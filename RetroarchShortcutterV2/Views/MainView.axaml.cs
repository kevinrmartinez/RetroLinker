using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using RetroarchShortcutterV2.Models;
using RetroarchShortcutterV2.Models.Icons;
using System.Threading.Tasks;
using MsBox.Avalonia.Enums;

namespace RetroarchShortcutterV2.Views;

public partial class MainView : UserControl
{
    System.TimeSpan timeSpan;
    public MainView()
    { 
        InitializeComponent();
        System.DateTime now = System.DateTime.Now;
        timeSpan = now - App.LaunchTime;
        System.Diagnostics.Debug.WriteLine($"Ejecuacion tras MainView(): {timeSpan}", "[Time]");
    }

    private string DefLinRAIcon;
    private int PrevConfigsCount;
    private int retrycount = 0;
    private Shortcutter Link = new();
    private Settings settings;
    private Bitmap ICONimage;
    private IconsItems IconItemSET;

    // true = Windows. false = Linux.
    // Esto es asumiendo que solo podra correr en Windows y Linux.
    public bool DesktopOS = System.OperatingSystem.IsWindows();

    // Window Object
    // Solucion basada en atresnjo en los issues de Avalonia
    private readonly IClassicDesktopStyleApplicationLifetime deskWindow = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
    private TopLevel topLevel;

    

    #region LOAD EVENTS
    // LOADS
    void View1_Loaded(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Trace.WriteLine($"OS actual: {System.Environment.OSVersion.VersionString}.", "[Info]");
        topLevel = TopLevel.GetTopLevel(this);
        SettingsOps.BuildConfFile();
        settings = FileOps.LoadSettingsFO();
        System.Diagnostics.Debug.WriteLine("Settings cargadas para la MainView.", "[Info]");
        System.Diagnostics.Debug.WriteLine(settings, "[Debg]");
        FileOps.SetDesktopDir(topLevel);
        deskWindow.MainWindow.RequestedThemeVariant = SettingsOps.LoadThemeVariant();
        var cores_task = FileOps.LoadCores();
        var icon_task = FileOps.LoadIcons(DesktopOS);
        
        // Condicion de OS
        if (!DesktopOS)
        {
            if (settings.DEFRADir == string.Empty) { settings.DEFRADir = "retroarch"; }
            txtRADir.IsReadOnly = false;
            DefLinRAIcon = FileOps.GetRAIcons();
        }
        else
        {
            WinFuncImport();
            IconItemSET = new();
        }

        LoadSettingsIntoControls();
        comboCore_Loaded(cores_task);
        comboConfig_Loaded();
        comboICONDir_Loaded(icon_task);

        System.DateTime now = System.DateTime.Now;
        timeSpan = now - App.LaunchTime;
        System.Diagnostics.Debug.WriteLine($"Ejecuacion tras View1_Loaded(): {timeSpan}", "[Time]");
    }

    async void comboCore_Loaded(Task<string[]> cores_task)
    {
        var cores = await cores_task;
        System.Diagnostics.Debug.WriteLine("Lista de Cores importada.", "[Info]");
        if (cores.Length < 1) { lblNoCores.IsVisible = true; }
        else { comboCore.ItemsSource = cores; }
     }

    void comboConfig_Loaded()
    {
        for (int i = 0; i < FileOps.ConfigDir.Count; i++)
        {
            comboConfig.Items.Add(FileOps.ConfigDir[i]);
        }
        comboConfig.SelectedIndex++;
    }

    async void comboICONDir_Loaded(Task<System.Collections.Generic.List<string>> icon_task)
    {
        var icons_list = await icon_task;
        comboICONDir.Items.Add("Default");
        if (icons_list != null)
        {
            System.Diagnostics.Debug.WriteLine("Lista de iconos importada", "[Info]");
            for (int i = 0; i < icons_list.Count; i++)
            {
                comboICONDir.Items.Add(icons_list[i]);
            }
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
        LoadSettingsIntoControls();
    }

    void LoadSettingsIntoControls()
    {
        txtRADir.Text = settings.DEFRADir;
        Link.RAdir = settings.DEFRADir;
        FileOps.SetROMPadre(settings.DEFROMPath, topLevel);

        // TODO: refactorizar esta parte sin ayuda de null
        if (settings.PrevConfig && SettingsOps.PrevConfigs == null) { SettingsOps.PrevConfigs = new(); }
        else if (!settings.PrevConfig && SettingsOps.PrevConfigs != null) { SettingsOps.PrevConfigs = null; }
        if (SettingsOps.PrevConfigs != null) { PrevConfigsCount = SettingsOps.PrevConfigs.Count; }
        else { PrevConfigsCount = -1; }     // -1 ayuda a indicar que la lista en cuestión no existe

        txtLINKDir.Watermark = "Super Mario Bros";
        txtLINKDir.Watermark += (DesktopOS) ? FileOps.WinLinkExt : FileOps.LinLinkExt;
        if (!settings.AllwaysDesktop) { LinkDirSetting(); }
        else { LinkNameSetting(); }
    }

    void WinFuncImport()
    {
        try { Models.WinFuncImport.FuncLoader.ImportWinFunc(); IconProc.StartImport(); }
        catch (System.Exception eMain)
        {
            System.Diagnostics.Trace.WriteLine($"El importado de {Models.WinFuncImport.FuncLoader.WinFunc} ha fallado!", "[Erro]");
            System.Diagnostics.Debug.WriteLine($"En MainView, el elemento {eMain.Source} a retrornado el error {eMain.Message}", "[Erro]");
            lock (this)
            { WinFuncImportFail(eMain); }
        }
    }

    async Task WinFuncImportFail(System.Exception eMain)
    {
        const string retry_btn = "Retry";
        const string abort_btn = "Abort";
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
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Icon = MsBox.Avalonia.Enums.Icon.Error,
            ContentTitle = "Error Fatal",
            ContentHeader = $"El importado de {Models.WinFuncImport.FuncLoader.WinFunc} ha fallado!",
            ContentMessage = $"El importado a fallado con el siguiente error:\n{eMain.Message}\n\nSin este módulo el programa no puede cumplir su funcion.",
            ButtonDefinitions = diag_btns
        };

        var msb = MessageBoxManager.GetMessageBoxCustom(msb_params);
        var diag_result = await msb.ShowWindowDialogAsync(deskWindow.MainWindow);
        switch (diag_result)
        {
            case abort_btn:
                deskWindow.MainWindow.Close();
                break;

            case retry_btn:
                retrycount++;
                WinFuncImport();
                break;

            default:
                break;
        }
    }

    void LinkDirSetting()
    {
        lblLinkDir.IsVisible = true;
        lblLinkName.IsVisible = false;
        txtLINKDir.IsReadOnly = true;
        btnLINKDir.IsEnabled = true;
        lblLinkDeskDir.IsVisible = false;
    }

    void LinkNameSetting()
    {
        lblLinkDir.IsVisible = false;
        lblLinkName.IsVisible = true;
        txtLINKDir.IsReadOnly = false;
        txtLINKDir.AcceptsReturn = false;
        btnLINKDir.IsEnabled = false;
        lblLinkDeskDir.IsVisible = true;
        lblLinkDeskDir.Content = FileOps.UserDesktop;
    }

    void FillIconBoxes(string DIR)
    {
        ICONimage = FileOps.GetBitmap(DIR);
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
    
    async Task<ButtonResult> MessageBoxPopUp(MessageBoxStandardParams standardParams)
    {
        standardParams.WindowIcon = deskWindow.MainWindow.Icon;
        standardParams.MaxWidth = 550;
        standardParams.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var msBox = MessageBoxManager.GetMessageBoxStandard(standardParams);
        var result = await msBox.ShowWindowDialogAsync(deskWindow.MainWindow);
        return result;
    }
    
    async Task<string> MessageBoxPopUp(MessageBoxCustomParams customParams)
    {
        customParams.WindowIcon = deskWindow.MainWindow.Icon;
        customParams.MaxWidth = 600;
        customParams.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var msBox = MessageBoxManager.GetMessageBoxCustom(customParams);
        var result = await msBox.ShowWindowDialogAsync(deskWindow.MainWindow);
        return result;
    }
    #endregion


    // TOP CONTROLS
    async void btnSettings_Click(object sender, RoutedEventArgs e)
    { 
        var config = new SettingsWindow();
        await config.ShowDialog(deskWindow.MainWindow);
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
        
        string file = await FileOps.OpenFileAsync(opt, topLevel);
        if (!string.IsNullOrEmpty(file))
        {
            int newIndex = comboICONDir.ItemCount;
            const int IndexNotFound = -1;
            int ExistingItem = IconProc.IconItemsList.IndexOf(IconProc.IconItemsList.Find(Item => Item.FilePath == file));
            if (ExistingItem == IndexNotFound)
            {
                comboICONDir.Items.Add(file);
                IconProc.BuildIconItem(file, newIndex, DesktopOS);
            }
            else { newIndex = (int)IconProc.IconItemsList[ExistingItem].comboIconIndex; }

            comboICONDir.SelectedIndex = newIndex;
        }
    }

    // Solucion del SelectionChangedEventArgs gracias a snurre en stackoverflow.com
    void comboICONDir_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {   
        int index = comboICONDir.SelectedIndex;
        panelIconNoImage.IsVisible = false;
        if (index > 0)
        {   // llena los controles pic con los iconos provistos por el usuario
            IconItemSET = IconProc.IconItemsList.Find(Item => Item.comboIconIndex == index);
            Link.ICONfile = IconItemSET.FilePath;
            
            if (IconItemSET.IconStream != null)
            {
                IconItemSET.IconStream.Position = 0;
                var bitm = FileOps.GetBitmap(IconItemSET.IconStream);
                FillIconBoxes(bitm);
            }
            //else if (!DesktopOS && FileOps.IsVectorImage(item))
            else
            {
                try 
                { FillIconBoxes(Link.ICONfile); }
                catch 
                {
                    Bitmap bitm = new(AssetLoader.Open(FileOps.GetNAimage()));
                    FillIconBoxes(bitm);
                    panelIconNoImage.IsVisible = true;
                } 
            }
        }
        else
        {   // llena los controles pic con el icono default (Indice 0)
            Bitmap bitm = new(AssetLoader.Open(FileOps.GetDefaultIcon()));
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
        else { opt = PickerOpt.OpenOpts.RAout; }                  // FilePicker Option para .AppImage de Windows
        string file = await FileOps.OpenFileAsync(opt, topLevel);
        if (!string.IsNullOrEmpty(file))
        {
            Link.RAdir = file;
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
        string file = await FileOps.OpenFileAsync(PickerOpt.OpenOpts.RAroms, topLevel);
        if (!string.IsNullOrEmpty(file))
        {
            Link.ROMdir = file;
            Link.ROMfile = file;
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
    // TODO
    void btnSubSys_Click(object sender, RoutedEventArgs e)
    {
        
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
                Link.CONFfile = string.Empty;
                break;
            default:
                Link.CONFfile = comboConfig.SelectedItem.ToString();
                break;
        }
    }

    async void btnCONFIGDir_Click(object sender, RoutedEventArgs e)
    {
        var file = await FileOps.OpenFileAsync(PickerOpt.OpenOpts.RAcfg, topLevel);
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

    // TODO
    void btnAppendConfig_Click(object sender, RoutedEventArgs e)
    {
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
                ContentTitle = "Observacion",
                ContentHeader = "El nombre utilizado aqui sera utilizado como el campo 'Name' del archivo, y como nombre del archivo en si.",
                ContentMessage = "Sin embargo los espacios en blanco seran reemplazados por '-' en el nombre de archivo, por razones de estandares y comodidad.\n\n\nPresione Cancel para no volver a mostrar este mensaje.",
                ButtonDefinitions = ButtonEnum.OkCancel,
                Icon = Icon.Folder
            };
            var box_result = await MessageBoxPopUp(msbox_params);
            if (box_result == ButtonResult.Cancel) { settings.LinDesktopPopUp = false; }
        }
        
        PickerOpt.SaveOpts opt;
        if (DesktopOS) { opt = PickerOpt.SaveOpts.WINlnk; }
        else { opt = PickerOpt.SaveOpts.LINdesktop; }
        
        var file = await FileOps.SaveFileAsync(opt, topLevel);
        if (!string.IsNullOrEmpty(file))
        {
            Link.LNKdir = file;
            txtLINKDir.Text = Link.LNKdir;
        }
#if DEBUG
        //else { Testing.LinShortcutTest(DesktopOS); }
        //else { var bitm = FileOps.IconExtractTest(); FillIconBoxes(bitm); }
#endif
    }

    void txtLINKDir_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (settings.AllwaysDesktop)
        {
            if (!string.IsNullOrWhiteSpace(txtLINKDir.Text)) 
            { lblLinkDeskDir.Content = FileOps.GetDeskLinkPath(txtLINKDir.Text, DesktopOS); }
            else { lblLinkDeskDir.Content = FileOps.UserDesktop; }
        }
    }
    #endregion

    // EXECUTE
    // La accion ocurre aqui
    void btnEXECUTE_Click(object sender, RoutedEventArgs e)
    {
        bool ShortcutPosible;
        var msbox_params = new MessageBoxStandardParams
        {
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        // CHECKS!
        Link.VerboseB = (bool)chkVerb.IsChecked;
        Link.FullscreenB = (bool)chkFull.IsChecked;
        Link.AccessibilityB = (bool)chkAccessi.IsChecked;

        // Validando si sera contentless o no
        Link.ROMdir = ((bool)chkContentless.IsChecked) ? Commander.contentless : Link.ROMfile;

        // Validar que haya ejecutable (LINUX)
        if (string.IsNullOrEmpty(Link.RAdir) && !txtRADir.IsReadOnly) { Link.RAdir = txtRADir.Text; }

        // Validando que haya un core
        Link.ROMcore = (string.IsNullOrWhiteSpace(comboCore.Text)) ? string.Empty : comboCore.Text;

        // Manejo del link en caso de 'AllwaysDesktop = true'
        if (settings.AllwaysDesktop && !string.IsNullOrWhiteSpace(txtLINKDir.Text))
        {
            Link.LNKdir = FileOps.GetDeskLinkPath(txtLINKDir.Text, DesktopOS);
        }

        // Validando que haya descripcion o no
        Link.Desc = (string.IsNullOrWhiteSpace(txtDesc.Text)) ? string.Empty : txtDesc.Text;

        // Manejo de iconos
        // Icono del ejecutable (Default)
        if (comboICONDir.SelectedIndex == 0)
        { Link.ICONfile = string.Empty; }
        else
        {
            // En caso de ser Winodws OS, hay que convertir las imagenes a .ico
            if (IconItemSET.ConvertionRequiered)
            { Link.ICONfile = FileOps.SaveWinIco(IconItemSET); }

            // En caso de tener que copiar el icono provisto por el usuario
            if (settings.CpyUserIcon)
            { Link.ICONfile = FileOps.CpyIconToUsrSet(Link.ICONfile); }
        }

        // REQUIERED FIELDS VALIDATION!
        if ((!string.IsNullOrEmpty(Link.RAdir)) && (!string.IsNullOrEmpty(Link.ROMdir)) && (!string.IsNullOrEmpty(Link.ROMcore)) && (!string.IsNullOrEmpty(Link.LNKdir)))
        { ShortcutPosible = true; System.Diagnostics.Debug.WriteLine("Todos los campos para el shortcut han sido aceptados", "Info"); }
        else
        {
            ShortcutPosible = false;
            msbox_params.ContentMessage = "Faltan campos Requeridos"; msbox_params.ContentTitle = "Sin Effecto"; msbox_params.Icon = MsBox.Avalonia.Enums.Icon.Forbidden;
            var msbox = MessageBoxManager.GetMessageBoxStandard(msbox_params);
            _ = msbox.ShowWindowDialogAsync(deskWindow.MainWindow);
        }

        while (ShortcutPosible)
        {
            // Comillas para directorios que iran de parametros...
            // para el directorio de la ROM
            if (Link.ROMdir != Commander.contentless) 
            { Link.ROMdir = Utils.FixUnusualDirectories(Link.ROMdir); }

            // para el archivo config
            if (!string.IsNullOrEmpty(Link.CONFfile)) 
            { Link.CONFfile = Utils.FixUnusualDirectories(Link.CONFfile); }


            if (Shortcutter.BuildWinShortcut(Link, DesktopOS) || Shortcutter.BuildLinShorcut(Link, DesktopOS))
            {
                msbox_params.ContentMessage = "El shortcut fue creado con éxtio"; msbox_params.ContentTitle = "Éxito";
                msbox_params.Icon = MsBox.Avalonia.Enums.Icon.Success;
                var msbox = MessageBoxManager.GetMessageBoxStandard(msbox_params);
                _ = msbox.ShowWindowDialogAsync(deskWindow.MainWindow);
            }
            else
            {
                msbox_params.ContentMessage = "Ha ocurrido un error al crear el shortcut."; msbox_params.ContentTitle = "Error"; 
                msbox_params.Icon = MsBox.Avalonia.Enums.Icon.Error; 
                var msbox = MessageBoxManager.GetMessageBoxStandard(msbox_params);
                _ = msbox.ShowWindowDialogAsync(deskWindow.MainWindow);
            }
            ShortcutPosible = false;
        }
    }

    // CLOSING
    void View1_Unloaded(object sender, RoutedEventArgs e)
    {  if ((PrevConfigsCount != SettingsOps.PrevConfigs?.Count) && (PrevConfigsCount > -1)) 
        { SettingsOps.WriteSettingsFile(settings); }  }

#if DEBUG
    void View2_Loaded(object sender, RoutedEventArgs e)
    {
        _ = sender.ToString();
    }

    async void testing1(object sender, RoutedEventArgs e)
    {
        Testing.FilePickerTesting(topLevel);
    }
#endif
}
