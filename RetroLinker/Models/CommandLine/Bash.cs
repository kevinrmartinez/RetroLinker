using CommandLine;
using RetroLinker.Models.DesktopLink;

namespace RetroLinker.Models.CommandLine;

public static partial class Manager
{
    public const string BashEnvCommand = "env";

    [SupportedOSPlatform(App.PlatformLin)]
    public static ISystemCommandLineWrapper ResolveExecLine(string command)
        => new BashWrapper(command);
    
    [SupportedOSPlatform(App.PlatformLin)]
    private class BashWrapper : ISystemCommandLineWrapper
    {
        public string RealTargetExecutable { get; }
        public string RealTargetArguments { get; }
        public HashSet<EnvironmentVariable> EnvironmentVariables { get; }
        
        private string _targetExecutable = string.Empty;
        private string _targetArguments = string.Empty; 
        
        public BashWrapper(string command)
        {
            EnvironmentVariables = new HashSet<EnvironmentVariable>();
            if (command.StartsWith(BashEnvCommand)) ResolveExecCommandWithEnv(command);
            else ResolveExecCommand(command.SplitArgs(keepQuote: true));
            
            RealTargetExecutable = _targetExecutable;
            RealTargetArguments = _targetArguments;
        }

        private void ResolveExecCommand(string[] dividedCommand)
        {
            // Just in case the split failed to keep the bin in the first position 
            var argIndex = 0;
            for (int i = 0; i < dividedCommand.Length; i++) {
                if (!dividedCommand[i].Contains('-')) continue;
                argIndex = i;
                break;
            }
            
            var reconstructedBin = string.Join(string.Empty, dividedCommand[..argIndex]);
            _targetExecutable = reconstructedBin.UnwrapStringBetweenQuotes();
            _targetArguments = string.Join(SepWhiteSpace, dividedCommand[argIndex..]);
        }

        private void ResolveExecCommandWithEnv(string command)
        {
            var split = command.SplitArgs(keepQuote: true);
            var execIndex = 0;
            for (int i = 0; i < split.Length; i++) {
                if (i == 0) continue;
                if (split[i].Contains('=')) continue;
                execIndex = i;
                break;
            }

            for (int i = 0; i < execIndex; i++) {
                if (i == 0) continue;
                var env = EnvironmentVariable.FromBashArgument(split[i]);
                if (env is not null) EnvironmentVariables.Add(env);
                else Logger.LogWarn($"[{nameof(BashWrapper)}] Could not parse environment variable '{split[i]}'");
            }
            
            var newCommand = split[execIndex..];
            ResolveExecCommand(newCommand);
        }
    }
}