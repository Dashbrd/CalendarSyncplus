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

        
    }
}