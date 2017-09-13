using System;
using System.Globalization;
using System.Windows.Data;

namespace Shunxi.App.CellMachine.Converters
{
    public class TextSingleLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            string text = value as string;
            return text?.Replace(Environment.NewLine, " ");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
