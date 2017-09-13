using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Shunxi.App.CellMachine.Common.Behaviors;

namespace Shunxi.App.CellMachine.Converters
{
    public class ProcessModeToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            var obj = value as EnumMemberInfo;
            if (obj == null)
                return Visibility.Collapsed;

            if (obj.Name == "None" || obj.Name == "SingleMode")
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
