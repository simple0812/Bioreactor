using System;
using System.Globalization;
using System.Windows.Data;

namespace Shunxi.App.CellMachine.Converters
{
    public class HalfValueConverter : IValueConverter
    {
        public bool NegativeValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (!(value is double))
                return value;
            var result = ((double) value) / 2;
            if (NegativeValue)
                result *= -1;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
