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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using OutlookGoogleSyncRefresh.Application.Services.CalendarUpdate;
using OutlookGoogleSyncRefresh.Application.Utilities;
using OutlookGoogleSyncRefresh.Common.Log;
using OutlookGoogleSyncRefresh.Domain.Helpers;
using OutlookGoogleSyncRefresh.Domain.Models;

#endregion

namespace OutlookGoogleSyncRefresh.Application.Services
{
    [Export(typeof (ISyncService))]
    public class SyncService : DataModel, ISyncService
    {
        #region Fields

        private readonly ApplicationLogger _applicationLogger;
        private readonly ICalendarUpdateService _calendarUpdateService;
        private readonly IMessageService _messageService;
        private readonly ISettingsProvider _settingsProvider;
        private bool _isSyncInProgress;
        private string _syncStatus;
        private Timer _syncTimer;

        #endregion

        #region Constructors

        [ImportingConstructor]
        public SyncService(ISettingsProvider settingsProvider, ICalendarUpdateService calendarUpdateService,
            IMessageService messageService, ApplicationLogger applicationLogger)
        {
            _settingsProvider = settingsProvider;
            _calendarUpdateService = calendarUpdateService;
            _messageService = messageService;
            _applicationLogger = applicationLogger;
        }

        #endregion

        #region ISyncService Members

        public string SyncStatus
        {
            get { return _syncStatus; }
            set { SetProperty(ref _syncStatus, value); }
        }

        public async Task<bool> Start(TimerCallback timerCallback)
        {
            Settings settings = _settingsProvider.GetSettings();
            if (settings.SavedCalendar == null || !settings.ValidateOutlookSettings())
            {
                _messageService.ShowMessageAsync("Please configure Google and Outlook calendar in settings to continue.");
                return false;
            }
            _syncTimer = new Timer(timerCallback, null, (60 - DateTime.Now.Second), 60000);
            return true;
        }

        public void Stop()
        {
            _syncTimer.Dispose();
            _syncTimer = null;
        }


        public async Task<bool> SyncNowAsync(Settings settings)
        {
            try
            {
                if (settings.SavedCalendar == null || !settings.ValidateOutlookSettings())
                {
                    _messageService.ShowMessageAsync(
                        "Please configure Google and Outlook calendar in settings to continue.");
                    return false;
                }

                ResetSyncData();
                SyncStatus = StatusHelper.GetMessage(SyncStateEnum.SyncStarted, DateTime.Now);
                bool isSyncComplete = await _calendarUpdateService.SyncCalendarAsync(settings);
                SyncStatus =
                    StatusHelper.GetMessage(isSyncComplete ? SyncStateEnum.SyncSuccess : SyncStateEnum.SyncFailed);

                SyncStatus = string.Empty;
                //IsSyncInProgress = false;
                return isSyncComplete;
            }
            catch (AggregateException exception)
            {
                AggregateException flattenException = exception.Flatten();
                _messageService.ShowMessageAsync(flattenException.Message);
                _applicationLogger.LogError(exception.ToString());
            }
            catch (Exception exception)
            {
                _messageService.ShowMessageAsync(exception.Message);
                _applicationLogger.LogError(exception.ToString());
            }
            return false;
        }

        public void Initialize()
        {
            AddWeakEventListener(_calendarUpdateService, CalendarUpdateNotificationChanged);
        }

        public void Run()
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            RemoveWeakEventListener(_calendarUpdateService, CalendarUpdateNotificationChanged);
        }

        #endregion

        private void ResetSyncData()
        {
            _syncStatus = null;
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
    }
}