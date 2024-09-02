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
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Projektanker.Icons.Avalonia;
using RetroLinker.Models;

namespace RetroLinker.Styles;

// TODO: All this file requires some documentation!!
public class LinkCopyItemGrid
{
    // Link Copy ListItem
    private Grid NewItemGrid { get; set; }
    private Label NewItemText { get; set; }
    private Button NewItemTrash { get; set; }

    public LinkCopyItemGrid()
    {
        DefineNewLinkCopyItem(null);
    }
    
    public LinkCopyItemGrid(string Dir)
    {
        DefineNewLinkCopyItem(Dir);
        
    }
    
    void DefineNewLinkCopyItem(string? path)
    {
        NewItemGrid = new Grid()
        {
            VerticalAlignment = VerticalAlignment.Center
        };
        NewItemGrid.ColumnDefinitions = new ColumnDefinitions()
        {
            new ColumnDefinition(1, GridUnitType.Star),
            new ColumnDefinition(1, GridUnitType.Auto)
        };
            
        NewItemText = new Label();
        if (!string.IsNullOrEmpty(path))
        {
            NewItemText.Content = path;
        }
            
        Icon trashcan = new()
        {
            Value = "fa-trash",
            Foreground = new SolidColorBrush(Colors.Crimson)
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

    public Grid GetNewCopyGrid(string path)
    {
        NewItemText.Content = path;
        return NewItemGrid;
    }
}

public class LocaleComboItem
{

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
        var grid = new Grid();
        grid.ColumnDefinitions = new ColumnDefinitions()
        {
            new(1, GridUnitType.Auto),
            new(1, GridUnitType.Star)
        };

        var icon = new Image()
        {
            Source = new Bitmap(AssetLoader.Open(locale.LangIconPath))
        };
        var pictureBox = new Viewbox()
        {
            Width = 35,
            Child = icon
        };

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