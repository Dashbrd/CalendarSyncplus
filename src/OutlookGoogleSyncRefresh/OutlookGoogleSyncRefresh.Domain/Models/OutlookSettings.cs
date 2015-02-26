using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    public class OutlookSettings
    {
        public OutlookOptionsEnum OutlookOptions { get; set; }

        public string OutlookProfileName { get; set; }

        public OutlookMailBox OutlookMailBox { get; set; }

        public OutlookCalendar OutlookCalendar { get; set; }

    }
}
