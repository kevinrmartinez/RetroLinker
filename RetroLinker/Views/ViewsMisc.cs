using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using RetroLinker.Models;

namespace RetroLinker.Views;

/* == Enums == */
public enum MainViewTypes
{ MainView, PatchesView, SubsysView, AppendView }
public enum PopUpWindowType { MoreRaOptions, RenameLinux }

public enum TextBoxActions { Browse, Restore, Clear }
public enum UserListActions { Add, Remove, Clear }

/* == Records == */
public record Contributor(string DisplayName, System.Uri? OnlinePageUrl);
public record ThirdParty(string DisplayName, System.Uri OnlinePageUrl, string? Comment = null);

/* == Interfaces == */
public interface IMainViewPopUp;

/* == Static Classes == */
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

/* == Dynamic Classes == */
public class RenamePopUpHelper(string givenPath, string? givenCore) : IMainViewPopUp
{
    public string GivenPath { get; } = givenPath;
    public string GivenName { get; } = FileOps.GetFileNameNoExtFromPath(givenPath);
    public string? GivenCore { get; } = givenCore;
    
    public string? NewPath { get; set; } = null;
    public string? NewFriendlyName { get; set; } = null;
    public bool CustomEntry { get; set; } = false;
    
    [MemberNotNullWhen(returnValue: true, nameof(NewPath), nameof(NewFriendlyName))]
    public bool ValidOutput { get; set; } = false;
}

public class MoreOptPopUpHelper(IUserLinkParameters linkObject, Settings settings) : IMainViewPopUp
{
    public IUserLinkParameters LinkObject { get; } =  linkObject;
    public Settings SettingsObject { get; } = settings;
}