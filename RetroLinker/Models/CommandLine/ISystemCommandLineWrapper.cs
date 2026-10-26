using RetroLinker.Models.DesktopLink;

namespace RetroLinker.Models.CommandLine;

/// <summary>
/// Provides the base interface for the abstraction of OS native script engines wrapping our executable of interest.
/// </summary>
public interface ISystemCommandLineWrapper
{
    public string RealTargetExecutable { get; }
    public string RealTargetArguments { get; }
    public HashSet<EnvironmentVariable> EnvironmentVariables { get; }
}