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
using CalendarSyncPlus.Services.Interfaces;
using CalendarSyncPlus.Services.Utilities;

#endregion

namespace CalendarSyncPlus.Services
{
    [Export(typeof(ISyncService))]
    public class SyncService : Model, ISyncService
    {
        #region Fields

        private readonly ApplicationLogger _applicationLogger;
        private readonly ICalendarUpdateService _calendarUpdateService;
        private readonly IMessageService _messageService;
        private readonly ISettingsProvider _settingsProvider;
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

        public async Task<bool> Start(ElapsedEventHandler timerCallback)
        {
            Settings settings = _settingsProvider.GetSettings();
            if (!settings.ValidateSettings())
            {
                _messageService.ShowMessageAsync("Please configure Google and Outlook calendar in settings to continue.");
                return false;
            }
            await Task.Delay(1000);
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

        public string SyncNow(CalendarSyncProfile syncProfile, SyncCallback syncCallback)
        {
            try
            {
                if (syncProfile.GoogleAccount == null || !syncProfile.ValidateOutlookSettings())
                {
                    _messageService.ShowMessageAsync(
                        "Please configure Google and Outlook calendar in settings to continue.");
                    return "Invalid Settings";
                }
                ResetSyncData();
                bool isSyncComplete = _calendarUpdateService.SyncCalendar(syncProfile, syncCallback);
                return isSyncComplete ? null : "Error Occurred";
            }
            catch (AggregateException exception)
            {
                AggregateException flattenException = exception.Flatten();
                _messageService.ShowMessageAsync(flattenException.Message);
                _applicationLogger.LogError(exception.ToString());
                return flattenException.Message;
            }
            catch (Exception exception)
            {
                _messageService.ShowMessageAsync(exception.Message);
                _applicationLogger.LogError(exception.ToString());
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