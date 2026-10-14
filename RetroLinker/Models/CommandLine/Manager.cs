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

using System.Diagnostics.CodeAnalysis;
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
    private static readonly Parser RetroarchParser = new(sett => {
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
        var command = string.Join(sepWhiteSpace, resultsArray);
        
        _userLinkParameters = null;
        return command;
    }

    private static string FormatRaCommand(RetroArch? raParams) {
        return (raParams is null) 
            ? string.Empty 
            : RetroarchParser.FormatCommandLine(raParams, RetroarchUnParsing);
    }
    
    private static string BuildCommandRaG1() {
        var group1 = RetroArch.GetGroup1(_userLinkParameters);
        return FormatRaCommand(group1);
    }

    private static string BuildCommandRaG2() {
        var group2 = RetroArch.GetGroup2(_userLinkParameters);
        return FormatRaCommand(group2);
    }
    
    private static string BuildCommandRaG3() {
        var group3 = RetroArch.GetGroup3(_userLinkParameters);
        return FormatRaCommand(group3);
    }
    
    private static string BuildCommandRaG4() {
        var group4 = RetroArch.GetGroup4(_userLinkParameters);
        return FormatRaCommand(group4);
    }
    
    private static string BuildCommandRaG5() {
        var group5 = RetroArch.GetGroup5(_userLinkParameters);
        return FormatRaCommand(group5);
    }
    
    private static string BuildCommandRaG6() {
        var group6 = RetroArch.GetGroup6(_userLinkParameters);
        return FormatRaCommand(group6);
    }
}