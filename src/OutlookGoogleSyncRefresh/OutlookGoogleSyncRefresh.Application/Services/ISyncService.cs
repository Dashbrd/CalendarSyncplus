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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

using OutlookGoogleSyncRefresh.Domain.Models;

#endregion

namespace OutlookGoogleSyncRefresh.Application.Services
{
    public interface ISyncService : INotifyPropertyChanged, IService
    {
        #region Properties
        bool IsSyncInProgress { get; set; }
        string SyncStatus { get; set; }
        #endregion

        #region Public Methods

        Task<bool> Start();

        void Stop();

        Task<bool> SyncNowAsync(Settings settings);

        #endregion
    }
}