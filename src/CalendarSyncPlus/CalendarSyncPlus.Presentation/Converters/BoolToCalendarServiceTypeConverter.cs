using System;
using System.Globalization;
using System.Windows.Data;
using CalendarSyncPlus.Common.MetaData;

namespace CalendarSyncPlus.Presentation.Converters
{
    public class BoolToCalendarServiceTypeConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null || parameter == null)
                {
                    return false;
                }
                var inputServiceType = (CalendarServiceType) Enum.Parse(typeof (CalendarServiceType), value.ToString());
                var parameterServiceType =
                    (CalendarServiceType) Enum.Parse(typeof (CalendarServiceType), parameter.ToString());
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null || parameter == null)
                {
                    return false;
                }
                var isValid = (bool) value;
                var parameterServiceType =
                    (CalendarServiceType) Enum.Parse(typeof (CalendarServiceType), parameter.ToString());
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

        #endregion
    }
}