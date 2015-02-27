using System;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    public class HourlySyncFrequency : SyncFrequency
    {
        public HourlySyncFrequency()
        {
            Name = "Hourly";
        }

        public int MinuteOffsetForHour { get; set; }

        public override bool ValidateTimer(DateTime dateTime)
        {
            if (dateTime.Minute.Equals(MinuteOffsetForHour))
            {
                return true;
            }
            return false;
        }

        public override DateTime GetNextSyncTime()
        {
            DateTime dateTime = DateTime.Now;
            if (dateTime.Minute > MinuteOffsetForHour)
            {
                return DateTime.Now.Add(new TimeSpan(0, 60 - (dateTime.Minute - MinuteOffsetForHour), 0));
            }
            return DateTime.Now.Add(new TimeSpan(0, MinuteOffsetForHour - dateTime.Minute, 0));
        }

        public override string ToString()
        {
            string str = string.Format("{0} : Minute Offset : {1}", GetType().Name, MinuteOffsetForHour);
            return str;
        }
    }
}