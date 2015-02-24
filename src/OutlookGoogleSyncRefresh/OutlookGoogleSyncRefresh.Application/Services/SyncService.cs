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
//  *      Modified On:    05-02-2015 12:46 PM
//  *      FileName:       SyncService.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Waf.Applications;
using OutlookGoogleSyncRefresh.Application.Services.CalendarUpdate;
using OutlookGoogleSyncRefresh.Application.Utilities;
using OutlookGoogleSyncRefresh.Common.Log;
using OutlookGoogleSyncRefresh.Domain.Models;

using DataModel = OutlookGoogleSyncRefresh.Application.Utilities.DataModel;

#endregion

namespace OutlookGoogleSyncRefresh.Application.Services
{
    [Export(typeof(ISyncService))]
    public class SyncService : DataModel, ISyncService
    {
        #region Fields

        private readonly ICalendarUpdateService _calendarUpdateService;
        private readonly IMessageService _messageService;
        private readonly ApplicationLogger _applicationLogger;
        private readonly ISettingsProvider _settingsProvider;
        private Settings _settings;
        private Timer _syncTimer;
        private bool _isSyncInProgress;
        private string _syncStatus;

        #endregion

        #region Constructors

        [ImportingConstructor]
        public SyncService(ISettingsProvider settingsProvider, ICalendarUpdateService calendarUpdateService, IMessageService messageService, ApplicationLogger applicationLogger)
        {
            _settingsProvider = settingsProvider;
            _calendarUpdateService = calendarUpdateService;
            _messageService = messageService;
            _applicationLogger = applicationLogger;
        }

        #endregion

        #region Private Methods



        private async void SyncStartTimerCallback(object state)
        {
            _settings = _settingsProvider.GetSettings();
            if (_settings != null && _settings.SyncFrequency.ValidateTimer(DateTime.Now))
            {
                await SyncNowAsync();
            }
        }

        #endregion

        #region ISyncService Members

        public bool IsSyncInProgress
        {
            get { return _isSyncInProgress; }
            set { SetProperty(ref _isSyncInProgress, value); }
        }


        public string SyncStatus
        {
            get { return _syncStatus; }
            set { SetProperty(ref _syncStatus, value); }
        }

        public async Task<bool> Start()
        {
            _settings = _settingsProvider.GetSettings();
            if (_settings.SavedCalendar == null || (!_settings.IsDefaultMailBox && _settings.OutlookCalendar == null))
            {
                _messageService.ShowMessageAsync("Please configure Google and Outlook calendar in settings to continue.");
                return false;
            }
            _syncTimer = new Timer(SyncStartTimerCallback, null, (60 - DateTime.Now.Second), 60000);
            return true;
        }

        public void Stop()
        {
            _syncTimer.Dispose();
            _syncTimer = null;
        }



        public async Task<bool> SyncNowAsync()
        {
            try
            {
                if (IsSyncInProgress)
                    return false;
                IsSyncInProgress = true;
                if (_settings == null)
                {
                    _settings = _settingsProvider.GetSettings();
                }

                if (_settings.SavedCalendar == null ||
                    (!_settings.IsDefaultMailBox && _settings.OutlookCalendar == null))
                {
                    _messageService.ShowMessageAsync(
                        "Please configure Google and Outlook calendar in settings to continue.");
                    IsSyncInProgress = false;
                    return false;
                }

                ResetSyncData();
                SyncStatus = StatusHelper.GetMessage(SyncStateEnum.SyncStarted, DateTime.Now);
                bool isSyncComplete = await _calendarUpdateService.SyncCalendarAsync(_settings);
                SyncStatus =
                    StatusHelper.GetMessage(isSyncComplete ? SyncStateEnum.SyncSuccess : SyncStateEnum.SyncFailed);

                SyncStatus = string.Empty;
                //IsSyncInProgress = false;
                return isSyncComplete;
            }
            catch (AggregateException exception)
            {
                var flattenException = exception.Flatten();
                _messageService.ShowMessageAsync(flattenException.Message);
                _applicationLogger.LogError(exception.ToString());
            }
            catch (Exception exception)
            {
                _messageService.ShowMessageAsync(exception.Message);
                _applicationLogger.LogError(exception.ToString());
            }
            finally
            {
                IsSyncInProgress = false;
            }
            return false;
        }

        private void ResetSyncData()
        {
            _syncStatus = null;
        }

        #endregion

        public void Initialize()
        {
            AddWeakEventListener(_calendarUpdateService, CalendarUpdateNotificationChanged);
        }

        private void CalendarUpdateNotificationChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SyncStatus":
                    SyncStatus = _calendarUpdateService.SyncStatus;
                    break;
            }
        }

        public void Run()
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            RemoveWeakEventListener(_calendarUpdateService, CalendarUpdateNotificationChanged);
        }
    }
}