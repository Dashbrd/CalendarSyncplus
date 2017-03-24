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
using CalendarSyncPlus.Domain.Models.Metrics;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Services.Calendars.Interfaces;
using CalendarSyncPlus.Services.Contacts.Interfaces;
using CalendarSyncPlus.Services.Interfaces;
using CalendarSyncPlus.Services.Sync.Interfaces;
using CalendarSyncPlus.Services.Tasks.Interfaces;
using CalendarSyncPlus.Services.Utilities;
using log4net;

#endregion

namespace CalendarSyncPlus.Services.Sync
{
    [Export(typeof(ISyncService))]
    public class SyncService : Model, ISyncService
    {
        public ICalendarUpdateService CalendarUpdateService { get; set; }
        public IContactUpdateService ContactUpdateService { get; set; }
        public ITaskUpdateService TaskUpdateService { get; set; }
        public IMessageService MessageService { get; set; }
        public ILog Logger { get; set; }

        #region Constructors

        [ImportingConstructor]
        public SyncService(ICalendarUpdateService calendarUpdateService,
            IContactUpdateService contactUpdateService, ITaskUpdateService taskUpdateService,
            IMessageService messageService, ApplicationLogger applicationLogger)
        {
            CalendarUpdateService = calendarUpdateService;
            ContactUpdateService = contactUpdateService;
            TaskUpdateService = taskUpdateService;
            MessageService = messageService;
            Logger = applicationLogger.GetLogger(GetType());
        }

        #endregion

        private void ResetSyncData()
        {
            _syncStatus = null;
        }

        private void UpdateServiceNotificationChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CalendarSyncStatus":
                    SyncStatus = CalendarUpdateService.CalendarSyncStatus;
                    break;
                case "TaskSyncStatus":
                    SyncStatus = TaskUpdateService.TaskSyncStatus;
                    break;
                case "ContactSyncStatus":
                    SyncStatus = ContactUpdateService.ContactSyncStatus;
                    break;
            }
        }

        #region Fields

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
            await Task.Run(() =>
            {
                if (_syncTimer == null)
                {
                    _syncTimer = new Timer(1000) {AutoReset = true};
                    _syncTimer.Elapsed += timerCallback;
                }
                _syncTimer.Start();
            });

            return true;
        }

        public void Stop(ElapsedEventHandler elapsedEventHandler)
        {
            _syncTimer.Stop();
            _syncTimer.Elapsed -= elapsedEventHandler;
            _syncTimer = null;
        }

        public string SyncNow(CalendarSyncProfile syncProfile, SyncMetric syncMetric, SyncCallback syncCallback)
        {
            try
            {
                if (syncProfile.GoogleSettings.GoogleAccount == null || 
                    syncProfile.GoogleSettings.GoogleCalendar == null ||
                    !syncProfile.ValidateOutlookSettings())
                {
                    MessageService.ShowMessageAsync(
                        "Please configure Google and Outlook calendar in settings to continue.");
                    return "Invalid Settings";
                }
                ResetSyncData();

                var isSyncComplete = CalendarUpdateService.SyncCalendar(syncProfile, syncMetric, syncCallback);
                return isSyncComplete ? null : "Error Occurred";
            }
            catch (AggregateException exception)
            {
                var flattenException = exception.Flatten();
                MessageService.ShowMessageAsync(flattenException.Message);
                Logger.Error(exception);
                return flattenException.Message;
            }
            catch (Exception exception)
            {
                MessageService.ShowMessageAsync(exception.Message);
                Logger.Error(exception);
                return exception.Message;
            }
        }
        public string SyncNow(ContactSyncProfile syncProfile, SyncMetric syncMetric, SyncCallback syncCallback)
        {
            try
            {
                if (syncProfile.GoogleSettings.GoogleAccount == null || 
                    syncProfile.GoogleSettings.GoogleCalendar == null ||
                    !syncProfile.ValidateOutlookSettings())
                {
                    MessageService.ShowMessageAsync(
                        "Please configure Google and Outlook calendar in settings to continue.");
                    return "Invalid Settings";
                }
                ResetSyncData();

                var isSyncComplete = ContactUpdateService.SyncContact(syncProfile, syncMetric, syncCallback);
                return isSyncComplete ? null : "Error Occurred";
            }
            catch (AggregateException exception)
            {
                var flattenException = exception.Flatten();
                MessageService.ShowMessageAsync(flattenException.Message);
                Logger.Error(exception);
                return flattenException.Message;
            }
            catch (Exception exception)
            {
                MessageService.ShowMessageAsync(exception.Message);
                Logger.Error(exception);
                return exception.Message;
            }
        }
        public string SyncNow(TaskSyncProfile syncProfile, SyncMetric syncMetric, SyncCallback syncCallback)
        {
            try
            {
                if (syncProfile.GoogleSettings.GoogleAccount == null || 
                    syncProfile.GoogleSettings.GoogleCalendar == null ||
                    !syncProfile.ValidateOutlookSettings())
                {
                    MessageService.ShowMessageAsync(
                        "Please configure Google and Outlook calendar in settings to continue.");
                    return "Invalid Settings";
                }
                ResetSyncData();

                var isSyncComplete = TaskUpdateService.SyncTask(syncProfile, syncMetric, syncCallback);
                return isSyncComplete ? null : "Error Occurred";
            }
            catch (AggregateException exception)
            {
                var flattenException = exception.Flatten();
                MessageService.ShowMessageAsync(flattenException.Message);
                Logger.Error(exception);
                return flattenException.Message;
            }
            catch (Exception exception)
            {
                MessageService.ShowMessageAsync(exception.Message);
                Logger.Error(exception);
                return exception.Message;
            }
        }
        public void Initialize()
        {
            PropertyChangedEventManager.AddHandler(CalendarUpdateService, UpdateServiceNotificationChanged, "");
            PropertyChangedEventManager.AddHandler(TaskUpdateService, UpdateServiceNotificationChanged, "");
            PropertyChangedEventManager.AddHandler(ContactUpdateService, UpdateServiceNotificationChanged, "");
        }

        public void Run()
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            if (CalendarUpdateService == null) return;
            PropertyChangedEventManager.RemoveHandler(CalendarUpdateService, UpdateServiceNotificationChanged, "");
            PropertyChangedEventManager.RemoveHandler(TaskUpdateService, UpdateServiceNotificationChanged, "");
            PropertyChangedEventManager.RemoveHandler(ContactUpdateService, UpdateServiceNotificationChanged, "");
        }

        #endregion
    }
}