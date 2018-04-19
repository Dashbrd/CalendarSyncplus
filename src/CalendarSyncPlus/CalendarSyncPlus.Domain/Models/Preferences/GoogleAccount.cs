using Newtonsoft.Json;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{   
    public class GoogleAccount : Model
    {
        [JsonProperty("name")]
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
    }
}