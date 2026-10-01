/*
    RetroLinker: A .NET GUI application to help create desktop links of games running on RetroArch.
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RetroLinker.Models;

namespace RetroLinker.Views;

public partial class PopUpWindow : Window
{
    public PopUpWindow()
    {
        // Constructor for Designer
        InitializeComponent();
        PopCC1.Content = "Blup";
    }

    public PopUpWindow(bool isDesigner) => InitializeComponent(); // <= For Designer

    public PopUpWindow(PopUpWindowType popUpWindowType, IMainViewPopUp popUpParams) : this(isDesigner: false)
    {
        switch (popUpWindowType)
        {
            case PopUpWindowType.MoreRaOptions:
                MoreOptionsPopUp(popUpParams);
                break;
            case PopUpWindowType.RenameLinux:
                RenamePopUp(popUpParams);
                break;
            default:
                break;
        }
    }

    private void MoreOptionsPopUp(IMainViewPopUp popUpParams)
    {
        if (popUpParams is not MoreOptPopUpHelper moreOptParams) return;
        Title = "More shortcut options";    // TODO: Localize 
        PopCC1.Content = new MoreRaOptionsView(this, moreOptParams);
        SizeToContent = SizeToContent.Height;
        Width = 500;
        MinWidth = 400;
        MaxWidth = 800;
        MinHeight = 500;
        MaxHeight = double.PositiveInfinity;
    }

    private void RenamePopUp(IMainViewPopUp popUpParams) {
        if (popUpParams is not RenamePopUpHelper renameParams) return;
        Title = Translations.resMainExtras.titleEntryName;
        PopCC1.Content = new RenameEntryView(this, renameParams);
        SizeToContent = SizeToContent.Height;
        Width = 600;
        MinWidth = 450;
        MaxWidth = 700;
        MinHeight = 130;
        MaxHeight = 200;
    }
}