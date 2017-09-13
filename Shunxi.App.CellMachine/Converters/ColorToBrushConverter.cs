using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Shunxi.App.CellMachine.Converters
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            return value is Color ? new SolidColorBrush((Color) value) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return value is SolidColorBrush ? ((SolidColorBrush) value).Color : Colors.Transparent;
        }
    }
}
