using System;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.ViewModels
{
    public class DailySyncViewModel : SyncFrequencyViewModel
    {
        private bool _customDay;
        private int _dayGap;
        private bool _everyWeekday;
        private DateTime _timeOfDay;

        public DailySyncViewModel()
        {
            TimeOfDay = DateTime.Now;
            DayGap = 1;
            CustomDay = true;
        }

        public DailySyncViewModel(DailySyncFrequency dailySyncFrequency)
        {
            TimeOfDay = dailySyncFrequency.TimeOfDay;
            DayGap = dailySyncFrequency.DayGap;
            EveryWeekday = dailySyncFrequency.EveryWeekday;
            CustomDay = dailySyncFrequency.CustomDay;
        }

        public DateTime TimeOfDay
        {
            get { return _timeOfDay; }
            set { SetProperty(ref _timeOfDay, value); }
        }

        public int DayGap
        {
            get { return _dayGap; }
            set { SetProperty(ref _dayGap, value); }
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

        public override SyncFrequency GetFrequency()
        {
            var frequency = new DailySyncFrequency
            {
                StartDate = DateTime.Now,
                EveryWeekday = EveryWeekday,
                CustomDay = CustomDay,
                DayGap = DayGap,
                TimeOfDay = TimeOfDay
            };

            return frequency;
        }
    }
}