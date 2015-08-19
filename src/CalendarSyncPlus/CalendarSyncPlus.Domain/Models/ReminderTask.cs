using System;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models
{

    public enum TaskStatusEnum
    {
        TaskNotStarted,
        TaskInProgress,
        TaskComplete,
        TaskWaiting,
        TaskDeferred,
    }

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

        public DateTime StartDate { get; set; }

        public DateTime? Due { get; set; }

        public DateTime? CompletedOn { get; set; }

        public bool IsCompleted { get; set; }

        public bool? IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public TaskStatusEnum StatusEnum { get; set; }

        public override string ToString()
        {
            return Title + Notes + Due.GetValueOrDefault().ToString("g") + IsCompleted + IsDeleted;
        }

        public override bool Equals(object obj)
        {
            return this.ToString().Equals(obj.ToString());
        }

        public override int GetHashCode()
        { 
            // Returning the hashcode of the Guid used for the ToString() will be 
            // sufficient and would only cause a problem if Appointments objects
            // were stored in a non-generic hash set along side other guid instances
            // which is very unlikely!
            return ToString().GetHashCode();
        }
    }
}
