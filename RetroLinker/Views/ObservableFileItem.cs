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

    protected string ogFilePath { get; init; }
}