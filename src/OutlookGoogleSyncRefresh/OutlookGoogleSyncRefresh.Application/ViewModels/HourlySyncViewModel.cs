using System;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.ViewModels
{
    public class HourlySyncViewModel : SyncFrequencyViewModel
    {
        private int _minutes;
        private int _hours;

        public HourlySyncViewModel()
        {
            Minutes = 1;
            Hours = 1;
        }

        public HourlySyncViewModel(HourlySyncFrequency syncFrequency)
        {
            Minutes = syncFrequency.Minutes;
        }

        public int Hours
        {
            get { return _hours; }
            set { SetProperty(ref _hours, value); }
        }

        public int Minutes
        {
            get { return _minutes; }
            set { SetProperty(ref _minutes, value); }
        }

        public override SyncFrequency GetFrequency()
        {
            var frequency = new HourlySyncFrequency
            {
                Hours = Hours,
                Minutes = Minutes,
            };
            var timeNow = DateTime.Now;
            frequency.StartTime = timeNow.Subtract(new TimeSpan(0,0,timeNow.Second));
            return frequency;
        }
    }
}