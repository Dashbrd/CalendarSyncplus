using System;
using System.Collections.Generic;
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

        public WeeklySyncViewModel()
        {
            TimeOfDay = DateTime.Now;
            WeekRecurrence = 1;
            LoadDayOfTheWeek(DateTime.Now.DayOfWeek);
        }

        public WeeklySyncViewModel(WeeklySyncFrequency weeklySyncFrequency)
        {
            WeekRecurrence = weeklySyncFrequency.WeekRecurrence;
            foreach (DayOfWeek dayOfWeekEnum in weeklySyncFrequency.DaysOfWeek)
            {
                LoadDayOfTheWeek(dayOfWeekEnum);
            }
        }

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

        public bool IsSunday
        {
            get { return _isSunday; }
            set { SetProperty(ref _isSunday, value); }
        }

        public bool IsMonday
        {
            get { return _isMonday; }
            set { SetProperty(ref _isMonday, value); }
        }

        public bool IsTuesday
        {
            get { return _isTuesday; }
            set { SetProperty(ref _isTuesday, value); }
        }

        public bool IsWednesday
        {
            get { return _isWednesday; }
            set { SetProperty(ref _isWednesday, value); }
        }

        public bool IsThursday
        {
            get { return _isThursday; }
            set { SetProperty(ref _isThursday, value); }
        }

        public bool IsFriday
        {
            get { return _isFriday; }
            set { SetProperty(ref _isFriday, value); }
        }

        public bool IsSaturday
        {
            get { return _isSaturday; }
            set { SetProperty(ref _isSaturday, value); }
        }

        public override SyncFrequency GetFrequency()
        {
            var frequency = new WeeklySyncFrequency
            {
                StartDate = DateTime.Now,
                DaysOfWeek = new List<DayOfWeek>(),
                WeekRecurrence = WeekRecurrence,
                TimeOfDay = TimeOfDay
            };
            UpdateDaysOfWeek(frequency, IsSunday, DayOfWeek.Sunday);
            UpdateDaysOfWeek(frequency, IsMonday, DayOfWeek.Monday);
            UpdateDaysOfWeek(frequency, IsTuesday, DayOfWeek.Tuesday);
            UpdateDaysOfWeek(frequency, IsWednesday, DayOfWeek.Wednesday);
            UpdateDaysOfWeek(frequency, IsThursday, DayOfWeek.Thursday);
            UpdateDaysOfWeek(frequency, IsFriday, DayOfWeek.Friday);
            UpdateDaysOfWeek(frequency, IsSaturday, DayOfWeek.Saturday);
            return frequency;
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