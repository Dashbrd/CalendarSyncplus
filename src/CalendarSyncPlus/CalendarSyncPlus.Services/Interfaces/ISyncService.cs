#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        CalendarSyncPlus
//  *      SubProject:     CalendarSyncPlus.Application
//  *      Author:         Dave, Ankesh
//  *      Created On:     03-02-2015 7:31 PM
//  *      Modified On:    05-02-2015 12:43 PM
//  *      FileName:       ISyncService.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Services.Utilities;

#endregion

namespace CalendarSyncPlus.Services.Interfaces
{
    public interface ISyncService : INotifyPropertyChanged, IService
    {
        #region Properties

        string SyncStatus { get; set; }

        #endregion

        #region Public Methods

        Task<bool> Start(ElapsedEventHandler timerCallback);

        void Stop(ElapsedEventHandler timerCallback);

        string SyncNow(CalendarSyncProfile syncProfile, SyncCallback syncCallback);

        #endregion
    }
}