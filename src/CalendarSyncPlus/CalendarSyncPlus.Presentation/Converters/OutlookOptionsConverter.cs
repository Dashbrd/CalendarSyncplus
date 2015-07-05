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
    public class OutlookOptionsConverter : IValueConverter
    {
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
            try
            {
                if (parameter == null)
                    return DependencyProperty.UnsetValue;

                var parameterString = parameter.ToString();

                if (Enum.IsDefined(typeof(OutlookOptionsEnum), value) == false)
                    return DependencyProperty.UnsetValue;

                var parameterValue = (OutlookOptionsEnum)Enum.Parse(typeof(OutlookOptionsEnum), parameterString);
                var valueEnum = (OutlookOptionsEnum)value;

                return valueEnum.HasFlag(parameterValue);
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
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
            var parameterString = parameter as string;
            return parameterString == null ? DependencyProperty.UnsetValue : Enum.Parse(targetType, parameterString);
        }
    }
}
