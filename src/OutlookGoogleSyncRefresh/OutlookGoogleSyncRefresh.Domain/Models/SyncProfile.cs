using System;
using System.Waf.Foundation;
using System.Xml.Serialization;
using OutlookGoogleSyncRefresh.Common.MetaData;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    [XmlInclude(typeof(Calendar))]
    [XmlInclude(typeof(SyncFrequency))]
    public class SyncProfile : Model
    {
        private CalendarSyncDirectionEnum _calendarSyncDirection;
        private DateTime _nextSync;

        public SyncProfile()
        {
            Name = "Default Profile";
            SyncSettings = new SyncSettings();
            ExchangeServerSettings = new ExchangeServerSettings();
            OutlookSettings = new OutlookSettings();
            SettingsVersion = new Version(1, 2);
            IsSyncEnabled = true;
        }

        public string Name { get; set; }

        public bool IsSyncEnabled { get; set; }

        public bool IsValid { get; set; }

        public bool IsDefault { get; set; }

        public int Order { get; set; }

        public Version SettingsVersion { get; set; }

        public SyncSettings SyncSettings { get; set; }

        public Calendar GoogleCalendar { get; set; }

        public OutlookSettings OutlookSettings { get; set; }

        /// <summary>
        /// To be implemented in future
        /// </summary>
        [XmlIgnore]
        public ExchangeServerSettings ExchangeServerSettings { get; set; }

        public CalendarEntryOptionsEnum CalendarEntryOptions { get; set; }

        public int DaysInPast { get; set; }

        public int DaysInFuture { get; set; }
        
        public LogSettings LogSettings { get; set; }

        public DateTime LastSuccessfulSync { get; set; }

        /// <summary>
        /// </summary>
        public DateTime NextSync
        {
            get { return _nextSync; }
            set { SetProperty(ref _nextSync, value); }
        }

        public void SetCalendarTypes()
        {
            if (SyncSettings.CalendarSyncDirection == CalendarSyncDirectionEnum.OutlookGoogleOneWay)
            {
                SyncSettings.SourceCalendar = OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.ExchangeWebServices)
                    ? CalendarServiceType.EWS
                    : CalendarServiceType.OutlookDesktop;
                SyncSettings.DestinationCalendar = CalendarServiceType.Google;
            }
            else
            {
                SyncSettings.SourceCalendar = CalendarServiceType.Google;
                SyncSettings.DestinationCalendar = OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.ExchangeWebServices)
                    ? CalendarServiceType.EWS
                    : CalendarServiceType.OutlookDesktop;
            }
            if (SyncSettings.CalendarSyncDirection == CalendarSyncDirectionEnum.OutlookGoogleTwoWay)
            {
                SyncSettings.SyncMode = SyncModeEnum.TwoWay;
                if (SyncSettings.MasterCalendar == CalendarServiceType.OutlookDesktop)
                {
                    SyncSettings.SourceCalendar = OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.ExchangeWebServices)
                    ? CalendarServiceType.EWS
                    : CalendarServiceType.OutlookDesktop;
                    SyncSettings.DestinationCalendar = CalendarServiceType.Google;
                }
                else
                {
                    SyncSettings.SourceCalendar = CalendarServiceType.Google;
                    SyncSettings.DestinationCalendar = OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.ExchangeWebServices)
                    ? CalendarServiceType.EWS
                    : CalendarServiceType.OutlookDesktop;
                }
            }
            else
            {
                SyncSettings.SyncMode = SyncModeEnum.OneWay;
            }
        }
    }
}