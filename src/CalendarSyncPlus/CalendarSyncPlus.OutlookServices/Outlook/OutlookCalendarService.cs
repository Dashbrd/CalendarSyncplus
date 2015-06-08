#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        CalendarSyncPlus
//  *      SubProject:     CalendarSyncPlus.Application
//  *      Author:         Ankesh Dave
//  *      Created On:     07-02-2015 1:02 PM
//  *      Modified On:    12-02-2015 10:28 PM
//  *      FileName:       OutlookCalendarService.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Helpers;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.OutlookServices.Utilities;
using CalendarSyncPlus.Services;
using CalendarSyncPlus.Services.Interfaces;
using CalendarSyncPlus.Services.Utilities;
using CalendarSyncPlus.Services.Wrappers;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Win32;
using AppRecipient = CalendarSyncPlus.Domain.Models.Recipient;
using Category = CalendarSyncPlus.Domain.Models.Category;
using Exception = System.Exception;
using Recipient = Microsoft.Office.Interop.Outlook.Recipient;

#endregion

namespace CalendarSyncPlus.OutlookServices.Outlook
{
    [Export(typeof(ICalendarService)), Export(typeof(IOutlookCalendarService))]
    [ExportMetadata("ServiceType", CalendarServiceType.OutlookDesktop)]
    public class OutlookCalendarService : IOutlookCalendarService
    {
        [ImportingConstructor]
        public OutlookCalendarService(ApplicationLogger applicationLogger)
        {
            ApplicationLogger = applicationLogger;
        }

        #region Properties

        public ApplicationLogger ApplicationLogger { get; set; }

        private OutlookCalendar OutlookCalendar { get; set; }

        public bool AddAsAppointments { get; set; }

        private string ProfileName { get; set; }

        private Category EventCategory { get; set; }

        public string CalendarServiceName
        {
            get { return "Outlook"; }
        }

        #endregion

        #region Private Methods

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

        private AppointmentListWrapper GetOutlookEntriesForSelectedTimeRange(DateTime startDate, DateTime endDate)
        {
            bool disposeOutlookInstances = false;
            Application application = null;
            NameSpace nameSpace = null;
            MAPIFolder defaultOutlookCalendar = null;
            Items outlookItems = null;
            var outlookAppointments = new List<Appointment>();

            //Close  and Shutdown
            try
            {
                // Get Application and Namespace
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);

                // Get Default Calendar
                defaultOutlookCalendar = OutlookCalendar != null
                    ? nameSpace.GetFolderFromID(OutlookCalendar.EntryId, OutlookCalendar.StoreId)
                    : nameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
                if (OutlookCalendar == null)
                {
                    OutlookCalendar = new OutlookCalendar
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

                    DateTime min = startDate;
                    DateTime max = endDate;

                    // create Final filter as string
                    //string filter = "[End] > '" + min.ToString("dd/MM/yyyy") + "' AND [Start] < '" + max.ToString("dd/MM/yyyy") + "'";
                    //string filter = "[Start] >= '" + min.ToString("dd/MM/yy hh:mm tt") + "' AND [End] <= '" + max.ToString("dd/MM/yy hh:mm tt") + "'";
                    string filter = "[Start] >= '" + min.ToString("g") + "' AND [End] <= '" + max.ToString("g") + "'";
                    //Set filter on outlookItems and Loop through to create appointment List
                    Items outlookEntries = outlookItems.Restrict(filter);
                    if (outlookEntries != null)
                    {
                        IEnumerable<AppointmentItem> appts = outlookEntries.Cast<AppointmentItem>();
                        AppointmentItem[] appointmentItems = appts as AppointmentItem[] ?? appts.ToArray();
                        if (appointmentItems.Any())
                        {
                            string id = defaultOutlookCalendar.EntryID;
                            foreach (AppointmentItem appointmentItem in appointmentItems)
                            {
                                try
                                {
                                    var app = GetAppointmentFromItem(id, appointmentItem);
                                    outlookAppointments.Add(app);
                                }
                                catch (Exception exception)
                                {
                                    ApplicationLogger.LogError(exception.ToString(), typeof(OutlookCalendarService));
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
                ApplicationLogger.LogError(exception.Message, typeof(OutlookCalendarService));
                return new AppointmentListWrapper
                {
                    Appointments = null,
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

            return new AppointmentListWrapper
            {
                Appointments = outlookAppointments,
                WaitForApplicationQuit = disposeOutlookInstances
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="appointmentItem"></param>
        /// <returns></returns>
        private Appointment GetAppointmentFromItem(string id, AppointmentItem appointmentItem)
        {
            var app = new Appointment(appointmentItem.Body, appointmentItem.Location,
                appointmentItem.Subject, appointmentItem.End, appointmentItem.Start)
            {
                AllDayEvent = appointmentItem.AllDayEvent,
                ReminderMinutesBeforeStart = appointmentItem.ReminderMinutesBeforeStart,
                ReminderSet = appointmentItem.ReminderSet,
                IsRecurring = appointmentItem.IsRecurring,
                AppointmentId = appointmentItem.IsRecurring
                    ? string.Format("{0}_{1}", appointmentItem.EntryID,
                        appointmentItem.Start.ToString("yy-MM-dd"))
                    : appointmentItem.EntryID,
                Privacy =
                    (appointmentItem.Sensitivity == OlSensitivity.olNormal) ? "default" : "private",
                MeetingStatus = appointmentItem.GetMeetingStatus()
            };

            GetRecipients(appointmentItem, app);

            app.Created = appointmentItem.CreationTime;
            app.LastModified = appointmentItem.LastModificationTime;
            app.SetBusyStatus(appointmentItem.BusyStatus);

            GetExtendedProperties(appointmentItem, app);
            app.CalendarId = id;
            return app;
        }

        private void GetExtendedProperties(AppointmentItem appointmentItem, Appointment app)
        {
            app.ExtendedProperties = new Dictionary<string, string>();
            UserProperties userProperties = appointmentItem.UserProperties;
            try
            {
                if (userProperties != null)
                {
                    foreach (UserProperty userProperty in userProperties)
                    {
                        if (userProperty != null && !app.ExtendedProperties.ContainsKey(userProperty.Name)
                            && userProperty.Value != null)
                        {
                            app.ExtendedProperties.Add(userProperty.Name,
                                userProperty.Value.ToString());
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.ToString(), typeof(OutlookCalendarService));
            }
            finally
            {
                if (userProperties != null)
                {
                    Marshal.FinalReleaseComObject(userProperties);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appointmentItem"></param>
        /// <param name="app"></param>
        private void GetRecipients(AppointmentItem appointmentItem, Appointment app)
        {
            foreach (Recipient attendee in appointmentItem.Recipients)
            {
                var recipient = new AppRecipient();
                string name, email;
                if (attendee.GetEmailFromName(out name, out email))
                {
                    recipient.Name = name;
                    recipient.Email = email;
                }
                else
                {
                    recipient.Name = attendee.Name;
                    recipient.Email = GetSMTPAddressForRecipients(attendee);
                }
                recipient.MeetingResponseStatus = attendee.GetMeetingResponseStatus();

                if (appointmentItem.RequiredAttendees != null &&
                    appointmentItem.RequiredAttendees.Contains(recipient.Name))
                {
                    if (!app.RequiredAttendees.Any(reci => reci.Email.Equals(recipient.Email)))
                    {
                        app.RequiredAttendees.Add(recipient);
                    }
                }
                if (appointmentItem.OptionalAttendees != null &&
                    appointmentItem.OptionalAttendees.Contains(recipient.Name))
                {
                    if (!app.OptionalAttendees.Any(reci => reci.Email.Equals(recipient.Email)))
                    {
                        app.OptionalAttendees.Add(recipient);
                    }
                }

                if (appointmentItem.Organizer != null &&
                    appointmentItem.Organizer.Contains(recipient.Name))
                {
                    app.Organizer = recipient;
                }
            }
        }

        private List<Appointment> GetAppointments(DateTime startDate, DateTime endDate)
        {
            AppointmentListWrapper list = GetOutlookEntriesForSelectedTimeRange(startDate, endDate);
            if (!list.WaitForApplicationQuit)
            {
                return list.Appointments;
            }
            while (Process.GetProcessesByName("OUTLOOK").Any())
            {
                Task.Delay(5000);
            }
            return list.Appointments;
        }

        private List<OutlookMailBox> GetOutlookMailBoxes(Folders rootFolders)
        {
            var mailBoxes = new List<OutlookMailBox>();
            if (rootFolders.Count > 0)
            {
                foreach (Folder rootFolder in rootFolders)
                {
                    string mailBoxName = rootFolder.Name;

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

                    GetCalendars(rootFolder, mailBox.Calendars);

                    Marshal.FinalReleaseComObject(rootFolder);
                }
            }
            return mailBoxes;
        }

        private string GetSMTPAddressForRecipients(Recipient recip)
        {
            const string PR_SMTP_ADDRESS =
                "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";

            PropertyAccessor pa = recip.PropertyAccessor;
            string smtpAddress = "fake.email.generated@for.user";
            try
            {
                smtpAddress = pa.GetProperty(PR_SMTP_ADDRESS).ToString();
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogInfo(string.Format("Unable to retrieve Email for the User : {0}{1}{2}", recip.Name,
                    Environment.NewLine, exception.Message), typeof(OutlookCalendarService));
            }
            return smtpAddress;
        }

        private void GetCalendars(MAPIFolder searchFolder, List<OutlookCalendar> outlookCalendars)
        {
            try
            {
                if (searchFolder == null)
                {
                    return;
                }

                if (searchFolder.DefaultMessageClass == "IPM.Appointment" &&
                    searchFolder.DefaultItemType == OlItemType.olAppointmentItem)
                {
                    //Add Calendar MAPIFolder to List
                    outlookCalendars.Add(new OutlookCalendar
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
                ApplicationLogger.LogError(exception.ToString(), typeof(OutlookCalendarService));
            }
            finally
            {
                if (searchFolder != null)
                {
                    Marshal.FinalReleaseComObject(searchFolder);
                }
            }
        }

        private List<string> GetOutlookProfileList()
        {
            var profileList = new List<string>();
            const string defaultProfilePath =
                @"Software\Microsoft\Windows NT\CurrentVersion\Windows Messaging Subsystem\Profiles";
            const string newProfilePath = @"Software\Microsoft\Office\15.0\Outlook\Profiles";

            RegistryKey defaultRegKey = Registry.CurrentUser.OpenSubKey(defaultProfilePath,
                RegistryKeyPermissionCheck.Default);

            if (defaultRegKey != null)
            {
                string[] list = defaultRegKey.GetSubKeyNames();

                if (list.Any())
                {
                    profileList.AddRange(list);
                }
            }

            RegistryKey newregKey = Registry.CurrentUser.OpenSubKey(newProfilePath, RegistryKeyPermissionCheck.Default);

            if (newregKey != null)
            {
                string[] list = newregKey.GetSubKeyNames();

                if (list.Any())
                {
                    foreach (string name in list.Where(name => !profileList.Contains(name)))
                    {
                        profileList.Add(name);
                    }
                }
            }

            return profileList;
        }

        private void SetColor(Category background)
        {
            AppointmentListWrapper list = SetColorForSelectedCalendar(background);
            if (!list.WaitForApplicationQuit)
            {
                return;
            }
            while (Process.GetProcessesByName("OUTLOOK").Any())
            {
                Task.Delay(5000);
            }
        }

        private AppointmentListWrapper SetColorForSelectedCalendar(Category background)
        {
            bool disposeOutlookInstances = false;
            Application application = null;
            NameSpace nameSpace = null;

            //Close  and Shutdown
            try
            {
                // Get Application and Namespace
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);

                if (nameSpace.Categories[background.CategoryName] == null)
                {
                    nameSpace.Categories.Add(background.CategoryName, CategoryHelper.GetOutlookColor(background.HexValue),
                        OlCategoryShortcutKey.olCategoryShortcutKeyNone);
                }
                else
                {
                    nameSpace.Categories[background.CategoryName].Color =
                        CategoryHelper.GetOutlookColor(background.HexValue);
                }
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.Message, typeof(OutlookCalendarService));
                return new AppointmentListWrapper
                {
                    Appointments = null,
                    WaitForApplicationQuit = disposeOutlookInstances
                };
            }
            finally
            {
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

            return new AppointmentListWrapper
            {
                Appointments = null,
                WaitForApplicationQuit = disposeOutlookInstances
            };
        }

        #endregion

        #region IOutlookCalendarService Members

        public async void SetCalendarColor(Category background, IDictionary<string, object> calendarSpecificData)
        {
            CheckCalendarSpecificData(calendarSpecificData);

            await Task.Factory.StartNew(
                () => SetColor(background));
        }

        public List<OutlookMailBox> GetAllMailBoxes(string profileName = "")
        {
            ProfileName = profileName;
            bool disposeOutlookInstances = false;
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
                ApplicationLogger.LogError(exception.ToString(), typeof(OutlookCalendarService));
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
                    Task.Delay(5000);
                }
            }

            return mailBoxes;
        }

        public async Task<List<string>> GetOutLookProfieListAsync()
        {
            return await Task<List<string>>.Factory.StartNew(GetOutlookProfileList);
        }


        public async Task<CalendarAppointments> GetCalendarEventsInRangeAsync(DateTime startDate, DateTime endDate,
            IDictionary<string, object> calendarSpecificData)
        {
            CheckCalendarSpecificData(calendarSpecificData);
            var calendarAppointments = new CalendarAppointments();

            List<Appointment> appointmentList =
                    await
                        Task<List<Appointment>>.Factory.StartNew(
                            () => GetAppointments(startDate, endDate));
            
            if (appointmentList == null)
            {
                return null;
            }

            if (OutlookCalendar != null)
            {
                calendarAppointments.CalendarId = OutlookCalendar.EntryId;
            }

            calendarAppointments.AddRange(appointmentList);

            return calendarAppointments;
        }

        private IEnumerable<Tuple<DateTime, DateTime>> SplitDateRange(DateTime start, DateTime end, int dayChunkSize)
        {
            DateTime chunkEnd;
            while ((chunkEnd = start.AddDays(dayChunkSize)) < end)
            {
                yield return Tuple.Create(start, chunkEnd);
                start = chunkEnd;
            }
            yield return Tuple.Create(start, end);
        }

        /// <exception cref="InvalidOperationException">Essential parameters are not present.</exception>
        public void CheckCalendarSpecificData(IDictionary<string, object> calendarSpecificData)
        {
            if (calendarSpecificData == null)
            {
                throw new ArgumentNullException("calendarSpecificData", "Calendar Specific Data cannot be null");
            }

            object profileValue;
            object outlookCalendarValue;
            object addAsAppointments;
            if (!(calendarSpecificData.TryGetValue("ProfileName", out profileValue) &&
                  calendarSpecificData.TryGetValue("OutlookCalendar", out outlookCalendarValue) &&
                  calendarSpecificData.TryGetValue("AddAsAppointments", out addAsAppointments)))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "{0} {1} and {2}  keys should be present, both of them can be null in case Default Profile and Default Calendar will be used. {0} is of 'string' type, {1} is of 'OutlookCalendar' type and {2} is of bool type.",
                        "ProfileName", "OutlookCalendar", "AddAsAppointments"));
            }
            ProfileName = profileValue as String;
            OutlookCalendar = outlookCalendarValue as OutlookCalendar;
            AddAsAppointments = (bool)addAsAppointments;
            object eventCategory;
            if (calendarSpecificData.TryGetValue("EventCategory", out eventCategory))
            {
                EventCategory = eventCategory as Category;
            }
            else
            {
                EventCategory = null;
            }

        }


        public async Task<CalendarAppointments> AddCalendarEvents(List<Appointment> calendarAppointments, bool addDescription,
            bool addReminder, bool addAttendees, bool attendeesToDescription,
            IDictionary<string, object> calendarSpecificData)
        {
            var addedAppointments = new CalendarAppointments();
            if (!calendarAppointments.Any())
            {
                addedAppointments.IsSuccess = true;
                return addedAppointments;
            }
            CheckCalendarSpecificData(calendarSpecificData);

            bool result = await
                Task<bool>.Factory.StartNew(
                    () =>
                        AddEvents(calendarAppointments, addDescription, addReminder, addAttendees,
                            attendeesToDescription, addedAppointments));

            addedAppointments.IsSuccess = result;
            return addedAppointments;
        }

        /// <summary>
        /// </summary>
        /// <param name="calendarAppointments"></param>
        /// <param name="calendarSpecificData"></param>
        /// <returns></returns>
        public async Task<bool> DeleteCalendarEvents(List<Appointment> calendarAppointments,
            IDictionary<string, object> calendarSpecificData)
        {
            if (!calendarAppointments.Any())
            {
                return true;
            }
            CheckCalendarSpecificData(calendarSpecificData);
            bool result = await Task<bool>.Factory.StartNew(() => DeleteEvents(calendarAppointments));

            return result;
        }

        public async Task<bool> ResetCalendar(IDictionary<string, object> calendarSpecificData)
        {
            DateTime startDate = DateTime.Today.AddDays(-(10 * 365));
            DateTime endDate = DateTime.Today.AddDays(10 * 365);
            CalendarAppointments appointments =
                await GetCalendarEventsInRangeAsync(startDate, endDate, calendarSpecificData);
            if (appointments != null)
            {
                bool success = await DeleteCalendarEvents(appointments, calendarSpecificData);
                return success;
            }
            return false;
        }

        #endregion

        private bool AddEvents(List<Appointment> calendarAppointments, bool addDescription,
            bool addReminder,
            bool addAttendees, bool attendeesToDescription, List<Appointment> addedAppointment)
        {
            AppointmentListWrapper wrapper = AddEventsToOutlook(calendarAppointments, addDescription, addReminder,
                addAttendees, attendeesToDescription, addedAppointment);

            if (!wrapper.WaitForApplicationQuit)
            {
                return wrapper.Success;
            }

            while (Process.GetProcessesByName("OUTLOOK").Any())
            {
                Task.Delay(5000);
            }
            return wrapper.Success;
        }

        /// <summary>
        /// </summary>
        /// <param name="calendarAppointments"></param>
        /// <param name="addDescription"></param>
        /// <param name="addReminder"></param>
        /// <param name="addAttendees"></param>
        /// <param name="attendeesToDescription"></param>
        /// <returns></returns>
        private AppointmentListWrapper AddEventsToOutlook(List<Appointment> calendarAppointments, bool addDescription,
            bool addReminder, bool addAttendees, bool attendeesToDescription, List<Appointment> addedAppointment)
        {
            bool disposeOutlookInstances = false;
            Application application = null;
            NameSpace nameSpace = null;
            MAPIFolder defaultOutlookCalendar = null;
            Items outlookItems = null;

            try
            {
                // Get Application and Namespace
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);

                // Get Default Calendar
                defaultOutlookCalendar = OutlookCalendar != null
                    ? nameSpace.GetFolderFromID(OutlookCalendar.EntryId, OutlookCalendar.StoreId)
                    : nameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
                outlookItems = defaultOutlookCalendar.Items;

                if (EventCategory != null)
                {
                    if (nameSpace.Categories[EventCategory.CategoryName] == null)
                    {
                        nameSpace.Categories.Add(EventCategory.CategoryName, CategoryHelper.GetOutlookColor(EventCategory.HexValue),
                        OlCategoryShortcutKey.olCategoryShortcutKeyNone);
                    }
                    else
                    {
                        nameSpace.Categories[EventCategory.CategoryName].Color =
                            CategoryHelper.GetOutlookColor(EventCategory.HexValue);
                    }
                }

                foreach (Appointment calendarAppointment in calendarAppointments)
                {
                    var appItem = outlookItems.Add(OlItemType.olAppointmentItem) as AppointmentItem;
                    if (appItem == null)
                    {
                        continue;
                    }
                    var newAppointment = AddAppointment(addDescription, addReminder, addAttendees, attendeesToDescription, appItem,
                        calendarAppointment);
                    addedAppointment.Add(newAppointment);
                    Marshal.FinalReleaseComObject(appItem);
                }
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.ToString(), typeof(OutlookCalendarService));
                return new AppointmentListWrapper
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
            return new AppointmentListWrapper
            {
                WaitForApplicationQuit = disposeOutlookInstances,
                Success = true
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="addDescription"></param>
        /// <param name="addReminder"></param>
        /// <param name="addAttendees"></param>
        /// <param name="attendeesToDescription"></param>
        /// <param name="appItem"></param>
        /// <param name="calendarAppointment"></param>
        private Appointment AddAppointment(bool addDescription, bool addReminder, bool addAttendees,
            bool attendeesToDescription, AppointmentItem appItem,
            Appointment calendarAppointment)
        {
            Recipients recipients = null;
            UserProperties userProperties = null;
            Appointment createdAppointment = null;
            try
            {
                appItem.Subject = calendarAppointment.Subject;
                if (!calendarAppointment.RequiredAttendees.Any() && !calendarAppointment.OptionalAttendees.Any()
                    && AddAsAppointments)
                {
                    appItem.MeetingStatus = OlMeetingStatus.olNonMeeting;
                }
                else
                {
                    appItem.MeetingStatus = OlMeetingStatus.olMeeting;
                }

                appItem.Location = calendarAppointment.Location;
                appItem.BusyStatus = calendarAppointment.GetOutlookBusyStatus();
                recipients = appItem.Recipients;
                if (EventCategory != null)
                {
                    appItem.Categories = EventCategory.CategoryName;
                }

                if (calendarAppointment.AllDayEvent)
                {
                    appItem.AllDayEvent = true;
                }

                appItem.Start = calendarAppointment.StartTime.GetValueOrDefault();
                appItem.End = calendarAppointment.EndTime.GetValueOrDefault();


                appItem.Body = calendarAppointment.GetDescriptionData(addDescription, attendeesToDescription);

                if (addAttendees && !attendeesToDescription)
                {
                    if (calendarAppointment.RequiredAttendees != null)
                    {
                        calendarAppointment.RequiredAttendees.ForEach(rcptName =>
                        {
                            Recipient recipient =
                                appItem.Recipients.Add(string.Format("{0}<{1}>", rcptName.Name, rcptName.Email));
                            recipient.Type = (int)OlMeetingRecipientType.olRequired;
                            recipient.Resolve();
                        });
                    }

                    if (calendarAppointment.OptionalAttendees != null)
                    {
                        calendarAppointment.OptionalAttendees.ForEach(rcptName =>
                        {
                            Recipient recipient =
                                appItem.Recipients.Add(string.Format("{0}<{1}>", rcptName.Name, rcptName.Email));
                            recipient.Type = (int)OlMeetingRecipientType.olOptional;
                            recipient.Resolve();
                        });
                    }
                }

                if (addReminder)
                {
                    appItem.ReminderMinutesBeforeStart = calendarAppointment.ReminderMinutesBeforeStart;
                    appItem.ReminderSet = calendarAppointment.ReminderSet;
                }

                userProperties = appItem.UserProperties;
                if (userProperties != null)
                {
                    UserProperty sourceProperty = userProperties.Add(calendarAppointment.GetSourceEntryKey(),
                        OlUserPropertyType.olText);
                    sourceProperty.Value = calendarAppointment.AppointmentId;
                }
                appItem.Save();

                createdAppointment = GetAppointmentFromItem(calendarAppointment.CalendarId, appItem);

            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.Message, typeof(OutlookCalendarService));
            }
            finally
            {
                if (userProperties != null)
                {
                    Marshal.ReleaseComObject(userProperties);
                }
                if (recipients != null)
                {
                    Marshal.ReleaseComObject(recipients);
                }
                if (appItem != null)
                {
                    Marshal.ReleaseComObject(appItem);
                }
            }
            return createdAppointment;
        }


        private bool DeleteEvents(List<Appointment> calendarAppointments)
        {
            AppointmentListWrapper wrapper = DeleteEventsFromOutlook(calendarAppointments);

            if (!wrapper.WaitForApplicationQuit)
            {
                return wrapper.Success;
            }

            while (Process.GetProcessesByName("OUTLOOK").Any())
            {
                Task.Delay(5000);
            }
            return wrapper.Success;
        }

        private AppointmentListWrapper DeleteEventsFromOutlook(List<Appointment> calendarAppointments)
        {
            bool disposeOutlookInstances = false;
            Application application = null;
            NameSpace nameSpace = null;

            try
            {
                // Get Application and Namespace
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);


                foreach (Appointment calendarAppointment in calendarAppointments)
                {
                    try
                    {
                        AppointmentItem appointmentItem = null;
                        if (calendarAppointment.IsRecurring)
                        {
                            var idArray = calendarAppointment.AppointmentId.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                            var parentAppointment = nameSpace.GetItemFromID(idArray.FirstOrDefault()) as AppointmentItem;
                            if (parentAppointment != null)
                            {
                                RecurrencePattern pattern = parentAppointment.GetRecurrencePattern();
                                appointmentItem = pattern.GetOccurrence(DateTime.Parse(idArray[1]));
                            }
                        }
                        else
                        {
                            appointmentItem = nameSpace.GetItemFromID(calendarAppointment.AppointmentId) as AppointmentItem;
                        }

                        if (appointmentItem != null)
                        {
                            appointmentItem.Delete();
                            Marshal.FinalReleaseComObject(appointmentItem);
                        }
                    }
                    catch (Exception exception)
                    {
                        ApplicationLogger.LogError(exception.ToString(), typeof(OutlookCalendarService));
                    }
                }
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.ToString(), typeof(OutlookCalendarService));
                return new AppointmentListWrapper
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
            return new AppointmentListWrapper
            {
                WaitForApplicationQuit = disposeOutlookInstances,
                Success = true
            };
        }


        public async Task<bool> UpdateCalendarEvents(List<Appointment> calendarAppointments, bool addDescription, bool addReminder, bool addAttendees, bool attendeesToDescription, IDictionary<string, object> calendarSpecificData)
        {
            if (!calendarAppointments.Any())
            {
                return true;
            }
            CheckCalendarSpecificData(calendarSpecificData);

            bool result = await
                Task<bool>.Factory.StartNew(
                    () =>
                        UpdateEvents(calendarAppointments, addDescription, addReminder, addAttendees,
                            attendeesToDescription));

            return result;
        }

        private bool UpdateEvents(List<Appointment> calendarAppointments, bool addDescription,
            bool addReminder,
            bool addAttendees, bool attendeesToDescription)
        {
            AppointmentListWrapper wrapper = UpdateEventsToOutlook(calendarAppointments, addDescription, addReminder,
                  addAttendees, attendeesToDescription);

            if (!wrapper.WaitForApplicationQuit)
            {
                return wrapper.Success;
            }

            while (Process.GetProcessesByName("OUTLOOK").Any())
            {
                Task.Delay(5000);
            }
            return wrapper.Success;

        }

        /// <summary>
        /// </summary>
        /// <param name="calendarAppointments"></param>
        /// <param name="addDescription"></param>
        /// <param name="addReminder"></param>
        /// <param name="addAttendees"></param>
        /// <param name="attendeesToDescription"></param>
        /// <returns></returns>
        private AppointmentListWrapper UpdateEventsToOutlook(List<Appointment> calendarAppointments, bool addDescription,
            bool addReminder, bool addAttendees, bool attendeesToDescription)
        {
            bool disposeOutlookInstances = false;
            Application application = null;
            NameSpace nameSpace = null;
            MAPIFolder defaultOutlookCalendar = null;
            Items outlookItems = null;

            try
            {
                // Get Application and Namespace
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);

                // Get Default Calendar
                defaultOutlookCalendar = OutlookCalendar != null
                    ? nameSpace.GetFolderFromID(OutlookCalendar.EntryId, OutlookCalendar.StoreId)
                    : nameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
                outlookItems = defaultOutlookCalendar.Items;


                foreach (Appointment calendarAppointment in calendarAppointments)
                {
                    try
                    {
                        AppointmentItem appItem = null;
                        if (calendarAppointment.IsRecurring)
                        {
                            var idArray = calendarAppointment.AppointmentId.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                            var parentAppointment = nameSpace.GetItemFromID(idArray.FirstOrDefault()) as AppointmentItem;
                            RecurrencePattern pattern = parentAppointment.GetRecurrencePattern();
                            appItem = pattern.GetOccurrence(calendarAppointment.OldStartTime.GetValueOrDefault());
                        }
                        else
                        {
                            appItem = nameSpace.GetItemFromID(calendarAppointment.AppointmentId) as AppointmentItem;
                        }

                        if (appItem == null)
                        {
                            continue;
                        }
                        UpdateAppointment(addDescription, addReminder, addAttendees, attendeesToDescription, appItem,
                            calendarAppointment);
                    }
                    catch (Exception exception)
                    {
                        ApplicationLogger.LogError(exception, typeof(OutlookCalendarService));
                    }
                }
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception, typeof(OutlookCalendarService));
                return new AppointmentListWrapper
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
            return new AppointmentListWrapper
            {
                WaitForApplicationQuit = disposeOutlookInstances,
                Success = true
            };
        }

        private void UpdateAppointment(bool addDescription, bool addReminder, bool addAttendees,
            bool attendeesToDescription, AppointmentItem appItem,
            Appointment calendarAppointment)
        {
            Recipients recipients = null;
            try
            {
                appItem.Subject = calendarAppointment.Subject;
                if (!calendarAppointment.RequiredAttendees.Any() && !calendarAppointment.OptionalAttendees.Any()
                    && AddAsAppointments)
                {
                    appItem.MeetingStatus = OlMeetingStatus.olNonMeeting;
                }
                else
                {
                    appItem.MeetingStatus = OlMeetingStatus.olMeeting;
                }

                appItem.Location = calendarAppointment.Location;
                appItem.BusyStatus = calendarAppointment.GetOutlookBusyStatus();
                recipients = appItem.Recipients;
                if (EventCategory != null)
                {
                    appItem.Categories = EventCategory.CategoryName;
                }

                if (calendarAppointment.AllDayEvent)
                {
                    appItem.AllDayEvent = true;
                }

                appItem.Start = calendarAppointment.StartTime.GetValueOrDefault();
                appItem.End = calendarAppointment.EndTime.GetValueOrDefault();
                appItem.Body = calendarAppointment.GetDescriptionData(addDescription, attendeesToDescription);

                if (addAttendees && !attendeesToDescription)
                {
                    if (calendarAppointment.RequiredAttendees != null)
                    {
                        calendarAppointment.RequiredAttendees.ForEach(rcptName =>
                        {
                            Recipient recipient =
                                appItem.Recipients.Add(string.Format("{0}<{1}>", rcptName.Name, rcptName.Email));
                            recipient.Type = (int)OlMeetingRecipientType.olRequired;
                            recipient.Resolve();
                        });
                    }

                    if (calendarAppointment.OptionalAttendees != null)
                    {
                        calendarAppointment.OptionalAttendees.ForEach(rcptName =>
                        {
                            Recipient recipient =
                                appItem.Recipients.Add(string.Format("{0}<{1}>", rcptName.Name, rcptName.Email));
                            recipient.Type = (int)OlMeetingRecipientType.olOptional;
                            recipient.Resolve();
                        });
                    }
                }

                if (addReminder)
                {
                    appItem.ReminderMinutesBeforeStart = calendarAppointment.ReminderMinutesBeforeStart;
                    appItem.ReminderSet = calendarAppointment.ReminderSet;
                }
                
                appItem.Save();
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.Message, typeof(OutlookCalendarService));
            }
            finally
            {
                if (recipients != null)
                {
                    Marshal.ReleaseComObject(recipients);
                }
                if (appItem != null)
                {
                    Marshal.ReleaseComObject(appItem);
                }
            }
        }
        
    }
}