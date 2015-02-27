using System;
using System.Xml.Serialization;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    [XmlInclude(typeof (Calendar))]
    [XmlInclude(typeof (SyncFrequency))]
    public class Settings
    {
        public Settings()
        {
            ExchangeServerSettings = new ExchangeServerSettings();
            LogSettings = new LogSettings();
            OutlookSettings = new OutlookSettings();
        }

        public Calendar SavedCalendar { get; set; }

        public OutlookSettings OutlookSettings { get; set; }

        public ExchangeServerSettings ExchangeServerSettings { get; set; }

        public CalendarEntryOptionsEnum CalendarEntryOptions { get; set; }

        public SyncFrequency SyncFrequency { get; set; }

        public int DaysInPast { get; set; }

        public int DaysInFuture { get; set; }

        public DateTime LastSuccessfulSync { get; set; }

        public bool IsFirstSave { get; set; }

        public bool MinimizeToSystemTray { get; set; }

        public bool HideSystemTrayTooltip { get; set; }

        public bool CheckForUpdates { get; set; }

        public bool RememberPeriodicSyncOn { get; set; }

        public bool PeriodicSyncOn { get; set; }

        public bool RunApplicationAtSystemStartup { get; set; }

        public LogSettings LogSettings { get; set; }
    }
}