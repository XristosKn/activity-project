using Avalonia.Data.Converters;
using ActivityProjectApp.Helpers;
using System;
using System.Globalization;

namespace ActivityProjectApp.Converters
{
    public class CategoryBrushConverter : IValueConverter
    {
        public object Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            string category = value?.ToString() ?? string.Empty;

            return CategoryStyleHelper.GetAvaloniaBrush(category);
        }

        public object ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}