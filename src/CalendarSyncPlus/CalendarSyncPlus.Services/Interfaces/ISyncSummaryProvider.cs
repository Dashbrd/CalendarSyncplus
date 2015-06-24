using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Services.Interfaces
{
    public interface ISyncSummaryProvider
    {
        SyncSummary GetSyncSummary();
    }
}