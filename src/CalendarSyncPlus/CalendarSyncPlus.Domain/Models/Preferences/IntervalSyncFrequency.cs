using System;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [Serializable]
    public class IntervalSyncFrequency : SyncFrequency
    {
        private DateTime _startTime;
        private int _hours;
        private int _minutes;

        public IntervalSyncFrequency()
        {
            Name = "Interval";
            Hours = 1;
            Minutes = 0;
            StartTime = DateTime.Now;
            StartTime = StartTime.AddSeconds(-StartTime.Second);
        }

        public DateTime StartTime
        {
            get { return _startTime; }
            set { SetProperty(ref _startTime, value); }
        }

        public int Hours
        {
            get { return _hours; }
            set
            {
                SetProperty(ref _hours, value);
                ValidateHours();
            }
        }

        public int Minutes
        {
            get { return _minutes; }
            set
            {
                SetProperty(ref _minutes, value);
                ValidateMinutes();
            }
        }

        private void ValidateMinutes()
        {
            if (Minutes < 15 && Hours == 0)
            {
                Hours = 1;
            }
        }

        private void ValidateHours()
        {
            if (Hours == 0 && Minutes < 15)
            {
                Minutes = 15;
            }
        }

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
            var str = $"{GetType().Name} : Minute Offset : {Minutes}";
            return str;
        }
    }
}