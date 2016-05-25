using System;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [Serializable]
    public class CalendarSyncSettings : SyncSettings
    {
        private bool _confirmOnDelete;
        private bool _disableDelete;
        private bool _keepLastModifiedVersion;
        private bool _skipPrivateEntries;

        /// <summary>
        /// </summary>
        public bool DisableDelete
        {
            get { return _disableDelete; }
            set { SetProperty(ref _disableDelete, value); }
        }

        public bool ConfirmOnDelete
        {
            get { return _confirmOnDelete; }
            set { SetProperty(ref _confirmOnDelete, value); }
        }

        public bool KeepLastModifiedVersion
        {
            get { return _keepLastModifiedVersion; }
            set { SetProperty(ref _keepLastModifiedVersion, value); }
        }

        public bool SkipPrivateEntries
        {
            get { return _skipPrivateEntries; }
            set { SetProperty(ref _skipPrivateEntries, value); }
        }

        public static CalendarSyncSettings GetDefault()
        {
            return new CalendarSyncSettings
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