using RetroLinker.Models;

namespace RetroLinker.Views;

public abstract class ObservableFileItem : NotifyPropertyBase, IFileItem
{
    public string FilePath
    {
        get;
        protected set {
            SetAndNotify(value, ref field);
            ManuallyNotify(nameof(FileName));
            ManuallyNotify(nameof(FileExt));
        }
    }

    public string FileName => IIconItem.GetFileName(this);

    public string FileExt => IIconItem.GetFileExt(this);
}