using System;
using CalendarSyncPlus.Common.MetaData;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    public class SyncSettings
    {
        /// <summary>
        /// </summary>
        public SyncRangeTypeEnum SyncRangeType { get; set; }

        /// <summary>
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// </summary>
        public int DaysInPast { get; set; }

        /// <summary>
        /// </summary>
        public int DaysInFuture { get; set; }

        public bool DisableDelete { get; set; }
        public bool ConfirmOnDelete { get; set; }
        public bool KeepLastModifiedVersion { get; set; }
        public bool MergeExistingEntries { get; set; }
        
        public static SyncSettings GetDefault()
        {
            return new SyncSettings
            {
                SyncRangeType = SyncRangeTypeEnum.SyncRangeInDays,
                DaysInFuture = 120,
                DaysInPast = 120,
                MergeExistingEntries = true,
                StartDate = DateTime.Today.AddDays(-(120)),
                EndDate = DateTime.Today.AddDays(120),
            };
        }
    }
}