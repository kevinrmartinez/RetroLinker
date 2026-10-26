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

namespace RetroLinker;

public class CommandLineParserException : ArgumentException
{
    private const string DefaultMessage = "An error ocurred while parsing command line arguments.";
    
    public CommandLineParserException() : base(DefaultMessage) { }
    public CommandLineParserException(string message) : base(message) { }
    public CommandLineParserException(string message, string? paramName) : base(message,  paramName) { }
    public CommandLineParserException(string message, Exception innerException) : base(message, innerException) { }
    public CommandLineParserException(string message, string? paramName, Exception innerException) : base(message, paramName, innerException) { }
}