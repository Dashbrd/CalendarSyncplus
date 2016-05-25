using System;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [Serializable]
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