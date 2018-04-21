using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{    
    [DataContract]
    [KnownType(typeof(IntervalSyncFrequency))]
    [KnownType(typeof(DailySyncFrequency))]
    [KnownType(typeof(WeeklySyncFrequency))]
    public class SyncFrequency : ValidatableModel
    {
        [DataMember]
        public string Name { get; protected set; }

        public virtual bool ValidateTimer(DateTime dateTimeNow)
        {
            return false;
        }

        public virtual DateTime GetNextSyncTime(DateTime dateTimeNow)
        {
            return dateTimeNow;
        }
    }
}