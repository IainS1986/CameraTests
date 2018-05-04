using MvvmCross.Platform.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmCrossTest.Core.Core.Converters
{
    public class IntToStringWithFormatConverter : MvxValueConverter<int, string>
    {
        protected override string Convert(int value, Type targetType, object parameter, CultureInfo culture)
        {
            string format = "{0}";
            if(parameter != null && parameter is string)
            {
                format = parameter as string;
            }

            return string.Format(format, value);
        }
    }
}
