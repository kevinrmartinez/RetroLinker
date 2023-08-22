using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using RetroarchShortcutterV2.Models;
using System;
using System.IO;

namespace RetroarchShortcutterV2.Views;

public partial class MainView : UserControl
{

    public Shortcutter shortcut = new();
    public static bool ROMenable = true;
    public Avalonia.Media.Imaging.Bitmap ICONimage;

    // true = Windows. false = Linux.
    // Esto es asumiendo que solo podra correr en Windows y Linux.
    public bool DesktopOS = OperatingSystem.IsWindows();

    public MainView()
    { InitializeComponent(); }


    void View1_Loaded(object sender, RoutedEventArgs e)
    {
        //comboCore.ItemsSource = File.ReadAllLines("cores.txt");
        
        
#if LINUX
        Console.Out.WriteLine("Esto es Linux");
        Console.Beep();
#endif
        if (!DesktopOS)
        {
            txtRADir.IsReadOnly = false;
            txtRADir.Text = "retroarch";
        }
    }

    async void comboCore_Loaded(object sender, RoutedEventArgs e)
    {
        string cores = Path.Combine(FileOps.UserAssetsDir, FileOps.CoresFile);
        //string cores = Path.Combine("coress.txt");
        var msbox = MessageBoxManager.GetMessageBoxStandard("Error", "Archivo '" + cores + "' no encontrado!", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
        if (Path.Exists(cores)) { comboCore.ItemsSource = File.ReadAllLines(cores); }
        //else { Console.Out.WriteLine("Archivo '" + cores + "' no encontrado!"); }
        else { await msbox.ShowAsync(); }
     }

    void comboConfig_Loaded(object sender, RoutedEventArgs e)
    {
        for (int i = 0; i < FileOps.ConfigDir.Count; i++)
        {
            comboConfig.Items.Add(FileOps.ConfigDir[i]);
        }
        comboConfig.SelectedIndex++;
    }


    void comboICONDir_Loaded(object sender, RoutedEventArgs e)
    {
        for (int i = 0; i < FileOps.IconsDir.Count; i++)
        {
            comboICONDir.Items.Add(FileOps.IconsDir[i]);
        }
        comboICONDir.SelectedIndex++;
        rdoIconDef.IsChecked = true;
    }


    void FillIconBoxes(string DIR)
    {
        ICONimage = FileOps.GetBitmap(DIR);
        pic16.Source = ICONimage;
        pic32.Source = ICONimage;
        pic64.Source = ICONimage;
        pic128.Source = ICONimage;
    }

    void rdoIcon_CheckedChanged(object sender, RoutedEventArgs e)
    {
        if ((bool)rdoIconDef.IsChecked) 
        {
            comboICONDir.SelectedIndex = 0;
            gridIconControl.IsEnabled = false;
        }
        else { gridIconControl.IsEnabled = true; }
    }


    // Usuario llenando object del Icono
    async void btnICONDir_Click(object sender, RoutedEventArgs e)
    {
        int template;
        if (DesktopOS) { template = 3; }        // FilePicker Option para iconos de Windows
        else { template = 5; }                  // FilePicker Option para iconos de Linux
        string dir = await FileOps.OpenFileAsync(template, TopLevel.GetTopLevel(this));
        if (dir != null)
        {
            FileOps.IconsDir.Add(dir);
            comboICONDir.Items.Add(dir);
            comboICONDir.SelectedIndex = comboICONDir.Items.Count - 1;
        }
    }

    void comboICONDir_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (comboICONDir.SelectedIndex > 2)
        {
            shortcut.ICONfile = comboICONDir.SelectedItem.ToString();
            FillIconBoxes(shortcut.ICONfile);
        }
        else
        {
            switch (comboICONDir.SelectedIndex)
            {
                case 0:
                    shortcut.ICONfile = Path.Combine(FileOps.UserAssetsDir, FileOps.DEFicon1);
                    break;
                case 1:
                    shortcut.ICONfile = Path.Combine(FileOps.UserAssetsDir, FileOps.DEFicon2);
                    break;
                case 2:
                    shortcut.ICONfile = Path.Combine(FileOps.UserAssetsDir, FileOps.DEFicon3);
                    break;
            }
            FillIconBoxes(shortcut.ICONfile);
        }
    }


    // Usuario llenando object del link
    async void btnRADir_Click(object sender, RoutedEventArgs e)
    {
        int template;
        if (DesktopOS) { template = 0; }        // FilePicker Option para .exe de Windows
        else { template = 4; }                  // FilePicker Option para .AppImage de Windows
        string file = await FileOps.OpenFileAsync(template, TopLevel.GetTopLevel(this));
        if (file != null)
        {
            shortcut.RAdir = file;
            txtRADir.Text = file;
        }
    }

    void chkContentless_CheckedChanged(object sender, RoutedEventArgs e)
    {
        if ((bool)chkContentless.IsChecked) { panelROMDirControl.IsEnabled = false; }
        else { panelROMDirControl.IsEnabled = true; }
        ROMenable = !(bool)chkContentless.IsChecked;
    }

    async void btnROMDir_Click(object sender, RoutedEventArgs e)
    {
        string file = await FileOps.OpenFileAsync(1, TopLevel.GetTopLevel(this));
        if (file != null)
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

    void btnSubSys_Click(object sender, RoutedEventArgs e)
    {
        // PENDIENTE
    }

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
        var file = await FileOps.OpenFileAsync(2, TopLevel.GetTopLevel(this));
        if (file != null)
        {
            //shortcut.CONFfile = file;
            FileOps.ConfigDir.Add(file);
            comboConfig.Items.Add(file);
            comboConfig.SelectedIndex = comboConfig.ItemCount - 1;
        }
    }

    void btnAppendConfig_Click(object sender, RoutedEventArgs e)
    {

    }

    async void btnLINKDir_Click(object sender, RoutedEventArgs e)
    {
        int template;
        if (DesktopOS) { template = 0; }        // Salvar link como un .lnk de Windows
        else { template = 1; }                  // Salvar link como un .desktop de Linux
        var file = await FileOps.SaveFileAsync(template, TopLevel.GetTopLevel(this));
        if (file != null)
        {
            shortcut.LNKdir = file;
            txtLINKDir.Text = shortcut.LNKdir;
        }
    }


    /* La accion ocurre aqui
     *  
     */
    void btnEXECUTE_Click(object sender, RoutedEventArgs e)
    {
        bool ShortcutPosible;
        var msbox_params = new MessageBoxStandardParams();
        msbox_params.ShowInCenter = true; 
        msbox_params.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        //var msbox = MessageBoxManager.GetMessageBoxStandard(msbox_params);

        // CHECKS!
        shortcut.verboseB = (bool)chkVerb.IsChecked;
        shortcut.fullscreenB = (bool)chkFull.IsChecked;
        shortcut.accessibilityB = (bool)chkAccessi.IsChecked;

        // Validando si sera contentless o no
        if (!ROMenable) { shortcut.ROMdir = Commander.contentless; }
        else { shortcut.ROMdir = shortcut.ROMfile; }

        // Validar que haya ejecutable (LINUX)
        if (shortcut.RAdir == null && !txtRADir.IsReadOnly)
        { shortcut.RAdir = txtRADir.Text; }

        // Validando que haya un core
        if (comboCore.Text == null || comboCore.Text == string.Empty) { shortcut.ROMcore = null; }
        else { shortcut.ROMcore = comboCore.Text; }

        // Validando que haya descripcion o no
        if (txtDesc.Text == null || txtDesc.Text == string.Empty) { shortcut.Desc = null; }
        else { shortcut.Desc = txtDesc.Text; }

        // Manejo de iconos
        if (comboICONDir.SelectedIndex < 3)
        {
            switch (comboICONDir.SelectedIndex)
            {
                case 0:
                    shortcut.ICONfile = null;
                    break;
                default:
                    shortcut.ICONfile = FileOps.CpyIconToUsrSet(shortcut.ICONfile);
                    break;

            }
        }
        else
        {

        }


        // REQUIERED FIELDS VALIDATION!
        if ((shortcut.RAdir != null) && (shortcut.ROMdir != null) && (shortcut.ROMcore != null) && (shortcut.LNKdir != null))
        { ShortcutPosible = true; }
        else
        {
            ShortcutPosible = false;
            msbox_params.ContentMessage = "Faltan campos Requeridos"; msbox_params.ContentTitle = "Sin Effecto"; msbox_params.Icon = MsBox.Avalonia.Enums.Icon.Forbidden;
            var msbox = MessageBoxManager.GetMessageBoxStandard(msbox_params);
            msbox.ShowAsync();
        }

        while (ShortcutPosible)
        {
            // Comillas para directorios que iran de parametros...
            // para el directorio de la ROM
            if (shortcut.ROMdir != null || shortcut.ROMdir != Commander.contentless) 
            { shortcut.ROMdir = Utils.FixUnusualDirectories(shortcut.ROMdir); }

            // para el archivo config
            if (shortcut.CONFfile != null) 
            { shortcut.CONFfile = Utils.FixUnusualDirectories(shortcut.CONFfile); }

            if (Shortcutter.BuildWinShortcut(shortcut, DesktopOS) || Shortcutter.BuildLinShorcut(shortcut, DesktopOS))
            {
                msbox_params.ContentMessage = "El shortcut fue creado con éxtio"; msbox_params.ContentTitle = "Éxito";
                msbox_params.Icon = MsBox.Avalonia.Enums.Icon.Success;
                var msbox = MessageBoxManager.GetMessageBoxStandard(msbox_params);
                msbox.ShowAsync();
            }
            else
            {
                msbox_params.ContentMessage = "Ha ocurrido un error al crear el shortcut."; msbox_params.ContentTitle = "Error"; 
                msbox_params.Icon = MsBox.Avalonia.Enums.Icon.Error; 
                var msbox = MessageBoxManager.GetMessageBoxStandard(msbox_params);
                msbox.ShowAsync();
            }
            ShortcutPosible = false;
        }
    }

#if DEBUG
    async void testing1(object sender, RoutedEventArgs e)
    {
        Testing.FilePickerTesting(TopLevel.GetTopLevel(this));
    }
#endif
}
