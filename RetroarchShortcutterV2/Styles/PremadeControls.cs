using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Projektanker.Icons.Avalonia;

namespace RetroarchShortcutterV2.Styles;

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