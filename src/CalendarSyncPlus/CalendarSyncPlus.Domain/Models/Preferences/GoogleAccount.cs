using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{   
    [DataContract]
    public class GoogleAccount : Model
    {        
        private string _name;
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
    }
}