using System;
using System.Globalization;
using System.Windows.Data;

namespace Shunxi.App.CellMachine.Converters
{
    public class CategoryNamesConverter : IValueConverter
    {
        public double TrueValue { get; set; }
        public double FalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            switch (value.ToString())
            {
                case "Automation": return "Auto";
                case "Televisions": return "TVs";
                case "VideoPlayers": return "Video";
                case "Video Players": return "Video";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotSupportedException();
        }
    }
}
