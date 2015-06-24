using System;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models
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

    public class CalendarMetric : Model
    {
        private int _originalCount;
        private int _addCount;
        private int _deleteCount;
        private int _updateCount;

        public int OriginalCount
        {
            get { return _originalCount; }
            set { SetProperty(ref _originalCount, value); }
        }

        public int AddCount
        {
            get { return _addCount; }
            set { SetProperty(ref _addCount, value); }
        }

        public int DeleteCount
        {
            get { return _deleteCount; }
            set { SetProperty(ref _deleteCount, value); }
        }

        public int UpdateCount
        {
            get { return _updateCount; }
            set { SetProperty(ref _updateCount, value); }
        }
    }
}