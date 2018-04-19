using Newtonsoft.Json;
using System;

namespace CalendarSyncPlus.Domain.Models.Preferences
{    
    public class CalendarSyncSettings : SyncSettings
    {
        [JsonProperty("disableDelete")]
        private bool _disableDelete;
        [JsonProperty("confirmOnDelete")]
        private bool _confirmOnDelete;
        [JsonProperty("keepLastModifiedVersion")]
        private bool _keepLastModifiedVersion;
        [JsonProperty("skipPrivateEntries")]
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
                StartDate = DateTime.Today.AddDays(-(120)),
                EndDate = DateTime.Today.AddDays(120),
                DisableDelete = true
            };
        }
    }
}