using System;
using System.Globalization;
using System.Windows.Data;

namespace NodeGraph.Converters
{
    [ValueConversion(typeof(float), typeof(string))]
    public class FloatToStringConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (float)double.Parse(value as string ?? string.Empty);
        }
        #endregion
    }
}