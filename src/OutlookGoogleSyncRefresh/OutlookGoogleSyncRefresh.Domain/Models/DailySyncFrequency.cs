
using System;
using System.Threading;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    public class DailySyncFrequency : SyncFrequency
    {
        public DailySyncFrequency()
        {
            this.Name = "Daily";
        }

        public DateTime StartDate { get; set; }

        public bool EveryWeekday { get; set; }

        public bool CustomDay { get; set; }

        public int DayGap { get; set; }

        public DateTime TimeOfDay { get; set; }

        public override bool ValidateTimer(DateTime dateTime)
        {
            if (IsDayValid(dateTime))
            {
                if (IsTimeValid(dateTime))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsDayValid(DateTime dateTime)
        {
            if (EveryWeekday && dateTime.DayOfWeek != DayOfWeek.Saturday &&
                dateTime.DayOfWeek != DayOfWeek.Sunday)
            {
                return true;
            }

            if (CustomDay)
            {
                if (DayGap == 1)
                {
                    return true;
                }

                if (StartDate.Date.Subtract(dateTime.Date).Days % DayGap == 0)
                {
                    return true;
                }
            }
            return false;
        }

        bool IsTimeValid(DateTime dateTime)
        {
            if (dateTime.TimeOfDay.ToString("t").Equals(TimeOfDay.TimeOfDay.ToString("t")))
            {
                return true;
            }
            return false;
        }

        public override DateTime GetNextSyncTime()
        {

            var dateTime = DateTime.Now;
            if (dateTime.CompareTo(TimeOfDay) > 0)
            {
                dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, TimeOfDay.Hour, TimeOfDay.Minute, TimeOfDay.Second);
                dateTime = dateTime.Add(new TimeSpan(1, 0, 0, 0));
            }

            while (!ValidateTimer(dateTime))
            {
                if (IsDayValid(dateTime))
                {
                    return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, TimeOfDay.Hour, TimeOfDay.Minute, TimeOfDay.Second);
                }
                dateTime = dateTime.Add(new TimeSpan(1, 0, 0, 0));
            }
            return dateTime;
        }

        public override string ToString()
        {
            string str = string.Format("{0} : {1}", this.GetType().Name, TimeOfDay);
            return str;
        }
    }
}