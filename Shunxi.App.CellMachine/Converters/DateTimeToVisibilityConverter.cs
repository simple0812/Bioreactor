using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Shunxi.App.CellMachine.Converters
{

    public class DateTimeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            try
            {
                var time = System.Convert.ToDateTime(value);
                return time != DateTime.MinValue && time != default(DateTime) ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception)
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
