using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Domain.Wrappers
{
    public class TasksWrapper : List<ReminderTask>
    {
        public string TaskListId { get; set; }
        public bool IsSuccess { get; set; }
    }
}
