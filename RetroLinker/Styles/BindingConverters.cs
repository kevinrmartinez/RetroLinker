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
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace RetroLinker.Styles;

public class TextToUpper : IValueConverter
{
    public static readonly TextToUpper Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str && targetType.IsInstanceOfType(str)) return str.ToUpper(culture);
        // converter used for the wrong type
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // throw new NotSupportedException();
        return value;
    }
}

public class TextToLower : IValueConverter
{
    public static readonly TextToLower Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str && targetType.IsInstanceOfType(str)) return str.ToLower(culture);
        // converter used for the wrong type
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // throw new NotSupportedException();
        return value;
    }
}

public class DateToLocal : IValueConverter
{
    public static readonly DateToLocal Instance = new();
    private DateTime ogDateTime; 
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dt)
        {
            ogDateTime = dt;
            dt = dt.ToLocalTime();
            if (targetType.IsAssignableTo(typeof(DateTime)) || targetType == typeof(object)) return dt;
            else if (targetType.IsAssignableTo(typeof(string))) return dt.ToString(culture);
        }
        
        // converter used for the wrong type
        return new BindingNotification(new InvalidCastException($"targetType is '{targetType.Name}'"), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // throw new NotSupportedException();
        // I think this is correct
        if (targetType.IsAssignableTo(typeof(DateTime))) return ogDateTime;
        else if (targetType.IsAssignableTo(typeof(string))) return ogDateTime.ToString(culture);
        return value;
    }
}