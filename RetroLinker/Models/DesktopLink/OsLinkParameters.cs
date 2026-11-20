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

namespace RetroLinker.Models.DesktopLink;

public abstract class OsLinkParameters : IOsLinkParameters
{
    public string RaExecutable { get; protected init; } = string.Empty;
    public string RaArguments { get; protected init; } = string.Empty;
    public string? Description { get; protected init; }
    public string IconPath { get; protected set; } = string.Empty;
    public string FriendlyName { get; protected init; } = string.Empty;
    public string OutputFileName { get; protected init; } = string.Empty;
    public string OutputPath { get; protected set; } = string.Empty;
    public IReadOnlyList<EnvironmentVariable> EnvironmentVariables { get; protected init; } = new List<EnvironmentVariable>();
    public HashSet<EnvironmentVariable> HashedEnvironmentVariables { get; protected init; } = new();
    protected string _ogOutputPath = string.Empty;

    /// <summary>
    /// An OS-agnostic command to write the shortcut to file
    /// </summary>
    /// <returns>A <see cref="ShortcutWriterResult"/> indicating if the file was whether written successfully or not.</returns>
    public abstract Task<ShortcutWriterResult> CreateOsShortcut();

    public virtual void SetToCopyOutputPath(string outputPath) => OutputPath = outputPath;

    protected void UpdateHashedVariables() {
        HashedEnvironmentVariables.Clear();
        foreach (var variable in EnvironmentVariables)  // Trace failed additions
            HashedEnvironmentVariables.Add(variable);
    }

    protected async Task<ShortcutWriterResult> CreateShortcut(Func<OsLinkParameters, Task> osCreateShortcut)
    {
        Logger.Info($"Creating \"{OutputPath}\"...");
        try {
            await osCreateShortcut(this);
            return new ShortcutWriterResult(OutputPath);
        }
        catch (Exception ex) {
            Logger.Warn($"\"{OutputPath}\" could not be created!");
            Logger.Error(ex);
            return new ShortcutWriterResult(OutputPath, ResultSuccess: false, ex.Message);
        }
    }
}