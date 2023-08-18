using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using RetroarchShortcutterV2.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace RetroarchShortcutterV2.Views;

public partial class MainView : UserControl
{
    public static string RAdir;
    public static string RApath;
    public static string ROMdir;
    public static bool ROMenable = true;
    public static string ROMcore;
    public static string CONFfile;
    public static string ICONfile;
    public static string LNKdir;
    public Bitmap ICONimage;

    public MainView()
    { InitializeComponent(); }


    void View1_Loaded(object sender, RoutedEventArgs e)
    {
        //comboCore.ItemsSource = File.ReadAllLines("cores.txt");
        rdoIconDef.IsChecked = true;
        comboICONDir.ItemsSource = FileOps.IconsDir;
        comboICONDir.SelectedIndex = 0;
    }

    async void comboCore_Loaded(object sender, RoutedEventArgs e)
    {
        string cores = Path.Combine(FileOps.UserAssetsDir, FileOps.CoresFile);
        //string cores = Path.Combine("coress.txt");
        var msbox = MessageBoxManager.GetMessageBoxStandard("Error", "Archivo '" + cores + "' no encontrado!", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
        if (Path.Exists(cores)) { comboCore.ItemsSource = File.ReadAllLines(cores); }
        //else { Console.Out.WriteLine("Archivo '" + cores + "' no encontrado!"); }
        else { msbox.ShowAsync(); }
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

    void chkContentless_CheckedChanged(object sender, RoutedEventArgs e)
    {
        if ((bool)chkContentless.IsChecked) { panelROMDirControl.IsEnabled = false; }
        else { panelROMDirControl.IsEnabled = true; }
    }


    void comboICONDir_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (comboICONDir.SelectedIndex > 2)
        {
            ICONfile = comboICONDir.SelectedItem.ToString();
            FillIconBoxes(ICONfile);
        }
        else
        {
            switch (comboICONDir.SelectedIndex)
            {
                case 0:
                    ICONfile = Path.Combine(FileOps.UserAssetsDir, FileOps.DEFicon1);
                    break;
                case 1:
                    ICONfile = Path.Combine(FileOps.UserAssetsDir, FileOps.DEFicon2);
                    break;
                case 2:
                    ICONfile = Path.Combine(FileOps.UserAssetsDir, FileOps.DEFicon3);
                    break;
            }
            FillIconBoxes(ICONfile);
        }
    }


    async void btnICONDir_Click(object sender, RoutedEventArgs e)
    {
        string dir = await FileOps.OpenFileAsync(3, TopLevel.GetTopLevel(this));
        if (dir != null)
        {
            ICONfile = dir;
            FileOps.IconsDir.Add(ICONfile);
            //comboICONDir.Items.Add(ICONfile);
            comboICONDir.SelectedIndex = comboICONDir.Items.Count - 1;
        }
    }






    async void testing1(object sender, RoutedEventArgs e)
    {
        Testing.FilePickerTesting(TopLevel.GetTopLevel(this));
    }
}
