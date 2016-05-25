using System.Collections.Generic;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Wrappers;

namespace CalendarSyncPlus.Services.Tasks.Interfaces
{
    public interface ITaskService
    {
        string TaskServiceName { get; }

        Task<TasksWrapper> DeleteReminderTasks(List<ReminderTask> reminderTasks,
            IDictionary<string, object> taskListSpecificData);

        Task<TasksWrapper> GetReminderTasksInRangeAsync(IDictionary<string, object> taskListSpecificData);

        Task<TasksWrapper> AddReminderTasks(List<ReminderTask> tasks,
            IDictionary<string, object> taskListSpecificData);

        void CheckTaskListSpecificData(IDictionary<string, object> taskListSpecificData);

        Task<TasksWrapper> UpdateReminderTasks(List<ReminderTask> reminderTasks,
            IDictionary<string, object> taskListSpecificData);

        //Task<bool> ResetReminderTasks(IDictionary<string, object> calendarSpecificData);

        Task<bool> ClearCalendar(IDictionary<string, object> taskListSpecificData);
    }
}