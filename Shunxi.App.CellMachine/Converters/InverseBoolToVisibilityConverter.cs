using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Shunxi.App.CellMachine.Converters
{
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {

            try
            {
                var t = System.Convert.ToBoolean(value);
                return t ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception)
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return value is SolidColorBrush ? ((SolidColorBrush)value).Color : Colors.Transparent;
        }
    }
}
