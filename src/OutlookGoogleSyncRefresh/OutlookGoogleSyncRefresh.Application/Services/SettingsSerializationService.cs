#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        OutlookGoogleSyncRefresh
//  *      SubProject:     OutlookGoogleSyncRefresh.Application
//  *      Author:         Ankesh Dave
//  *      Created On:     02-02-2015 12:14 AM
//  *      Modified On:    02-02-2015 12:47 AM
//  *      FileName:       SettingsSerializationService.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using System.Waf.Applications;
using OutlookGoogleSyncRefresh.Common.Log;
using OutlookGoogleSyncRefresh.Domain.File.Xml;
using OutlookGoogleSyncRefresh.Domain.Models;

#endregion

namespace OutlookGoogleSyncRefresh.Application.Services
{
    [Export(typeof(ISettingsSerializationService))]
    public class SettingsSerializationService : ISettingsSerializationService
    {
        private readonly ApplicationLogger _applicationLogger;

        #region Fields

        private readonly string applicationDataDirectory;
        private readonly string settingsFilePath;

        #endregion

        #region Constructors

        [ImportingConstructor]
        public SettingsSerializationService(ApplicationLogger applicationLogger)
        {
            _applicationLogger = applicationLogger;
            applicationDataDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "CalendarSyncPlus");
            settingsFilePath = Path.Combine(applicationDataDirectory, "Settings.xml");
        }

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

        private void SerializeSettingsBackgroundTask(Settings settings)
        {
            if (!Directory.Exists(ApplicationDataDirectory))
            {
                Directory.CreateDirectory(ApplicationDataDirectory);
            }

            var serializer = new XmlSerializer<Settings>();
            serializer.SerializeToFile(settings, SettingsFilePath);
        }

        private Settings DeserializeSettingsBackgroundTask()
        {
            if (!File.Exists(SettingsFilePath))
            {
                _applicationLogger.LogInfo("Settings file does not exist");
                return null;
            }
            var serializer = new XmlSerializer<Settings>();
            return serializer.DeserializeFromFile(SettingsFilePath);
        }

        #endregion

        #region Public Methods

        public async Task<bool> SerializeSettingsAsync(Settings settings)
        {
            await TaskEx.Run(() => SerializeSettingsBackgroundTask(settings));
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

        public bool SerializeSettings(Settings settings)
        {
            SerializeSettingsBackgroundTask(settings);
            return true;
        }

        public Settings DeserializeSettings()
        {
            var result = DeserializeSettingsBackgroundTask();
            if (result == null)
            {
                return GetDefaultSettings();
            }
            if (result.SyncSettings.SyncFrequency == null)
            {
                result.SyncSettings.SyncFrequency = new HourlySyncFrequency();
            }
            result.SetCalendarTypes();
            return result;
        }

        private Settings GetDefaultSettings()
        {
            var settings = new Settings
            {
                DaysInFuture = 7,
                DaysInPast = 1,
                IsFirstSave = true,
                MinimizeToSystemTray = true,
                CheckForUpdates = true,
                RememberPeriodicSyncOn = true,
                RunApplicationAtSystemStartup = true
            };
            settings.SyncSettings.CalendarSyncDirection = CalendarSyncDirectionEnum.OutlookGoogleOneWay;
            settings.SyncSettings.SyncFrequency = new HourlySyncFrequency();
            settings.OutlookSettings.OutlookOptions = OutlookOptionsEnum.DefaultProfile | OutlookOptionsEnum.DefaultCalendar;
            settings.CalendarEntryOptions = CalendarEntryOptionsEnum.None;
            settings.SetCalendarTypes();
            return settings;
        }

        #endregion
    }
}