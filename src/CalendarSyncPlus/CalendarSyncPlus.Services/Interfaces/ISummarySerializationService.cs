using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Services.Interfaces
{
    public interface ISummarySerializationService
    {
        Task<bool> SerializeSyncSummaryAsync(SyncSummary syncProfile);
        Task<SyncSummary> DeserializeSyncSummaryAsync();
        bool SerializeSyncSummary(SyncSummary syncProfile);
        SyncSummary DeserializeSyncSummary();
    }
}