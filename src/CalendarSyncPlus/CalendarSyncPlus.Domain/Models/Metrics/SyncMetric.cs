using System;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Metrics
{
    public class SyncMetric : Model
    {
        private DateTime _startTime;
        private int _elapsedSeconds;
        private string _syncError;
        private bool _isSuccess;
        private string _profileName;
        private CalendarMetric _sourceMetric;
        private CalendarMetric _destMetric;
        private string _calendarSyncDirection;

        public SyncMetric()
        {
            SourceMetric = new CalendarMetric();
            DestMetric = new CalendarMetric();
        }

        public DateTime StartTime
        {
            get { return _startTime; }
            set { SetProperty(ref _startTime, value); }
        }

        public int ElapsedSeconds
        {
            get { return _elapsedSeconds; }
            set { SetProperty(ref _elapsedSeconds, value); }
        }

        public string SyncError
        {
            get { return _syncError; }
            set { SetProperty(ref _syncError, value); }
        }

        public bool IsSuccess
        {
            get { return _isSuccess; }
            set { SetProperty(ref _isSuccess, value); }
        }

        public string CalendarSyncDirection
        {
            get { return _calendarSyncDirection; }
            set { SetProperty(ref _calendarSyncDirection, value); }
        }

        public string ProfileName
        {
            get { return _profileName; }
            set { SetProperty(ref _profileName, value); }
        }

        public CalendarMetric SourceMetric
        {
            get { return _sourceMetric; }
            set { SetProperty(ref _sourceMetric, value); }
        }

        public CalendarMetric DestMetric
        {
            get { return _destMetric; }
            set { SetProperty(ref _destMetric, value); }
        }
    }
}