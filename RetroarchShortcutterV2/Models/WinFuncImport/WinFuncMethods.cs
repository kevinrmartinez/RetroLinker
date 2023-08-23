﻿using RetroarchShortcutterV2.Models.WinIco;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RetroarchShortcutterV2.Models.WinFuncImport
{
    public class WinFuncMethods : IEquatable<WinFuncMethods>
    {
        public string MethodName { get; set; }
        public object objInstance { get; set; }
        public MethodInfo mInfo { get; set; }

        public WinFuncMethods(string name, object instance, MethodInfo methodInfo) 
        {
            this.MethodName = name;
            this.objInstance = instance;
            this.mInfo = methodInfo;
        }

        public bool Equals(WinFuncMethods other)
        {
            return other.mInfo == this.mInfo;
        }

        public override bool Equals(object obj)
        { return base.Equals(obj); }



    }
}