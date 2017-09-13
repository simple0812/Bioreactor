using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Shunxi.App.CellMachine.Converters
{
    public class BoolToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            int p = -1;
            if (parameter != null)
            {
                p = int.TryParse(parameter.ToString(), out p) ? p : -1;
            }

            try
            {
                var t = System.Convert.ToBoolean(value);
                return t ? (p == -1 ?  new SolidColorBrush(Colors.Wheat) : null)  : new SolidColorBrush(Colors.DarkGray);
            }
            catch (Exception)
            {
                return new SolidColorBrush(Colors.DarkGray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return value is SolidColorBrush ? ((SolidColorBrush)value).Color : Colors.Transparent;
        }
    }
}
