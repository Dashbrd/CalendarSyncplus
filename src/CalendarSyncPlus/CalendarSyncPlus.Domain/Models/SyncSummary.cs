using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models
{
    public class SyncSummary : Model
    {
        public int TotalSyncs { get; set; }
        public int CompletedSyncs { get; set; }
        public int FailedSyncs { get; set; }

        public static SyncSummary GetDefault()
        {
            return new SyncSummary();
        }
    }
}