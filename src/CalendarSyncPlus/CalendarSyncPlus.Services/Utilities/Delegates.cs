using System.Threading.Tasks;

namespace CalendarSyncPlus.Services.Utilities
{
    public delegate Task<bool> SyncCallback(SyncEventArgs e);
}