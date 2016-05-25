using CalendarSyncPlus.Domain.Models.Metrics;

namespace CalendarSyncPlus.Services.Interfaces
{
    public interface ISyncSummaryProvider
    {
        SyncSummary GetSyncSummary();
    }
}