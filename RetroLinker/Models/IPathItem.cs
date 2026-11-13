using RetroLinker.Models.IO;

namespace RetroLinker.Models;

public interface IFileItem
{
    public string FilePath { get; }
    public string FileName => GetFileName(this);
    public string FileExt => GetFileExt(this);

    protected static string GetFileName(IFileItem p) => FileOps.GetFileNameFromPath(p.FilePath);
    protected static string GetFileExt(IFileItem p) => FileOps.GetFileExtFromPath(p.FilePath);
}

public interface IIconItem : IFileItem
{

    public bool ConversionRequired { get; }

    public static async Task<System.IO.MemoryStream> GenerateImageStream(IIconItem i, string? altPath = null)
    {
        var path = altPath ?? i.FilePath;
        System.IO.MemoryStream imageStream;
        if (FileOps.IsVectorImage(path)) {
            var tmpStream = await FileOps.ReadFileBytesToEndAsync(path);
            imageStream = IconProc.DecodeSvg(new System.IO.MemoryStream(tmpStream));
        }
        else if (App.PlatformOs && FileOps.IsFileWinPE(path)) {
            const int defaultIndex = 0;
            imageStream = IconProc.ExtractIco(path, defaultIndex);
        }
        else imageStream = FileOps.BytesToStream(await FileOps.ReadFileBytesToEndAsync(path));

        imageStream.Position = 0;
        return imageStream;
    }

    public async Task<System.IO.MemoryStream> GetImageStream() => await GenerateImageStream(this);
}