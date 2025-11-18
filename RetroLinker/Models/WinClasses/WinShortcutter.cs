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
using System.Diagnostics;
using Microsoft.ClearScript.Windows.Core;

namespace RetroLinker.Models.WinClasses;

public static class WinShortcutter
{
    private const string objShell = "shell";
    private const string objLink = "link";
    private const string objArray = "valueArray";
    private const string createLink = "CreateLink";
    private const string readLink = "ReadLink";
    private static readonly string scriptTitle = $"{App.AppName} Script Runner";
    
    public static void CreateShortcut(Shortcutter _shortcut, string _outputPath)
    {
        var iconPath = (string.IsNullOrEmpty(_shortcut.ICONfile)) ? _shortcut.RAdir : _shortcut.ICONfile;
        var scriptStrings = $"""
                          ' {App.AppName} v{App.AppVersion}
                          Function {createLink}()
                            Set {objShell} = CreateObject("WScript.Shell")
                            Set {objLink} = {objShell}.CreateShortcut("{_outputPath}")
                            {objLink}.TargetPath = "{_shortcut.RAdir}"
                            {objLink}.WorkingDirectory = "{_shortcut.RApath}"
                            {objLink}.Arguments = "{_shortcut.Command}"
                            {objLink}.Description = "{_shortcut.Desc}"
                            {objLink}.IconLocation = "{iconPath}"
                            {objLink}.Save
                          	{createLink} = 0
                          End Function
                          """;
        
        RunLinkWriteScript(scriptStrings);
        App.Logger?.LogInfo($"\"{_outputPath}\" file created successfully.");
    }

    // Return a Shortcutter type
    public static string?[] ReadShortcut(string linkPath)
    {
        // Why does creating a Array(4) is VBS results in an array with 5 positions?
        var scriptStrings = $"""
                            ' {App.AppName} v{App.AppVersion}
                            Function {readLink}()
                                Dim {objArray}(4)
                                Set {objShell} = CreateObject("WScript.Shell")
                                Set {objLink} = {objShell}.CreateShortcut("{linkPath}")
                                {objArray}(0) = {objLink}.TargetPath
                                {objArray}(1) = {objLink}.WorkingDirectory
                                {objArray}(2) = {objLink}.Arguments
                                {objArray}(3) = {objLink}.Description
                                {objArray}(4) = {objLink}.IconLocation
                                {readLink} = {objArray}
                            End Function
                            """;
        
        var linkContent = RunLinkReadScript(scriptStrings);
        App.Logger?.LogInfo($"\"{linkPath}\" file read successfully.");
        var linkStrings = new string?[linkContent.Length];
        for (int i = 0; i < linkContent.Length; i++)
            linkStrings[i] = linkContent[i].ToString();
        return linkStrings;
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
        // VBS returns Int16 (short) instead of Int32 (int)
        var result = (short)engine.Invoke($"{createLink}");
        
        if (result == 0) return;
        var err = "The LinkWrite script was not executed properly!";
        App.Logger?.LogErro(err);
        throw new ApplicationException(err);
    }
    
    private static object[] RunLinkReadScript(string script)
    {
        var engine = RunScriptEngine(script);
        var values = (object[])engine.Invoke($"{readLink}");
        return values;
    }
}