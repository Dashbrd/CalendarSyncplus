using System;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    public class HourlySyncFrequency : SyncFrequency
    {
        public HourlySyncFrequency()
        {
            Name = "Hourly";
            Hours = 1;
            Minutes = 0;
        }

        public DateTime StartTime { get; set; }

        public int Hours { get; set; }

        public int Minutes { get; set; }

        public override bool ValidateTimer(DateTime dateTime)
        {
            TimeSpan totalTimeElapsed = StartTime.Subtract(dateTime);
            var timeElapsed = new TimeSpan(Hours, Minutes, 0);
            if (Math.Abs(totalTimeElapsed.TotalSeconds%timeElapsed.TotalSeconds) < 1)
            {
                return true;
            }

            return false;
        }

        public override DateTime GetNextSyncTime(DateTime dateTimeNow)
        {
            try
            {
                if (Hours == 0 && Minutes == 0)
                {
                    return dateTimeNow;
                }
                var timeSpan = new TimeSpan(Hours, Minutes, 0);
                DateTime dateTime = StartTime;
                while (dateTimeNow.Subtract(dateTime).TotalSeconds > timeSpan.TotalSeconds)
                {
                    dateTime = dateTime.Add(timeSpan);
                }

                if (dateTimeNow.Subtract(dateTime).TotalSeconds > 0)
                {
                    dateTime = dateTime.Add(timeSpan);
                }
                return dateTime;
            }
            catch
            {
            }
            return DateTime.Now;
        }

        public override string ToString()
        {
            string str = string.Format("{0} : Minute Offset : {1}", GetType().Name, Minutes);
            return str;
        }
    }
}