using System;
using System.Waf.Foundation;
using System.Xml.Serialization;
using CalendarSyncPlus.Common.MetaData;

namespace CalendarSyncPlus.Domain.Models
{
    [XmlInclude(typeof(Calendar))]
    [XmlInclude(typeof(SyncFrequency))]
    [XmlInclude(typeof(Category))]
    public class CalendarSyncProfile : Model
    {
        private bool _isDefault;
        private bool _isSyncEnabled;
        private DateTime? _lastSync;
        private string _name;
        private DateTime? _nextSync;
        private SyncSettings _syncSettings;

        public CalendarSyncProfile()
        {
            Name = "Default Profile";
            SyncSettings = new SyncSettings();
            ExchangeServerSettings = new ExchangeServerSettings();
            OutlookSettings = new OutlookSettings();
            IsSyncEnabled = true;
            IsDefault = true;
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public bool IsSyncEnabled
        {
            get { return _isSyncEnabled; }
            set { SetProperty(ref _isSyncEnabled, value); }
        }

        public bool IsDefault
        {
            get { return _isDefault; }
            set { SetProperty(ref _isDefault, value); }
        }

        public SyncSettings SyncSettings
        {
            get { return _syncSettings; }
            set { SetProperty(ref _syncSettings, value); }
        }

        public GoogleAccount GoogleAccount { get; set; }

        public OutlookSettings OutlookSettings { get; set; }

        public string Category { get; set; }

        /// <summary>
        ///     To be implemented in future
        /// </summary>
        [XmlIgnore]
        public ExchangeServerSettings ExchangeServerSettings { get; set; }

        public CalendarEntryOptionsEnum CalendarEntryOptions { get; set; }

        public int DaysInPast { get; set; }

        public int DaysInFuture { get; set; }

        public LogSettings LogSettings { get; set; }

        public bool SetCalendarCategory { get; set; }

        public Category EventCategory { get; set; }


        public DateTime? LastSync
        {
            get { return _lastSync; }
            set { SetProperty(ref _lastSync, value); }
        }

        /// <summary>
        /// </summary>
        public DateTime? NextSync
        {
            get { return _nextSync; }
            set { SetProperty(ref _nextSync, value); }
        }


        public void SetCalendarTypes()
        {
            if (SyncSettings.CalendarSyncDirection == CalendarSyncDirectionEnum.OutlookGoogleOneWay)
            {
                SyncSettings.SourceCalendar =
                    OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.ExchangeWebServices)
                        ? CalendarServiceType.EWS
                        : CalendarServiceType.OutlookDesktop;
                SyncSettings.DestinationCalendar = CalendarServiceType.Google;
            }
            else
            {
                SyncSettings.SourceCalendar = CalendarServiceType.Google;
                SyncSettings.DestinationCalendar =
                    OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.ExchangeWebServices)
                        ? CalendarServiceType.EWS
                        : CalendarServiceType.OutlookDesktop;
            }
            if (SyncSettings.CalendarSyncDirection == CalendarSyncDirectionEnum.OutlookGoogleTwoWay)
            {
                SyncSettings.SyncMode = SyncModeEnum.TwoWay;
                if (SyncSettings.MasterCalendar == CalendarServiceType.OutlookDesktop)
                {
                    SyncSettings.SourceCalendar =
                        OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.ExchangeWebServices)
                            ? CalendarServiceType.EWS
                            : CalendarServiceType.OutlookDesktop;
                    SyncSettings.DestinationCalendar = CalendarServiceType.Google;
                }
                else
                {
                    SyncSettings.SourceCalendar = CalendarServiceType.Google;
                    SyncSettings.DestinationCalendar =
                        OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.ExchangeWebServices)
                            ? CalendarServiceType.EWS
                            : CalendarServiceType.OutlookDesktop;
                }
            }
            else
            {
                SyncSettings.SyncMode = SyncModeEnum.OneWay;
            }
        }

        public static CalendarSyncProfile GetDefaultSyncProfile()
        {
            var syncProfile = new CalendarSyncProfile
            {
                DaysInFuture = 7,
                DaysInPast = 1,
            };
            syncProfile.SyncSettings.CalendarSyncDirection = CalendarSyncDirectionEnum.OutlookGoogleOneWay;
            syncProfile.SyncSettings.SyncFrequency = new HourlySyncFrequency();
            syncProfile.OutlookSettings.OutlookOptions = OutlookOptionsEnum.DefaultProfile |
                                                         OutlookOptionsEnum.DefaultCalendar;
            syncProfile.CalendarEntryOptions = CalendarEntryOptionsEnum.None;
            syncProfile.SetCalendarTypes();
            return syncProfile;
        }
    }
}