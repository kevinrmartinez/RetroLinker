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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using CommandLine;
using RetroLinker.Models.DesktopLink;
using RetroLinker.Models.IO;
using RetroLinker.Models.Windows;

namespace RetroLinker.Models.CommandLine;

public static partial class Manager
{
    public const string PwsEnvSetter = "$env:";
    
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
                           | DynamicallyAccessedMemberTypes.PublicConstructors, typeof(StartProcess))]
        private StartProcess _pwsStartProcess;
        
        private const char CharEscape = '^';
        private const string EscapedSqPlHld = "@#SQ#@";
        private const string CommandOpt = "Command";
        private const string CommandsSeparator = ";";
        private const string StartCommand = "Start-Process";
        private const string StartFilePath = "FilePath";
        private const string StartArgumentList = "ArgumentList";
        private const string StartSeparatorPlHld = "@#Sep#@";
        
        private static readonly string[] startOptions = [StartFilePath, StartArgumentList];

        public WindowsPwsWrapper(string realTargetExecutable, string realTargetArguments,
            HashSet<EnvironmentVariable> environmentVariables)
        {
            RealTargetExecutable = realTargetExecutable;
            RealTargetArguments = realTargetArguments;
            EnvironmentVariables = environmentVariables;
            _pwsStartProcess = new StartProcess() {
                FilePath = RealTargetExecutable,
                ArgumentList = RealTargetArguments
            };
            _pws = new WindowsPws();
        }
        
        public WindowsPwsWrapper(string singleLineArguments)
        {
            if (string.IsNullOrWhiteSpace(singleLineArguments) || !singleLineArguments.Contains(CommandOpt))
                throw new ArgumentException($"The '{CommandOpt}' argument is missing.");

            EnvironmentVariables = new();
            singleLineArguments =  singleLineArguments.Trim();
            var corrected = singleLineArguments.Insert(0, "-");
            var wht = corrected.IndexOf(SepWhiteSpace);
            corrected = corrected.Remove(wht, 1);
            corrected = corrected.Insert(wht, "=");
            
            var parsedPws = RetroLinkerParser.ParseArguments<WindowsPws?>([corrected]);
            _pws = parsedPws.Value ?? throw new MissingFieldException();
            
            var strpCandidates = new Collection<string>();
            var separatedCommands = _pws.Command.UnwrapStringBetweenQuotes().Split(CommandsSeparator);
            foreach (var command in separatedCommands)
            {
                if (!command.Contains(PwsEnvSetter)) {
                    strpCandidates.Add(command);
                    continue;
                }
                
                var env = EnvironmentVariable.FromPwsArgument(command);
                if (env is not null) EnvironmentVariables.Add(env);
                else Logger.LogWarn($"[{nameof(WindowsPwsWrapper)}] Could not parse variable '{command}'");
            }

            var correctedArguments = new List<string>();
            foreach (var candidate in strpCandidates) {
                if (!candidate.Contains(StartCommand) || 
                    !candidate.Contains(StartFilePath) || 
                    !candidate.Contains(StartArgumentList))
                {
                    Logger.LogWarn($"[{nameof(WindowsPwsWrapper)}] Could not parse argument '{candidate}'");
                    continue;
                }
                
                var start = candidate.Replace(StartCommand, "");
                start = Utils.UndoTwoDoubleQuotes(start);
                start = start.Trim();
                
                foreach (var option in startOptions)
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
                
                correctedArguments.AddRange(start.Split(StartSeparatorPlHld));
                break;
            }

            if (correctedArguments is null) throw new MissingFieldException("Invalid arguments, or not formatted correctly.");
            
            var strpParsed = RetroLinkerParser.ParseArguments<StartProcess?>(correctedArguments);
            _pwsStartProcess = strpParsed.Value ?? throw new MissingFieldException();   // TODO: Replace with dedicated exception

            var correctedFilePath = _pwsStartProcess.FilePath.UnwrapStringBetweenQuotes(doubleQuotes: false);
            var correctedArgumentList = _pwsStartProcess.ArgumentList.UnwrapStringBetweenQuotes(doubleQuotes: false);
            correctedArgumentList = Utils.UndoTwoDoubleQuotes(correctedArgumentList);
            
            RealTargetExecutable = correctedFilePath;
            RealTargetArguments = correctedArgumentList;
        }

        public string BuildCommandLineArg()
        {
            const string quoteArtifact = "\\\"";
            const string quoteArtifactPlHld = "#{DQ}#";
            
            var strpToString = RetroLinkerParser.FormatCommandLine(_pwsStartProcess, RetroLinkerUnParsing);
            strpToString = strpToString.Replace(quoteArtifact, quoteArtifactPlHld);
            strpToString = strpToString.Replace('\"', '\'');
            
            foreach (var option in startOptions) {
                var strpIndex = strpToString.IndexOf(option, StringComparison.Ordinal);
                strpToString = strpToString.Remove(strpIndex - 1, 1);
            }
            strpToString = $"{StartCommand} {strpToString}";
            var pwsArgument = new List<string>();
            foreach (var variable in EnvironmentVariables) pwsArgument.Add(variable.ToStringPws());
            pwsArgument.Add(strpToString);

            _pws.Command = string.Join("; ", pwsArgument); 
            var pwsToStringArray = RetroLinkerParser.FormatCommandLineArgs(_pws, RetroLinkerUnParsing);
            var pwsIndex = pwsToStringArray[0].IndexOf(CommandOpt, StringComparison.Ordinal);
            if (pwsIndex < 0) throw new MissingFieldException("Invalid arguments, or not formatted correctly.");
            pwsToStringArray[0] = pwsToStringArray[0].Remove(pwsIndex - 1, 1);

            pwsToStringArray[1] = pwsToStringArray[1].WrapStringBetweenQuotes();
            pwsToStringArray[1] = pwsToStringArray[1].Replace(quoteArtifactPlHld, $"{Utils.DQ}{Utils.DQ}");
            
            var pwsToString = string.Join(SepWhiteSpace, pwsToStringArray);
            return pwsToString;
        }
    }
}