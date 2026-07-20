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