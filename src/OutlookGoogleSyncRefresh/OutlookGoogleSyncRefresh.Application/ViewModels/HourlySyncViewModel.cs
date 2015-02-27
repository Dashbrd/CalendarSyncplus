using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.ViewModels
{
    public class HourlySyncViewModel : SyncFrequencyViewModel
    {
        private int _minuteOffsetForHour;

        public HourlySyncViewModel()
        {
            MinuteOffsetForHour = 1;
        }

        public HourlySyncViewModel(HourlySyncFrequency syncFrequency)
        {
            MinuteOffsetForHour = syncFrequency.MinuteOffsetForHour;
        }

        public int MinuteOffsetForHour
        {
            get { return _minuteOffsetForHour; }
            set { SetProperty(ref _minuteOffsetForHour, value); }
        }

        public override SyncFrequency GetFrequency()
        {
            var frequency = new HourlySyncFrequency
            {
                MinuteOffsetForHour = MinuteOffsetForHour
            };
            return frequency;
        }
    }
}