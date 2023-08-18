using Avalonia.Controls;
using Avalonia.Media.Imaging;
using RetroarchShortcutterV2.Models;
using RetroarchShortcutterV2.Views;
using System.Runtime.Intrinsics.Arm;

namespace RetroarchShortcutterV2.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";
    public Bitmap TestBitmap => Testing.bitmap;

    //public void fillPicTest()
    //{
    //    pic128.Source = Testing.bitmap;
    //    pic64.Source = Testing.bitmap;
    //    pic32.Source = Testing.bitmap;
    //}
}
