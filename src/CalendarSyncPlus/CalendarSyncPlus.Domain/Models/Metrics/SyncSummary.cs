using System.Collections.Generic;
using System.Linq;
using System.Waf.Foundation;
using System.Xml.Serialization;

namespace CalendarSyncPlus.Domain.Models.Metrics
{
    public class SyncSummary : Model
    {
        private List<SyncMetric> _syncMetrics;

        public SyncSummary()
        {
            SyncMetrics = new List<SyncMetric>();
        }
        [XmlIgnore]
        public int TotalSyncs
        {
            get { return SyncMetrics == null ? 0 : SyncMetrics.Count; }

        }
        [XmlIgnore]
        public int SuccessSyncs
        {
            get { return SyncMetrics == null ? 0 : SyncMetrics.Count(t => t.IsSuccess); }

        }
        [XmlIgnore]
        public int FailedSyncs
        {
            get { return SyncMetrics == null ? 0 : SyncMetrics.Count(t => !t.IsSuccess); }

        }
        [XmlIgnore]
        public long TotalSyncSeconds
        {
            get { return SyncMetrics == null ? 0 : SyncMetrics.Sum(t => t.ElapsedSeconds); }
        }

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