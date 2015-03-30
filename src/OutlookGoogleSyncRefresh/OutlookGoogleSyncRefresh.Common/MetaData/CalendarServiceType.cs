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
//  *      Created On:     05-03-2015 2:40 PM
//  *      Modified On:    05-03-2015 2:40 PM
//  *      FileName:       CalendarServiceType.cs
//  * 
//  *****************************************************************************/

#endregion

namespace OutlookGoogleSyncRefresh.Common.MetaData
{
    public enum CalendarServiceType
    {
        None = 0,
        Google,
        OutlookDesktop,
        EWS,
        Live
    }
}