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
public enum UserListActions { Add, Remove, Clear, Lock }

/* == Records == */
public record Contributor(string DisplayName, System.Uri? OnlinePageUrl);
public record ThirdParty(string DisplayName, System.Uri OnlinePageUrl, string? Comment = null);

/* == Interfaces == */
public interface IMainViewPopUp;

/* == Static Classes == */


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