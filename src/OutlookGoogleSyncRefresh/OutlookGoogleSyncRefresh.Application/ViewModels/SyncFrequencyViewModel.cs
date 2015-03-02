using System.Waf.Foundation;

using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.ViewModels
{
    public abstract class SyncFrequencyViewModel : Model
    {
        public virtual SyncFrequency GetFrequency()
        {
            return null;
        }
    }
}