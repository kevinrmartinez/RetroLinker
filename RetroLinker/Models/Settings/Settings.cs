/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2025  Kevin Rafael Martinez Johnston

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

namespace RetroLinker.Models.Settings;

// TODO: Create another Settings class (private setters) for saving the props, making validations in the process 
public class Settings : ILocalSerializable, ICloneable
{
    public string UserAssetsPath { get; set; } = FileOps.DefUserAssetsDir;
    public string DEFRADir { get; set; } = string.Empty;
    public string DEFROMPath { get; set; } = string.Empty;
    public string IconParentPath { get; set; } = string.Empty;
    public bool PrevConfig { get; set; } = false;
    public bool AlwaysAskOutput { get; set; } = true;
    public string DEFLinkOutput { get; set; } = string.Empty;
    public bool MakeLinkCopy { get; set; } = false;
    public bool CpyUserIcon { get; set; } = false;
    public string IcoSavPath { get; set; }
    public bool ExtractIco { get; set; } = false;
    public bool IcoLinkName { get; set; } = false;
    public string? TileIcoPath { get; set; }
    public byte ChosenTheme { get; set; } = 0;
    public string LanguageLocale { get; set; } = DefaultLanguage;
    public List<string> SavedConfigs { get; set; } = new();
    public List<string> SavedCopyPaths { get; set; } = new();
    public RememberedUserChoices UserChoices { get; init; } = new();
    
    private static readonly string DefaultLanguage = LanguageManager.ENLocale.Name;

    public Settings() {
        IcoSavPath = UserAssetsPath; 
    }

    public object Clone() => this.MemberwiseClone();

    public void SetDefaultLanguage() => LanguageLocale = DefaultLanguage;
    
    public string? GetBase64()
    {   // Solution thanks to Kevin Driedger @ Stackoverflow.com
        var jsonString = JsonHelper.SerializeProxy(this);
        if (string.IsNullOrEmpty(jsonString)) return null;
        var object64 = Utils.GenerateBase64(jsonString);
        return object64;
    }
}

public class RememberedUserChoices
{
    public bool? StoredRaFullscreen { get; set; }
    public bool? StoredRaMenuOnError { get; set; }
    public bool? StoredRaAccessibility { get; set; }
    public bool? StoredVerbose { get; set; }
    public string? StoredRaLogFile { get; set; }
    public HashSet<EnvironmentVariable> StoredEnvironmentVariables { get; init; } = new();
}