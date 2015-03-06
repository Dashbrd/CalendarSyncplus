using System;
using System.Security.Cryptography;
using System.Xml.Serialization;
using OutlookGoogleSyncRefresh.Common.MetaData;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    [XmlInclude(typeof(Calendar))]
    [XmlInclude(typeof(SyncFrequency))]
    public class Settings
    {
        private CalendarSyncModeEnum _calendarSyncMode;

        public Settings()
        {
            ExchangeServerSettings = new ExchangeServerSettings();
            LogSettings = new LogSettings();
            OutlookSettings = new OutlookSettings();
            SettingsVersion = new Version(1, 2);
        }

        public Version SettingsVersion { get; set; }

        public CalendarSyncModeEnum CalendarSyncMode { get; set; }

        [XmlIgnore]
        public CalendarServiceType SourceCalendar { get; private set; }

        [XmlIgnore]
        public CalendarServiceType DestinationCalendar { get; private set; }

        public Calendar GoogleCalendar { get; set; }

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

        public void SetCalendarTypes()
        {
            if (CalendarSyncMode == CalendarSyncModeEnum.OutlookGoogleOneWay ||
                CalendarSyncMode == CalendarSyncModeEnum.OutlookGoogleTwoWay)
            {
                SourceCalendar = OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.ExchangeWebServices)
                    ? CalendarServiceType.EWS
                    : CalendarServiceType.OutlookDesktop;
                DestinationCalendar = CalendarServiceType.Google;
            }
            else
            {
                SourceCalendar = CalendarServiceType.Google;
                DestinationCalendar = OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.ExchangeWebServices)
                    ? CalendarServiceType.EWS
                    : CalendarServiceType.OutlookDesktop;
            }
        }
    }
}