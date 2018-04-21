using System;
using System.Runtime.Serialization;

namespace CalendarSyncPlus.Domain.Models.Preferences
{    
    [DataContract]
    public class CalendarSyncSettings : SyncSettings
    {
        private bool _disableDelete;
        private bool _confirmOnDelete;
        private bool _keepLastModifiedVersion;
        private bool _skipPrivateEntries;
        [DataMember]
        /// <summary>
        /// </summary>
        public bool DisableDelete
        {
            get { return _disableDelete; }
            set { SetProperty(ref _disableDelete, value); }
        }
        [DataMember]
        public bool ConfirmOnDelete
        {
            get { return _confirmOnDelete; }
            set { SetProperty(ref _confirmOnDelete, value); }
        }
        [DataMember]
        public bool KeepLastModifiedVersion
        {
            get { return _keepLastModifiedVersion; }
            set { SetProperty(ref _keepLastModifiedVersion, value); }
        }
        [DataMember]
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