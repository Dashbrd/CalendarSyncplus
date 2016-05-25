using System.Collections.Generic;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Domain.Wrappers
{
    public class TasksWrapper : List<ReminderTask>
    {
        public string TaskListId { get; set; }
        public bool IsSuccess { get; set; }
    }
}