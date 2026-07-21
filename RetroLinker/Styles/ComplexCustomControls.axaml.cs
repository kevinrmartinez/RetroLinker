/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Metadata;

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
    // public MainViewExtensionNotice() {}
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

public class UserStringList : TemplatedControl
{
    // == Avalonia Properties ==
    // Items
    public static readonly DirectProperty<UserStringList, ICollection<string>> ItemsProperty =
        AvaloniaProperty.RegisterDirect<UserStringList, ICollection<string>>(
            nameof(Items),
            l => l.Items,
            (l,  v) => l.Items = v);
    
    // private static System.Collections.Generic.List<string> _itemsFill = new();
    private ICollection<string> _items = new ObservableCollection<string>();
    // To keep it simple, only ObservableCollection<string> is used to handle the Items 
    [Content] public ICollection<string> Items
    {
        get => _items;
        set => SetAndRaise(ItemsProperty, ref _items, value);
    }
    
    // Add Button
    public static readonly DirectProperty<UserStringList, Button?> AddButtonProperty =
        AvaloniaProperty.RegisterDirect<UserStringList, Button?>(
            nameof(AddButton),
            l => l.AddButton);
    public Button? AddButton
    {
        get;
        set => SetAndRaise(AddButtonProperty, ref field, value);
    }
    
    // Clear Button
    public static readonly DirectProperty<UserStringList, Button?> ClearButtonProperty =
        AvaloniaProperty.RegisterDirect<UserStringList, Button?>(
            nameof(ClearButton),
            l => l.ClearButton);
    public Button? ClearButton
    {
        get;
        set => SetAndRaise(ClearButtonProperty, ref field, value);
    }
    
    // == Avalonia Events ==
    // AddItem
    public static readonly RoutedEvent<RoutedEventArgs> AddItemEvent =
        RoutedEvent.Register<UserStringList, RoutedEventArgs>(nameof(AddItemClick), RoutingStrategies.Direct);
    
    public event System.EventHandler<RoutedEventArgs> AddItemClick
    {
        add => AddHandler(AddItemEvent, value);
        remove => RemoveHandler(AddItemEvent, value);
    }

    protected virtual void OnAddItemClick() {
        var args = new RoutedEventArgs(AddItemEvent);
        RaiseEvent(args);
    }

    // == Constructor ==
    // public UserStringList() { }

    // == Functions & Handlers ==
    private void ButtonAdd_OnClick(object? sender, RoutedEventArgs e) => OnAddItemClick();

    private void ClearItems() => Items.Clear();
    private void ButtonClr_OnClick(object? sender, RoutedEventArgs e) => ClearItems();

    private void DeleteItem(string item) => Items.Remove(item);
    private void ButtonTrash_OnClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Button button) return;
        if (button.Name != "PART_ButtonDel") return;
        if (button.DataContext is string item) DeleteItem(item);
    }

    private void ItemsControl_OnTemplateApplied(object? obj, TemplateAppliedEventArgs e) {
        if (obj is not ItemsControl itemsControl) return;
        itemsControl.AddHandler(Button.ClickEvent, ButtonTrash_OnClick);
    }
    
    // == Overrides ==
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var btnAdd = e.NameScope.Find<Button>("PART_ButtonAdd");
        btnAdd?.Click += ButtonAdd_OnClick;
        AddButton = btnAdd;
        var btnClr = e.NameScope.Find<Button>("PART_ButtonClr");
        btnClr?.Click += ButtonClr_OnClick;
        ClearButton = btnClr;
        var itemsCtrl = e.NameScope.Find<ItemsControl>("PART_ItemsControl");
        itemsCtrl?.TemplateApplied += ItemsControl_OnTemplateApplied;
    }
}