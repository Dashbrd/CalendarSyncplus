using System;
using System.Runtime.Serialization;

namespace CalendarSyncPlus.Domain.Models.Preferences
{   
    [DataContract]
    public class ContactSyncSettings : SyncSettings
    {
        public static ContactSyncSettings GetDefault()
        {
            return new ContactSyncSettings
            {
                SyncRangeType = SyncRangeTypeEnum.SyncRangeInDays,
                DaysInFuture = 120,
                DaysInPast = 120,
                StartDate = DateTime.Today.AddDays(-120),
                EndDate = DateTime.Today.AddDays(120)
            };
        }
    }
}