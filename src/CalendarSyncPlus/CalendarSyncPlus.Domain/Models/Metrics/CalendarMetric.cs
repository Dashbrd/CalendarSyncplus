using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Metrics
{
    public class CalendarMetric : Model
    {
        private int _originalCount;
        private int _addCount;
        private int _deleteCount;
        private int _updateCount;
        private int _updateFailedCount;
        private int _deleteFailedCount;
        private int _addFailedCount;

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

        public int AddFailedCount
        {
            get { return _addFailedCount; }
            set { SetProperty(ref _addFailedCount, value); }
        }

        public int DeleteCount
        {
            get { return _deleteCount; }
            set { SetProperty(ref _deleteCount, value); }
        }

        public int DeleteFailedCount
        {
            get { return _deleteFailedCount; }
            set { SetProperty(ref _deleteFailedCount, value); }
        }

        public int UpdateCount
        {
            get { return _updateCount; }
            set { SetProperty(ref _updateCount, value); }
        }

        public int UpdateFailedCount
        {
            get { return _updateFailedCount; }
            set { SetProperty(ref _updateFailedCount, value); }
        }
    }
}
