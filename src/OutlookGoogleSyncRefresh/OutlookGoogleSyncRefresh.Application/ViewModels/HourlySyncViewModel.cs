using System;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Application.ViewModels
{
    public class HourlySyncViewModel : SyncFrequencyViewModel
    {
        private int _hours;
        private int _minutes;
        private HourlySyncFrequency _syncFrequency;

        public HourlySyncViewModel()
        {
            Hours = 1;
            Minutes = 0;
        }

        public HourlySyncViewModel(HourlySyncFrequency syncFrequency)
        {
            _syncFrequency = syncFrequency;
            Hours = syncFrequency.Hours;
            Minutes = syncFrequency.Minutes;
            IsModified = false;
        }

        public int Hours
        {
            get { return _hours; }
            set
            {
                if (!IsModified && _hours != value)
                {
                    IsModified = true;
                }
                SetProperty(ref _hours, value);
                ValidateHours();
            }
        }

        public int Minutes
        {
            get { return _minutes; }
            set
            {
                if (!IsModified && _minutes != value)
                {
                    IsModified = true;
                }
                SetProperty(ref _minutes, value);
                ValidateMinutes();
            }
        }

        private void ValidateMinutes()
        {
            if (Minutes == 0 && Hours == 0)
            {
                Hours = 1;
            }
        }

        private void ValidateHours()
        {
            if (Hours == 0 && Minutes == 0)
            {
                Minutes = 5;
            }
        }

        public override SyncFrequency GetFrequency()
        {
            if (_syncFrequency == null)
            {
                _syncFrequency = new HourlySyncFrequency();
            }

            if (IsModified)
            {
                DateTime timeNow = DateTime.Now;
                _syncFrequency.StartTime = timeNow.Subtract(new TimeSpan(0, 0, timeNow.Second));
                _syncFrequency.Hours = Hours;
                _syncFrequency.Minutes = Minutes;
            }
            return _syncFrequency;
        }
    }
}