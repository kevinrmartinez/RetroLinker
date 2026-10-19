using System.Collections.Generic;
using RetroLinker.Models.DesktopLink;

namespace RetroLinker.Models.CommandLine;

public interface ISystemCommandLineWrapper
{
    public string RealTargetExecutable { get; }
    public string RealTargetArguments { get; }
    public HashSet<EnvironmentVariable> EnvironmentVariables { get; }
}