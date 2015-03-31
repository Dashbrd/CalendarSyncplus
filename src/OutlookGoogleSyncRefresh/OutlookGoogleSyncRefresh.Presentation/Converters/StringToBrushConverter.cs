using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CalendarSyncPlus.Presentation.Converters
{
    public class StringToBrushConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null)
                {
                    var color = (Color) value;
                    return new SolidColorBrush(color);
                }
            }
            catch (Exception)
            {
            }
            return new SolidColorBrush(Color.FromArgb(1, 1, 1, 1));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null)
                {
                    var colorBrush = value as SolidColorBrush;
                    if (colorBrush != null)
                    {
                        return colorBrush.Color;
                    }
                }
            }
            catch (Exception)
            {
            }
            return Color.FromArgb(1, 1, 1, 1);
        }

        #endregion
    }
}