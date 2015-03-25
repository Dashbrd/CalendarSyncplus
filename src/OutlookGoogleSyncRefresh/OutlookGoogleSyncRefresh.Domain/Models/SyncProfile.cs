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
        private DateTime _nextSync;
        private bool _isDefault;
        private string _name;
        private DateTime _lastSync;

        public SyncProfile()
        {
            Name = "Default Profile";
            SyncSettings = new SyncSettings();
            ExchangeServerSettings = new ExchangeServerSettings();
            OutlookSettings = new OutlookSettings();
            IsSyncEnabled = true;
            IsDefault = true;
        }

        public string Id { get; set; }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public bool IsSyncEnabled { get; set; }

        public bool IsValid { get; set; }

        public bool IsDefault
        {
            get { return _isDefault; }
            set { SetProperty(ref _isDefault, value); }
        }

        public int Order { get; set; }

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

        public DateTime LastSync
        {
            get { return _lastSync; }
            set { SetProperty(ref _lastSync, value); }
        }

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

        public static SyncProfile GetDefaultSyncProfile()
        {
            var syncProfile = new SyncProfile
            {
                DaysInFuture = 7,
                DaysInPast = 1,
            };
            syncProfile.SyncSettings.CalendarSyncDirection = CalendarSyncDirectionEnum.OutlookGoogleOneWay;
            syncProfile.SyncSettings.SyncFrequency = new HourlySyncFrequency();
            syncProfile.OutlookSettings.OutlookOptions = OutlookOptionsEnum.DefaultProfile | OutlookOptionsEnum.DefaultCalendar;
            syncProfile.CalendarEntryOptions = CalendarEntryOptionsEnum.None;
            syncProfile.SetCalendarTypes();
            return syncProfile;
        }
    }
}