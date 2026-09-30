using System.Collections.Generic;
using Avalonia.Controls;
using RetroLinker.Models;

namespace RetroLinker.Views;

public static class GlobalCommands
{
    public static System.Windows.Input.ICommand TrashFromListCommand { get; }
        = new MsBox.Avalonia.ViewModels.Commands.RelayCommand(BtnTrashFromList_OnClick);

    private static void BtnTrashFromList_OnClick(object? parameter)
    {
        if (parameter is not Button ogButton || ogButton.DataContext is null) return;
        var valueType = ogButton.DataContext.GetType();
        if (valueType.IsAssignableTo(typeof(string))) 
            BtnTrashFromList_OnClick(ogButton, ogButton.DataContext as string);
        if (valueType.IsAssignableTo(typeof(EnvironmentVariable))) 
            BtnTrashFromList_OnClick(ogButton, ogButton.DataContext as EnvironmentVariable);
    }
    
    private static void BtnTrashFromList_OnClick<T>(Button ogButton, T value) {
        if (ogButton.Tag is not ICollection<T> collection) return;
        collection.Remove(value);
    }
}