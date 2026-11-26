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

using Avalonia.Animation;
using Avalonia.Input;
using Avalonia.Threading;
using RetroLinker.Models;
using RetroLinker.Models.Avalonia;
using AvaloniaTemplatedControl = Avalonia.Controls.Primitives.TemplatedControl;

namespace RetroLinker.Views;

public static class DragDropHandler
{
    public static AvaloniaTemplatedControl ActiveControl { get; private set; } = null!;
    public static PathArgument ActiveArgument { get; private set; } = null!;

    private static readonly System.Threading.Mutex _mutex = new();
    private static bool _activeArgumentValid;
    private static readonly Dictionary<AvaloniaTemplatedControl, Func<Task>> ControlHandlersMap = new();

    private static readonly BrushTransition BorderBrushTransition = new()
    {
        Property = AvaloniaTemplatedControl.BorderBrushProperty,
        Duration = TimeSpan.FromMilliseconds(250)
    };

    /// <summary> Adds DragDrop events to a provided Avalonia Control. </summary>
    /// <param name="control"> The <see cref="T:AvaloniaTemplatedControl"/> that will have the events added to. </param>
    /// <param name="inputHandler"> The function that runs and changes a Control's state with the dropped input. It should also be responsible for refusing an input. </param>
    /// <remarks> If <paramref name="control"/> has gone through this method earlier, no operation will be done. </remarks>
    public static void AddEventsToControl(AvaloniaTemplatedControl control, Func<Task> inputHandler)
    {
        if (!ControlHandlersMap.TryAdd(control, inputHandler)) return;    // throw?
        if (control.Transitions is { } transitions) transitions.Add(BorderBrushTransition);
        else control.Transitions = [ BorderBrushTransition ];
        DragDrop.SetAllowDrop(control, true);
        DragDrop.AddDragEnterHandler(control, ControlBox_DragEnter);
        DragDrop.AddDragLeaveHandler(control, ControlBox_DragLeave);
        DragDrop.AddDropHandler(control, ControlBox_DropParse);
        // https://docs.avaloniaui.net/docs/how-to/drag-and-drop-how-to
    }

    public static void ParseSuccessful() => _activeArgumentValid = true;
    public static void ParseFailure() => _activeArgumentValid = false;

    private static void ResetBorderBrush(AvaloniaTemplatedControl control) {
        // control.ClearValue(BorderBrushProperty); // should do this, but I'm having mixed results
        // control.SetValue(BorderBrushProperty, AvaloniaProperty.UnsetValue);
        // control.BorderBrush = txtLINKRename.BorderBrush;
        control.ClearValue(AvaloniaTemplatedControl.BorderBrushProperty);
    }

    private static void ControlBox_DragEnter(object? sender, DragEventArgs e) {
        if (sender is not AvaloniaTemplatedControl control) return;
        control.BorderBrush = Styles.StyleData.BrushBlueLight1;
    }

    private static void ControlBox_DragLeave(object? sender, DragEventArgs e) {
        if (sender is not AvaloniaTemplatedControl control) return;
        ResetBorderBrush(control);
    }

    private static async Task ControlBox_DropParseAsync() {
        var handler = ControlHandlersMap[ActiveControl];
        await handler();
        await ControlBox_DropResult_UIThread();
    }

    private static async void ControlBox_DropParse(object? sender, DragEventArgs e)
    {
        try
        {
            if (sender is not AvaloniaTemplatedControl control) return;
            var filesEnum = e.DataTransfer.TryGetFile(); // Single File
            var text = e.DataTransfer.TryGetText();

            var input = string.Empty;
            if (filesEnum is not null) input = filesEnum.Path.LocalPath;
            else if (!string.IsNullOrWhiteSpace(text)) input = text;

            _mutex.WaitOne();
            _activeArgumentValid = false;
            if (string.IsNullOrWhiteSpace(input)) {
                await Dispatcher.UIThread.InvokeAsync(ControlBox_DropResult_UIThread, DispatcherPriority.Input);
                return;
            }

            ActiveControl = control;
            ActiveArgument = new PathArgument(input);

            await Dispatcher.UIThread.InvokeAsync(ControlBox_DropParseAsync, DispatcherPriority.Input);
            _mutex.ReleaseMutex();
        }
        catch (Exception ex) { await App.Instance.PopUpGenericError(ex); }
    }

    private static async Task ControlBox_DropResult_UIThread()
    {
        var control = ActiveControl;
        var resultBrush = _activeArgumentValid ? Styles.StyleData.BrushGreen2 : Styles.StyleData.BrushRed2;
        control.BorderBrush = resultBrush;
        await Task.Delay(1200);
        ResetBorderBrush(control);
    }
}