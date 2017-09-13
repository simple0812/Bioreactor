using System;
using System.Globalization;
using System.Windows.Data;

namespace Shunxi.App.CellMachine.Converters
{
   
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            try
            {
                var time = System.Convert.ToDateTime(value);
                if (time == DateTime.MinValue || time == default(DateTime)) return "";
                return time.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception)
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotSupportedException();
        }
    }
}
