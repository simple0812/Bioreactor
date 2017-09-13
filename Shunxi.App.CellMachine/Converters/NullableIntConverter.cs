using System;
using System.Globalization;
using System.Windows.Data;

namespace Shunxi.App.CellMachine.Converters
{
    public class NullableIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return (int?) ((double) value);
        }
    }
}
