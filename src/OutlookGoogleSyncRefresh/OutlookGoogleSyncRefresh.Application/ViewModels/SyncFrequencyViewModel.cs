using System.Waf.Foundation;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.ViewModels
{
    public abstract class SyncFrequencyViewModel : Model
    {
        public bool IsModified { get; protected set; }

        public virtual SyncFrequency GetFrequency()
        {
            return null;
        }
    }
}