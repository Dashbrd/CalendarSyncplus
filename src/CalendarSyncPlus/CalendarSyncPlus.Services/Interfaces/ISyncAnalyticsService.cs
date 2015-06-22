using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Analytics.Interfaces
{
    /// <summary>
    /// </summary>
    public interface ISyncAnalyticsService
    {
        Task<bool> UploadSyncData(SyncMetric syncMetric, string accountName);
    }
}