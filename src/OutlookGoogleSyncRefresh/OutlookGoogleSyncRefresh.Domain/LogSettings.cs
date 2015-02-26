using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OutlookGoogleSyncRefresh.Domain
{
    public class LogSettings
    {
        public bool LogSyncInfo { get; set; }

        public bool CreateNewFileForEverySync { get; set; }
    }
}
