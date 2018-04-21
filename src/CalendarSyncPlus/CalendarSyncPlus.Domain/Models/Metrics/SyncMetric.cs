using System;
using System.Runtime.Serialization;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Metrics
{
    [DataContract]
    public class SyncMetric : Model
    {
        private string _calendarSyncDirection;
        private CalendarMetric _destMetric;
        private int _elapsedSeconds;
        private bool _isSuccess;
        private string _profileName;
        private CalendarMetric _sourceMetric;
        private DateTime _startTime;
        private string _syncError;

        public SyncMetric()
        {
            SourceMetric = new CalendarMetric();
            DestMetric = new CalendarMetric();
        }
        [DataMember]
        public DateTime StartTime
        {
            get { return _startTime; }
            set { SetProperty(ref _startTime, value); }
        }
        [DataMember]
        public int ElapsedSeconds
        {
            get { return _elapsedSeconds; }
            set { SetProperty(ref _elapsedSeconds, value); }
        }
        [DataMember]
        public string SyncError
        {
            get { return _syncError; }
            set { SetProperty(ref _syncError, value); }
        }
        [DataMember]
        public bool IsSuccess
        {
            get { return _isSuccess; }
            set { SetProperty(ref _isSuccess, value); }
        }
        [DataMember]
        public string CalendarSyncDirection
        {
            get { return _calendarSyncDirection; }
            set { SetProperty(ref _calendarSyncDirection, value); }
        }
        [DataMember]
        public string ProfileName
        {
            get { return _profileName; }
            set { SetProperty(ref _profileName, value); }
        }
        [DataMember]
        public CalendarMetric SourceMetric
        {
            get { return _sourceMetric; }
            set { SetProperty(ref _sourceMetric, value); }
        }
        [DataMember]
        public CalendarMetric DestMetric
        {
            get { return _destMetric; }
            set { SetProperty(ref _destMetric, value); }
        }
    }
}