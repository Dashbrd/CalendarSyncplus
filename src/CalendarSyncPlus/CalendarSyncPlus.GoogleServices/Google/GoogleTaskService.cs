using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;
using CalendarSyncPlus.Authentication.Google;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.Services.Tasks.Interfaces;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using log4net;
using Task = Google.Apis.Tasks.v1.Data.Task;

namespace CalendarSyncPlus.GoogleServices.Google
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

        

        //public Task<TaskWrapper> DeleteReminderTasks(List<Appointment> reminderTasks,
        //    IDictionary<string, object> calendarSpecificData)
        //{
            
        //}

        public async Task<TaskWrapper> GetReminderTasksInRangeAsync(DateTime startDate, DateTime endDate,
            IDictionary<string, object> calendarSpecificData)
        {
            CheckCalendarSpecificData(calendarSpecificData);

            var finalTaskList = new List<ReminderTask>();

            //Get Calendar Service
            var tasksService = GetTasksService(AccountName);

            var taskListRequest = tasksService.Tasks.List(TasksId);
            taskListRequest.MaxResults = 1000;

            try
            {
                Tasks result =   taskListRequest.Execute();
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

        //public Task<TaskWrapper> AddReminderTasks(List<ReminderTask> reminderTasks, bool addDescription,
        //    bool addReminder, bool addAttendees, bool attendeesToDescription,
        //    IDictionary<string, object> calendarSpecificData) { }
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

        //public Task<TaskWrapper> UpdateReminderTasks(List<ReminderTask> reminderTasks, bool addDescription,
        //    bool addReminder, bool addAttendees, bool attendeesToDescription,
        //    IDictionary<string, object> calendarSpecificData) { }

        //public Task<bool> ResetReminderTasks(IDictionary<string, object> calendarSpecificData) { }

        private TasksService GetTasksService(string accountName)
        {
            return AccountAuthenticationService.AuthenticateTasksOauth(accountName);
        }
    }
}
