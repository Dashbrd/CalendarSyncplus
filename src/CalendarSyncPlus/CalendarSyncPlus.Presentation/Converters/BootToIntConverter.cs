using System;
using System.Globalization;
using System.Windows.Data;

namespace CalendarSyncPlus.Presentation.Converters
{
    public class BootToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if ((bool) value)
                    return 0;
            }
            catch (Exception)
            {
                throw;
            }
            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (((int) value) == 0)
                    return true;
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
    }
}