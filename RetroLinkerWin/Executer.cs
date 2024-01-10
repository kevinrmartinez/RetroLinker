using System.IO;

namespace RetroLinkerWin
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
