using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models
{
    public class ReminderTask : Model
    {
        public ReminderTask(string id, string title, string notes, DateTime? due)
        {
            TaskId = id;
            Title = title;
            Notes = notes;
            Due = due;
        }

        public string TaskId { get; set; }

        public string Title { get; set; }

        public string Notes { get; set; }

        public DateTime? Due { get; set; }

        public DateTime? Completed { get; set; }

        public bool? Deleted { get; set; }

        public override string ToString()
        {
            return Title + Notes + Completed + Deleted;
        }

        public bool Equal(ReminderTask reminderTask)
        {
            return this.ToString().Equals(reminderTask.ToString());
        }

    }
}
