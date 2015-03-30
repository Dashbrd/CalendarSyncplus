using System.Threading.Tasks;

namespace OutlookGoogleSyncRefresh.Application.Utilities
{
    public delegate Task<bool> SyncCallback(SyncEventArgs e);
}
