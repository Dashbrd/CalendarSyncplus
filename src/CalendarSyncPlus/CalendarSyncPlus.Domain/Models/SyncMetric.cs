using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models
{
    public class SyncMetric : Model
    {
        public SyncMetric()
        {
            SourceAddErrors = new List<string>();
            SourceDeleteErrors = new List<string>();
            SourceUpdateErrors = new List<string>();

            DestAddErrors = new List<string>();
            DestDeleteErrors = new List<string>();
            DestUpdateErrors = new List<string>();
        }

        public int ElapsedSeconds { get; set; }

        public string SyncError { get; set; }

        public bool IsSuccess { get; set; }

        public CalendarSyncDirectionEnum CalendarSyncDirection { get; set; }

        public int SourceAppointmentCount { get; set; }

        public int SourceAddCount { get; set; }

        public int SourceAddErrorCount { get; set; }

        public List<string> SourceAddErrors { get; set; }

        public int SourceDeleteCount { get; set; }

        public int SourceDeleteErrorCount { get; set; }

        public List<string> SourceDeleteErrors { get; set; }

        public int SourceUpdateCount { get; set; }

        public int SourceUpdateErrorCount { get; set; }

        public List<string> SourceUpdateErrors { get; set; }

        public int DestAppointmentCount { get; set; }

        public int DestAddCount { get; set; }

        public int DestAddErrorCount { get; set; }

        public List<string> DestAddErrors { get; set; }

        public int DestDeleteCount { get; set; }

        public int DestDeleteErrorCount { get; set; }

        public List<string> DestDeleteErrors { get; set; }

        public int DestUpdateCount { get; set; }

        public int DestUpdateErrorCount { get; set; }

        public List<string> DestUpdateErrors { get; set; }

    }
}
