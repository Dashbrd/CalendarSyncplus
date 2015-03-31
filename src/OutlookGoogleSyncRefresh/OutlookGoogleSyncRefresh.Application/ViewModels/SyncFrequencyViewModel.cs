using System.Waf.Foundation;
using CalendarSyncPlus.Domain.Models;

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