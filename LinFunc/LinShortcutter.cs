﻿using System.Security.AccessControl;
using System.Text;

namespace LinFunc
{
    public class LinShortcutter
    {
        const string line1 = "[Desktop Entry]";
        const string notify = "StartupNotify=true";
        const string type = "Type=Application";

        public static void CreateShortcut(IList<object> _shortcut)
        {
            string dir = _shortcut[9].ToString();
            
            List<string> shortcut = new List<string>();
            shortcut.Add(line1);

            if (_shortcut[8] != null) { shortcut.Add("Comment=" + _shortcut[8].ToString()); }
            else { shortcut.Add("Comment="); }

            shortcut.Add("Exec=" + _shortcut[0].ToString() + " " + _shortcut[7].ToString());

            if (_shortcut[6] != null) { shortcut.Add("Icon=" + _shortcut[6].ToString()); }
            else { shortcut.Add("Icon="); }

            shortcut.Add(notify);

            if ((bool)_shortcut[10]) { shortcut.Add("Terminal=true"); }
            else { shortcut.Add("Terminal=false"); }

            shortcut.Add(type);

            File.WriteAllLines( dir, shortcut, Encoding.UTF8);
        }
    }
}