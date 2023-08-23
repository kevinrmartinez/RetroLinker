using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RetroarchShortcutterV2.Models.WinFuncImport
{
    public class FuncLoader
    {

        const string WinFunc = "WinFunc.dll";           // Este dll de estar presente junto al ejecutable! (Windows)
        static Assembly DLL;

        public static void ImportWinFunc()
        {
            DLL = Assembly.LoadFrom(WinFunc);
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
