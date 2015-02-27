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
//  *      Created On:     06-02-2015 1:08 PM
//  *      Modified On:    06-02-2015 1:08 PM
//  *      FileName:       IService.cs
//  * 
//  *****************************************************************************/

#endregion

namespace OutlookGoogleSyncRefresh.Application.Services
{
    public interface IService
    {
        void Initialize();

        void Run();

        void Shutdown();
    }
}