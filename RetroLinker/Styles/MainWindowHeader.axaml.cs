using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace RetroLinker.Styles;

public class MainWindowHeader : TemplatedControl
{ 
    // Avalonia Properties
    public static readonly DirectProperty<MainWindowHeader, string> TitleProperty = 
        AvaloniaProperty.RegisterDirect<MainWindowHeader, string>(
            nameof(Title), 
            h => h.Title, 
            (h,  v) => h.Title = v);
    
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    public static readonly DirectProperty<MainWindowHeader, Control?> LeftControlProperty =
        AvaloniaProperty.RegisterDirect<MainWindowHeader, Control?>(
            nameof(LeftControl), 
            h => h.LeftControl);

    private Control? _leftControl;
    public Control? LeftControl
    {
        get => _leftControl;
        set => SetAndRaise(LeftControlProperty, ref _leftControl, value);
    }
    
    public static readonly DirectProperty<MainWindowHeader, Control?> RightControlProperty =
        AvaloniaProperty.RegisterDirect<MainWindowHeader, Control?>(
            nameof(RightControl), 
            h => h.RightControl);

    private Control? _rightControl;
    public Control? RightControl
    {
        get => _rightControl;
        set => SetAndRaise(RightControlProperty, ref _rightControl, value);
    }
    
    // private Grid? InternalGrid { get; set; }

    public MainWindowHeader() { }
    
    public MainWindowHeader(string title) : this() =>  Title = title;
}