using System;
using CalendarSyncPlus.Domain.Helpers;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [Serializable]
    public class DailySyncFrequency : SyncFrequency
    {
        private bool _customDay;
        private DateTime _startDate;
        private bool _everyWeekday;
        private int _dayGap;
        private DateTime _timeOfDay;

        public DailySyncFrequency()
        {
            Name = "Daily";
            DayGap = 1;
            CustomDay = true;
            StartDate = DateTime.Today;
            TimeOfDay = DateTime.Now;
        }

        public DateTime StartDate
        {
            get { return _startDate; }
            set { SetProperty(ref _startDate, value); }
        }

        public bool EveryWeekday
        {
            get { return _everyWeekday; }
            set { SetProperty(ref _everyWeekday, value); }
        }

        public bool CustomDay
        {
            get { return _customDay; }
            set { SetProperty(ref _customDay, value); }
        }

        public int DayGap
        {
            get { return _dayGap; }
            set { SetProperty(ref _dayGap, value); }
        }

        public DateTime TimeOfDay
        {
            get { return _timeOfDay; }
            set { SetProperty(ref _timeOfDay, value); }
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

                if (StartDate.Date.Subtract(dateTime.Date).Days%DayGap == 0)
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