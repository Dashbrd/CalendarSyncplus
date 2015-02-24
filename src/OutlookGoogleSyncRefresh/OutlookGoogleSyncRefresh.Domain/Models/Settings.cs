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

        public bool AddDescription { get; set; }

        public bool AddAttendees { get; set; }

        public bool AddReminders { get; set; }

        public bool LogSyncInfo { get; set; }

        public bool CreateNewFileForEverySync { get; set; }

        public DateTime LastSuccessfulSync { get; set; }

        public bool IsDefaultMailBox { get; set; }
        public bool IsDefaultProfile { get; set; }

        public string OutlookProfileName { get; set; }

        public OutlookMailBox OutlookMailBox { get; set; }

        public OutlookCalendar OutlookCalendar { get; set; }

        public bool IsFirstSave { get; set; }

        public bool MinimizeToSystemTray { get; set; }

        public bool HideSystemTrayTooltip { get; set; }
    }
}
