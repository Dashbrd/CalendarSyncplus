using System;
using System.Windows;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.ViewModels
{
    public class HourlySyncViewModel : SyncFrequencyViewModel
    {
        private int _minutes;
        private int _hours;

        public HourlySyncViewModel()
        {
            Hours = 1;
            Minutes = 0;
        }

        public HourlySyncViewModel(HourlySyncFrequency syncFrequency)
        {
            Hours = syncFrequency.Hours;
            Minutes = syncFrequency.Minutes;
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
            var frequency = new HourlySyncFrequency
            {
                Hours = Hours,
                Minutes = Minutes,
            };
            var timeNow = DateTime.Now;
            frequency.StartTime = timeNow.Subtract(new TimeSpan(0, 0, timeNow.Second));
            return frequency;
        }
    }
}