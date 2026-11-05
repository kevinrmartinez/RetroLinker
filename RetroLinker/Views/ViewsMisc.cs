/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023-2026 Kevin Rafael Martinez Johnston

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

using System.Runtime.CompilerServices;
using RetroLinker.Models;
using RetroLinker.Models.Avalonia;
using RetroLinker.Models.DesktopLink;
using RetroLinker.Models.IO;
using RetroLinker.Models.Settings;

using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

namespace RetroLinker.Views;

/* == Enums == */
public enum MainViewTypes { MainView, PatchesView, SubsysView, AppendView }
public enum PopUpWindowType { MoreRaOptions, RenameLinux }

public enum TextBoxActions { Browse, Restore, Clear }
public enum UserListActions { Add, Remove, Clear, Lock }

/* == Records == */
public record Contributor(string DisplayName, Uri? OnlinePageUrl);
public record ThirdParty(string DisplayName, Uri OnlinePageUrl, string? Comment = null);

/* == Interfaces == */
public interface IMainViewPopUp;

/* == Static Classes == */


/* == Dynamic Classes == */
public class ViewableIconItem : NotifyPropertyBase, IIconItem, IDisposable
{
    public string FilePath
    {
        get;
        private set {
            SetAndNotify(value, ref field);
            ManuallyNotify(nameof(FileName));
            ManuallyNotify(nameof(FileExt));
        }
    }
    
    private string? FilePathPreConversion { get; init; }
    
    [MemberNotNullWhen(true, nameof(FilePathPreConversion))] 
    public bool ConversionRequired { get; private set => SetAndNotify(value, ref field); }
    
    public string FileName => IIconItem.GetFileName(this);

    public string FileExt => IIconItem.GetFileExt(this);

    public AvaloniaBitmap? ImageBitmap { get; private set => SetAndNotify(value, ref field); }
    
    private bool IsDefault { get; }
    
    private static readonly Uri DefaultIconAsset = Operations.GetDefaultIcon();
    
    
    public static ViewableIconItem DefaultInstance { get; } = new();

    private ViewableIconItem([CallerMemberName] string? caller = null) {
        switch (caller)
        {
            case nameof(DefaultInstance):
                FilePath = Translations.resGeneric.btnDefault;
                ConversionRequired = false;
                IsDefault = true;
                _ = SetBitmapAsync();
                break;
            default:
                throw new InvalidOperationException($"[{nameof(ViewableIconItem)}] Unknown caller '{caller}'");
        }
    }
    
    public ViewableIconItem(string filePath, bool? conversionRequired = null)
    {
        FilePath = filePath;
        ConversionRequired = conversionRequired ?? IconProc.IsIcoConversionRequired(FileExt);
        if (ConversionRequired) FilePathPreConversion = FilePath;

        _ = SetBitmapAsync();
    }

    public static ViewableIconItem FromIIconItem(IIconItem i) => new(i.FilePath, i.ConversionRequired);
    
    public void SetBitmap(System.IO.Stream newImageStream) => ImageBitmap = GetBitmap(newImageStream);
    public async Task SetBitmapAsync(string newImagePath) => ImageBitmap = await GetBitmap(newImagePath);

    public void ResetBitmap() {
        ImageBitmap?.Dispose();
        _ = SetBitmapAsync();
    }
    
    public async Task<System.IO.MemoryStream> GetImageStream() => await IIconItem.GenerateImageStream(this);

    public override string ToString() => FileName;

    public void Dispose() {
        if (IsDefault) {
            Logger.LogWarn($"'{nameof(Dispose)}' was called on '{nameof(ViewableIconItem)}.{nameof(DefaultInstance)}', ignoring...");
            return;
        }
        FilePath = string.Empty;
        var holder = ImageBitmap;
        ImageBitmap = null;
        holder?.Dispose();
    }

    private async Task SetBitmapAsync() => ImageBitmap = await GetBitmap();
    
    private async Task<AvaloniaBitmap?> GetBitmap(string? path = null)
    {
        if (IsDefault) return GetBitmap(Avalonia.Platform.AssetLoader.Open(DefaultIconAsset));
        System.IO.MemoryStream streamToDecode;
        try {
            streamToDecode = await IIconItem.GenerateImageStream(this);
        }
        catch (Exception ex) {
            Logger.LogWarn($"Icon '{FilePath}' could not be decoded into a bitmap");
            Logger.LogErro(ex);
            streamToDecode = new System.IO.MemoryStream();
        }
        
        return GetBitmap(streamToDecode);
    }

    private AvaloniaBitmap? GetBitmap(System.IO.Stream imageStream) {
        var avaloniaBitmap = (imageStream is not { Length: > 0 }) ? null : Operations.GetBitmap(imageStream);
        imageStream.Dispose();
        return avaloniaBitmap;
    }
}

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