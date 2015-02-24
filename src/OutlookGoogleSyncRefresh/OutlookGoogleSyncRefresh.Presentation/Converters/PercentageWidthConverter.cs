#region File Header
// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        OutlookGoogleSyncRefresh
//  *      SubProject:     OutlookGoogleSyncRefresh.Presentation
//  *      Author:         Dave, Ankesh
//  *      Created On:     06-02-2015 5:09 PM
//  *      Modified On:    06-02-2015 5:09 PM
//  *      FileName:       PercentageWidthConverter.cs
//  * 
//  *****************************************************************************/
#endregion

using System;
using System.Globalization;
using System.Windows.Data;

namespace OutlookGoogleSyncRefresh.Presentation.Converters
{
    public class PercentageWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && parameter != null)
            {
                double width;
                int percentage;
                if (double.TryParse(value.ToString(), out width) && int.TryParse(parameter.ToString(), out percentage))
                {
                    return width * percentage / 100;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}