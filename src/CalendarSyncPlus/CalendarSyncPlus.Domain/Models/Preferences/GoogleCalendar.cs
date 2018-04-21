using System.Runtime.Serialization;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{    
    [DataContract]
    public class GoogleCalendar : Model
    {
        private string _id;
        private string _name;
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        [DataMember]
        public string Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }
    }
}