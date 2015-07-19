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

        public ILog Logger { get; set; }

        private OutlookFolder OutlookCalendar { get; set; }
        
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

                    GetCalendars(rootFolder, mailBox.Folders);

                    Marshal.FinalReleaseComObject(rootFolder);
                }
            }
            return mailBoxes;
        }
        private void GetCalendars(MAPIFolder searchFolder, List<OutlookFolder> outlookCalendars)
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
                        GetCalendars(subFolder, outlookCalendars);
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
                            string.Format(
                                "Current Outlook instance is opened with a Different Profile Name ({0}).{1}Close Outlook and try again.",
                                nameSpace.CurrentProfileName, Environment.NewLine));
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
            var app = new ReminderTask(taskItem.EntryID,taskItem.Subject,taskItem.Body, taskItem.DueDate);
            return app;
        }

        private List<ReminderTask> GetTasks(DateTime startDate, DateTime endDate)
        {
            var list = GetOutlookEntriesForSelectedTimeRange(startDate, endDate);
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

        private OutlookTasksWrapper GetOutlookEntriesForSelectedTimeRange(DateTime startDate, DateTime endDate)
        {
            var disposeOutlookInstances = false;
            Application application = null;
            NameSpace nameSpace = null;
            MAPIFolder defaultOutlookCalendar = null;
            Items outlookItems = null;
            var outlookAppointments = new List<ReminderTask>();

            //Close  and Shutdown
            try
            {
                // Get Application and Namespace
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);

                // Get Default Calendar
                defaultOutlookCalendar = OutlookCalendar != null
                    ? nameSpace.GetFolderFromID(OutlookCalendar.EntryId, OutlookCalendar.StoreId)
                    : nameSpace.GetDefaultFolder(OlDefaultFolders.olFolderTasks);
                if (OutlookCalendar == null)
                {
                    OutlookCalendar = new OutlookFolder
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
                    //Add Filter to outlookItems to limit and Avoid endless loop(appointments without end date)
                    outlookItems.Sort("[Start]", Type.Missing);
                    outlookItems.IncludeRecurrences = true;

                    var min = startDate;
                    var max = endDate;

                    // create Final filter as string
                    var filter = "[Start] >= '" + min.ToString("g") + "' AND [End] <= '" + max.ToString("g") + "'";
                    //Set filter on outlookItems and Loop through to create appointment List
                    var outlookEntries = outlookItems.Restrict(filter);
                    if (outlookEntries != null)
                    {
                        var appts = outlookEntries.Cast<TaskItem>();
                        var appointmentItems = appts as TaskItem[] ?? appts.ToArray();
                        if (appointmentItems.Any())
                        {
                            var id = defaultOutlookCalendar.EntryID;
                            foreach (var appointmentItem in appointmentItems)
                            {
                                try
                                {
                                    var app = GetTaskFromItem(id, appointmentItem);
                                    outlookAppointments.Add(app);
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
                Tasks = outlookAppointments,
                WaitForApplicationQuit = disposeOutlookInstances
            };
        }

        public string TaskServiceName
        {
            get { return "Outlook"; }
        }

        /// <exception cref="InvalidOperationException">
        ///     Essential parameters are not present.
        /// </exception>
        public void CheckCalendarSpecificData(IDictionary<string, object> calendarSpecificData)
        {
            if (calendarSpecificData == null)
            {
                throw new ArgumentNullException("calendarSpecificData", "Calendar Specific Data cannot be null");
            }

            object profileValue;
            object outlookCalendarValue;
            if (!(calendarSpecificData.TryGetValue("ProfileName", out profileValue) &&
                  calendarSpecificData.TryGetValue("OutlookCalendar", out outlookCalendarValue)))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "{0} and {1} keys should be present, both of them can be null in case Default Profile and Default Calendar will be used. {0} is of 'string' type, {1} is of 'OutlookCalendar' type and {2} is of bool type.",
                        "ProfileName", "OutlookCalendar"));
            }
            ProfileName = profileValue as String;
            OutlookCalendar = outlookCalendarValue as OutlookFolder;
        }


        public async Task<TasksWrapper> GetCalendarEventsInRangeAsync(DateTime startDate, DateTime endDate,
            IDictionary<string, object> calendarSpecificData)
        {
            CheckCalendarSpecificData(calendarSpecificData);
            var taskWrapper = new TasksWrapper();

            var appointmentList =
                await
                    Task<List<ReminderTask>>.Factory.StartNew(
                        () => GetTasks(startDate, endDate));

            if (appointmentList == null)
            {
                return null;
            }

            taskWrapper.AddRange(appointmentList);

            return taskWrapper;
        }

        public async Task<List<string>> GetOutLookProfieListAsync()
        {
            return await Task<List<string>>.Factory.StartNew(GetOutlookProfileList);
        }


        public async Task<bool> ClearCalendar(IDictionary<string, object> calendarSpecificData)
        {
            var startDate = DateTime.Today.AddDays(-(10 * 365));
            var endDate = DateTime.Today.AddDays(10 * 365);
            var appointments =
                await GetCalendarEventsInRangeAsync(startDate, endDate, calendarSpecificData);
            if (appointments != null)
            {
                var success = await DeleteReminderTasks(appointments, calendarSpecificData);
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



        public Task<TasksWrapper> DeleteReminderTasks(List<ReminderTask> reminderTasks, IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<TasksWrapper> GetReminderTasksInRangeAsync(DateTime startDate, DateTime endDate, IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<TasksWrapper> AddReminderTasks(List<ReminderTask> reminderTasks, IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<TasksWrapper> UpdateReminderTasks(List<ReminderTask> reminderTasks,  IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }
    }
}
