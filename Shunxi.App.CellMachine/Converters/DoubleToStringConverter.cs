using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Shunxi.App.CellMachine.Converters
{
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value == null) return "";
            var ret = value.ToString();
            if (!ret.Contains("."))
            {
                ret += ".";
            }

            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value == null) return 0;
            var str = value.ToString();
            if (str.Contains(".") && str.IndexOf(".", StringComparison.Ordinal) == str.Length - 1)
            {
                str += "0";
            }

            return ConvertStr(str);
        }

        private double ConvertStr(string str)
        {
            if (str.Length == 0) return 0;

            double ret = double.TryParse(str, out ret) ? ret : -1D;
            if(ret > 0)
                return ret;

            return ConvertStr(str.Remove(str.Length -1));
        }
    }
}
