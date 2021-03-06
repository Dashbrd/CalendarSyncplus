﻿using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [DataContract]
    /// <summary>
    ///     Task Sync Profile
    /// </summary>
    public class TaskSyncProfile : SyncProfile
    {
        private TaskSyncSettings _syncSettings;

        /// <summary>
        ///     Constructor
        /// </summary>
        public TaskSyncProfile()
        {
            Name = "Default Task Profile";
            GoogleSettings = new GoogleSettings();
            ExchangeServerSettings = new ExchangeServerSettings();
            OutlookSettings = new OutlookSettings();
            IsSyncEnabled = true;
            IsDefault = true;
        }
        [DataMember]
        public TaskSyncSettings SyncSettings
        {
            get { return _syncSettings; }
            set { SetProperty(ref _syncSettings, value); }
        }

        public static TaskSyncProfile GetDefaultSyncProfile()
        {
            var syncProfile = new TaskSyncProfile
            {
                SyncSettings = TaskSyncSettings.GetDefault(),
                OutlookSettings =
                {
                    OutlookOptions = OutlookOptionsEnum.OutlookDesktop |
                                     OutlookOptionsEnum.DefaultProfile |
                                     OutlookOptionsEnum.DefaultMailBoxCalendar
                },
                SyncDirection = SyncDirectionEnum.OutlookGoogleOneWay,
                SyncFrequency = new IntervalSyncFrequency {Hours = 1, Minutes = 0, StartTime = DateTime.Now}
            };
            syncProfile.SetSourceDestTypes();
            return syncProfile;
        }
    }
}