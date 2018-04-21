using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Waf.Foundation;
using System.Xml.Serialization;

namespace CalendarSyncPlus.Domain.Models.Metrics
{
    [DataContract]
    public class SyncSummary : Model
    {
        private List<SyncMetric> _syncMetrics;

        public SyncSummary()
        {
            SyncMetrics = new List<SyncMetric>();
        }

        public int TotalSyncs
        {
            get { return SyncMetrics == null ? 0 : SyncMetrics.Count; }
        }

        public int SuccessSyncs
        {
            get { return SyncMetrics == null ? 0 : SyncMetrics.Count(t => t.IsSuccess); }
        }

        public int FailedSyncs
        {
            get { return SyncMetrics == null ? 0 : SyncMetrics.Count(t => !t.IsSuccess); }
        }

        public long TotalSyncSeconds
        {
            get { return SyncMetrics == null ? 0 : SyncMetrics.Sum(t => t.ElapsedSeconds); }
        }
        [DataMember]
        public List<SyncMetric> SyncMetrics
        {
            get { return _syncMetrics; }
            set { SetProperty(ref _syncMetrics, value); }
        }

        public static SyncSummary GetDefault()
        {
            return new SyncSummary();
        }
    }
}