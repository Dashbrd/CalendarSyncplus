using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutlookGoogleSyncRefresh.Common.MetaData;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    public class SyncSettings
    {
        public CalendarSyncDirectionEnum CalendarSyncDirection { get; set; }

        public CalendarServiceType SourceCalendar { get; set; }

        public CalendarServiceType DestinationCalendar { get; set; }

        public SyncModeEnum SyncMode { get; set; }

        public SyncFrequency SyncFrequency { get; set; }
    }
}
