using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models
{
    public class SyncSummary : Model
    {
        public int TotalSyncs { get; set; }

        public int CompletedSyncs { get; set; }

        public int FailedSyncs { get; set; }

        public static SyncSummary GetDefault()
        {
            return new SyncSummary();
        }
    }
}
