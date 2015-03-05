#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        OutlookGoogleSyncRefresh
//  *      SubProject:     OutlookGoogleSyncRefresh.Common
//  *      Author:         Dave, Ankesh
//  *      Created On:     05-03-2015 2:38 PM
//  *      Modified On:    05-03-2015 2:38 PM
//  *      FileName:       ExportCalendarMetaDataAttribute.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System;
using System.ComponentModel.Composition;

using OutlookGoogleSyncRefresh.Common.MetaData;

#endregion

namespace OutlookGoogleSyncRefresh.Common.Attributes
{
    [MetadataAttribute, AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ExportCalendarMetaDataAttribute : Attribute, ICalendarServiceMetaData
    {
        #region Constructors

        public ExportCalendarMetaDataAttribute(CalendarServiceType calendarServiceType)
        {
            CalendarServiceType = calendarServiceType;
        }

        #endregion

        #region ICalendarServiceMetaData Members

        public CalendarServiceType CalendarServiceType { get; set; }

        #endregion
    }
}