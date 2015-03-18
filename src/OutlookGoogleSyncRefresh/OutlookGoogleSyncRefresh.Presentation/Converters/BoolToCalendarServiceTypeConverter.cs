using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using OutlookGoogleSyncRefresh.Common.MetaData;

namespace OutlookGoogleSyncRefresh.Presentation.Converters
{
    public class BoolToCalendarServiceTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null || parameter == null)
                    return false;
                CalendarServiceType inputServiceType = (CalendarServiceType)Enum.Parse(typeof(CalendarServiceType), value.ToString());
                CalendarServiceType parameterServiceType = (CalendarServiceType)Enum.Parse(typeof(CalendarServiceType), parameter.ToString());
                if (inputServiceType == parameterServiceType)
                {
                    return true;
                }
            }
            catch (Exception)
            {


            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null || parameter == null)
                    return false;
                bool isValid = (bool)value;
                CalendarServiceType parameterServiceType = (CalendarServiceType)Enum.Parse(typeof(CalendarServiceType), parameter.ToString());
                if (isValid)
                {
                    return parameterServiceType;
                }

            }
            catch (Exception)
            {

            }
            return CalendarServiceType.None;
        }
    }
}
