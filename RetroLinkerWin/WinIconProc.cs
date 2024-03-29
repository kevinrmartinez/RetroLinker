﻿/*
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
using System.Drawing;
using System.IO;

namespace RetroLinkerWin
{
    public class WinIconProc
    {
        public static MemoryStream ExtractIco(string dir)
        {
            // Extract the first icon
            var stream = new MemoryStream();
            var IcoExtracr = new IconExtractor(dir);
            //int index = IcoExtracr.Count - 1;
            Icon exeIco = IcoExtracr.GetIcon(0);          
            exeIco.Save(stream);
            stream.Position = 0;
            return stream;
        }

        public static MemoryStream ExtractIcoByIndex(string dir, int index)
        {
            // Extract the icon specified by the index
            var stream = new MemoryStream();
            var IcoExtracr = new IconExtractor(dir);
            if (index >= IcoExtracr.Count) 
            {
                Console.WriteLine("El indice indicado no existe!");
                Console.WriteLine("Intentando indice inferior");
                index = IcoExtracr.Count - 1;
            }
            Icon exeIco = IcoExtracr.GetIcon(index);
            exeIco.Save(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
