using System;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [Serializable]
    public class IntervalSyncFrequency : SyncFrequency
    {
        public IntervalSyncFrequency()
        {
            Name = "Interval";
            Hours = 1;
            Minutes = 0;
            StartTime = DateTime.Now;
            StartTime = StartTime.AddSeconds(-StartTime.Second);
        }

        public DateTime StartTime { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }

        public override bool ValidateTimer(DateTime dateTimeNow)
        {
            if (Hours == 0 && Minutes == 0)
            {
                return false;
            }
            var timeSpan = new TimeSpan(Hours, Minutes, 0);
            var dateTime = StartTime;
            while (dateTime.CompareTo(dateTimeNow) < 0)
            {
                dateTime = dateTime.Add(timeSpan);
            }

            if (dateTime.CompareTo(dateTimeNow) == 0)
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
                var dateTime = StartTime;
                while (dateTime.CompareTo(dateTimeNow) <= 0)
                {
                    dateTime = dateTime.Add(timeSpan);
                }
                return dateTime;
            }
            catch (Exception ex)
            {
            }
            return DateTime.Now;
        }

        public override string ToString()
        {
            var str = string.Format("{0} : Minute Offset : {1}", GetType().Name, Minutes);
            return str;
        }
    }
}