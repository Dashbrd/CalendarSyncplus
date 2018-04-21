using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [DataContract]
    [KnownType(typeof(CalendarSyncSettings))]
    [KnownType(typeof(TaskSyncSettings))]
    [KnownType(typeof(ContactSyncSettings))]
    public class SyncSettings : Model
    {
        private int _daysInFuture;
        private int _daysInPast;
        private DateTime _endDate;
        private DateTime _startDate;
        private SyncRangeTypeEnum _syncRangeType;

        [DataMember]
        /// <summary>
        /// </summary>
        public SyncRangeTypeEnum SyncRangeType
        {
            get { return _syncRangeType; }
            set { SetProperty(ref _syncRangeType, value); }
        }
        [DataMember]
        /// <summary>
        /// </summary>
        public DateTime StartDate
        {
            get { return _startDate; }
            set { SetProperty(ref _startDate, value); }
        }
        [DataMember]
        /// <summary>
        /// </summary>
        public DateTime EndDate
        {
            get { return _endDate; }
            set { SetProperty(ref _endDate, value); }
        }
        [DataMember]
        /// <summary>
        /// </summary>
        public int DaysInPast
        {
            get { return _daysInPast; }
            set { SetProperty(ref _daysInPast, value); }
        }
        [DataMember]
        /// <summary>
        /// </summary>
        public int DaysInFuture
        {
            get { return _daysInFuture; }
            set { SetProperty(ref _daysInFuture, value); }
        }
    }
}