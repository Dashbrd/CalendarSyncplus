using System;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Application.ViewModels
{
    public class DailySyncViewModel : SyncFrequencyViewModel
    {
        private bool _customDay;
        private int _dayGap;
        private bool _everyWeekday;
        private DailySyncFrequency _syncFrequency;
        private DateTime _timeOfDay;

        public DailySyncViewModel()
        {
            TimeOfDay = DateTime.Now;
            DayGap = 1;
            CustomDay = true;
        }

        public DailySyncViewModel(DailySyncFrequency dailySyncFrequency)
        {
            _syncFrequency = dailySyncFrequency;
            TimeOfDay = dailySyncFrequency.TimeOfDay;
            DayGap = dailySyncFrequency.DayGap;
            EveryWeekday = dailySyncFrequency.EveryWeekday;
            CustomDay = dailySyncFrequency.CustomDay;
            IsModified = false;
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

        public int DayGap
        {
            get { return _dayGap; }
            set
            {
                if (!IsModified && _dayGap != value)
                {
                    IsModified = true;
                }
                SetProperty(ref _dayGap, value);
            }
        }

        public bool EveryWeekday
        {
            get { return _everyWeekday; }
            set
            {
                if (!IsModified && _everyWeekday != value)
                {
                    IsModified = true;
                }
                SetProperty(ref _everyWeekday, value);
            }
        }

        public bool CustomDay
        {
            get { return _customDay; }
            set
            {
                if (!IsModified && _customDay != value)
                {
                    IsModified = true;
                }
                SetProperty(ref _customDay, value);
            }
        }

        public override SyncFrequency GetFrequency()
        {
            if (_syncFrequency == null)
            {
                _syncFrequency = new DailySyncFrequency();
            }

            if (IsModified)
            {
                DateTime timeNow = DateTime.Now;
                _syncFrequency.StartDate = timeNow.Subtract(new TimeSpan(0, 0, timeNow.Second));
                _syncFrequency.EveryWeekday = EveryWeekday;
                _syncFrequency.CustomDay = CustomDay;
                _syncFrequency.DayGap = DayGap;
                _syncFrequency.TimeOfDay = TimeOfDay;
            }

            return _syncFrequency;
        }
    }
}