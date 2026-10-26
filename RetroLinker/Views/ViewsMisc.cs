/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2026  Kevin Rafael Martinez Johnston

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

using RetroLinker.Models.DesktopLink;
using RetroLinker.Models.IO;
using RetroLinker.Models.Settings;

namespace RetroLinker.Views;

/* == Enums == */
public enum MainViewTypes { MainView, PatchesView, SubsysView, AppendView }
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

public class MoreOptPopUpHelper(IUserLinkBuilder linkObject, Settings settings) : IMainViewPopUp
{
    public IUserLinkBuilder LinkObject { get; } =  linkObject;
    public Settings SettingsObject { get; } = settings;
}