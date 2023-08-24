using System;
using System.Drawing;
using System.IO;
using TsudaKageyu;

namespace WinFunc
{
    public class WinIconProc
    {
        public static MemoryStream ExtractIco(string DIR)
        {
            // Extraer el primer icono
            var stream = new MemoryStream();
            var IcoExtracr = new IconExtractor(DIR);
            //int index = IcoExtracr.Count - 1;
            Icon exeIco = IcoExtracr.GetIcon(0);          
            exeIco.Save(stream);
            stream.Position = 0;
            return stream;
        }

        public static MemoryStream ExtractIcoByIndex(string DIR, int INDEX)
        {
            // Extraer el icono especificado por indice
            var stream = new MemoryStream();
            var IcoExtracr = new IconExtractor(DIR);
            if (INDEX >= IcoExtracr.Count) 
            {
                Console.WriteLine("El indice indicado no existe!");
                Console.WriteLine("Intentando indice inferior");
                INDEX = IcoExtracr.Count - 1;
            }
            Icon exeIco = IcoExtracr.GetIcon(INDEX);
            exeIco.Save(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
