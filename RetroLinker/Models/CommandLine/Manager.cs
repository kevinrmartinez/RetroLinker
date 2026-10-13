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
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using RetroLinker.Models.DesktopLink;

namespace RetroLinker.Models.CommandLine;

// TODO: Re-implement with CommandLineParser
public static partial class Manager
{
    public const string Contentless = "Contentless";
    private const char optValueSeparator = '=';
    private const string verbose = "-v ";
    private const string fullscreen = "-f ";
    private const string accessibility = "--accessibility ";
    private const string menuOnError = "--load-menu-on-error ";
    private const string appendConfig = "--appendconfig=";
    private const char appendConfigDeli = '|';
    private const string subsystem = "--subsystem=";
        
    public const string UpsExt = "ups";
    public const string BpsExt = "bps";
    public const string IpsExt = "ips";
    public const string XdExt = "xdelta";
    private const string UpsOpt = $"--{UpsExt}=";
    private const string BpsOpt = $"--{BpsExt}=";
    private const string IpsOpt = $"--{IpsExt}=";
    private const string XdOpt = $"--{XdExt}=";
    private const string NpOpt = "--no-patch";
    public static SoftPatch UpsPatch = new(UpsExt, UpsOpt, RomPatchType.UPS);
    public static SoftPatch BpsPatch = new(BpsExt, BpsOpt,  RomPatchType.BPS);
    public static SoftPatch IpsPatch = new(IpsExt, IpsOpt,  RomPatchType.IPS);
    public static SoftPatch XdPatch = new(XdExt, XdOpt,   RomPatchType.XDelta);
    public static SoftPatch NoPatch = new("no", string.Empty,  RomPatchType.NoPatch);
    public static SoftPatch ExNoPatch = new("explicit-no", NpOpt, RomPatchType.ExNoPatch);

    public static string CommandBuilder(IUserLinkParameters link)
    {
        return string.Empty;
    }

    private static IUserLinkParameters? _userLinkParameters;
    private static readonly Parser RetroarchParser = new(sett => {
        // sett.AutoHelp = false;
        // sett.AutoVersion = false;
        sett.CaseSensitive = true;
        sett.IgnoreUnknownArguments = false;
        sett.HelpWriter = null;
    });

    private static void RetroarchUnParsing(UnParserSettings settings) {
        settings.PreferShortName = true;
        settings.UseEqualToken = false;
        settings.SkipDefault = true;
    }

    private static string FormatRaCommand(RetroArch? raParams)
    {
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
        var group2 = RetroArch.GetGroup3(_userLinkParameters);
        return FormatRaCommand(group2);
    }
    
    private static string BuildCommandRaG4() {
        var group2 = RetroArch.GetGroup4(_userLinkParameters);
        return FormatRaCommand(group2);
    }
    
    private static string BuildCommandRaG5() {
        var group2 = RetroArch.GetGroup5(_userLinkParameters);
        return FormatRaCommand(group2);
    }
    
    private static string BuildCommandRaG6() {
        var group2 = RetroArch.GetGroup6(_userLinkParameters);
        return FormatRaCommand(group2);
    }
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RetroArch))]
    public static void TestParser()
    {
        var ra = RetroarchParser.ParseArguments<RetroArch>(
            ["-f", "--accessibility", "-L", "fceumm", "/mnt/ARIES/Games/ROMs/nes/Super Mario Bros. (World).nes", "--ips=/mnt/ARIES/Games/ROMs/nes/patch/SMB1__DAISY_PEACH.ips", "--no-patch"]);
        var ro = (RetroArch?)ra.Value;
        if (ro is null) return;
        Logger.LogDebg(RetroarchParser.FormatCommandLine(ro, RetroarchUnParsing));

        _userLinkParameters = new UserLinkBuilder() {
            RaCore = "fceumm",
            RaRom = "supermario.nes",
            RaConfig = "/path/to/config",
            RaLogFile = "abteeeeeeee",
            RaVerbose = true,
            RaMenuOnError = true,
            RaFullscreen = false
        };
        // var methodList = new List<System.Func<string>>() {
        //     BuildCommandRaG1,
        //     BuildCommandRaG2,
        //     BuildCommandRaG3,
        //     BuildCommandRaG4,
        //     BuildCommandRaG5,
        //     BuildCommandRaG6
        // };
        
        var timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        
        // var taskList = new List<Task<string>>();
        // foreach (var method in methodList) taskList.Add(Task.Run(method));
        var g1 = Task.Run(BuildCommandRaG1);
        var g2 = Task.Run(BuildCommandRaG2);
        var g3 = Task.Run(BuildCommandRaG3);
        var g4 = Task.Run(BuildCommandRaG4);
        var g5 = Task.Run(BuildCommandRaG5);
        var g6 = Task.Run(BuildCommandRaG6);
        // var g1 = BuildCommandRaG1();
        // var g2 = BuildCommandRaG2();
        // var g3 = BuildCommandRaG3();
        // var g4 = BuildCommandRaG4();
        // var g5 = BuildCommandRaG5();
        // var g6 = BuildCommandRaG6();
        
        var taskSet = timer.Elapsed;
        Task.WaitAll(g1, g2, g3, g4, g5, g6);
        var taskDone = timer.Elapsed;
        var command = $"{g1.Result} {g2.Result} {g3.Result} {g4.Result} {g5.Result} {g6.Result}";
        // var command = $"{g1} {g2} {g3} {g4} {g5} {g6}";
        
        timer.Stop();
        Logger.LogDebg(command);
        Logger.LogDebg($"Task set: {taskSet.TotalMicroseconds} µs");
        Logger.LogDebg($"Task done: {taskDone.TotalMicroseconds} µs");
        Logger.LogDebg($"Total time taken: {timer.Elapsed.TotalMicroseconds} µs");
        timer.Reset();
    }

    private static (string, string) GetOptionAndArgument(string arg)
    {
        var firstEqual = arg.IndexOf(optValueSeparator);
        if (firstEqual < 0) return (arg, string.Empty);
            
        var option = arg.Substring(0, firstEqual);
        var argument =  arg.Substring(firstEqual + 1);
        return (option, argument);
    }

    public static string GetArgumentNoOption(string arg) {
        var firstEqual = arg.IndexOf(optValueSeparator);
        return (firstEqual > 0) ? arg.Substring(firstEqual + 1) : arg;
    }
        
    // SoftPatching
    public static (string, SoftPatch) ResolveSoftPatchingArg(string arg)
    {
        var argException = new System.ArgumentException(@"Invalid patch argument: '" + arg + @"'",  nameof(arg));
        var (patchOpt, argument) = GetOptionAndArgument(arg);
        if (string.IsNullOrEmpty(argument)) 
            return (patchOpt == ExNoPatch.Option) ? (ExNoPatch.Option, ExNoPatch) : throw argException;
            
            
        var optEval = patchOpt + optValueSeparator;
        var argEval = Utils.ReversePutPathBetweenQuotes(argument);
        return optEval switch
        {
            UpsOpt => (argEval, UpsPatch),
            BpsOpt => (argEval, BpsPatch),
            IpsOpt => (argEval, IpsPatch),
            XdExt => (argEval, XdPatch), 
            _ => throw argException
        };
    }

    public static RomPatchArgs? CreateSoftPatchingArg(string patchFile, SoftPatch patchType)
    {
        if (patchType.PatchType is RomPatchType.NoPatch) return null;
        var arg = patchType.PatchType switch
        {
            RomPatchType.UPS or RomPatchType.BPS or RomPatchType.IPS or RomPatchType.XDelta 
                => Utils.PutPathBetweenQuotes(patchFile),
            RomPatchType.ExNoPatch => patchType.Option,
            _ => string.Empty
        };
        return new RomPatchArgs(patchType.PatchType, arg);
    }
        
        
    // AppendConfig
    public static (string, List<string>) ResolveAppendConfigArg(string arg)
    {
        if(string.IsNullOrWhiteSpace(arg))
            throw new System.ArgumentNullException(nameof(arg));
        if (!arg.StartsWith(appendConfig))
            throw new System.ArgumentException(@"Invalid append config argument: '" + arg + @"'", nameof(arg));
            
        var pathsCombined = GetArgumentNoOption(arg);
        pathsCombined = Utils.ReversePutPathBetweenQuotes(pathsCombined);
        var paths = pathsCombined.Split(appendConfigDeli);
        return (pathsCombined, new List<string>(paths));
    }

    public static string CreateAppendConfigArg(List<string> configFiles) {
        string appendConfigArg;
        if (configFiles.Count == 1)
            appendConfigArg = Utils.PutPathBetweenQuotes(configFiles.First());
        else {
            appendConfigArg = string.Join(appendConfigDeli, configFiles);
            appendConfigArg = Utils.PutPathBetweenQuotes(appendConfigArg);
        }
        return appendConfig + appendConfigArg;
    }
        
    // Subsystem
    public static (string, List<string>) ResolveSubsystemArg(string arg)
    {
        if(string.IsNullOrWhiteSpace(arg))
            throw new System.ArgumentNullException(nameof(arg));
        if (!arg.StartsWith(subsystem))
            throw new System.ArgumentException(@"Invalid subsystem argument: '" + arg + @"'", nameof(arg));
            
        var noOption = GetArgumentNoOption(arg);
        var subsys = noOption.Split(' ').First();
        var fullSubsysArgs = noOption.Substring(subsys.Length + 1);
        var subsysArgs = Utils.ParseSingleLineArguments(fullSubsysArgs);
        return (subsys, subsysArgs);
    }

    public static string CreateSubsystemArg(string subsys, ICollection<string> subsysArgs)
    {
        var arg = subsystem + subsys;
        var fixedArgs = new List<string>();
        foreach (var subsysArg in subsysArgs)
            fixedArgs.Add(Utils.PutPathBetweenQuotes(subsysArg));
        arg += " " + string.Join(" ", fixedArgs);
        return arg;
    }
}

// TODO
public enum RomPatchType {
    UPS, BPS, IPS,
    XDelta, NoPatch, ExNoPatch
}

public record SoftPatch(string ExtName, string Option, RomPatchType PatchType);