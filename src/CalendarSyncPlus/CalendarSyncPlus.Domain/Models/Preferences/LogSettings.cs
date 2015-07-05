using System;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [Serializable]
    public class LogSettings : Model
    {
        public bool LogSyncInfo { get; set; }
        public bool CreateNewFileForEverySync { get; set; }
    }
}