using System;
using System.Waf.Foundation;
using System.Xml.Serialization;
using CalendarSyncPlus.Common.MetaData;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [Serializable]
    [XmlInclude(typeof (SyncFrequency))]
    public class CalendarSyncProfile : SyncProfile
    {
        private CalendarSyncSettings _syncSettings;
        private CalendarEntryOptionsEnum _calendarEntryOptions;
        private bool _setCalendarCategory;
        private Category _eventCategory;

        public CalendarSyncProfile()
        {
            Name = "Default Calendar Profile";
            GoogleSettings = new GoogleSettings();
            SyncSettings = new CalendarSyncSettings();
            ExchangeServerSettings = new ExchangeServerSettings();
            OutlookSettings = new OutlookSettings();
            IsSyncEnabled = true;
            IsDefault = true;
        }

        /// <summary>
        /// </summary>
        public CalendarSyncSettings SyncSettings
        {
            get { return _syncSettings; }
            set { SetProperty(ref _syncSettings, value); }
        }

        /// <summary>
        /// </summary>
        public CalendarEntryOptionsEnum CalendarEntryOptions
        {
            get { return _calendarEntryOptions; }
            set { SetProperty(ref _calendarEntryOptions, value); }
        }

        /// <summary>
        /// </summary>
        public bool SetCalendarCategory
        {
            get { return _setCalendarCategory; }
            set { SetProperty(ref _setCalendarCategory, value); }
        }

        /// <summary>
        /// </summary>
        public Category EventCategory
        {
            get { return _eventCategory; }
            set { SetProperty(ref _eventCategory, value); }
        }
        
        /// <summary>
        ///     Gets default calendar profile for the user
        /// </summary>
        /// <returns>
        /// </returns>
        public static CalendarSyncProfile GetDefaultSyncProfile()
        {
            var syncProfile = new CalendarSyncProfile
            {
                SyncSettings = CalendarSyncSettings.GetDefault(),
                OutlookSettings =
                {
                    OutlookOptions =  OutlookOptionsEnum.OutlookDesktop |
                                        OutlookOptionsEnum.DefaultProfile |
                                     OutlookOptionsEnum.DefaultMailBoxCalendar
                },
                CalendarEntryOptions =
                    CalendarEntryOptionsEnum.Description | CalendarEntryOptionsEnum.Attendees |
                    CalendarEntryOptionsEnum.AttendeesToDescription |
                    CalendarEntryOptionsEnum.Reminders | CalendarEntryOptionsEnum.AsAppointments,
                    SyncDirection = SyncDirectionEnum.OutlookGoogleOneWay,
                SyncFrequency = new IntervalSyncFrequency {Hours = 1, Minutes = 0, StartTime = DateTime.Now}
            };
            syncProfile.SetSourceDestTypes();
            return syncProfile;
        }
    }
}