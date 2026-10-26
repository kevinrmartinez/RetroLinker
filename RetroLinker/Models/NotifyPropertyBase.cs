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

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RetroLinker.Models;

/// <summary>
/// Abstract Class that defines the default implementation of <see cref="INotifyPropertyChanged"/> in RetroLinker.
/// </summary>
public abstract class NotifyPropertyBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    /// <summary>
    /// All properties that wishes to notify their changes should use this generic setter.
    /// </summary>
    /// <param name="value">The new value that's going to be set.</param>
    /// <param name="field">The reference to the field that holds the current value of the property.</param>
    /// <param name="propertyName">The name (<see cref="string"/>) of the property being set. It's annotated with
    /// <see cref="CallerMemberNameAttribute"/>, so it gets the name automatically.</param>
    /// <typeparam name="T">The type of the property to set.</typeparam>
    /// <remarks>If <paramref name="value"/> and <paramref name="field"/> are equal (via <see cref="EqualityComparer{T}.Default"/>)
    /// no changes/notifications will occur.</remarks>
    protected void SetAndNotify<T>(T value, ref T field, [CallerMemberName] string? propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        // trace
        // Logger.LogDebg($"Property '{propertyName}' changed to {value}");
    }
}