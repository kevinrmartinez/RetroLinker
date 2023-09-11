using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using RetroarchShortcutterV2.Models;
using RetroarchShortcutterV2.Models.Icons;
using System.Threading.Tasks;

namespace RetroarchShortcutterV2.Views;

public partial class MainView : UserControl
{

    //private System.Collections.Generic.List<string> _logging = new();

    public static int PrevConfigsCount;
    public static bool ROMenable = true;    // Creo no es necesario
    private Shortcutter shortcut = new();
    private Settings settings;
    private Bitmap ICONimage;
    private static IconsItems IconItemSET;

    // true = Windows. false = Linux.
    // Esto es asumiendo que solo podra correr en Windows y Linux.
    public bool DesktopOS = System.OperatingSystem.IsWindows();

    // Window Object
    // Solucion basada en atresnjo en los issues de Avalonia
    private IClassicDesktopStyleApplicationLifetime deskWindow = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
    private TopLevel topLevel;

    public MainView()
    { InitializeComponent(); }

    #region LOAD EVENTS
    // LOADS
    void View1_Loaded(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"OS actual: {System.Environment.OSVersion.VersionString}.");
        topLevel = TopLevel.GetTopLevel(this);
        SettingsOps.BuildConfFile();
        settings = FileOps.LoadSettingsFO();
        System.Diagnostics.Debug.WriteLine("Settings cargadas para la MainView.");
        FileOps.SetDesktopDir(topLevel);
        deskWindow.MainWindow.RequestedThemeVariant = SettingsOps.LoadThemeVariant();
        var cores_task = FileOps.LoadCores();
        var icon_task = FileOps.LoadIcons(DesktopOS);
        
        // Condicion de OS
        if (!DesktopOS)
        {
            if (settings.DEFRADir == string.Empty) { settings.DEFRADir = "retroarch"; }
            txtRADir.IsReadOnly = false;
        }
        else
        {
            try { Models.WinFuncImport.FuncLoader.ImportWinFunc(); IconProc.StartImport(); }
            catch { System.Diagnostics.Debug.WriteLine($"El importado de {Models.WinFuncImport.FuncLoader.WinFunc} ha fallado!"); }
            // PENDIENTE: Insertar msbox indicando un problema
            IconItemSET = new();
        }

        LoadSettingsIntoControls();
        comboCore_Loaded(cores_task);
        comboConfig_Loaded();
        comboICONDir_Loaded(icon_task);
    }

    async void comboCore_Loaded(Task<string[]> cores_task)
    {
        var cores = await cores_task;
        System.Diagnostics.Debug.WriteLine("Lista de Cores importada.");
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
            System.Diagnostics.Debug.WriteLine("Lista de iconos importada");
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
        shortcut.RAdir = settings.DEFRADir;
        FileOps.SetROMPadre(settings.DEFROMPath, topLevel);

        if (settings.PrevConfig && SettingsOps.PrevConfigs == null) { SettingsOps.PrevConfigs = new(); }
        else if (!settings.PrevConfig && SettingsOps.PrevConfigs != null) { SettingsOps.PrevConfigs = null; }
        if (SettingsOps.PrevConfigs != null) { PrevConfigsCount = SettingsOps.PrevConfigs.Count; }
        else { PrevConfigsCount = -1; }     // -1 ayuda a indicar que la lista en cuestión no existe

        if (!settings.AllwaysDesktop) { LinkDirSetting(); }
        else { LinkNameSetting(); }
    }

    void LinkDirSetting()
    {
        lblLinkDir.IsVisible = true;
        lblLinkName.IsVisible = false;
        txtLINKDir.IsReadOnly = true;
        btnLINKDir.IsEnabled = true;
    }

    void LinkNameSetting()
    {
        lblLinkDir.IsVisible = false;
        lblLinkName.IsVisible = true;
        txtLINKDir.IsReadOnly = false;
        txtLINKDir.AcceptsReturn = false;
        btnLINKDir.IsEnabled = false;
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
            int ExistingItem = IconProc.IconItemsList.IndexOf(IconProc.IconItemsList.Find(x => x.FilePath == file));
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
            string item = comboICONDir.SelectedItem.ToString();
            IconItemSET = IconProc.IconItemsList.Find(x => x.comboIconIndex == index);
            shortcut.ICONfile = IconItemSET.FilePath;
            
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
                { FillIconBoxes(shortcut.ICONfile); }
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
            shortcut.RAdir = file;
            txtRADir.Text = file;
        }
    }
    #endregion

    #region ROM Controls
    // ROM
    void chkContentless_CheckedChanged(object sender, RoutedEventArgs e)
    {
        if ((bool)chkContentless.IsChecked) { panelROMDirControl.IsEnabled = false; }
        else { panelROMDirControl.IsEnabled = true; }
        ROMenable = !(bool)chkContentless.IsChecked;
    }

    async void btnROMDir_Click(object sender, RoutedEventArgs e)
    {
        string file = await FileOps.OpenFileAsync(PickerOpt.OpenOpts.RAroms, topLevel);
        if (!string.IsNullOrEmpty(file))
        {
            shortcut.ROMdir = file;
            shortcut.ROMfile = file;
            txtROMDir.Text = file;
        }
    }

    void btnPatches_Click(object sender, RoutedEventArgs e)
    {
        // PENDIENTE
    }
    #endregion

    #region RACore Controls
    // CORE
    void btnSubSys_Click(object sender, RoutedEventArgs e)
    {
        // PENDIENTE
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
                shortcut.CONFfile = null;
                break;
            default:
                shortcut.CONFfile = comboConfig.SelectedItem.ToString();
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

    void btnAppendConfig_Click(object sender, RoutedEventArgs e)
    {
        // PENDIENTE
    }
    #endregion

    #region LinkDir Controls
    // Link Directory
    async void btnLINKDir_Click(object sender, RoutedEventArgs e)   // PENDIENTE: resolver async
    {
        PickerOpt.SaveOpts opt;
        if (DesktopOS) { opt = PickerOpt.SaveOpts.WINlnk; }        // Salvar link como un .lnk de Windows
        else { opt = PickerOpt.SaveOpts.LINdesktop; }              // Salvar link como un .desktop de Linux
        
        var file = await FileOps.SaveFileAsync(opt, topLevel);
        if (!string.IsNullOrEmpty(file))
        {
            shortcut.LNKdir = file;
            txtLINKDir.Text = shortcut.LNKdir;
        }
#if DEBUG
        else { Testing.LinShortcutTest(DesktopOS); }
        //else { var bitm = FileOps.IconExtractTest(); FillIconBoxes(bitm); }
#endif
    }
    #endregion

    // EXECUTE
    // La accion ocurre aqui
    void btnEXECUTE_Click(object sender, RoutedEventArgs e)
    {
        bool ShortcutPosible;
        var msbox_params = new MessageBoxStandardParams();
        msbox_params.ShowInCenter = true; 
        msbox_params.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        // CHECKS!
        shortcut.verboseB = (bool)chkVerb.IsChecked;
        shortcut.fullscreenB = (bool)chkFull.IsChecked;
        shortcut.accessibilityB = (bool)chkAccessi.IsChecked;

        // Validando si sera contentless o no
        shortcut.ROMdir = ((bool)chkContentless.IsChecked) ? Commander.contentless : shortcut.ROMfile;

        // Validar que haya ejecutable (LINUX)
        if (shortcut.RAdir == null && !txtRADir.IsReadOnly) { shortcut.RAdir = txtRADir.Text; }

        // Validando que haya un core
        shortcut.ROMcore = (comboCore.Text == null || comboCore.Text == string.Empty) ? null : comboCore.Text;

        // Manejo del link en caso de 'AllwaysDesktop = true'
        if (settings.AllwaysDesktop)
        {
            string link = txtLINKDir.Text;
            shortcut.LNKdir = FileOps.GetAllDeskPath(link, DesktopOS);
        }

        // Validando que haya descripcion o no
        shortcut.Desc = (txtDesc.Text == null || txtDesc.Text == string.Empty) ? null : txtDesc.Text;

        // Manejo de iconos
        // Icono del ejecutable (Default)
        if (comboICONDir.SelectedIndex == 0)
        { shortcut.ICONfile = null; }
        else
        {
            // En caso de ser Winodws OS, hay que convertir las imagenes a .ico
            if (IconItemSET.ConvertionRequiered)
            { shortcut.ICONfile = FileOps.SaveWinIco(IconItemSET); }

            // En caso de tener que copiar el icono provisto por el usuario
            if (settings.CpyUserIcon)
            { shortcut.ICONfile = FileOps.CpyIconToUsrSet(shortcut.ICONfile); }
        }

        // REQUIERED FIELDS VALIDATION!
        if ((shortcut.RAdir != null) && (shortcut.ROMdir != null) && (shortcut.ROMcore != null) && (shortcut.LNKdir != null))
        { ShortcutPosible = true; }
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
            if (shortcut.ROMdir != Commander.contentless) 
            { shortcut.ROMdir = Utils.FixUnusualDirectories(shortcut.ROMdir); }

            // para el archivo config
            if (!string.IsNullOrEmpty(shortcut.CONFfile)) 
            { shortcut.CONFfile = Utils.FixUnusualDirectories(shortcut.CONFfile); }


            if (Shortcutter.BuildWinShortcut(shortcut, DesktopOS) || Shortcutter.BuildLinShorcut(shortcut, DesktopOS))
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
    {  if (PrevConfigsCount != SettingsOps.PrevConfigs?.Count && PrevConfigsCount > -1) { SettingsOps.WriteSettingsFile(settings); }  }

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
