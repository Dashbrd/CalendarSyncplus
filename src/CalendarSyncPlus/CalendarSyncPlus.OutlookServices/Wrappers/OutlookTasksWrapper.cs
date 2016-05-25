using System.Collections.Generic;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.OutlookServices.Wrappers
{
    public class OutlookTasksWrapper
    {
        public List<ReminderTask> Tasks { get; set; }
        public bool WaitForApplicationQuit { get; set; }
        public bool Success { get; set; }
    }
}