using System;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    public class HourlySyncFrequency : SyncFrequency
    {
        public HourlySyncFrequency()
        {
            Name = "Hourly";
        }

        public DateTime StartTime { get; set; }

        public int Minutes { get; set; }

        public int Hours { get; set; }

        public override bool ValidateTimer(DateTime dateTime)
        {
            var timeElapsed = StartTime.Subtract(dateTime);
            if (timeElapsed.Hours == Hours && timeElapsed.Minutes == Minutes)
            {
                return true;
            }
            return false;
        }

        public override DateTime GetNextSyncTime()
        {
            DateTime dateTime = DateTime.Now;
            return dateTime.Add(new TimeSpan(0, Hours, Minutes));

        }

        public override string ToString()
        {
            string str = string.Format("{0} : Minute Offset : {1}", GetType().Name, Minutes);
            return str;
        }
    }
}