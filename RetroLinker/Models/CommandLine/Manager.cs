/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023  Kevin Rafael Martinez Johnston

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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using CommandLine;
using RetroLinker.Models.DesktopLink;

namespace RetroLinker.Models.CommandLine;

public static partial class Manager
{
    public const string Contentless = "Contentless";
    private const char sepWhiteSpace = ' ';
    private const char sepPipe = '|';
        
    public const string UpsExt = "ups";
    public const string BpsExt = "bps";
    public const string IpsExt = "ips";
    public const string XdExt = "xdelta";

    private static IUserLinkParameters? _userLinkParameters;
    /// <summary>
    /// General purpose parser used 
    /// </summary>
    private static readonly Parser RetroLinkerParser = new(sett => {
        // sett.AutoHelp = false;
        // sett.AutoVersion = false;
        sett.HelpWriter = null;
        sett.CaseSensitive = true;
        sett.IgnoreUnknownArguments = false;
    });

    private static void RetroarchUnParsing(UnParserSettings settings) {
        settings.PreferShortName = true;
        settings.UseEqualToken = false;
        settings.SkipDefault = true;
    }
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RetroArch))]
    public static string CommandBuilder(IUserLinkParameters link)
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
        
        var command = string.Join(sepWhiteSpace, correctedResults);
        
        _userLinkParameters = null;
        return command;
    }
}