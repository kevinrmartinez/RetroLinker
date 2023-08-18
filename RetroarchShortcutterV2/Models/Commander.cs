using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroarchShortcutterV2.Models
{
    public class Commander
    {
        public static bool verboseB = false;
        public static bool fullscreenB = false;
        public static bool accessibilityB = false;
        public const string contentless = "Contentless";
        const string verbose = "-v ";
        const string fullscreen = "-f ";
        const string accessibility = "--accessibility ";


        public static string CommandBuilder(string Core, string ROM)
        {
            string command1 = "-L " + Core;
            if (ROM != contentless) { command1 += " " + ROM; }
            string command2 = CommandExtras(command1);
            return command2;
        }

        public static string CommandBuilder(string Core, string ROM, string? Conf = "retroarch.cfg")
        {
            //if (Conf == null) { Conf = ""; }
            string command1 = "-c " + Conf + " -L " + Core;
            if (ROM != contentless) { command1 += " " + ROM; }
            string command2 = CommandExtras(command1);
            return command2;
        }

        private static string CommandExtras(string command)
        {
            // Parametros extras comunes
            if (accessibilityB) { command = command.Insert(0, accessibility); }
            if (fullscreenB) { command = command.Insert(0, fullscreen); }
            if (verboseB) { command = command.Insert(0, verbose); }

            return command;
        }
    }
}
