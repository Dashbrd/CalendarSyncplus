using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using CalendarSyncPlus.Authentication.Google;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.GoogleServices.Google;
using CalendarSyncPlus.Services.Tasks.Interfaces;
using Google.Apis.Tasks.v1;
using log4net;
using Task = Google.Apis.Tasks.v1.Data.Task;

namespace CalendarSyncPlus.GoogleServices.Tasks
{
    [Export(typeof(ITaskService)), Export(typeof(IGoogleTaskService))]
    [ExportMetadata("ServiceType", ServiceType.Google)]
    public class GoogleTaskService : IGoogleTaskService
    {
        public IAccountAuthenticationService AccountAuthenticationService { get; set; }
        public string ServiceName { get { return "Google"; } }
        private ILog Logger { get; set; }
        public string AccountName { get; set; }
        public string TasksId { get; set; }
        #region Constructors

        [ImportingConstructor]
        public GoogleTaskService(IAccountAuthenticationService accountAuthenticationService,
            ApplicationLogger applicationLogger)
        {
            AccountAuthenticationService = accountAuthenticationService;
            Logger = applicationLogger.GetLogger(GetType());
        }

        #endregion

        
        public async Task<TasksWrapper> GetReminderTasksInRangeAsync(DateTime startDate, DateTime endDate,
            IDictionary<string, object> calendarSpecificData)
        {
            CheckCalendarSpecificData(calendarSpecificData);

            var finalTaskList = new List<ReminderTask>();

            //Get Calendar Service
            var tasksService = GetTasksService(AccountName);

            var taskList = tasksService.Tasklists.List().Execute();

            var taskListRequest = tasksService.Tasks.List(taskList.Items.First().Id);
            taskListRequest.MaxResults = 1000;

            try
            {
                global::Google.Apis.Tasks.v1.Data.Tasks result =   taskListRequest.Execute();
                if (result != null)
                {
                    while (result.Items != null)
                    {
                        // Add events to list, Split recurring appointments
                        foreach (var eventItem in result.Items)
                        {
                            if (eventItem.Status == "cancelled")
                            {
                                continue;
                            }

                            var reminderTask = CreateReminderTask(eventItem);
                            finalTaskList.Add(reminderTask);
                        }

                        //If all pages are over break
                        if (result.NextPageToken == null)
                        {
                            break;
                        }

                        //Set the next page to pull from request
                        taskListRequest.PageToken = result.NextPageToken;

                        result = await taskListRequest.ExecuteAsync();
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
            return null;
        }

        private ReminderTask CreateReminderTask(Task eventItem)
        {
            var reminderTask = new ReminderTask(eventItem.Id, eventItem.Title, eventItem.Notes,
                eventItem.Due);
            return reminderTask;
        }

      
        private const string dictionaryKey_AccountName = "AccountName";
        private const string dictionaryKey_TasksId = "AccountName";

        public void CheckCalendarSpecificData(IDictionary<string, object> calendarSpecificData)
        {
            if (calendarSpecificData == null)
            {
                throw new ArgumentNullException("calendarSpecificData", "Calendar Specific Data cannot be null");
            }

            object tasksId;
            if (!calendarSpecificData.TryGetValue(dictionaryKey_TasksId, out tasksId))
            {
                throw new InvalidOperationException(string.Format("{0} is a required.", dictionaryKey_TasksId));
            }

            TasksId = tasksId as string;

            object accountNameValue;
            if (!calendarSpecificData.TryGetValue(dictionaryKey_AccountName, out accountNameValue))
            {
                throw new InvalidOperationException(string.Format("{0} is a required.", dictionaryKey_AccountName));
            }

            AccountName = accountNameValue as string;

           
        }

        private TasksService GetTasksService(string accountName)
        {
            return AccountAuthenticationService.AuthenticateTasksOauth(accountName);
        }


        public async Task<List<GoogleCalendar>> GetAvailableTaskList(string accountName)
        {
            //Get Calendar Service
            var tasksService = GetTasksService(accountName);

            var taskList = await tasksService.Tasklists.List().ExecuteAsync();

            var localTasksList =
                taskList.Items.Select(
                    taskListEntry =>
                        new GoogleCalendar { Id = taskListEntry.Id, Name = taskListEntry.Title })
                    .ToList();
            return localTasksList;
        }


        public Task<TasksWrapper> DeleteReminderTasks(List<ReminderTask> reminderTasks, IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<TasksWrapper> AddReminderTasks(List<ReminderTask> reminderTasks, bool addDescription, bool addReminder, bool addAttendees, bool attendeesToDescription, IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<TasksWrapper> UpdateReminderTasks(List<ReminderTask> reminderTasks, bool addDescription, bool addReminder, bool addAttendees, bool attendeesToDescription, IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ClearCalendar(IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }
    }
}
