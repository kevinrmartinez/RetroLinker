/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2026  Kevin Rafael Martinez Johnston

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

using CommandLine;
using RetroLinker.Models.DesktopLink;
using RetroLinker.Models.IO;

namespace RetroLinker.Models.CommandLine;

public static partial class Manager
{
    public const string PwsEnvSetter = "$env:";
    public const string PwsCommandOpt = "Command";
    private const string PwsStartCommand = "Start-Process";
    private const string PwsStartFilePath = "FilePath";
    private const string PwsStartArgumentList = "ArgumentList";
    
    public static void TestWindowsCmd()
    {
        const string arg = """
                           -Command "$env:LIBRETRO_CHEATS_DIRECTORY='C:\Program Files'; Start-Process -FilePath 'C:\RetroArch Win64\retroarch.exe' -ArgumentList '-v -f -L fbneo ""C:\Users\Developer\Downloads\gigawing\giga wing.zip""'"
                           """;
        const string arg2 = """
                            -Command "$env:LIBRETRO_CHEATS_DIRECTORY='C:\Program Files'; Start-Process -ArgumentList '-v -f -L fbneo ""C:\Users\Developer\Downloads\gigawing\giga wing.zip""' -FilePath C:\RetroArchWin64\retroarch.exe"
                            """;
        var pwsUnparse = new WindowsPwsWrapper(arg2);
        
        var pwsParse = new WindowsPwsWrapper(pwsUnparse.RealTargetExecutable, pwsUnparse.RealTargetArguments, pwsUnparse.EnvironmentVariables);
        var result = pwsParse.BuildCommandLineArg();
        Logger.LogDebg(result);
        Logger.LogDebg(result == arg2);
    }

    [SupportedOSPlatform(App.PlatformWin)]
    public static string GetPowershellExecutable()
    {
        const string powershell = "powershell.exe";
        const string usualDir = "C:\\Windows\\System32\\WindowsPowerShell\\v1.0";
        var pwsExecutable = FileOps.CombineMultipleInputs(usualDir, powershell);
        return (FileOps.PathAlreadyExists(pwsExecutable)) ? pwsExecutable : powershell;
    }

    [SupportedOSPlatform(App.PlatformWin)]
    public static string CreatePowershellArguments(IShortcutBase link, HashSet<EnvironmentVariable> environmentVariables) {
        var wrapper = new WindowsPwsWrapper(link.RaExecutable, link.RaArguments, environmentVariables);
        return wrapper.BuildCommandLineArg();
    }
    
    [SupportedOSPlatform(App.PlatformWin)]
    public static ISystemCommandLineWrapper ResolvePowershellArguments(string args) => new WindowsPwsWrapper(args);
    
    
    [SupportedOSPlatform(App.PlatformWin)]
    private partial class WindowsPwsWrapper : ISystemCommandLineWrapper
    {
        public string RealTargetExecutable { get; }
        public string RealTargetArguments { get; }
        public HashSet<EnvironmentVariable> EnvironmentVariables { get; }

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties 
                           | DynamicallyAccessedMemberTypes.PublicConstructors, typeof(WindowsPws))]
        private WindowsPws _pws;
        
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties 
                           | DynamicallyAccessedMemberTypes.PublicConstructors, typeof(PwsStartProcess))]
        private PwsStartProcess _pwsStartProcess;
        
        private const string CommandsSeparator = ";";
        private const string StartSeparatorPlHld = "@#Sep#@";
        
        private static readonly string[] StartOptions = [PwsStartFilePath, PwsStartArgumentList];

        /// <summary>
        /// Create an instance of <c>WindowsPwsWrapper</c>, and prepares it for *un-parsing*.
        /// </summary>
        /// <param name="realTargetExecutable">Path to the executable the user expects to run.</param>
        /// <param name="realTargetArguments">Arguments to pass to the executable.</param>
        /// <param name="environmentVariables">A <see cref="HashSet{EnvironmentVariable}"/> to include for the executable.</param>
        public WindowsPwsWrapper(string realTargetExecutable, string realTargetArguments,
            HashSet<EnvironmentVariable> environmentVariables)
        {
            RealTargetExecutable = realTargetExecutable;
            RealTargetArguments = realTargetArguments;
            EnvironmentVariables = environmentVariables;
            _pwsStartProcess = new PwsStartProcess() {
                FilePath = RealTargetExecutable,
                ArgumentList = RealTargetArguments
            };
            _pws = new WindowsPws();
        }
        
        /// <summary>
        /// Builds a command-line argument for Powershell, that works from a shortcut file (.lnk).
        /// </summary>
        /// <returns>The command-line as a single <see langword="string"/>.</returns>
        /// <exception cref="CommandLineParserException">The un-parser failed to create the command-line.</exception>
        /// <remarks>The return value can be passed to <see cref="WindowsPwsWrapper(string)"/>.</remarks>
        public string BuildCommandLineArg()
        {
            const string quoteArtifact = "\\\"";
            const string quoteArtifactPlHld = "#{DQ}#";
            
            var strpToString = RetroLinkerParser.FormatCommandLine(_pwsStartProcess, RetroLinkerUnParsing);
            strpToString = strpToString.Replace(quoteArtifact, quoteArtifactPlHld);
            strpToString = strpToString.Replace('\"', '\'');
            
            foreach (var option in StartOptions) {
                var strpIndex = strpToString.IndexOf(option, StringComparison.Ordinal);
                strpToString = strpToString.Remove(strpIndex - 1, 1);
            }
            strpToString = $"{PwsStartCommand} {strpToString}";
            var pwsArgument = new List<string>();
            foreach (var variable in EnvironmentVariables) pwsArgument.Add(variable.ToStringPws());
            pwsArgument.Add(strpToString);

            _pws.Command = string.Join("; ", pwsArgument); 
            var pwsToStringArray = RetroLinkerParser.FormatCommandLineArgs(_pws, RetroLinkerUnParsing);
            var pwsIndex = pwsToStringArray[0].IndexOf(PwsCommandOpt, StringComparison.Ordinal);
            string PwsArrayToString() => string.Join(SepWhiteSpace, pwsToStringArray);
            
            if (pwsIndex < 0) throw new CommandLineParserException(InvalidArgumentsMessage(PwsArrayToString()));
            
            pwsToStringArray[0] = pwsToStringArray[0].Remove(pwsIndex - 1, 1);
            pwsToStringArray[1] = pwsToStringArray[1].WrapStringBetweenQuotes();
            pwsToStringArray[1] = pwsToStringArray[1].Replace(quoteArtifactPlHld, $"{Utils.DQ}{Utils.DQ}");
            
            return PwsArrayToString();
        }
        
        /// <summary>
        /// Create an instance of <c>WindowsPwsWrapper</c> from an existing command-line.
        /// </summary>
        /// <param name="singleLineArguments">The command-line to parse.</param>
        /// <exception cref="CommandLineParserException">Parsing fails; invalid command-line or improperly formatted.</exception>
        /// <remarks>The <paramref name="singleLineArguments"/> should be one that works from a shortcut file (.lnk).</remarks>
        public WindowsPwsWrapper(string singleLineArguments)
        {
            if (string.IsNullOrWhiteSpace(singleLineArguments) || !singleLineArguments.Contains(PwsCommandOpt))
                throw new CommandLineParserException($"The '{PwsCommandOpt}' argument is missing.", nameof(singleLineArguments));

            EnvironmentVariables = new();
            singleLineArguments =  singleLineArguments.Trim();
            var corrected = singleLineArguments.Insert(0, "-");
            var wht = corrected.IndexOf(SepWhiteSpace);
            corrected = corrected.Remove(wht, 1);
            corrected = corrected.Insert(wht, "=");
            
            var parsedPws = RetroLinkerParser.ParseArguments<WindowsPws?>([corrected]);
            _pws = parsedPws.Value ?? throw new CommandLineParserException(
                $"<{nameof(WindowsPws)}> could not be parsed from argument '{corrected}'", nameof(corrected));

            var separatedCommand = _pws.Command.UnwrapStringBetweenQuotes().Split(CommandsSeparator);
            var strpCandidates = ResolveCommendArgument(separatedCommand, EnvironmentVariables);
            var correctedArguments = ResolveStartProcessArguement(strpCandidates);

            string CorrectedArgumentsForError() => string.Join(SepWhiteSpace, correctedArguments);
            
            const int minValidArgumentCount = 2;
            if (correctedArguments.Count < minValidArgumentCount)
                throw new CommandLineParserException(
                    InvalidArgumentsMessage(CorrectedArgumentsForError()),
                    nameof(correctedArguments));
            
            var strpParsed = RetroLinkerParser.ParseArguments<PwsStartProcess?>(correctedArguments);
            _pwsStartProcess = strpParsed.Value ?? throw new CommandLineParserException(
                $"<{nameof(PwsStartProcess)}> could not be parsed from argument '{CorrectedArgumentsForError()}'",
                nameof(correctedArguments));

            var correctedFilePath = _pwsStartProcess.FilePath.UnwrapStringBetweenQuotes(doubleQuotes: false);
            var correctedArgumentList = _pwsStartProcess.ArgumentList.UnwrapStringBetweenQuotes(doubleQuotes: false);
            correctedArgumentList = correctedArgumentList.UndoTwoDoubleQuotes();
            
            RealTargetExecutable = correctedFilePath;
            RealTargetArguments = correctedArgumentList;
        }

        private Collection<string> ResolveCommendArgument(string[] separatedCommand, HashSet<EnvironmentVariable> environmentVariables)
        {
            var strpCandidates = new Collection<string>();
            foreach (var command in separatedCommand)
            {
                if (!command.Contains(PwsEnvSetter)) {
                    strpCandidates.Add(command);
                    continue;
                }
                
                var env = EnvironmentVariable.FromPwsArgument(command);
                if (env is not null) environmentVariables.Add(env);
                else Logger.LogWarn($"[{nameof(WindowsPwsWrapper)}] Could not parse variable '{command}'");
            }
            
            return strpCandidates;
        }

        private Collection<string> ResolveStartProcessArguement(IEnumerable<string> candidates)
        {
            var argCandidates = new Collection<string>();
            foreach (var candidate in candidates) {
                if (!candidate.Contains(PwsStartCommand) || 
                    !candidate.Contains(PwsStartFilePath) || 
                    !candidate.Contains(PwsStartArgumentList))
                {
                    Logger.LogWarn($"[{nameof(WindowsPwsWrapper)}] Could not parse argument '{candidate}'");
                    continue;
                }
                
                var start = candidate.Replace(PwsStartCommand, "");
                start = start.UndoTwoDoubleQuotes().Trim();
                
                foreach (var option in StartOptions)
                {
                    var ind = start.IndexOf(option, StringComparison.Ordinal);
                    start = start.Insert(ind - 1, "-");
                    
                    int[] indOffset = [ind - 3, ind];
                    foreach (var i in indOffset)
                    {
                        if (i < 0) continue;
                        var sep = start.IndexOf(SepWhiteSpace, i);
                        start = start.Remove(sep, 1);
                        start = start.Insert(sep, StartSeparatorPlHld);
                    }
                }

                foreach (var arg in start.Split(StartSeparatorPlHld))
                    argCandidates.Add(arg);
                break;
            }

            return argCandidates;
        } 
    }
}