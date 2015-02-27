using System;
using System.Collections.Generic;
using OutlookGoogleSyncRefresh.Domain.Helpers;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    public class WeeklySyncFrequency : SyncFrequency
    {
        public WeeklySyncFrequency()
        {
            Name = "Weekly";
        }

        public DateTime StartDate { get; set; }

        public int WeekRecurrence { get; set; }

        public DateTime TimeOfDay { get; set; }

        public List<DayOfWeek> DaysOfWeek { get; set; }


        public override bool ValidateTimer(DateTime dateTime)
        {
            if (IsDayValid(dateTime))
            {
                if (dateTime.IsTimeValid(TimeOfDay))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsDayValid(DateTime dateTime)
        {
            if (WeekRecurrence == 1)
            {
                if (DaysOfWeek.Contains(dateTime.DayOfWeek))
                {
                    return true;
                }
            }
            else if (StartDate.Date.Subtract(dateTime.Date).Days > 7*WeekRecurrence)
            {
                if (DaysOfWeek.Contains(dateTime.DayOfWeek))
                {
                    return true;
                }
            }
            return false;
        }

        public override DateTime GetNextSyncTime()
        {
            DateTime dateTime = DateTime.Now;
            if (dateTime.CompareTo(TimeOfDay) > 0)
            {
                dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, TimeOfDay.Hour, TimeOfDay.Minute,
                    TimeOfDay.Second);
                dateTime = dateTime.Add(new TimeSpan(1, 0, 0, 0));
            }

            while (!ValidateTimer(dateTime))
            {
                if (IsDayValid(dateTime))
                {
                    return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, TimeOfDay.Hour, TimeOfDay.Minute,
                        TimeOfDay.Second);
                }
                dateTime = dateTime.Add(new TimeSpan(1, 0, 0, 0));
            }
            return dateTime;
        }

        public override string ToString()
        {
            string str = string.Format("{0} : {1}", GetType().Name, TimeOfDay);
            return str;
        }
    }
}