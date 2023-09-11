using System;
using System.Reflection;

namespace RetroarchShortcutterV2.Models.WinFuncImport
{
    public class FuncLoader
    {
        public const string WinFunc = "WinFunc.dll";           // Este dll debe estar presente junto al ejecutable! (Windows)
        static Assembly DLL;

        public static void ImportWinFunc()
        {
            System.Diagnostics.Debug.WriteLine($"Importando {WinFunc}...");
            DLL = Assembly.LoadFrom(WinFunc);
            System.Diagnostics.Debug.WriteLine($"{WinFunc} importado exitosamente como \"{DLL.FullName}\".");
        }

        public static WinFuncMethods GetShortcutMethod()
        {
            const string mName = "CreateShortcut";
            var WinShortcutter = DLL.GetType("WinFunc.WinShortcutter");
            var objWinShortcutter = Activator.CreateInstance(WinShortcutter);

            var methodArgsTypes = new Type[]
            {
                typeof(string), typeof(string), typeof(string),
                typeof(string), typeof(string), typeof(string)
            };
            MethodInfo method = WinShortcutter.GetMethod(mName, methodArgsTypes);
            var createShortcut = new WinFuncMethods(mName, objWinShortcutter, method);
            return createShortcut;
        }

        public static WinFuncMethods GetIcoExtractMethod()
        {
            const string mName = "ExtractIco";
            var WinShortcutter = DLL.GetType("WinFunc.WinIconProc");
            var objWinShortcutter = Activator.CreateInstance(WinShortcutter);

            var methodArgsTypes = new Type[]
                { typeof(string) };
            MethodInfo method = WinShortcutter.GetMethod(mName, methodArgsTypes);
            var ExtractIco = new WinFuncMethods(mName, objWinShortcutter, method);
            return ExtractIco;
        }

#if DEBUG
        public static WinFuncMethods GetIcoExecuterMethod()
        {
            const string mName = "Main";
            var WinShortcutter = DLL.GetType("WinFunc.Executer");
            var objWinShortcutter = Activator.CreateInstance(WinShortcutter);

            MethodInfo method = WinShortcutter.GetMethod(mName);
            var ExecuteIco = new WinFuncMethods(mName, objWinShortcutter, method);
            return ExecuteIco;
        }
#endif
    }
}
