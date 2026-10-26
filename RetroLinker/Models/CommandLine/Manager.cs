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

using CommandLine;
using RetroLinker.Models.DesktopLink;

namespace RetroLinker.Models.CommandLine;

public static partial class Manager
{
    public const string Contentless = "Contentless";
    public const char SepWhiteSpace = ' ';
    public const char SepPipe = '|';
        
    public const string UpsExt = "ups";
    public const string BpsExt = "bps";
    public const string IpsExt = "ips";
    public const string XdExt = "xdelta";

    private static IUserLinkParameters? _userLinkParameters;
    
    /// <summary>
    /// General purpose parser.
    /// </summary>
    private static readonly Parser RetroLinkerParser = new(sett => {
        // sett.AutoHelp = false;
        // sett.AutoVersion = false;
        sett.HelpWriter = null;
        sett.CaseSensitive = true;
        sett.IgnoreUnknownArguments = false;
    });

    /// <summary>
    /// General purpose unparser.
    /// </summary>
    /// <param name="settings"></param>
    /// <remarks>This meyhod is meant to be passed to <see cref="UnParserExtensions.FormatCommandLine{T}(Parser,T,System.Action{UnParserSettings})"/>.</remarks>
    private static void RetroLinkerUnParsing(UnParserSettings settings) {
        settings.PreferShortName = true;
        settings.UseEqualToken = false;
        settings.SkipDefault = true;
    }
    
    private static string InvalidArgumentsMessage(string argument) 
        => $"Invalid arguments, or not formatted correctly: '{argument}'";
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties 
                       | DynamicallyAccessedMemberTypes.PublicConstructors, typeof(RetroArch))]
    public static string RetroArchCommandBuilder(IUserLinkParameters link)
    {
        _userLinkParameters = link;
        var g1 = Task.Run(BuildCommandRaG1);
        var g2 = Task.Run(BuildCommandRaG2);
        var g3 = Task.Run(BuildCommandRaG3);
        var g4 = Task.Run(BuildCommandRaG4);
        var g5 = Task.Run(BuildCommandRaG5);
        var g6 = Task.Run(BuildCommandRaG6);
        Task.WaitAll(g1, g2, g3, g4, g5, g6);
        string[] resultsArray = [g1.Result, g2.Result, g3.Result, g4.Result, g5.Result, g6.Result];
        
        var correctedResults = new List<string>();
        foreach (var result in resultsArray) {
            // Discard empty results, and shave leading and trailing white-spaces
            if (string.IsNullOrWhiteSpace(result)) continue;
            correctedResults.Add(result.Trim());
        }
        
        var command = string.Join(SepWhiteSpace, correctedResults);
        
        _userLinkParameters = null;
        return command;
    }

    public static IUserLinkParameters RetroArchCommandParser(string command) => RetroArchCommandParser(command.SplitArgs());

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties 
                       | DynamicallyAccessedMemberTypes.PublicConstructors, typeof(RetroArch))]
    public static IUserLinkParameters RetroArchCommandParser(string[] commandArray)
    {
        var parsed = RetroLinkerParser.ParseArguments<RetroArch>(commandArray);
        if (parsed.Tag == ParserResultType.NotParsed)
            throw new CommandLineParserException(InvalidArgumentsMessage(string.Join(SepWhiteSpace, commandArray)));
        return new ParsedRetroArch(parsed.Value);
    }
}