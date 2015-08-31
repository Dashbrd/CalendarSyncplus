using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CalendarSyncPlus.Authentication.Google;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Helpers;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.Services.Tasks.Interfaces;
using Google.Apis.Requests;
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
        public string TaskServiceName { get { return "Google"; } }
        private ILog Logger { get; set; }
        public string AccountName { get; set; }
        public string TaskListId { get; set; }
        #region Constructors

        [ImportingConstructor]
        public GoogleTaskService(IAccountAuthenticationService accountAuthenticationService,
            ApplicationLogger applicationLogger)
        {
            AccountAuthenticationService = accountAuthenticationService;
            Logger = applicationLogger.GetLogger(GetType());
        }

        #endregion

        
        public async Task<TasksWrapper> GetReminderTasksInRangeAsync(IDictionary<string, object> taskListSpecificData)
        {
            CheckTaskListSpecificData(taskListSpecificData);

            var finalTaskList = new TasksWrapper();

            //Get Calendar Service
            var tasksService = GetTasksService(AccountName);

            var taskListRequest = tasksService.Tasks.List(TaskListId);
            taskListRequest.MaxResults = 1000;
            try
            {
                var result =   taskListRequest.Execute();
                if (result != null)
                {
                    while (result.Items != null)
                    {
                        // Add events to list, Split recurring appointments
                        foreach (var eventItem in result.Items)
                        {
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
            return finalTaskList;
        }

        private ReminderTask CreateReminderTask(Task eventItem)
        {
            var reminderTask = new ReminderTask(eventItem.Id, eventItem.Title, eventItem.Notes,
                eventItem.Due);
            reminderTask.IsDeleted = eventItem.Deleted;
            reminderTask.UpdatedOn = eventItem.Updated;
            return reminderTask;
        }
        private Task CreaterGoogleTask(ReminderTask reminderTask)
        {
            //Create Event
            var task = new Task
            {
                Title   = reminderTask.Title,
                Notes = reminderTask.Notes,
                Deleted = reminderTask.IsDeleted,
                Due =  reminderTask.Due,
                Status = "needsAction",
                Completed = null,
            };

            if (reminderTask.IsCompleted && reminderTask.CompletedOn != null)
            {                
                task.CompletedRaw = reminderTask.CompletedOn.Value.Rfc339FFormat();
                task.Status = "completed";
            }

            return task;
        }

      
        private const string DictionaryKeyAccountName = "AccountName";
        private const string DictionaryKeyTaskListId = "TaskListId";

        public void CheckTaskListSpecificData(IDictionary<string, object> taskListSpecificData)
        {
            if (taskListSpecificData == null)
            {
                throw new ArgumentNullException("taskListSpecificData", "Calendar Specific Data cannot be null");
            }

            object tasksId;
            if (!taskListSpecificData.TryGetValue(DictionaryKeyTaskListId, out tasksId))
            {
                throw new InvalidOperationException(string.Format("{0} is a required.", DictionaryKeyTaskListId));
            }

            TaskListId = tasksId as string;

            object accountNameValue;
            if (!taskListSpecificData.TryGetValue(DictionaryKeyAccountName, out accountNameValue))
            {
                throw new InvalidOperationException(string.Format("{0} is a required.", DictionaryKeyAccountName));
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


        public async Task<TasksWrapper> DeleteReminderTasks(List<ReminderTask> reminderTasks, IDictionary<string, object> taskListSpecificData)
        {
            var deletedTasks = new TasksWrapper();
            if (!reminderTasks.Any())
            {
                deletedTasks.IsSuccess = true;
                return deletedTasks;
            }

            CheckTaskListSpecificData(taskListSpecificData);

            var errorList = new Dictionary<int, ReminderTask>();
            //Get Calendar Service
            var calendarService = GetTasksService(AccountName);

            if (reminderTasks == null || string.IsNullOrEmpty(TaskListId))
            {
                deletedTasks.IsSuccess = false;
                return deletedTasks;
            }

            try
            {
                if (reminderTasks.Any())
                {
                    //Create a Batch Request
                    var batchRequest = new BatchRequest(calendarService);

                    //Split the list of calendarAppointments by 1000 per list
                    //Iterate over each appointment to create a event and batch it 
                    for (var i = 0; i < reminderTasks.Count; i++)
                    {
                        if (i != 0 && i % 999 == 0)
                        {
                            await batchRequest.ExecuteAsync();
                            batchRequest = new BatchRequest(calendarService);
                        }

                        var appointment = reminderTasks[i];
                        var deleteRequest = calendarService.Tasks.Delete(TaskListId,
                            appointment.TaskId);
                        batchRequest.Queue<Task>(deleteRequest,
                            (content, error, index, message) =>
                                CallbackEventErrorMessage(content, error, index, message, reminderTasks,
                                "Error in deleting events", errorList, deletedTasks,null));
                    }
                    await batchRequest.ExecuteAsync();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                deletedTasks.IsSuccess = false;
                return deletedTasks;
            }
            deletedTasks.IsSuccess = true;
            return deletedTasks;
        }

        public async Task<TasksWrapper> AddReminderTasks(List<ReminderTask> tasks, IDictionary<string, object> taskListSpecificData)
        {
            var addedTasks = new TasksWrapper();
            if (!tasks.Any())
            {
                addedTasks.IsSuccess = true;
                return addedTasks;
            }

            CheckTaskListSpecificData(taskListSpecificData);

            var errorList = new Dictionary<int, ReminderTask>();
            //Get Calendar Service
            var calendarService = GetTasksService(AccountName);

            if (tasks == null || string.IsNullOrEmpty(TaskListId))
            {
                addedTasks.IsSuccess = false;
                return addedTasks;
            }
            try
            {
                if (tasks.Any())
                {
                    
                    //Split the list of calendarAppointments by 1000 per list
                    var appts = await AddTasksInternal(tasks, calendarService, errorList);
                    addedTasks.AddRange(appts);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                addedTasks.IsSuccess = false;
                return addedTasks;
            }
            addedTasks.IsSuccess = addedTasks.Count == tasks.Count;
            return addedTasks;
        }

        private async Task<List<ReminderTask>> AddTasksInternal(List<ReminderTask> reminderTasks,
            TasksService taskService, Dictionary<int, ReminderTask> errorList)
        {
            var addedEvents = new List<ReminderTask>();
            //Create a Batch Request
            var batchRequest = new BatchRequest(taskService);
            
            for (var i = 0; i < reminderTasks.Count; i++)
            {
                if (i != 0 && i % 999 == 0)
                {
                    await batchRequest.ExecuteAsync();
                    batchRequest = new BatchRequest(taskService);
                }

                var reminderTask = reminderTasks[i];
                var googleTask = CreaterGoogleTask(reminderTask);
                var insertRequest = taskService.Tasks.Insert(googleTask,
                    TaskListId);
                batchRequest.Queue<Task>(insertRequest,
                    (content, error, index, message) =>
                        CallbackEventErrorMessage(content, error, index, message,
                        reminderTasks, "Error in adding events", errorList,
                            addedEvents));
            }

            await batchRequest.ExecuteAsync();
            return addedEvents;
        }

        private void CallbackEventErrorMessage(Task content, RequestError error, int index, 
            HttpResponseMessage message, List<ReminderTask> reminderTasks,
            string errorMessage, Dictionary<int, ReminderTask> errorAppointments, 
            List<ReminderTask> addedTasks)
        {
            var phrase = message.ReasonPhrase;
            var googleEvent = reminderTasks[index];
            if (!message.IsSuccessStatusCode)
            {
                errorAppointments.Add(index, googleEvent);
                Logger.ErrorFormat("{0} : {1}{2} - {3}", errorMessage, Environment.NewLine, phrase, googleEvent);
            }
            else if (content != null)
            {
                addedTasks.Add(CreateReminderTask(content));
            }
        }


        public async Task<TasksWrapper> UpdateReminderTasks(List<ReminderTask> reminderTasks,  IDictionary<string, object> taskListSpecificData)
        {
            var updatedAppointments = new TasksWrapper();
            if (!reminderTasks.Any())
            {
                updatedAppointments.IsSuccess = true;
                return updatedAppointments;
            }

            CheckTaskListSpecificData(taskListSpecificData);

            var errorList = new Dictionary<int, ReminderTask>();
            //Get Calendar Service
            var calendarService = GetTasksService(AccountName);

            if (reminderTasks == null || string.IsNullOrEmpty(TaskListId))
            {
                updatedAppointments.IsSuccess = false;
                return updatedAppointments;
            }

            try
            {
                if (reminderTasks.Any())
                {
                    //Create a Batch Request
                    var batchRequest = new BatchRequest(calendarService);

                    //Split the list of calendarAppointments by 1000 per list

                    //Iterate over each appointment to create a event and batch it 
                    for (var i = 0; i < reminderTasks.Count; i++)
                    {
                        if (i != 0 && i % 999 == 0)
                        {
                            await batchRequest.ExecuteAsync();
                            batchRequest = new BatchRequest(calendarService);
                        }

                        var appointment = reminderTasks[i];
                        var task = CreateUpdatedGoogleTask(appointment);
                        var updateRequest = calendarService.Tasks.Update(task,
                            TaskListId, task.Id);
                        batchRequest.Queue<Task>(updateRequest,
                            (content, error, index, message) =>
                                CallbackEventErrorMessage(content, error, index, message, reminderTasks,
                                "Error in updating event", errorList, updatedAppointments));
                    }

                    await batchRequest.ExecuteAsync();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                updatedAppointments.IsSuccess = false;
                return updatedAppointments;
            }
            updatedAppointments.IsSuccess = reminderTasks.Count == updatedAppointments.Count;
            return updatedAppointments;
        }

        private Task CreateUpdatedGoogleTask(ReminderTask reminderTask)
        {
            var task = new Task
            {
                Id = reminderTask.TaskId,
                Title = reminderTask.Title,
                Notes = reminderTask.Notes,
                Due = reminderTask.Due,
                Deleted = reminderTask.IsDeleted,
                Status = "needsAction",
                Completed = null,
            };

            if (reminderTask.IsCompleted && reminderTask.CompletedOn != null)
            {                
                task.CompletedRaw = reminderTask.CompletedOn.Value.Rfc339FFormat();
                task.Status = "completed";
            }
            return task;
        }

        public async Task<bool> ClearCalendar(IDictionary<string, object> taskListSpecificData)
        {
            var appointments =
                await GetReminderTasksInRangeAsync(taskListSpecificData);
            if (appointments != null)
            {
                var success = await DeleteReminderTasks(appointments, taskListSpecificData);
                return success.IsSuccess;
            }
            return false;
        }
    }
}
