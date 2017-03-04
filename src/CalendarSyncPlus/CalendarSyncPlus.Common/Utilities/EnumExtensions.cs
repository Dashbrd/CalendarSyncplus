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
//  *      Created On:     06-02-2015 12:19 PM
//  *      Modified On:    06-02-2015 12:19 PM
//  *      FileName:       EnumExtensions.cs
//  * 
//  *****************************************************************************/

#endregion

using System;

namespace CalendarSyncPlus.Common
{
    public static class EnumExtensions
    {
        public static string ToString(this Enum eff)
        {
            return Enum.GetName(eff.GetType(), eff);
        }
    }
}