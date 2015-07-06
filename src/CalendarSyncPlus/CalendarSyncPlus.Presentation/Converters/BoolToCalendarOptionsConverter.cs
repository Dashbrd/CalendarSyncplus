using System;
using System.Globalization;
using System.Windows.Data;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Presentation.Converters
{
    public class BoolToCalendarOptionsConverter: IValueConverter
    {
        private CalendarEntryOptionsEnum target;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CalendarEntryOptionsEnum mask = (CalendarEntryOptionsEnum)parameter;
            this.target = (CalendarEntryOptionsEnum)value;
            return ((mask & this.target) != 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null || parameter == null)
                {
                    return false;
                }
                var isValid = (bool)value;
                var parameterServiceType = (CalendarEntryOptionsEnum)Enum.Parse(typeof(CalendarEntryOptionsEnum), parameter.ToString());
                if (isValid)
                {
                    return target | parameterServiceType;
                }
                if (parameterServiceType == CalendarEntryOptionsEnum.Attendees)
                {
                    target &= ~CalendarEntryOptionsEnum.AttendeesToDescription;
                }
                return target &= ~parameterServiceType;
            }
            catch (Exception)
            {
            }
            return CalendarEntryOptionsEnum.None;
        }

    }
}
