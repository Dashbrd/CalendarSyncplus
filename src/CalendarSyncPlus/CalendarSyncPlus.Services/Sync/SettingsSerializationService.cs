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
using CalendarSyncPlus.Domain.File.Xml;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Services.Interfaces;
using log4net;

#endregion

namespace CalendarSyncPlus.Services
{
    [Export(typeof (ISettingsSerializationService))]
    public class SettingsSerializationService : ISettingsSerializationService
    {
        private readonly ILog _applicationLogger;

        #region Constructors

        [ImportingConstructor]
        public SettingsSerializationService(ApplicationLogger applicationLogger)
        {
            _applicationLogger = applicationLogger.GetLogger(GetType());
            applicationDataDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "CalendarSyncPlus");
            settingsFilePath = Path.Combine(applicationDataDirectory, "Settings.xml");
        }

        #endregion

        #region Fields

        private readonly string applicationDataDirectory;
        private readonly string settingsFilePath;

        #endregion

        #region Properties

        public string SettingsFilePath
        {
            get { return settingsFilePath; }
        }

        public string ApplicationDataDirectory
        {
            get { return applicationDataDirectory; }
        }

        #endregion

        #region Private Methods

        private void SerializeSettingsBackgroundTask(Settings syncProfile)
        {
            if (!Directory.Exists(ApplicationDataDirectory))
            {
                Directory.CreateDirectory(ApplicationDataDirectory);
            }

            var serializer = new XmlSerializer<Settings>();
            serializer.SerializeToFile(syncProfile, SettingsFilePath);
        }

        private Settings DeserializeSettingsBackgroundTask()
        {
            if (!File.Exists(SettingsFilePath))
            {
                _applicationLogger.Warn("Settings file does not exist");
                return null;
            }
            try
            {
                var serializer = new XmlSerializer<Settings>();
                return serializer.DeserializeFromFile(SettingsFilePath);
            }
            catch (Exception exception)
            {
                _applicationLogger.Error(exception);
                return null;
            }
        }

        #endregion

        #region Public Methods

        public async Task<bool> SerializeSettingsAsync(Settings syncProfile)
        {
            await TaskEx.Run(() => SerializeSettingsBackgroundTask(syncProfile));
            return true;
        }

        public async Task<Settings> DeserializeSettingsAsync()
        {
            if (!File.Exists(SettingsFilePath))
            {
                return null;
            }
            return await TaskEx.Run(() => DeserializeSettingsBackgroundTask());
        }

        public bool SerializeSettings(Settings syncProfile)
        {
            SerializeSettingsBackgroundTask(syncProfile);
            return true;
        }

        public Settings DeserializeSettings()
        {
            var result = DeserializeSettingsBackgroundTask();
            if (result == null)
            {
                return Settings.GetDefaultSettings();
            }

            if (string.IsNullOrEmpty(result.SettingsVersion) || !result.SettingsVersion.Equals(ApplicationInfo.Version))
            {
                result.IsFirstSave = true;
            }
            var settingsVersion = new Version(result.SettingsVersion);
            if (settingsVersion < new Version("1.3.2.3"))
            {
                return Settings.GetDefaultSettings();
            }

            ValidateSettings(result);
            return result;
        }

        private void ValidateSettings(Settings result)
        {
            if (result.SyncProfiles == null)
            {
                result.SyncProfiles = new ObservableCollection<CalendarSyncProfile>();
            }

            if (result.SyncProfiles.Count == 0)
            {
                result.SyncProfiles.Add(CalendarSyncProfile.GetDefaultSyncProfile());
            }

            foreach (var syncProfile in result.SyncProfiles)
            {
                syncProfile.SetCalendarTypes();
                if (syncProfile.SyncSettings == null || syncProfile.SyncSettings.SyncFrequency == null)
                {
                    syncProfile.SyncSettings = SyncSettings.GetDefault();
                }
                else if (syncProfile.SyncSettings.SyncRangeType == SyncRangeTypeEnum.SyncEntireCalendar)
                {
                    syncProfile.SyncSettings.SyncRangeType = SyncRangeTypeEnum.SyncRangeInDays;
                    syncProfile.SyncSettings.DaysInPast = 120;
                    syncProfile.SyncSettings.DaysInFuture = 120;
                }
            }

            if (result.AppSettings == null)
            {
                result.AppSettings = new AppSettings();
            }

            if (result.AppSettings.ProxySettings == null)
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