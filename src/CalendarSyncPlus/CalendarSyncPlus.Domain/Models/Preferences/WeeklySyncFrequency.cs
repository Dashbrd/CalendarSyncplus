using System;
using System.Collections.Generic;
using CalendarSyncPlus.Domain.Helpers;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [Serializable]
    public class WeeklySyncFrequency : SyncFrequency
    {
        private int _weekRecurrence;
        private DateTime _timeOfDay;
        private List<DayOfWeek> _daysOfWeek;

        public WeeklySyncFrequency()
        {
            Name = "Weekly";
            DaysOfWeek = new List<DayOfWeek>() {DateTime.Today.DayOfWeek};
            WeekRecurrence = 1;
            TimeOfDay = DateTime.Now;
        }

        public DateTime StartDate { get; set; }

        public int WeekRecurrence
        {
            get { return _weekRecurrence; }
            set { SetProperty(ref _weekRecurrence, value); }
        }

        public DateTime TimeOfDay
        {
            get { return _timeOfDay; }
            set { SetProperty(ref _timeOfDay, value); }
        }

        public List<DayOfWeek> DaysOfWeek
        {
            get { return _daysOfWeek; }
            set { SetProperty(ref _daysOfWeek, value); }
        }

        public override bool ValidateTimer(DateTime dateTimeNow)
        {
            if (IsDayValid(dateTimeNow))
            {
                if (dateTimeNow.IsTimeValid(TimeOfDay))
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

        public override DateTime GetNextSyncTime(DateTime dateTimeNow)
        {
            if (dateTimeNow.CompareTo(TimeOfDay) > 0)
            {
                dateTimeNow = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, TimeOfDay.Hour,
                    TimeOfDay.Minute,
                    TimeOfDay.Second);
                dateTimeNow = dateTimeNow.Add(new TimeSpan(1, 0, 0, 0));
            }

            while (!ValidateTimer(dateTimeNow))
            {
                if (IsDayValid(dateTimeNow))
                {
                    return new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, TimeOfDay.Hour,
                        TimeOfDay.Minute,
                        TimeOfDay.Second);
                }
                dateTimeNow = dateTimeNow.Add(new TimeSpan(1, 0, 0, 0));
            }
            return dateTimeNow;
        }

        public override string ToString()
        {
            var str = $"{GetType().Name} : {TimeOfDay}";
            return str;
        }
    }
}