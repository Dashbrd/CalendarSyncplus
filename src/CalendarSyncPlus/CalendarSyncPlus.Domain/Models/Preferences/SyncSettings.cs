using System;
using System.Waf.Foundation;
using CalendarSyncPlus.Common.MetaData;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [Serializable]
    public class SyncSettings : Model
    {
        private SyncRangeTypeEnum _syncRangeType;
        private DateTime _startDate;
        private DateTime _endDate;
        private int _daysInPast;
        private int _daysInFuture;
        private bool _disableDelete;
        private bool _confirmOnDelete;
        private bool _keepLastModifiedVersion;

        /// <summary>
        /// </summary>
        public SyncRangeTypeEnum SyncRangeType
        {
            get { return _syncRangeType; }
            set { SetProperty(ref _syncRangeType, value); }
        }

        /// <summary>
        /// </summary>
        public DateTime StartDate
        {
            get { return _startDate; }
            set { SetProperty(ref _startDate, value); }
        }

        /// <summary>
        /// </summary>
        public DateTime EndDate
        {
            get { return _endDate; }
            set { SetProperty(ref _endDate, value); }
        }

        /// <summary>
        /// </summary>
        public int DaysInPast
        {
            get { return _daysInPast; }
            set { SetProperty(ref _daysInPast, value); }
        }

        /// <summary>
        /// </summary>
        public int DaysInFuture
        {
            get { return _daysInFuture; }
            set { SetProperty(ref _daysInFuture, value); }
        }

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
        
        public static SyncSettings GetDefault()
        {
            return new SyncSettings
            {
                SyncRangeType = SyncRangeTypeEnum.SyncRangeInDays,
                DaysInFuture = 120,
                DaysInPast = 120,
                StartDate = DateTime.Today.AddDays(-(120)),
                EndDate = DateTime.Today.AddDays(120),
            };
        }
    }
}