using System;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.ViewModels
{
    public class WeeklySyncViewModel : SyncFrequencyViewModel
    {
        private bool _isFriday;
        private bool _isMonday;
        private bool _isSaturday;
        private bool _isSunday;
        private bool _isThursday;
        private bool _isTuesday;
        private bool _isWednesday;
        private DateTime _timeOfDay;
        private int _weekRecurrence;
        private WeeklySyncFrequency _weeklySyncFrequency;

        public WeeklySyncViewModel()
        {
            TimeOfDay = DateTime.Now;
            WeekRecurrence = 1;
            LoadDayOfTheWeek(DateTime.Now.DayOfWeek);
        }

        public WeeklySyncViewModel(WeeklySyncFrequency weeklyWeeklySyncFrequency)
        {
            _weeklySyncFrequency = weeklyWeeklySyncFrequency;
            TimeOfDay = weeklyWeeklySyncFrequency.TimeOfDay;
            WeekRecurrence = weeklyWeeklySyncFrequency.WeekRecurrence;
            foreach (DayOfWeek dayOfWeekEnum in weeklyWeeklySyncFrequency.DaysOfWeek)
            {
                LoadDayOfTheWeek(dayOfWeekEnum);
            }
            IsModified = false;
        }

        public int WeekRecurrence
        {
            get { return _weekRecurrence; }
            set
            {
                if (!IsModified && _weekRecurrence != value)
                {
                    IsModified = true;
                }
                SetProperty(ref _weekRecurrence, value);
            }
        }

        public DateTime TimeOfDay
        {
            get { return _timeOfDay; }
            set
            {
                if (!IsModified && _timeOfDay != value)
                {
                    IsModified = true;
                }
                SetProperty(ref _timeOfDay, value);
            }
        }

        public bool IsSunday
        {
            get { return _isSunday; }
            set
            {
                if (!IsModified && _isSunday != value)
                {
                    IsModified = true;
                }
                SetProperty(ref _isSunday, value);
            }
        }

        public bool IsMonday
        {
            get { return _isMonday; }
            set
            {
                if (!IsModified && _isMonday != value)
                {
                    IsModified = true;
                }
                SetProperty(ref _isMonday, value);
            }
        }

        public bool IsTuesday
        {
            get { return _isTuesday; }
            set
            {
                if (!IsModified && _isTuesday != value)
                {
                    IsModified = true;
                }
                SetProperty(ref _isTuesday, value);
            }
        }

        public bool IsWednesday
        {
            get { return _isWednesday; }
            set
            {
                if (!IsModified && _isWednesday != value)
                {
                    IsModified = true;
                }
                SetProperty(ref _isWednesday, value);
            }
        }

        public bool IsThursday
        {
            get { return _isThursday; }
            set
            {
                if (!IsModified && _isThursday != value)
                {
                    IsModified = true;
                }
                SetProperty(ref _isThursday, value);
            }
        }

        public bool IsFriday
        {
            get { return _isFriday; }
            set
            {
                if (!IsModified && _isFriday != value)
                {
                    IsModified = true;
                }
                SetProperty(ref _isFriday, value);
            }
        }

        public bool IsSaturday
        {
            get { return _isSaturday; }
            set
            {
                if (!IsModified && _isSaturday != value)
                {
                    IsModified = true;
                }
                SetProperty(ref _isSaturday, value);
            }
        }

        public override SyncFrequency GetFrequency()
        {
            if (_weeklySyncFrequency == null)
            {
                _weeklySyncFrequency = new WeeklySyncFrequency();
            }

            if (IsModified)
            {
                DateTime timeNow = DateTime.Now;
                _weeklySyncFrequency.StartDate = timeNow.Subtract(new TimeSpan(0, 0, timeNow.Second));
                _weeklySyncFrequency.WeekRecurrence = WeekRecurrence;
                _weeklySyncFrequency.TimeOfDay = TimeOfDay;
                UpdateDaysOfWeek(_weeklySyncFrequency, IsSunday, DayOfWeek.Sunday);
                UpdateDaysOfWeek(_weeklySyncFrequency, IsMonday, DayOfWeek.Monday);
                UpdateDaysOfWeek(_weeklySyncFrequency, IsTuesday, DayOfWeek.Tuesday);
                UpdateDaysOfWeek(_weeklySyncFrequency, IsWednesday, DayOfWeek.Wednesday);
                UpdateDaysOfWeek(_weeklySyncFrequency, IsThursday, DayOfWeek.Thursday);
                UpdateDaysOfWeek(_weeklySyncFrequency, IsFriday, DayOfWeek.Friday);
                UpdateDaysOfWeek(_weeklySyncFrequency, IsSaturday, DayOfWeek.Saturday);
            }
            IsModified = false;
            return _weeklySyncFrequency;
        }

        private void UpdateDaysOfWeek(WeeklySyncFrequency frequency, bool isValid, DayOfWeek dayOfWeek)
        {
            if (isValid)
            {
                frequency.DaysOfWeek.Add(dayOfWeek);
            }
        }

        private void LoadDayOfTheWeek(DayOfWeek dayOfTheWeek)
        {
            switch (dayOfTheWeek)
            {
                case DayOfWeek.Sunday:
                    IsSunday = true;
                    break;
                case DayOfWeek.Monday:
                    IsMonday = true;
                    break;
                case DayOfWeek.Tuesday:
                    IsTuesday = true;
                    break;
                case DayOfWeek.Wednesday:
                    IsWednesday = true;
                    break;
                case DayOfWeek.Thursday:
                    IsThursday = true;
                    break;
                case DayOfWeek.Friday:
                    IsFriday = true;
                    break;
                case DayOfWeek.Saturday:
                    IsSaturday = true;
                    break;
            }
        }
    }
}