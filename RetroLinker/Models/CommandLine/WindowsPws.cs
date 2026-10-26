/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2023-2026 Kevin Rafael Martinez Johnston

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

using CommandLine;

namespace RetroLinker.Models.CommandLine;

public static partial class Manager
{
    private partial class WindowsPwsWrapper
    {
        private class WindowsPws
        {
            [Option(PwsCommandOpt, Required = true)]
            public string Command { get; set; }
        }
        
        private class PwsStartProcess
        {
            [Option(PwsStartFilePath, Required = true)]
            public string FilePath { get; set; }
            
            [Option(PwsStartArgumentList, Required = true)]
            public string ArgumentList { get; set; }
        }
    }
}