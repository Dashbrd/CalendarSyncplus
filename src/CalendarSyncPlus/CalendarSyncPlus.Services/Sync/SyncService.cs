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
//  *      Modified On:    05-02-2015 12:46 PM
//  *      FileName:       SyncService.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Timers;
using System.Waf.Foundation;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Domain.Helpers;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Metrics;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Services.Calendars.Interfaces;
using CalendarSyncPlus.Services.Interfaces;
using CalendarSyncPlus.Services.Utilities;
using log4net;

#endregion

namespace CalendarSyncPlus.Services
{
    [Export(typeof(ISyncService))]
    public class SyncService : Model, ISyncService
    {
        #region Constructors

        [ImportingConstructor]
        public SyncService(ISettingsProvider settingsProvider, ICalendarUpdateService calendarUpdateService,
            IMessageService messageService, ApplicationLogger applicationLogger)
        {
            _settingsProvider = settingsProvider;
            _calendarUpdateService = calendarUpdateService;
            _messageService = messageService;
            _applicationLogger = applicationLogger.GetLogger(GetType());
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

        #region Fields

        private readonly ILog _applicationLogger;
        private readonly ICalendarUpdateService _calendarUpdateService;
        private readonly IMessageService _messageService;
        private readonly ISettingsProvider _settingsProvider;
        private string _syncStatus;
        private Timer _syncTimer;

        #endregion

        #region ISyncService Members

        public string SyncStatus
        {
            get { return _syncStatus; }
            set { SetProperty(ref _syncStatus, value); }
        }

        public async Task<bool> Start(ElapsedEventHandler timerCallback)
        {
            if (_syncTimer == null)
            {
                _syncTimer = new Timer(1000) { AutoReset = true };
                _syncTimer.Elapsed += timerCallback;
            }
            _syncTimer.Start();

            return true;
        }

        public void Stop(ElapsedEventHandler ElapsedEventHandler)
        {
            _syncTimer.Stop();
            _syncTimer.Elapsed -= ElapsedEventHandler;
            _syncTimer = null;
        }

        public string SyncNow(CalendarSyncProfile syncProfile, SyncMetric syncMetric, SyncCallback syncCallback)
        {
            try
            {
                if (syncProfile.GoogleAccount == null || syncProfile.GoogleAccount.GoogleCalendar == null ||
                    !syncProfile.ValidateOutlookSettings())
                {
                    _messageService.ShowMessageAsync(
                        "Please configure Google and Outlook calendar in settings to continue.");
                    return "Invalid Settings";
                }
                ResetSyncData();

                var isSyncComplete = _calendarUpdateService.SyncCalendar(syncProfile, syncMetric, syncCallback);
                return isSyncComplete ? null : "Error Occurred";
            }
            catch (AggregateException exception)
            {
                var flattenException = exception.Flatten();
                _messageService.ShowMessageAsync(flattenException.Message);
                _applicationLogger.Error(exception);
                return flattenException.Message;
            }
            catch (Exception exception)
            {
                _messageService.ShowMessageAsync(exception.Message);
                _applicationLogger.Error(exception);
                return exception.Message;
            }
        }

        public void Initialize()
        {
            PropertyChangedEventManager.AddHandler(_calendarUpdateService, CalendarUpdateNotificationChanged, "");
        }

        public void Run()
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            if (_calendarUpdateService != null)
            {
                PropertyChangedEventManager.RemoveHandler(_calendarUpdateService, CalendarUpdateNotificationChanged, "");
            }
        }

        #endregion
    }
}