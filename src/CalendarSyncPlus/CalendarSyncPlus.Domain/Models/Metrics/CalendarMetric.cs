using System.Runtime.Serialization;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Metrics
{
    [DataContract]
    public class CalendarMetric : Model
    {
        private int _addCount;
        private int _addFailedCount;
        private int _deleteCount;
        private int _deleteFailedCount;
        private int _originalCount;
        private int _updateCount;
        private int _updateFailedCount;
        [DataMember]
        public int OriginalCount
        {
            get { return _originalCount; }
            set { SetProperty(ref _originalCount, value); }
        }
        [DataMember]
        public int AddCount
        {
            get { return _addCount; }
            set { SetProperty(ref _addCount, value); }
        }
        [DataMember]
        public int AddFailedCount
        {
            get { return _addFailedCount; }
            set { SetProperty(ref _addFailedCount, value); }
        }
        [DataMember]
        public int DeleteCount
        {
            get { return _deleteCount; }
            set { SetProperty(ref _deleteCount, value); }
        }
        [DataMember]
        public int DeleteFailedCount
        {
            get { return _deleteFailedCount; }
            set { SetProperty(ref _deleteFailedCount, value); }
        }
        [DataMember]
        public int UpdateCount
        {
            get { return _updateCount; }
            set { SetProperty(ref _updateCount, value); }
        }
        [DataMember]
        public int UpdateFailedCount
        {
            get { return _updateFailedCount; }
            set { SetProperty(ref _updateFailedCount, value); }
        }
    }
}