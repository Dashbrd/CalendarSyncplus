using System;
using System.Xml.Serialization;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    [XmlInclude(typeof (HourlySyncFrequency))]
    [XmlInclude(typeof (DailySyncFrequency))]
    [XmlInclude(typeof (WeeklySyncFrequency))]
    public class SyncFrequency
    {
        [XmlIgnore]
        public string Name { get; protected set; }

        public virtual bool ValidateTimer(DateTime dateTime)
        {
            return false;
        }

        public virtual DateTime GetNextSyncTime(DateTime dateTimeNow)
        {
            return dateTimeNow;
        }
    }
}