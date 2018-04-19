using Newtonsoft.Json;
using System;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    
    public class SyncSettings : Model
    {
        [JsonProperty("daysInFuture")]
        private int _daysInFuture;
        [JsonProperty("daysInPast")]
        private int _daysInPast;
        [JsonProperty("endDate")]
        private DateTime _endDate;
        [JsonProperty("startDate")]
        private DateTime _startDate;
        [JsonProperty("syncRangeType")]
        private SyncRangeTypeEnum _syncRangeType;


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