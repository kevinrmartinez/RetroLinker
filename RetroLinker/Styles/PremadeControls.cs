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

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using RetroLinker.Models;

using ProjektankerIcon = Projektanker.Icons.Avalonia.Icon;

namespace RetroLinker.Styles;

// All this file requires some documentation!!
public class LinkCopyItemGrid
{
    // TODO: Reimplement using DataTemplates
    
    // Link Copy ListItem
    public Grid NewItemGrid { get; private set; }
    public Label NewItemText { get;  private set; }
    public Button NewItemTrash { get; private set; }

    public LinkCopyItemGrid(string path)
    {
        NewItemGrid = new Grid()
        {
            VerticalAlignment = VerticalAlignment.Center,
            ColumnDefinitions = new ColumnDefinitions("*, Auto")
        };
            
        NewItemText = new Label();
        if (!string.IsNullOrEmpty(path)) NewItemText.Content = path;
            
        ProjektankerIcon trashcan = new()
        {
            Value = "fa-trash",
            Foreground = new SolidColorBrush(Colors.Firebrick)
        };
        NewItemTrash = new Button()
        {
            Height = 30,
            Content = trashcan
        };
        
        Grid.SetColumn(NewItemText,0);
        Grid.SetColumn(NewItemTrash,1);
        NewItemGrid.Children.Add(NewItemText);
        NewItemGrid.Children.Add(NewItemTrash);
    }
}

public static class LocaleComboItem
{
    // TODO: Reimplement using DataTemplates
    public static ComboBoxItem GetLocaleComboItem(LanguageItem locale)
    {
        var item = new ComboBoxItem()
        {
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center
        };

        item.Content = DefineNewLocaleComboItem(locale);
        return item;
    }

    private static Grid DefineNewLocaleComboItem(LanguageItem locale)
    {
        var grid = new Grid(){ ColumnDefinitions = new ColumnDefinitions("Auto, *") };

        var icon = new Image()
            { Source = new Bitmap(AssetLoader.Open(locale.LangIconPath)) };
        var pictureBox = new Viewbox()
            { Width = 35, Child = icon };

        var text = new Label()
        {
            Content = locale.Name,
            Margin = new Thickness(3,0)
        };
        
        Grid.SetColumn(pictureBox,0);
        Grid.SetColumn(text,1);
        grid.Children.Add(pictureBox);
        grid.Children.Add(text);
        return grid;
    }
}


public class MainWindowHeader : Grid
{
    // Overrides
    protected override Type StyleKeyOverride { get; } = typeof(Grid);

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
    
    public MainWindowHeader()
    {
        SetDimmensions();
        var child = PopulateChildren();
        SetColumn(child,1);
        Children.Add(child);
    }
    
    public MainWindowHeader(string title) : this() {
        Title = title;
    }

    private void SetDimmensions() {
        Height = 45;
        ColumnDefinitions =  new ColumnDefinitions("Auto,*,Auto");
    }

    private StackPanel PopulateChildren()
    {
        return new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Children = { new Label()
            {
                DataContext = this,
                [!ContentControl.ContentProperty] = new Binding(nameof(Title))
            } }
        };
    }
}


public class ExtraMainControlButton : Button
{
    // Content Properties
    const string blankText = "blank";
    const string blankIcon = "fa-question";
    
    // Overrides
    protected override Type StyleKeyOverride { get; } = typeof(Button);
    
    
    // Avalonia Properties
    public enum ButtonFunctions
    { None, Save, Discard }

    // - ButtonFunction Selection
    public static readonly DirectProperty<ExtraMainControlButton, ButtonFunctions> ButtonFunctionProperty =
        AvaloniaProperty.RegisterDirect<ExtraMainControlButton, ButtonFunctions>(
            nameof(ButtonFunction),
            b => b.ButtonFunction,
            (b, v) => b.ButtonFunction = v);

    private ButtonFunctions _buttonFunction = ButtonFunctions.None;
    public ButtonFunctions ButtonFunction {
        get => _buttonFunction;
        set => SetButtonFunction(value);
    }
    
    // Content Properties
    private string ButtonText { get; set; } = blankText;
    private string ButtonIconValue { get; set; } =  blankIcon;
    private SolidColorBrush? ButtonIconColor { get; set; }

    public ExtraMainControlButton() {
        RefreshContent();
    }

    private StackPanel GetContent()
    {
        var icon = new ProjektankerIcon()
        {
            DataContext = this,
            // [!ProjektankerIcon.ValueProperty] = new Binding(nameof(ButtonIconValue)),
            // [!ProjektankerIcon.ForegroundProperty] =  new Binding(nameof(ButtonIconColor))
            Value = ButtonIconValue,
            Foreground = ButtonIconColor
        };
        var textBlock = new TextBlock()
        {
            DataContext = this,
            // [!TextBlock.TextProperty] = new Binding(nameof(ButtonText))
            Text = ButtonText
        };
        return new StackPanel() {
            Children = { icon, textBlock }
        };
    }

    private void RefreshContent() {
        // I did not want to do this, but the method of binding the controls to the objects did not worked.
        Content = GetContent();
    }

    private void SetButtonFunction(ButtonFunctions function)
    {
        var yesIcon = "fa-check";
        var yesColor = new SolidColorBrush(Colors.Green);
        var yesText = Translations.resGeneric.btnConfirm;
        
        var noIcon = "fa-x";
        var noColor = new SolidColorBrush(Colors.Crimson);
        var noText = Translations.resGeneric.btnDiscard;
        
        
        switch (function)
        {
            case ButtonFunctions.Save:
                ButtonText = yesText;
                ButtonIconValue = yesIcon;
                ButtonIconColor = yesColor;
                break;
            case ButtonFunctions.Discard:
                ButtonText = noText;
                ButtonIconValue = noIcon;
                ButtonIconColor = noColor;
                break;
            default:
                ButtonText = blankText;
                ButtonIconValue = blankIcon;
                ButtonIconColor = null;
                break;
        }
        SetAndRaise(ButtonFunctionProperty, ref _buttonFunction, function);
        RefreshContent();
    }
}