/*
    A .NET GUI application to help create desktop links of games running on RetroArch.
    Copyright (C) 2024  Kevin Rafael Martinez Johnston

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
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.ClearScript.Windows.Core;

namespace RetroLinker.Models.WinClasses;

public static class WinShortcutter
{
    private const string objShell = "shell";
    private const string objLink = "link";
    private const string objArray = "valueArray";
    private static readonly string scriptTitle = $"{App.AppName} Script Runner";
    
    public static void CreateShortcut(Shortcutter _shortcut, string _outputPath)
    {
        var iconPath = (string.IsNullOrEmpty(_shortcut.ICONfile)) ? _shortcut.RAdir : _shortcut.ICONfile;
        var scriptStrings = $"""
                          ' {App.AppName} v{App.AppVersion}
                          Function CreateLink()
                            Set {objShell} = CreateObject("WScript.Shell")
                            Set {objLink} = {objShell}.CreateShortcut("{_outputPath}")
                            {objLink}.TargetPath = "{_shortcut.RAdir}"
                            {objLink}.WorkingDirectory = "{_shortcut.RApath}"
                            {objLink}.Arguments = "{_shortcut.Command}"
                            {objLink}.Description = "{_shortcut.Desc}"
                            {objLink}.IconLocation = "{iconPath}"
                            {objLink}.Save
                          	CreateLink = 0
                          End Function
                          """;
        
        RunLinkWriteScript(scriptStrings);
        Trace.WriteLine($"\"{_outputPath}\" file created successfully.", App.InfoTrace);
    }

    public static string[] ReadShortcut(string linkPath)
    {
        // Why does creating a Array(4) is VBS results in a 5 positions array?
        var scriptStrings = $"""
                            ' {App.AppName} v{App.AppVersion}
                            Function ReadLink()
                                Dim {objArray}(4)
                                Set {objShell} = CreateObject("WScript.Shell")
                                Set {objLink} = {objShell}.CreateShortcut("{linkPath}")
                                {objArray}(0) = {objLink}.TargetPath
                                {objArray}(1) = {objLink}.WorkingDirectory
                                {objArray}(2) = {objLink}.Arguments
                                {objArray}(3) = {objLink}.Description
                                {objArray}(4) = {objLink}.IconLocation
                                ReadLink = {objArray}
                            End Function
                            """;
        
        var linkContent = RunLinkReadScript(scriptStrings);
        Trace.WriteLine($"\"{linkPath}\" file read successfully.", App.InfoTrace);
        return linkContent;
    }
    
    private static VBScriptEngine RunScriptEngine(string script)
    {
        var engine = new VBScriptEngine($"{scriptTitle}", NullSyncInvoker.Instance);
        engine.Execute(script);
        return engine;
    }

    private static void RunLinkWriteScript(string script)
    {
        var engine = RunScriptEngine(script);
        int result = engine.Script.CreateLink();
        
        if (result == 0) return;
        var err = "The LinkWrite script was not executed properly!";
        Trace.WriteLine(err, App.ErroTrace);
        throw new ApplicationException(err);
    }

    private static string[] RunLinkReadScript(string script)
    {
        var engine = RunScriptEngine(script);
        object[] values = engine.Script.ReadLink();
        
        var linkContent = new List<string>();
        foreach (var o in values)
            linkContent.Add((string)o);
        return linkContent.ToArray();
    }
}