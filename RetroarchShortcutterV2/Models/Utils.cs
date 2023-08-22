using System;
using System.Linq;
using System.Reflection;

namespace RetroarchShortcutterV2.Models
{
    public class Utils
    {
        const string comilla = "\"";
        public static string FixUnusualDirectories(string dir)
        {
            string newdir = dir;
            if (newdir.ElementAt(0) != comilla.ElementAt(0))                    // Si la primera letra de newdir no es ", entonces se le agrega
            { newdir = newdir.Insert(0, comilla); }
            if (newdir.ElementAt(newdir.Length - 1) != comilla.ElementAt(0))    // Si la última letra de newdir no es ", entonces se le agrega
            { newdir = newdir + comilla; }
            return newdir;
        }

        const string WinFunc = "C:\\Users\\force\\source\\C#\\RetroarchShortcutterV2\\WinFunc\\bin\\Debug\\net7.0\\WinFunc.dll";
        public static object? objInstance;
        public static MethodInfo GetShortcutMethod()
        {
            var DLL = Assembly.LoadFrom(WinFunc);
            var WinShortcutter = DLL.GetType("WinFunc.WinShortcutter");
            var objWinShortcutter = Activator.CreateInstance(WinShortcutter);
            objInstance = objWinShortcutter;

            var methodArgsTypes = new Type[]
            {
                typeof(string), typeof(string), typeof(string), 
                typeof(string), typeof(string), typeof(string)
            };
            MethodInfo method = WinShortcutter.GetMethod("CreateShortcut", methodArgsTypes);
            return method;
        }
    }
}
