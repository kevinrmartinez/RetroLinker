/*
    A .NET GUI application to help create desktop links of games running on RetroArch.
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

using System;
using System.Reflection;

namespace RetroLinker.Models.WinFuncImport
{
    public static class FuncLoader
    {
        // Este .dll debe estar presente junto al ejecutable! (Windows)
        public const string WinFunc = "RetroLinkerWinLib.dll";
        static Assembly DLL;

        public static void ImportWinFunc()
        {
            System.Diagnostics.Trace.WriteLine($"Importando {WinFunc}...", "[Info]");
            DLL = Assembly.LoadFrom(WinFunc);
            System.Diagnostics.Trace.WriteLine($"{WinFunc} fue importado exitosamente.", "[Info]");
            System.Diagnostics.Debug.WriteLine($"{WinFunc} importado como '{DLL.FullName}'.", "[Info]");
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
