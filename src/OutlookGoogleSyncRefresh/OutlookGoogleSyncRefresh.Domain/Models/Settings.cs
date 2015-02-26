using System;
using System.Xml.Serialization;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    [XmlInclude(typeof(Calendar))]
    [XmlInclude(typeof(SyncFrequency))]
    public class Settings
    {
        public Calendar SavedCalendar { get; set; }

        public SyncFrequency SyncFrequency { get; set; }

        public int DaysInPast { get; set; }

        public int DaysInFuture { get; set; }

        public CalendarEntryOptionsEnum CalendarEntryOptions { get; set; }

        public bool LogSyncInfo { get; set; }

        public bool CreateNewFileForEverySync { get; set; }

        public DateTime LastSuccessfulSync { get; set; }

        public OutlookOptionsEnum OutlookOptions { get; set; }

        public string OutlookProfileName { get; set; }

        public OutlookMailBox OutlookMailBox { get; set; }

        public OutlookCalendar OutlookCalendar { get; set; }

        public bool IsFirstSave { get; set; }

        public bool MinimizeToSystemTray { get; set; }

        public bool HideSystemTrayTooltip { get; set; }
    }
}
