using System.Waf.Foundation;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;

namespace CalendarSyncPlus.Application.ViewModels
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