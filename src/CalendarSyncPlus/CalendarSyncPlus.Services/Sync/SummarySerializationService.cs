using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Domain.File.Xml;
using CalendarSyncPlus.Domain.Models.Metrics;
using CalendarSyncPlus.Services.Interfaces;
using log4net;

namespace CalendarSyncPlus.Services.Sync
{
    [Export(typeof (ISummarySerializationService))]
    public class SummarySerializationService : ISummarySerializationService
    {
        private readonly string _applicationDataDirectory;
        private ILog Logger { get; set; }
        private readonly string _settingsFilePath;

        [ImportingConstructor]
        public SummarySerializationService(ApplicationLogger applicationLogger)
        {
            Logger = applicationLogger.GetLogger(GetType());
            _applicationDataDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "CalendarSyncPlus");
            _applicationDataDirectory = Path.Combine(_applicationDataDirectory, "Stats");
            _settingsFilePath = Path.Combine(_applicationDataDirectory, "Summary.xml");
        }

        public async Task<bool> SerializeSyncSummaryAsync(SyncSummary syncProfile)
        {
            await TaskEx.Run(() => SerializeSyncSummaryBackgroundTask(syncProfile));
            return true;
        }

        public async Task<SyncSummary> DeserializeSyncSummaryAsync()
        {
            if (!File.Exists(SettingsFilePath))
            {
                return null;
            }
            return await TaskEx.Run(() => DeserializeSyncSummaryBackgroundTask());
        }

        public bool SerializeSyncSummary(SyncSummary syncProfile)
        {
            SerializeSyncSummaryBackgroundTask(syncProfile);
            return true;
        }

        public SyncSummary DeserializeSyncSummary()
        {
            var result = DeserializeSyncSummaryBackgroundTask();
            if (result == null)
            {
                return SyncSummary.GetDefault();
            }
            return result;
        }

        private void SerializeSyncSummaryBackgroundTask(SyncSummary syncProfile)
        {
            if (!Directory.Exists(ApplicationDataDirectory))
            {
                Directory.CreateDirectory(ApplicationDataDirectory);
            }

            var serializer = new XmlSerializer<SyncSummary>();
            serializer.SerializeToFile(syncProfile, SettingsFilePath);
        }
        
        private SyncSummary DeserializeSyncSummaryBackgroundTask()
        {
            if (!File.Exists(SettingsFilePath))
            {
                Logger.Warn("Sync summary file does not exist");
                return null;
            }
            try
            {
                var serializer = new XmlSerializer<SyncSummary>();
                return serializer.DeserializeFromFile(SettingsFilePath);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return null;
            }
        }

        #region Properties

        public string SettingsFilePath
        {
            get { return _settingsFilePath; }
        }

        public string ApplicationDataDirectory
        {
            get { return _applicationDataDirectory; }
        }

        #endregion
    }
}