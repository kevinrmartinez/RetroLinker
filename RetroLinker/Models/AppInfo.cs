using System;

namespace RetroLinker.Models;

public readonly struct AppInfo
{
    public string FullName { get; init; }
    public string Name { get; init; }
    public string Version { get; init; }
    public DateTime? BuildDate { get; init; }
    public string? GitHash { get; init; }

    public AppInfo(string fullName, string name, string version, DateTime? buildDate = null, string? gitHash = null)
    {
        FullName = fullName;
        Name = name;
        Version = version;
        BuildDate = buildDate;
        GitHash = gitHash;
    }
}