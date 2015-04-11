#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        CalendarSyncPlus
//  *      SubProject:     CalendarSyncPlus.Common
//  *      Author:         Dave, Ankesh
//  *      Created On:     05-03-2015 2:39 PM
//  *      Modified On:    05-03-2015 2:39 PM
//  *      FileName:       ICalendarServiceMetaData.cs
//  * 
//  *****************************************************************************/

#endregion

namespace CalendarSyncPlus.Common.MetaData
{
    public interface ICalendarServiceMetaData
    {
        CalendarServiceType ServiceType { get; }
    }
}