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
//  *      FileName:       ExtensionMethods.cs
//  * 
//  *****************************************************************************/

#endregion

using System;

namespace CalendarSyncPlus.Common
{
    public static class ExtensionMethods
    {
        public static string convertToString(this Enum eff)
        {
            return Enum.GetName(eff.GetType(), eff);
        }

        public static EnumType converToEnum<EnumType>(this string enumValue) where EnumType : new()
        {
            try
            {
                return (EnumType) Enum.Parse(typeof(EnumType), enumValue);
            }
            catch (Exception exception)
            {
                return new EnumType();
            }
        }
    }
}