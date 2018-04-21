using CalendarSyncPlus.Domain.Models.Preferences;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSyncPlus.Domain.File.Json
{
    public class FrequencyConverter : CustomJsonConverter<SyncFrequency>
    {
        protected override SyncFrequency Create(Type objectType, JObject jsonObject)
        {
            // examine the $type value
            string typeName = (jsonObject["Name"]).ToString();

            // based on the $type, instantiate and return a new object
            switch (typeName)
            {
                case "Interval":
                    return new IntervalSyncFrequency();
                case "Daily":
                    return new DailySyncFrequency();
                case "Weekly":
                    return new WeeklySyncFrequency();
                default:
                    return null;
            }
        }
    }
}
