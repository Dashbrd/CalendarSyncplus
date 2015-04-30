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
//  *      Created On:     29-04-2015 11:55 AM
//  *      Modified On:    29-04-2015 11:55 AM
//  *      FileName:       ILocalizationService.cs
//  * 
//  *****************************************************************************/
#endregion

using System.Globalization;

namespace CalendarSyncPlus.Common
{
    public interface ILocalizationService
    {
        string GetLocalizedString(string key);

        CultureInfo CurrentCulture { get; }
    }
}