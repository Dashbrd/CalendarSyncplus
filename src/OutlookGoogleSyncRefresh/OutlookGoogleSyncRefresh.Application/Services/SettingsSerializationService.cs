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

using OutlookGoogleSyncRefresh.Domain.File.Xml;
using OutlookGoogleSyncRefresh.Domain.Models;

#endregion

namespace OutlookGoogleSyncRefresh.Application.Services
{
    [Export(typeof(ISettingsSerializationService))]
    public class SettingsSerializationService : ISettingsSerializationService
    {
        #region Fields

        private readonly string applicationDataDirectory;
        private readonly string settingsFilePath;

        #endregion

        #region Constructors

        public SettingsSerializationService()
        {
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
            if (!File.Exists(SettingsFilePath))
            {
                return new Settings()
                {
                    DaysInFuture = 7,
                    DaysInPast = 1,
                    IsDefaultProfile = true,
                    IsFirstSave = true,
                    IsDefaultMailBox = true,
                    MinimizeToSystemTray = true
                };
            }
            return DeserializeSettingsBackgroundTask();
        }

        #endregion
    }
}