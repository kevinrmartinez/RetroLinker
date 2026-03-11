/*
    A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2026 Kevin Rafael Martinez Johnston

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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace RetroLinker.Styles;

public class MainViewExtensionNotice : TemplatedControl
{
    // == Avalonia Properties ==
    // Title
    public static readonly DirectProperty<MainViewExtensionNotice, string> TitleProperty = 
        AvaloniaProperty.RegisterDirect<MainViewExtensionNotice, string>(
            nameof(Title), 
            h => h.Title, 
            (h,  v) => h.Title = v);
    
    private string _title = string.Empty;
    public string Title {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }
    
    // Text
    public static readonly DirectProperty<MainViewExtensionNotice, string> TextProperty = 
        AvaloniaProperty.RegisterDirect<MainViewExtensionNotice, string>(
            nameof(Text), 
            h => h.Text, 
            (h,  v) => h.Text = v);
    
    private string _text = string.Empty;
    public string Text {
        get => _text;
        set => SetAndRaise(TextProperty, ref _text, value);
    }
    
    // Constructors
    public MainViewExtensionNotice() {}
} 

public class MainWindowHeader : TemplatedControl
{
    // == Avalonia Properties ==
    // Title
    public static readonly DirectProperty<MainWindowHeader, string> TitleProperty = 
        AvaloniaProperty.RegisterDirect<MainWindowHeader, string>(
            nameof(Title), 
            h => h.Title, 
            (h,  v) => h.Title = v);
    
    private string _title = string.Empty;
    public string Title {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    // LeftControl
    public static readonly DirectProperty<MainWindowHeader, Control?> LeftControlProperty =
        AvaloniaProperty.RegisterDirect<MainWindowHeader, Control?>(
            nameof(LeftControl), 
            h => h.LeftControl);

    private Control? _leftControl;
    public Control? LeftControl {
        get => _leftControl;
        set => SetAndRaise(LeftControlProperty, ref _leftControl, value);
    }
    
    // RightControl
    public static readonly DirectProperty<MainWindowHeader, Control?> RightControlProperty =
        AvaloniaProperty.RegisterDirect<MainWindowHeader, Control?>(
            nameof(RightControl), 
            h => h.RightControl);

    private Control? _rightControl;
    public Control? RightControl {
        get => _rightControl;
        set => SetAndRaise(RightControlProperty, ref _rightControl, value);
    }
    
    // Constructors
    public MainWindowHeader() { }
    
    public MainWindowHeader(string title) : this() =>  Title = title;
}

// TODO: UserControlledList (string)