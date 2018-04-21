using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{   
    [DataContract]
    public class GoogleSettings : Model
    {
        private GoogleAccount _googleAccount;
        private GoogleCalendar _googleCalendar;
        private List<GoogleCalendar> _googleCalendars;

        public GoogleSettings()
        {
            GoogleCalendars = new List<GoogleCalendar>();
        }
        [DataMember]
        public GoogleCalendar GoogleCalendar
        {
            get { return _googleCalendar; }
            set { SetProperty(ref _googleCalendar, value); }
        }
        [DataMember]
        public List<GoogleCalendar> GoogleCalendars
        {
            get { return _googleCalendars; }
            set { SetProperty(ref _googleCalendars, value); }
        }
        [DataMember]
        public GoogleAccount GoogleAccount
        {
            get { return _googleAccount; }
            set { SetProperty(ref _googleAccount, value); }
        }
    }
}