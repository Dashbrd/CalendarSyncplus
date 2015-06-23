using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models
{
    public class SyncSummary : Model
    {
        private int _totalSyncs;
        private int _successSyncs;
        private int _failedSyncs;
        private long _totalSyncSeconds;

        public int TotalSyncs
        {
            get { return _totalSyncs; }
            set { SetProperty(ref _totalSyncs, value); }
        }

        public int SuccessSyncs
        {
            get { return _successSyncs; }
            set { SetProperty(ref _successSyncs, value); }
        }

        public int FailedSyncs
        {
            get { return _failedSyncs; }
            set { SetProperty(ref _failedSyncs, value); }
        }

        public long TotalSyncSeconds
        {
            get { return _totalSyncSeconds; }
            set { SetProperty(ref _totalSyncSeconds, value); }
        }

        public static SyncSummary GetDefault()
        {
            return new SyncSummary();
        }
    }
}