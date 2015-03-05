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
//  *      Created On:     05-03-2015 2:47 PM
//  *      Modified On:    05-03-2015 2:47 PM
//  *      FileName:       ICalendarServiceMultiMetaData.cs
//  * 
//  *****************************************************************************/
#endregion
namespace OutlookGoogleSyncRefresh.Common.MetaData
{
    public interface ICalendarServiceMultiMetaData
    {
        CalendarServiceType[] CalendarServiceTypes { get; set; } 
    }
}