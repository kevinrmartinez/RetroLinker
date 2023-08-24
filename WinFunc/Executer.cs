using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFunc
{
    public class Executer
    {
        public static MemoryStream Main(string dir)
        {
            var iconStream = WinIconProc.ExtractIco(dir);
            return iconStream;
        }
    }
}
