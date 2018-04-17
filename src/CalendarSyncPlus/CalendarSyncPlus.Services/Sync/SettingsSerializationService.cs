#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        CalendarSyncPlus
//  *      SubProject:     CalendarSyncPlus.Application
//  *      Author:         Ankesh Dave
//  *      Created On:     02-02-2015 12:14 AM
//  *      Modified On:    02-02-2015 12:47 AM
//  *      FileName:       SettingsSerializationService.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using System.Waf.Applications;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Domain.File.Binary;
using CalendarSyncPlus.Domain.File.Json;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Services.Interfaces;
using log4net;

#endregion

namespace CalendarSyncPlus.Services.Sync
{
    [Export(typeof (ISettingsSerializationService))]
    public class SettingsSerializationService : ISettingsSerializationService
    {
        ILog Logger { get; set; }

        #region Constructors

        [ImportingConstructor]
        public SettingsSerializationService(ApplicationLogger applicationLogger)
        {
            Logger = applicationLogger.GetLogger(GetType());
            applicationDataDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "CalendarSyncPlus");
            SettingsFilePath = Path.Combine(applicationDataDirectory, "Settings.json");
        }

        #endregion

        #region Fields

        private readonly string applicationDataDirectory;

        #endregion

        #region Properties

        public string SettingsFilePath { get; }

        public string ApplicationDataDirectory => applicationDataDirectory;

        #endregion

        #region Private Methods

        private void SerializeSettingsBackgroundTask(Settings syncProfile)
        {
            if (!Directory.Exists(ApplicationDataDirectory))
            {
                Directory.CreateDirectory(ApplicationDataDirectory);
            }

            //var serializer = new XmlSerializer<Settings>();
            var serializer = new JsonSerializer<Settings>();
            serializer.SerializeToFile(syncProfile, SettingsFilePath);
        }

        private Settings DeserializeSettingsBackgroundTask()
        {
            if (!File.Exists(SettingsFilePath))
            {
                Logger.Warn("Settings file does not exist");
                return null;
            }
            try
            {
                //var serializer = new XmlSerializer<Settings>();
                var serializer = new JsonSerializer<Settings>();
                return serializer.DeserializeFromFile(SettingsFilePath);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return null;
            }
        }

        #endregion

        #region Public Methods

        public async Task<bool> SerializeSettingsAsync(Settings syncProfile)
        {
            await Task.Run(() => SerializeSettingsBackgroundTask(syncProfile));
            return true;
        }

        public async Task<Settings> DeserializeSettingsAsync()
        {
            if (!File.Exists(SettingsFilePath))
            {
                return null;
            }
            return await Task.Run(() => DeserializeSettingsBackgroundTask());
        }

        public bool SerializeSettings(Settings syncProfile)
        {
            SerializeSettingsBackgroundTask(syncProfile);
            return true;
        }

        public Settings DeserializeSettings()
        {
            var result = DeserializeSettingsBackgroundTask();
            if (result == null || result.SettingsVersion == null)
            {
                return Settings.GetDefaultSettings();
            }

            if (string.IsNullOrEmpty(result.SettingsVersion) || !result.SettingsVersion.Equals(ApplicationInfo.Version))
            {
                result.IsFirstSave = true;
            }
            var settingsVersion = new Version(result.SettingsVersion);
            if (settingsVersion < new Version("1.5.0.0"))
            {
                return Settings.GetDefaultSettings();
            }

            ValidateSettings(result);
            return result;
        }

        private void ValidateSettings(Settings result)
        {
            if (result.GoogleAccounts == null)
            {
                result.GoogleAccounts = new ObservableCollection<GoogleAccount>();
            }

            if (result.CalendarSyncProfiles == null || result.CalendarSyncProfiles.Count == 0)
            {
                result.CalendarSyncProfiles = new ObservableCollection<CalendarSyncProfile>()
                {
                    CalendarSyncProfile.GetDefaultSyncProfile()
                };
            }
            else
            {
                foreach (var syncProfile in result.CalendarSyncProfiles)
                {
                    syncProfile.SetSourceDestTypes();
                    if (syncProfile.SyncSettings == null)
                    {
                        syncProfile.SyncSettings = CalendarSyncSettings.GetDefault();
                    }
                    else if (syncProfile.SyncSettings.SyncRangeType == SyncRangeTypeEnum.SyncEntireCalendar)
                    {
                        syncProfile.SyncSettings.SyncRangeType = SyncRangeTypeEnum.SyncRangeInDays;
                        syncProfile.SyncSettings.DaysInPast = 120;
                        syncProfile.SyncSettings.DaysInFuture = 120;
                    }

                    if (syncProfile.SyncFrequency == null)
                    {
                        syncProfile.SyncFrequency = new IntervalSyncFrequency();
                    }
                }
            }

            if (result.TaskSyncProfiles == null || result.TaskSyncProfiles.Count == 0)
            {
                result.TaskSyncProfiles = new ObservableCollection<TaskSyncProfile>()
                {
                    TaskSyncProfile.GetDefaultSyncProfile()
                };
            }
            else
            {
                foreach (var syncProfile in result.TaskSyncProfiles)
                {
                    syncProfile.SetSourceDestTypes();
                    if (syncProfile.SyncSettings == null)
                    {
                        syncProfile.SyncSettings = TaskSyncSettings.GetDefault();
                    }

                    if (syncProfile.SyncFrequency == null)
                    {
                        syncProfile.SyncFrequency = new IntervalSyncFrequency();
                    }
                }
            }
            if (result.ContactSyncProfiles == null || result.ContactSyncProfiles.Count == 0)
            {
                result.ContactSyncProfiles = new ObservableCollection<ContactSyncProfile>()
                {
                    ContactSyncProfile.GetDefaultSyncProfile()
                };
            }

            if (result.AppSettings == null)
            {
                result.AppSettings = AppSettings.GetDefault();
            }
            else if (result.AppSettings.ProxySettings == null)
            {
                result.AppSettings.ProxySettings = new ProxySetting
                {
                    ProxyType = ProxyType.Auto
                };
            }
        }

        #endregion
    }
}