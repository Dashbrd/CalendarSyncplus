using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models
{
    public class GoogleAccount : Model
    {
        private GoogleCalendar _googleCalendar;
        private string _name;

        public GoogleCalendar GoogleCalendar
        {
            get { return _googleCalendar; }
            set { SetProperty(ref _googleCalendar, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
    }

    public class GoogleCalendar : Model
    {
        private string _name;
        private string _id;

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public string Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }
    }
}