using System;
using System.Globalization;
using System.Windows.Data;

namespace NodeGraph.Converters
{
    [ValueConversion(typeof(byte), typeof(string))]
    public class ByteToStringConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (byte)double.Parse(value as string ?? string.Empty);
        }
        #endregion
    }
}