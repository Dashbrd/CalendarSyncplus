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
//  *      Created On:     03-02-2015 7:31 PM
//  *      Modified On:    05-02-2015 12:43 PM
//  *      FileName:       ISyncService.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OutlookGoogleSyncRefresh.Domain.Models;

#endregion

namespace OutlookGoogleSyncRefresh.Application.Services
{
    public interface ISyncService : INotifyPropertyChanged, IService
    {
        #region Properties

        string SyncStatus { get; set; }

        #endregion

        #region Public Methods

        Task<bool> Start(TimerCallback timerCallback);

        void Stop();

        Task<string> SyncNowAsync(Settings settings);

        #endregion
    }
}