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

namespace CalendarSyncPlus.Presentation.Converters
{
    public class PercentageWidthConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double width = 450;
            if (value != null && parameter != null)
            {
                double parentWidth;
                int percentage;
                if (double.TryParse(value.ToString(), out parentWidth) && int.TryParse(parameter.ToString(), out percentage))
                {
                    width = parentWidth * percentage / 100;
                    if (width < 450)
                    {
                        width = 450;
                    }
                }
            }

            return width;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}