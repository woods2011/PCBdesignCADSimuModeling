using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PcbDesignSimuModeling.WPF;

[ValueConversion(typeof(bool), typeof(Visibility))]
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public Visibility TrueValue { get; set; }
    public Visibility FalseValue { get; set; }

    public BoolToVisibilityConverter()
    {
        // set defaults
        TrueValue = Visibility.Visible;
        FalseValue = Visibility.Collapsed;
    }

    public object? Convert(object value, Type targetType, 
        object parameter, CultureInfo culture)
    {
        if (value is not bool flag)
            return null;
        return flag ? TrueValue : FalseValue;    
    }

    public object? ConvertBack(object value, Type targetType, 
        object parameter, CultureInfo culture)
    {
        if (Equals(value, TrueValue))
            return true;
        if (Equals(value, FalseValue))
            return false;
        return null;
    }
}