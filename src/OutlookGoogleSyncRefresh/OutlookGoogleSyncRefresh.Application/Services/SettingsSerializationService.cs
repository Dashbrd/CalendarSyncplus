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
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Domain.File.Xml;
using CalendarSyncPlus.Domain.Models;

#endregion

namespace CalendarSyncPlus.Application.Services
{
    [Export(typeof (ISettingsSerializationService))]
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
                _applicationLogger.LogInfo("Settings file does not exist");
                return null;
            }
            var serializer = new XmlSerializer<Settings>();
            return serializer.DeserializeFromFile(SettingsFilePath);
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
            Settings result = DeserializeSettingsBackgroundTask();
            if (result == null)
            {
                return Settings.GetDefaultSettings();
            }

            if (result.SyncProfiles == null)
            {
                result.SyncProfiles = new ObservableCollection<CalendarSyncProfile>();
            }

            if (result.SyncProfiles.Count == 0)
            {
                result.SyncProfiles.Add(CalendarSyncProfile.GetDefaultSyncProfile());
            }

            foreach (CalendarSyncProfile syncProfile in result.SyncProfiles)
            {
                syncProfile.SetCalendarTypes();
                if (syncProfile.SyncSettings.SyncFrequency == null)
                {
                    syncProfile.SyncSettings.SyncFrequency = new HourlySyncFrequency();
                }
            }

            return result;
        }

        #endregion
    }
}