using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace CalendarSyncPlus.Presentation.Converters
{
    public class WeekDaysToBoolConverter : IValueConverter
    {
        private List<DayOfWeek> _daysofweek;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            _daysofweek = (List<DayOfWeek>) value;
            return _daysofweek.Contains((DayOfWeek) parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool) value)
            {
                _daysofweek.Add((DayOfWeek) parameter);
            }
            else
            {
                _daysofweek.Remove((DayOfWeek) parameter);
            }
            return _daysofweek;
        }

        #endregion
    }
}