using System;
using System.Globalization;
using System.Windows.Data;

namespace Shunxi.App.CellMachine.Converters
{
    public class IntToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            return System.Convert.ChangeType(value, typeof(double));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
