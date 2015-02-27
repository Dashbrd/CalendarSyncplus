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
//  *      Created On:     06-02-2015 12:19 PM
//  *      Modified On:    06-02-2015 12:19 PM
//  *      FileName:       CallerMemberNameAttribute.cs
//  * 
//  *****************************************************************************/

#endregion

namespace System.Runtime.CompilerServices
{
    /// <summary>
    ///     To mimic .NET 4.5 Compiler Behavior on .NET 4.0 App
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class CallerFilePathAttribute : Attribute
    {
    }
}