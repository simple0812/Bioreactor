using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Shunxi.App.CellMachine.Converters
{
    public class EmptyImageToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            byte[] image = value as byte[];
            if (ForceEmptyObject)
            {
                if (image == null || image.Length == 0)
                {
                    return (Inverse) ? Visibility.Collapsed : Visibility.Visible;
                }
                return (Inverse) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (image == null)
                return GetInverse(Visibility.Collapsed);
            if (image.Length == 0)
                return GetInverse(Visibility.Visible);
            return GetInverse(Visibility.Collapsed);
        }

        Visibility GetInverse(Visibility visibility)
        {
            return !Inverse ? visibility : visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }

        public bool Inverse { get; set; }
        public bool ForceEmptyObject { get; set; }
    }
}
