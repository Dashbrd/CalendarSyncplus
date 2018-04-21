using System;
using System.Runtime.Serialization;

namespace CalendarSyncPlus.Domain.Models.Preferences
{   
    [DataContract]
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

        [DataMember]
        /// <summary>
        /// </summary>
        public CalendarSyncSettings SyncSettings
        {
            get { return _syncSettings; }
            set { SetProperty(ref _syncSettings, value); }
        }
        [DataMember]
        /// <summary>
        /// </summary>
        public CalendarEntryOptionsEnum CalendarEntryOptions
        {
            get { return _calendarEntryOptions; }
            set { SetProperty(ref _calendarEntryOptions, value); }
        }
        [DataMember]
        /// <summary>
        /// </summary>
        public bool SetCalendarCategory
        {
            get { return _setCalendarCategory; }
            set { SetProperty(ref _setCalendarCategory, value); }
        }
        [DataMember]
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
                                     OutlookOptionsEnum.DefaultMailBoxCalendar,
                    SetOrganizer = true
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