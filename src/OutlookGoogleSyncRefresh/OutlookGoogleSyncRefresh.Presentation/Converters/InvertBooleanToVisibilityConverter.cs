using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OutlookGoogleSyncRefresh.Presentation.Converters
{
    public class InvertBooleanToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (bool) value)
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && ((Visibility) value) == Visibility.Collapsed)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}