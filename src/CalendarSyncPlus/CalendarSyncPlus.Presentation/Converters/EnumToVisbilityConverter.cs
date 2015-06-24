using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CalendarSyncPlus.Common.MetaData;

namespace CalendarSyncPlus.Presentation.Converters
{
    public class EnumToVisbilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return Visibility.Collapsed;
            var parameterString = parameter.ToString();

            if (Enum.IsDefined(value.GetType(), value) == false)
                return Visibility.Collapsed;

            var parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value) ? Visibility.Visible : Visibility.Collapsed;
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