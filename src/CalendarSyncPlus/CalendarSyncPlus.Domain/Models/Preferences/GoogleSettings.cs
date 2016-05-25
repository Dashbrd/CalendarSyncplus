using System;
using System.Collections.Generic;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [Serializable]
    public class GoogleSettings : Model
    {
        private GoogleAccount _googleAccount;
        private GoogleCalendar _googleCalendar;
        private List<GoogleCalendar> _googleCalendars;

        public GoogleSettings()
        {
            GoogleCalendars = new List<GoogleCalendar>();
        }

        public GoogleCalendar GoogleCalendar
        {
            get { return _googleCalendar; }
            set { SetProperty(ref _googleCalendar, value); }
        }

        public List<GoogleCalendar> GoogleCalendars
        {
            get { return _googleCalendars; }
            set { SetProperty(ref _googleCalendars, value); }
        }

        public GoogleAccount GoogleAccount
        {
            get { return _googleAccount; }
            set { SetProperty(ref _googleAccount, value); }
        }
    }
}