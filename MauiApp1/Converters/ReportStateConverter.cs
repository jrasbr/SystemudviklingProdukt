using MauiApp1.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1.Converters
{
    public class ReportStateConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
           if (value == null)
                return null;

           if (value is ReportType reportType)
            {
                switch (reportType)
                {
                  
                    case ReportType.Normal:
                        return Colors.Transparent;
                    case ReportType.FollowUp:
                        return Colors.Red;
                }
            }
            return Colors.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
