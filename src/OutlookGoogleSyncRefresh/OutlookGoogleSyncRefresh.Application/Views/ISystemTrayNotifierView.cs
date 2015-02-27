#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        OutlookGoogleSyncRefresh
//  *      SubProject:     OutlookGoogleSyncRefresh.Application
//  *      Author:         Dave, Ankesh
//  *      Created On:     20-02-2015 2:05 PM
//  *      Modified On:    20-02-2015 2:05 PM
//  *      FileName:       ISystemTrayNotifierView.cs
//  * 
//  *****************************************************************************/

#endregion

using System.Waf.Applications;

namespace OutlookGoogleSyncRefresh.Application.Views
{
    public interface ISystemTrayNotifierView : IView
    {
        void ShowCustomBalloon();

        void CloseBalloon();

        void Quit();
    }
}