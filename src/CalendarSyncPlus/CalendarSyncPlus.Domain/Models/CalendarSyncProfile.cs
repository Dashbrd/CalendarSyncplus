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
        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsSyncEnabled
        {
            get { return _isSyncEnabled; }
            set { SetProperty(ref _isSyncEnabled, value); }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsDefault
        {
            get { return _isDefault; }
            set { SetProperty(ref _isDefault, value); }
        }
        /// <summary>
        /// 
        /// </summary>
        public SyncSettings SyncSettings
        {
            get { return _syncSettings; }
            set { SetProperty(ref _syncSettings, value); }
        }

        public GoogleAccount GoogleAccount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public OutlookSettings OutlookSettings { get; set; }

        /// <summary>
        ///     To be implemented in future
        /// </summary>
        [XmlIgnore]
        public ExchangeServerSettings ExchangeServerSettings { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CalendarEntryOptionsEnum CalendarEntryOptions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public LogSettings LogSettings { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool SetCalendarCategory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Category EventCategory { get; set; }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// Gets default calendar profile for the user
        /// </summary>
        /// <returns></returns>
        public static CalendarSyncProfile GetDefaultSyncProfile()
        {
            var syncProfile = new CalendarSyncProfile
            {
                SyncSettings = SyncSettings.GetDefault(),
                OutlookSettings =
                {
                    OutlookOptions = OutlookOptionsEnum.DefaultProfile |
                                     OutlookOptionsEnum.DefaultCalendar
                },
                CalendarEntryOptions = CalendarEntryOptionsEnum.Description | CalendarEntryOptionsEnum.Attendees |
                                       CalendarEntryOptionsEnum.Reminders | CalendarEntryOptionsEnum.AsAppointments,
            };
            syncProfile.SetCalendarTypes();
            return syncProfile;
        }
    }
}