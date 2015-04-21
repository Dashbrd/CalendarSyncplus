using System;
using System.Xml.Serialization;

namespace CalendarSyncPlus.Domain.Models
{
    [XmlInclude(typeof (IntervalSyncFrequency))]
    [XmlInclude(typeof (DailySyncFrequency))]
    [XmlInclude(typeof (WeeklySyncFrequency))]
    public class SyncFrequency
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