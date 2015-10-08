using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.OutlookServices.Utilities;
using CalendarSyncPlus.OutlookServices.Wrappers;
using CalendarSyncPlus.Services.Tasks.Interfaces;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Win32;
using Exception = System.Exception;
using ThreadingTask = System.Threading.Tasks.Task;
namespace CalendarSyncPlus.OutlookServices.Task
{
    [Export(typeof(ITaskService)), Export(typeof(IOutlookTaskService))]
    [ExportMetadata("ServiceType", ServiceType.OutlookDesktop)]
    public class OutlookTaskService : IOutlookTaskService
    {

        [ImportingConstructor]
        public OutlookTaskService(ApplicationLogger applicationLogger)
        {
            Logger = applicationLogger.GetLogger(GetType());
        }

        public string TaskServiceName
        {
            get { return "Outlook"; }
        }

        public ILog Logger { get; set; }

        private OutlookFolder OutlookTaskList { get; set; }
        
        private string ProfileName { get; set; }

        private List<string> GetOutlookProfileList()
        {
            var profileList = new List<string>();
            const string defaultProfilePath =
                @"Software\Microsoft\Windows NT\CurrentVersion\Windows Messaging Subsystem\Profiles";
            const string newProfilePath = @"Software\Microsoft\Office\15.0\Outlook\Profiles";

            var defaultRegKey = Registry.CurrentUser.OpenSubKey(defaultProfilePath,
                RegistryKeyPermissionCheck.Default);

            if (defaultRegKey != null)
            {
                var list = defaultRegKey.GetSubKeyNames();

                if (list.Any())
                {
                    profileList.AddRange(list);
                }
            }

            var newregKey = Registry.CurrentUser.OpenSubKey(newProfilePath, RegistryKeyPermissionCheck.Default);

            if (newregKey != null)
            {
                var list = newregKey.GetSubKeyNames();

                if (list.Any())
                {
                    foreach (var name in list.Where(name => !profileList.Contains(name)))
                    {
                        profileList.Add(name);
                    }
                }
            }

            return profileList;
        }
        private List<OutlookMailBox> GetOutlookMailBoxes(Folders rootFolders)
        {
            var mailBoxes = new List<OutlookMailBox>();
            if (rootFolders.Count > 0)
            {
                foreach (Folder rootFolder in rootFolders)
                {
                    var mailBoxName = rootFolder.Name;

                    //All mailBoxes Scanned Leave Public calendars and Folders
                    if (mailBoxName.Contains("Public Folders"))
                    {
                        Marshal.FinalReleaseComObject(rootFolder);
                        continue;
                    }

                    var mailBox = new OutlookMailBox
                    {
                        Name = mailBoxName,
                        EntryId = rootFolder.EntryID,
                        StoreId = rootFolder.StoreID
                    };
                    mailBoxes.Add(mailBox);

                    GetTaskLists(rootFolder, mailBox.Folders);

                    Marshal.FinalReleaseComObject(rootFolder);
                }
            }
            return mailBoxes;
        }
        private void GetTaskLists(MAPIFolder searchFolder, List<OutlookFolder> outlookCalendars)
        {
            try
            {
                if (searchFolder == null)
                {
                    return;
                }

                if (searchFolder.DefaultMessageClass == "IPM.Task" &&
                    searchFolder.DefaultItemType == OlItemType.olTaskItem)
                {
                    //Add Calendar MAPIFolder to List
                    outlookCalendars.Add(new OutlookFolder
                    {
                        Name = searchFolder.Name,
                        EntryId = searchFolder.EntryID,
                        StoreId = searchFolder.EntryID
                    });
                }

                if (searchFolder.Folders != null && searchFolder.Folders.Count > 0)
                {
                    //Walk through all subFolders in MAPIFolder
                    foreach (MAPIFolder subFolder in searchFolder.Folders)
                    {
                        //Get Calendar MAPIFolders
                        GetTaskLists(subFolder, outlookCalendars);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
            finally
            {
                if (searchFolder != null)
                {
                    Marshal.FinalReleaseComObject(searchFolder);
                }
            }
        }

        private void GetOutlookApplication(out bool disposeOutlookInstances,
          out Application application,
          out NameSpace nameSpace, string profileName)
        {
            // Check whether there is an Outlook process running.
            if (Process.GetProcessesByName("OUTLOOK").Any())
            {
                // If so, use the GetActiveObject method to obtain the process and cast it to an Application object.
                application =
                    Marshal.GetActiveObject("Outlook.Application") as Application;
                disposeOutlookInstances = false;
                nameSpace = null;
                if (application != null)
                {
                    nameSpace = application.GetNamespace("MAPI");
                    if (!string.IsNullOrEmpty(profileName) && !nameSpace.CurrentProfileName.Equals(profileName))
                    {
                        throw new InvalidOperationException(
                            $"Current Outlook instance is opened with a Different Profile Name ({nameSpace.CurrentProfileName}).{Environment.NewLine}Close Outlook and try again.");
                    }
                }
            }
            else
            {
                // If not, create a new instance of Outlook and log on to the default profile.
                application = new Application();
                nameSpace = application.GetNamespace("MAPI");
                nameSpace.Logon(profileName, "", false, true);
                disposeOutlookInstances = true;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="taskItem"></param>
        /// <returns>
        /// </returns>
        private ReminderTask GetTaskFromItem(string id, TaskItem taskItem)
        {
            var reminderTask = new ReminderTask(taskItem.EntryID,taskItem.Subject,taskItem.Body, taskItem.DueDate);
            reminderTask.IsCompleted = taskItem.Complete;
            reminderTask.CreatedOn = taskItem.CreationTime;
            reminderTask.UpdatedOn = taskItem.LastModificationTime;
            reminderTask.CompletedOn = taskItem.DateCompleted;
            reminderTask.StartDate = taskItem.StartDate;
            reminderTask.StatusEnum = taskItem.GetTaskStatus();
            return reminderTask;
        }

        private List<ReminderTask> GetTasks()
        {
            var list = GetOutlookEntriesForSelectedTimeRange();
            if (!list.WaitForApplicationQuit)
            {
                return list.Tasks;
            }
            while (Process.GetProcessesByName("OUTLOOK").Any())
            {
                ThreadingTask.Delay(5000);
            }
            return list.Tasks;
        }

        private OutlookTasksWrapper GetOutlookEntriesForSelectedTimeRange()
        {
            var disposeOutlookInstances = false;
            Application application = null;
            NameSpace nameSpace = null;
            MAPIFolder defaultOutlookCalendar = null;
            Items outlookItems = null;
            var outlookTasks = new List<ReminderTask>();

            //Close  and Shutdown
            try
            {
                // Get Application and Namespace
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);

                // Get Default Calendar
                defaultOutlookCalendar = OutlookTaskList != null
                    ? nameSpace.GetFolderFromID(OutlookTaskList.EntryId, OutlookTaskList.StoreId)
                    : nameSpace.GetDefaultFolder(OlDefaultFolders.olFolderTasks);
                if (OutlookTaskList == null)
                {
                    OutlookTaskList = new OutlookFolder
                    {
                        Name = defaultOutlookCalendar.Name,
                        EntryId = defaultOutlookCalendar.EntryID,
                        StoreId = defaultOutlookCalendar.StoreID
                    };
                }
                // Get outlook Items
                outlookItems = defaultOutlookCalendar.Items;

                if (outlookItems != null)
                {
                    var items = outlookItems.Cast<TaskItem>();
                    var taskItems = items as TaskItem[] ?? items.ToArray();
                    if (taskItems.Any())
                    {
                        var id = defaultOutlookCalendar.EntryID;
                        foreach (var appointmentItem in taskItems)
                        {
                            try
                            {
                                var app = GetTaskFromItem(id, appointmentItem);
                                outlookTasks.Add(app);
                            }
                            catch (Exception exception)
                            {
                                Logger.Error(exception);
                            }
                            finally
                            {
                                Marshal.FinalReleaseComObject(appointmentItem);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return new OutlookTasksWrapper
                {
                    Tasks = null,
                    WaitForApplicationQuit = disposeOutlookInstances
                };
            }
            finally
            {
                if (disposeOutlookInstances)
                {
                    if (nameSpace != null)
                    {
                        nameSpace.Logoff();
                    }
                }

                //Unassign all instances
                if (outlookItems != null)
                {
                    Marshal.FinalReleaseComObject(outlookItems);
                    outlookItems = null;
                }

                if (defaultOutlookCalendar != null)
                {
                    Marshal.FinalReleaseComObject(defaultOutlookCalendar);
                    defaultOutlookCalendar = null;
                }

                if (nameSpace != null)
                {
                    Marshal.FinalReleaseComObject(nameSpace);
                    nameSpace = null;
                }

                if (disposeOutlookInstances)
                {
                    // Casting Removes a warninig for Ambigous Call
                    application.Quit();
                    Marshal.FinalReleaseComObject(application);
                }
                application = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            return new OutlookTasksWrapper
            {
                Tasks = outlookTasks,
                WaitForApplicationQuit = disposeOutlookInstances
            };
        }

        /// <exception cref="InvalidOperationException">
        ///     Essential parameters are not present.
        /// </exception>
        public void CheckTaskListSpecificData(IDictionary<string, object> taskListSpecificData)
        {
            if (taskListSpecificData == null)
            {
                throw new ArgumentNullException("taskListSpecificData", "Calendar Specific Data cannot be null");
            }

            object profileValue;
            object outlookCalendarValue;
            if (!(taskListSpecificData.TryGetValue("ProfileName", out profileValue) &&
                  taskListSpecificData.TryGetValue("OutlookTaskList", out outlookCalendarValue)))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "{0} and {1} keys should be present, both of them can be null in case Default Profile and Default Calendar will be used. {0} is of 'string' type, {1} is of 'OutlookCalendar' type and {2} is of bool type.",
                        "ProfileName", "OutlookCalendar"));
            }
            ProfileName = profileValue as String;
            OutlookTaskList = outlookCalendarValue as OutlookFolder;
        }


       

        public async Task<List<string>> GetOutLookProfieListAsync()
        {
            return await Task<List<string>>.Factory.StartNew(GetOutlookProfileList);
        }


        public async Task<bool> ClearCalendar(IDictionary<string, object> taskListSpecificData)
        {
            var startDate = DateTime.Today.AddDays(-(10 * 365));
            var endDate = DateTime.Today.AddDays(10 * 365);
            var appointments =
                await GetReminderTasksInRangeAsync(taskListSpecificData);
            if (appointments != null)
            {
                var success = await DeleteReminderTasks(appointments, taskListSpecificData);
                return success.IsSuccess;
            }
            return false;
        }

        public List<OutlookMailBox> GetAllMailBoxes(string profileName = "")
        {
            ProfileName = profileName;
            var disposeOutlookInstances = false;
            Application application = null;
            NameSpace nameSpace = null;
            Folders rootFolders = null;
            var mailBoxes = new List<OutlookMailBox>();


            try
            {
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);
                rootFolders = nameSpace.Folders;
                mailBoxes = GetOutlookMailBoxes(rootFolders);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
            finally
            {
                //Close  and Shutdown
                //Unassign all instances
                if (rootFolders != null)
                {
                    Marshal.FinalReleaseComObject(rootFolders);
                }

                if (disposeOutlookInstances)
                {
                    nameSpace.Logoff();
                }

                if (nameSpace != null)
                {
                    Marshal.FinalReleaseComObject(nameSpace);
                }

                if (disposeOutlookInstances)
                {
                    // Casting Removes a warninig for Ambigous Call
                    application.Quit();
                    Marshal.FinalReleaseComObject(application);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();

                while (disposeOutlookInstances && Process.GetProcessesByName("OUTLOOK").Any())
                {
                    ThreadingTask.Delay(5000);
                }
            }

            return mailBoxes;
        }



        public async Task<TasksWrapper> DeleteReminderTasks(List<ReminderTask> reminderTasks, IDictionary<string, object> taskListSpecificData)
        {
            var tasksWrapper = new TasksWrapper();
            if (!reminderTasks.Any())
            {
                tasksWrapper.IsSuccess = true;
                return tasksWrapper;
            }
            CheckTaskListSpecificData(taskListSpecificData);
            var result = await
                Task<bool>.Factory.StartNew(() =>
                    DeleteTasks(reminderTasks, tasksWrapper));

            tasksWrapper.IsSuccess = result;
            return tasksWrapper;
        }

        private bool DeleteTasks(List<ReminderTask> reminderTasks, List<ReminderTask> deletedTasks)
        {
            OutlookTasksWrapper wrapper = DeleteTasksFromOutlook(reminderTasks, deletedTasks);

            if (!wrapper.WaitForApplicationQuit)
            {
                return wrapper.Success;
            }

            while (Process.GetProcessesByName("OUTLOOK").Any())
            {
                ThreadingTask.Delay(5000);
            }
            return wrapper.Success;
        }

        private OutlookTasksWrapper DeleteTasksFromOutlook(List<ReminderTask> reminderTasks,
            List<ReminderTask> deletedTasks)
        {
            var disposeOutlookInstances = false;
            Application application = null;
            NameSpace nameSpace = null;

            try
            {
                // Get Application and Namespace
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);


                foreach (var calendarAppointment in reminderTasks)
                {
                    try
                    {
                        TaskItem taskItem = null;
                        taskItem = nameSpace.GetItemFromID(calendarAppointment.TaskId) as TaskItem;
                       
                        if (taskItem != null)
                        {
                            taskItem.Delete();
                            Marshal.FinalReleaseComObject(taskItem);
                            deletedTasks.Add(calendarAppointment);
                        }
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return new OutlookTasksWrapper
                {
                    WaitForApplicationQuit = disposeOutlookInstances,
                    Success = false
                };
            }
            finally
            {
                //Close  and Shutdown
                if (disposeOutlookInstances)
                {
                    nameSpace.Logoff();
                }

                Marshal.FinalReleaseComObject(nameSpace);
                nameSpace = null;

                if (disposeOutlookInstances)
                {
                    // Casting Removes a warninig for Ambigous Call
                    application.Quit();
                    Marshal.FinalReleaseComObject(application);
                }
                application = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return new OutlookTasksWrapper
            {
                WaitForApplicationQuit = disposeOutlookInstances,
                Success = true
            };
        }

        public  async Task<TasksWrapper> GetReminderTasksInRangeAsync(IDictionary<string, object> taskListSpecificData)
        {
            CheckTaskListSpecificData(taskListSpecificData);
            var taskWrapper = new TasksWrapper();

            var appointmentList =
                await
                    Task<List<ReminderTask>>.Factory.StartNew(
                        GetTasks);

            if (appointmentList == null)
            {
                return null;
            }

            taskWrapper.AddRange(appointmentList);

            return taskWrapper;
        }

        public async Task<TasksWrapper> AddReminderTasks(List<ReminderTask> reminderTasks, IDictionary<string, object> taskListSpecificData)
        {
            var tasksWrapper = new TasksWrapper();
            if (!reminderTasks.Any())
            {
                tasksWrapper.IsSuccess = true;
                return tasksWrapper;
            }
            CheckTaskListSpecificData(taskListSpecificData);

            var result = await Task<bool>.Factory.StartNew(() =>
                        AddTasks(reminderTasks,tasksWrapper));

            tasksWrapper.IsSuccess = result;
            return tasksWrapper;
        }

        private bool AddTasks(List<ReminderTask> reminderTasks, TasksWrapper tasksWrapper)
        {
            OutlookTasksWrapper wrapper = AddTasksToOutlook(reminderTasks, tasksWrapper);

            if (!wrapper.WaitForApplicationQuit)
            {
                return wrapper.Success;
            }

            while (Process.GetProcessesByName("OUTLOOK").Any())
            {
                ThreadingTask.Delay(5000);
            }
            return wrapper.Success;
        }

        private OutlookTasksWrapper AddTasksToOutlook(List<ReminderTask> reminderTasks, TasksWrapper tasksWrapper)
        {
            var disposeOutlookInstances = false;
            Application application = null;
            NameSpace nameSpace = null;
            MAPIFolder defaultOutlookFolder = null;
            Items outlookItems = null;

            try
            {
                // Get Application and Namespace
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);

                // Get Default Calendar
                defaultOutlookFolder = OutlookTaskList != null
                    ? nameSpace.GetFolderFromID(OutlookTaskList.EntryId, OutlookTaskList.StoreId)
                    : nameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
                outlookItems = defaultOutlookFolder.Items;

                string id = defaultOutlookFolder.EntryID;
                foreach (var reminderTask in reminderTasks)
                {
                    var taskItem = outlookItems.Add(OlItemType.olAppointmentItem) as TaskItem;
                    if (taskItem == null)
                    {
                        continue;
                    }
                    UpdateTask(taskItem, reminderTask);

                    var addedTask = GetTaskFromItem(id, taskItem);
                    tasksWrapper.Add(addedTask);
                    Marshal.FinalReleaseComObject(taskItem);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return new OutlookTasksWrapper()
                {
                    WaitForApplicationQuit = disposeOutlookInstances,
                    Success = false
                };
            }
            finally
            {
                //Close  and Shutdown

                if (disposeOutlookInstances)
                {
                    nameSpace.Logoff();
                }

                //Unassign all instances
                if (outlookItems != null)
                {
                    Marshal.FinalReleaseComObject(outlookItems);
                    outlookItems = null;
                }

                Marshal.FinalReleaseComObject(defaultOutlookFolder);
                defaultOutlookFolder = null;

                Marshal.FinalReleaseComObject(nameSpace);
                nameSpace = null;

                if (disposeOutlookInstances)
                {
                    // Casting Removes a warninig for Ambigous Call
                    application.Quit();
                    Marshal.FinalReleaseComObject(application);
                }
                application = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return new OutlookTasksWrapper
            {
                WaitForApplicationQuit = disposeOutlookInstances,
                Success = true
            };
        }
        
        public async Task<TasksWrapper> UpdateReminderTasks(List<ReminderTask> reminderTasks,  IDictionary<string, object> taskListSpecificData)
        {
            var tasksWrapper = new TasksWrapper();
            if (!reminderTasks.Any())
            {
                tasksWrapper.IsSuccess = true;
                return tasksWrapper;
            }
            CheckTaskListSpecificData(taskListSpecificData);

            var result = await
                Task<bool>.Factory.StartNew(() =>
                        UpdateTasks(reminderTasks, tasksWrapper));
            tasksWrapper.IsSuccess = result;
            return tasksWrapper;
        }

        private bool UpdateTasks(List<ReminderTask> reminderTasks, TasksWrapper tasksWrapper)
        {
            OutlookTasksWrapper wrapper = UpdateTasksToOutlook(reminderTasks, tasksWrapper);

            if (!wrapper.WaitForApplicationQuit)
            {
                return wrapper.Success;
            }

            while (Process.GetProcessesByName("OUTLOOK").Any())
            {
                ThreadingTask.Delay(5000);
            }
            return wrapper.Success;
        }

        private OutlookTasksWrapper UpdateTasksToOutlook(List<ReminderTask> reminderTasks, TasksWrapper tasksWrapper)
        {
            var disposeOutlookInstances = false;
            Application application = null;
            NameSpace nameSpace = null;
            MAPIFolder defaultOutlookCalendar = null;
            Items outlookItems = null;

            try
            {
                // Get Application and Namespace
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);

                // Get Default Calendar
                defaultOutlookCalendar = OutlookTaskList != null
                    ? nameSpace.GetFolderFromID(OutlookTaskList.EntryId, OutlookTaskList.StoreId)
                    : nameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
                outlookItems = defaultOutlookCalendar.Items;


                foreach (var reminderTask in reminderTasks)
                {
                    try
                    {
                        TaskItem appItem = null;
                        
                        appItem = nameSpace.GetItemFromID(reminderTask.TaskId) as TaskItem;
                       
                        var success = UpdateTask(appItem,
                            reminderTask);
                        if (success)
                        {
                            tasksWrapper.Add(reminderTask);
                        }
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return new OutlookTasksWrapper
                {
                    WaitForApplicationQuit = disposeOutlookInstances,
                    Success = false
                };
            }
            finally
            {
                //Close  and Shutdown

                if (disposeOutlookInstances)
                {
                    nameSpace.Logoff();
                }

                //Unassign all instances
                if (outlookItems != null)
                {
                    Marshal.FinalReleaseComObject(outlookItems);
                    outlookItems = null;
                }

                Marshal.FinalReleaseComObject(defaultOutlookCalendar);
                defaultOutlookCalendar = null;

                Marshal.FinalReleaseComObject(nameSpace);
                nameSpace = null;

                if (disposeOutlookInstances)
                {
                    // Casting Removes a warninig for Ambigous Call
                    application.Quit();
                    Marshal.FinalReleaseComObject(application);
                }
                application = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return new OutlookTasksWrapper()
            {
                WaitForApplicationQuit = disposeOutlookInstances,
                Success = true
            };
        }

        private bool UpdateTask(TaskItem taskItem, ReminderTask reminderTask)
        {
            try
            {
                taskItem.Subject = reminderTask.Title;
                taskItem.Body = reminderTask.Notes;
                taskItem.DueDate = reminderTask.Due.GetValueOrDefault();
                taskItem.StartDate = reminderTask.StartDate;
                taskItem.Complete = reminderTask.IsCompleted;
                if (reminderTask.IsCompleted)
                {
                    taskItem.DateCompleted = reminderTask.CompletedOn.GetValueOrDefault();
                }
                
                taskItem.Status = reminderTask.GetOlTaskStatus();
                taskItem.Save();
                return true;
            } 
            catch (Exception exception)
            {
                Logger.Error(exception);
                return false;
            }
            
        }
    }
}
