using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Presentation.Converters
{
    public class OutlookOptionsToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return Visibility.Collapsed;
            var parameterString = parameter.ToString();
            
            var parameterValue = (OutlookOptionsEnum)Enum.Parse(value.GetType(), parameterString);

            return ((OutlookOptionsEnum)value).HasFlag(parameterValue) ? Visibility.Visible : Visibility.Collapsed;
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
                    (OutlookOptionsEnum)Enum.Parse(typeof(OutlookOptionsEnum), parameter.ToString());
                if (isValid)
                {
                    return parameterServiceType;
                }
            }
            catch (Exception)
            {
            }
            return OutlookOptionsEnum.None;
        }

        #endregion
    }
}