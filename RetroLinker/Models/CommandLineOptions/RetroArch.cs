using System.Collections.Generic;
using CommandLine;

namespace RetroLinker.Models.CommandLineOptions;

/* Due to how CommandLine works, the options are formatted to text following the 'longName' attribute (A->Z)
 * To export the way we want, we'll have to separete the operation in 6:
 * Each operation will just create the text for its group, and everything will be concatenated at the end.
 */
// TODO: This class can be nested inside the new CommandManager
public class RetroArch
{
    private const char sepPipe = '|';
    private const char sepWhiteSpace = ' ';

    // Toggles
    [Option('f', "fullscreen", Default = false,
        HelpText = "Start the program in fullscreen regardless of config setting.")]
    public bool G1_Fullscreen { get; set; }

    [Option("accessibility", Default = false,
        HelpText = "Enables accessibility for blind users using text-to-speech.")]
    public bool G1_Accessibility { get; set; }
    
    [Option("load-menu-on-error", Default = false,
        HelpText = "Open menu instead of quitting if specified core or content fails to load.")]
    public bool G1_MenuOnError { get; set; }

    // Log
    [Option('v', "verbose", Default = false,
        HelpText = "Verbose logging.")]
    public bool G2_Verbose { get; set; }
    
    [Option("log-file", 
        HelpText = "Log messages to a file.")]
    public string? G2_LogFile { get; set; }

    // Config
    [Option('c', "config", 
        HelpText = "Path for config file.")]
    public string? G3_Config { get; set; }

    [Option("appendconfig", Separator = sepPipe,
        HelpText = "Extra config files are loaded in, and take priority over config selected in -c (or default).")]
    public IEnumerable<string>? G3_AppendConfig { get; set; }
    
    // Core & Content
    [Option('L', "libretro", Required = true, Min  = 1, Max = 3,
        HelpText = "Path to libretro implementation. Overrides any config setting.")]
    public required IEnumerable<string> G4_Libretro { get; set; }
    
    // Patch
    [Option("no-patch", Default = false,
        HelpText = "Disables all forms of content patching.")]
    public bool G5_NoPatch { get; set; }

    [Option('U', "ups",
        HelpText = "Specifies path for UPS patch that will be applied to content.")]
    public string? G5_Ups { get; set; }
    
    [Option("bps", 
        HelpText = "Specifies path for BPS patch that will be applied to content.")]
    public string? G5_Bps { get; set; }
    
    [Option("ips", 
        HelpText = "Specifies path for IPS patch that will be applied to content.")]
    public string? G5_Ips { get; set; }
    
    [Option("xdelta", 
        HelpText = "Specifies path for Xdelta patch that will be applied to content.")]
    public string? G5_Xdelta { get; set; }
    
    // SubSystem
    [Option("subsystem", Separator = sepWhiteSpace,
        HelpText = "Use a subsystem of the libretro core.")]
    public IEnumerable<string>? G6_SubSystem { get; set; }
}