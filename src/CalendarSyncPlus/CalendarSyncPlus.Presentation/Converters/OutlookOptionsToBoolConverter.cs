using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Presentation.Converters
{
    public class OutlookOptionsToBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        ///     Converts a value.
        /// </summary>
        /// <param name="value">
        ///     The value produced by the binding source.
        /// </param>
        /// <param name="targetType">
        ///     The type of the binding target property.
        /// </param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        ///     A converted value. If the method returns null, the valid
        ///     <see langword="null" /> <paramref name="value" /> is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return Visibility.Collapsed;
            var parameterString = parameter.ToString();

            var parameterValue = (OutlookOptionsEnum)Enum.Parse(value.GetType(), parameterString);

            return ((OutlookOptionsEnum)value).HasFlag(parameterValue);
        }

        /// <summary>
        ///     Converts a value.
        /// </summary>
        /// <param name="value">
        ///     The value that is produced by the binding target.
        /// </param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        ///     A converted value. If the method returns null, the valid
        ///     <see langword="null" /> <paramref name="value" /> is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null || parameter == null)
                {
                    return false;
                }
                var isValid = (bool)value;
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
