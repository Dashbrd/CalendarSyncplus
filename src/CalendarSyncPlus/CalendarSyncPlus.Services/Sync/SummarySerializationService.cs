using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Domain.File.Xml;
using CalendarSyncPlus.Domain.Models.Metrics;
using CalendarSyncPlus.Services.Interfaces;
using log4net;

using static System.Threading.Tasks.Task;

namespace CalendarSyncPlus.Services.Sync
{
    [Export(typeof (ISummarySerializationService))]
    public class SummarySerializationService : ISummarySerializationService
    {
        private readonly string _applicationDataDirectory;
        private ILog Logger { get; set; }
        private string _summaryFilePath;

        [ImportingConstructor]
        public SummarySerializationService(ApplicationLogger applicationLogger)
        {
            Logger = applicationLogger.GetLogger(GetType());
            _applicationDataDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "CalendarSyncPlus");
            _applicationDataDirectory = Path.Combine(_applicationDataDirectory, "Stats");
            _summaryFilePath = Path.Combine(_applicationDataDirectory, "Summary.xml");
        }

        public async Task<bool> SerializeSyncSummaryAsync(SyncSummary syncProfile)
        {
            await Run(() => SerializeSyncSummaryBackgroundTask(syncProfile));
            return true;
        }

        public async Task<SyncSummary> DeserializeSyncSummaryAsync()
        {
            if (!File.Exists(SummaryFilePath))
            {
                return null;
            }
            return await Run(() => DeserializeSyncSummaryBackgroundTask());
        }

        public bool SerializeSyncSummary(SyncSummary syncProfile)
        {
            SerializeSyncSummaryBackgroundTask(syncProfile);
            return true;
        }

        public SyncSummary DeserializeSyncSummary()
        {
            var result = DeserializeSyncSummaryBackgroundTask();
            return result ?? SyncSummary.GetDefault();
        }

        private void SerializeSyncSummaryBackgroundTask(SyncSummary syncProfile)
        {
            if (!Directory.Exists(ApplicationDataDirectory))
            {
                Directory.CreateDirectory(ApplicationDataDirectory);
            }
            int i = 0;
            while (File.Exists(SummaryFilePath))
            {
                _summaryFilePath = Path.Combine(_applicationDataDirectory, "Summary"+ i++ + ".xml");
            }

            var serializer = new XmlSerializer<SyncSummary>();
            serializer.SerializeToFile(syncProfile, SummaryFilePath);
        }
        
        private SyncSummary DeserializeSyncSummaryBackgroundTask()
        {
            if (!File.Exists(SummaryFilePath))
            {
                Logger.Warn("Sync summary file does not exist");
                return null;
            }
            try
            {
                var serializer = new XmlSerializer<SyncSummary>();
                return serializer.DeserializeFromFile(SummaryFilePath);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return null;
            }
        }

        #region Properties

        public string SummaryFilePath => _summaryFilePath;

        public string ApplicationDataDirectory => _applicationDataDirectory;

        #endregion
    }
}