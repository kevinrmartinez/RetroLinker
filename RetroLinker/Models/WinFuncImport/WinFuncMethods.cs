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
    public class WinFuncMethods : IEquatable<WinFuncMethods>
    {
        public string MethodName { get; set; }
        public object ObjInstance { get; set; }
        public MethodInfo MInfo { get; set; }

        public WinFuncMethods(string name, object instance, MethodInfo methodInfo) 
        {
            MethodName = name;
            ObjInstance = instance;
            MInfo = methodInfo;
        }

        public bool Equals(WinFuncMethods other)
        {
            return other.MInfo == this.MInfo;
        }

        public override bool Equals(object obj)
        { return base.Equals(obj); }
    }
}
