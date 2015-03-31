using System.Threading.Tasks;

namespace CalendarSyncPlus.Application.Utilities
{
    public delegate Task<bool> SyncCallback(SyncEventArgs e);
}
