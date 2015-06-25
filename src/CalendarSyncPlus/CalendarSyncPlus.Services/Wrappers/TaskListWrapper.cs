using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Services.Wrappers
{
    public class TaskListWrapper
    {
        public List<ReminderTask> Tasks { get; set; }
        public bool WaitForApplicationQuit { get; set; }
        public bool Success { get; set; }
    }
}
