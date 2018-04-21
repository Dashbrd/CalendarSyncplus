using System;
using System.Runtime.Serialization;

namespace CalendarSyncPlus.Domain.Models.Preferences
{   
    [DataContract]
    public class TaskSyncSettings : SyncSettings
    {
        public static TaskSyncSettings GetDefault()
        {
            return new TaskSyncSettings
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