using System;
using System.Globalization;
using System.Windows.Data;

namespace Shunxi.App.CellMachine.Converters
{
    public class AbsoluteToLocalConverter : IValueConverter
    {
        public double MaxValue { get; set; }
        public double MinValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (!(value is double))
                return value;
            return (double) value * (MaxValue - MinValue) + MinValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
