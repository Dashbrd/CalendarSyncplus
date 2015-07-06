using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [Serializable]
    public class ContactsSyncProfile : SyncProfile
    {
        public static ContactsSyncProfile GetDefaultSyncProfile()
        {
            return new ContactsSyncProfile();
        }
    }
}
