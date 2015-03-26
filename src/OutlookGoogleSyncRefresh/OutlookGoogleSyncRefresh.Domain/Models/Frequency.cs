using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    public class Frequency
    {
        public FrequencyTypeEnum FrequencyType { get; set; }

        public DateTime StartDate { get; set; }

        public List<DayOfWeek> DaysOfWeek { get; set; }

        public int RepeatGap { get; set; }

        public int Count { get; set; }

        public DateTime EndDate { get; set; }
    }
}
