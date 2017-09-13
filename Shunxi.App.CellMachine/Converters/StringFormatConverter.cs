using System;
using System.Globalization;
using System.Windows.Data;

namespace Shunxi.App.CellMachine.Converters
{
    public sealed class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value == null)
                return null;
            if (parameter == null)
                return value;
            return string.Format((string) parameter, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
