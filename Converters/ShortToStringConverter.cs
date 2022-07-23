using System;
using System.Globalization;
using System.Windows.Data;

namespace NodeGraph.Converters
{
	[ValueConversion( typeof( short ), typeof( string ) )]
	public class ShortToStringConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return value.ToString();
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return ( short )double.Parse( value as string );
		}
	}
}
