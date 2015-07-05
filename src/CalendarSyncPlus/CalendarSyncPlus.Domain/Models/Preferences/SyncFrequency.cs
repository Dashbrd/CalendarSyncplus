using System;
using System.Waf.Foundation;
using System.Xml.Serialization;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [Serializable]
    [XmlInclude(typeof (IntervalSyncFrequency))]
    [XmlInclude(typeof (DailySyncFrequency))]
    [XmlInclude(typeof (WeeklySyncFrequency))]
    public class SyncFrequency : ValidatableModel
    {
        [XmlIgnore]
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