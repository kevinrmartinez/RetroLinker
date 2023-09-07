using System;
using System.Reflection;

namespace RetroarchShortcutterV2.Models.WinFuncImport
{
    public class WinFuncMethods : IEquatable<WinFuncMethods>
    {
        public string MethodName { get; set; }
        public object ObjInstance { get; set; }
        public MethodInfo MInfo { get; set; }

        public WinFuncMethods(string name, object instance, MethodInfo methodInfo) 
        {
            this.MethodName = name;
            this.ObjInstance = instance;
            this.MInfo = methodInfo;
        }

        public bool Equals(WinFuncMethods other)
        {
            return other.MInfo == this.MInfo;
        }

        public override bool Equals(object obj)
        { return base.Equals(obj); }
    }
}
