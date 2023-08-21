namespace RetroarchShortcutterV2.Models
{
    public class Commander
    {
        public const string contentless = "Contentless";
        const string verbose = "-v ";
        const string fullscreen = "-f ";
        const string accessibility = "--accessibility ";


        public static Shortcutter CommandBuilder(Shortcutter shortcut)
        {
            shortcut.Command = "-L " + shortcut.ROMcore;
            if (shortcut.CONFfile != null) 
            { shortcut.Command = shortcut.Command.Insert(0, "-c " + shortcut.CONFfile + " "); }
            if (shortcut.ROMdir != contentless) 
            { shortcut.Command += " " + shortcut.ROMdir; }

            // Parametros extras comunes
            if (shortcut.accessibilityB) { shortcut.Command = shortcut.Command.Insert(0, accessibility); }
            if (shortcut.fullscreenB) { shortcut.Command = shortcut.Command.Insert(0, fullscreen); }
            if (shortcut.verboseB) { shortcut.Command = shortcut.Command.Insert(0, verbose); }

            return shortcut;
        }
    }
}
