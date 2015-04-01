#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        OutlookGoogleSyncRefresh
//  *      SubProject:     OutlookGoogleSyncRefresh.Application
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
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Common.MetaData;
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

        private AppointmentListWrapper GetOutlookEntriesForSelectedTimeRange(int daysInPast, int daysInFuture)
        {
            bool disposeOutlookInstances = false;
            Application application = null;
            NameSpace nameSpace = null;
            MAPIFolder defaultOutlookCalender = null;
            Items outlookItems = null;
            var outlookAppointments = new List<Appointment>();

            //Close  and Shutdown
            try
            {
                // Get Application and Namespace
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);

                // Get Default Calender
                defaultOutlookCalender = OutlookCalendar != null
                    ? nameSpace.GetFolderFromID(OutlookCalendar.EntryId, OutlookCalendar.StoreId)
                    : nameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
                if (OutlookCalendar == null)
                {
                    OutlookCalendar = new OutlookCalendar
                    {
                        Name = defaultOutlookCalender.Name,
                        EntryId = defaultOutlookCalender.EntryID,
                        StoreId = defaultOutlookCalender.StoreID
                    };
                }
                // Get outlook Items
                outlookItems = defaultOutlookCalender.Items;

                if (outlookItems != null)
                {
                    //Add Filter to outlookItems to limit and Avoid endless loop(appointments without end date)
                    outlookItems.Sort("[Start]", Type.Missing);
                    outlookItems.IncludeRecurrences = true;

                    DateTime min = DateTime.Today.AddDays(-daysInPast);
                    DateTime max = DateTime.Today.AddDays((daysInFuture + 1));

                    // create Final filter as string
                    //string filter = "[End] > '" + min.ToString("dd/MM/yyyy") + "' AND [Start] < '" + max.ToString("dd/MM/yyyy") + "'";
                    string filter = "[Start] >= '"+ min.ToString("dd/MM/yyyy hh:mm tt")+ "' AND [End] <= '"+ max.ToString("dd/MM/yyyy hh:mm tt") + "'";
                    
                        //Set filter on outlookItems and Loop through to create appointment List
                    Items outlookEntries = outlookItems.Restrict(filter);
                    if (outlookEntries != null)
                    {
                        IEnumerable<AppointmentItem> appts = outlookEntries.Cast<AppointmentItem>();
                        AppointmentItem[] appointmentItems = appts as AppointmentItem[] ?? appts.ToArray();
                        GetAppointmentFromItem(appointmentItems, defaultOutlookCalender.EntryID, outlookAppointments);
                    }
                }
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.Message);
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

                //Unassign all instances
                if (outlookItems != null)
                {
                    Marshal.FinalReleaseComObject(outlookItems);
                    outlookItems = null;
                }

                Marshal.FinalReleaseComObject(defaultOutlookCalender);
                defaultOutlookCalender = null;

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
                Appointments = outlookAppointments,
                WaitForApplicationQuit = disposeOutlookInstances
            };
        }

        private void GetAppointmentFromItem(AppointmentItem[] appointmentItems, string id,
            List<Appointment> outlookAppointments)
        {
            if (appointmentItems.Any())
            {
                foreach (AppointmentItem appointmentItem in appointmentItems)
                {
                    var app = new Appointment(appointmentItem.Body, appointmentItem.Location,
                        appointmentItem.Subject, appointmentItem.End, appointmentItem.Start)
                    {
                        AllDayEvent = appointmentItem.AllDayEvent,
                        ReminderMinutesBeforeStart = appointmentItem.ReminderMinutesBeforeStart,
                        ReminderSet = appointmentItem.ReminderSet,
                        AppointmentId = appointmentItem.IsRecurring
                            ? string.Format("{0}_{1}", appointmentItem.EntryID,
                                appointmentItem.Start.ToString("yy-MM-dd"))
                            : appointmentItem.EntryID,
                        Privacy =
                            (appointmentItem.Sensitivity == OlSensitivity.olNormal) ? "default" : "private",
                        IsRecurring = appointmentItem.IsRecurring,
                    };

                    foreach (Recipient attendee in appointmentItem.Recipients)
                    {
                        var recipient = new AppRecipient
                        {
                            Name = attendee.Name,
                            Email = GetSMTPAddressForRecipients(attendee)
                        };
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
                    app.Created = appointmentItem.CreationTime;
                    app.LastModified = appointmentItem.LastModificationTime;
                    app.SetBusyStatus(appointmentItem.BusyStatus);

                    UserProperties userProperties = appointmentItem.UserProperties;
                    if (userProperties
                        != null)
                    {
                        foreach (UserProperty userProperty in userProperties)
                        {
                            app.ExtendedProperties.Add(userProperty.Name,
                                userProperty.Value);
                        }

                        Marshal.FinalReleaseComObject(userProperties);
                    }
                    app.CalendarId = id;
                    outlookAppointments.Add(app);
                }
            }
        }

        private List<Appointment> GetAppointments(int daysInPast, int daysInFuture)
        {
            AppointmentListWrapper list = GetOutlookEntriesForSelectedTimeRange(daysInPast, daysInFuture);
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

                    //All mailBoxes Scanned Leave Public Calenders and Folders
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
                ApplicationLogger.LogInfo(string.Format("Unable to retrive Email for the User : {0}{1}{2}", recip.Name,
                    Environment.NewLine, exception.Message));
            }
            return smtpAddress;
        }

        private void GetCalendars(MAPIFolder searchFolder, List<OutlookCalendar> outlookCalendars)
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
            //Walk through all subFolders in MAPIFolder
            //Walk through all subFolders in MAPIFolder
            foreach (MAPIFolder subFolder in searchFolder.Folders)
            {
                //Get Calendar MAPIFolders
                GetCalendars(subFolder, outlookCalendars);
            }

            Marshal.FinalReleaseComObject(searchFolder);
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
                ApplicationLogger.LogError(exception.Message);
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
                ApplicationLogger.LogError(exception.ToString());
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


        public async Task<CalendarAppointments> GetCalendarEventsInRangeAsync(int daysInPast, int daysInFuture,
            IDictionary<string, object> calendarSpecificData)
        {
            CheckCalendarSpecificData(calendarSpecificData);
            //Get Outlook Entries
            List<Appointment> appointmentList =
                await
                    Task<List<Appointment>>.Factory.StartNew(
                        () => GetAppointments(daysInPast, daysInFuture));
            var calendarAppointments = new CalendarAppointments();
            if (OutlookCalendar != null)
            {
                calendarAppointments.CalendarId = OutlookCalendar.EntryId;
            }

            if (appointmentList != null)
            {
                calendarAppointments.AddRange(appointmentList);
            }
            return calendarAppointments;
        }

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
                        "{0} and {1}  keys should be present, both of them can be null in case Default Profile and Default Calendar will be used. {0} is or 'string' and {1} is of 'OutlookCalendar' type",
                        "ProfileName", OutlookCalendar));
            }
            ProfileName = profileValue as String;
            OutlookCalendar = outlookCalendarValue as OutlookCalendar;

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


        public async Task<bool> AddCalendarEvent(List<Appointment> calendarAppointments, bool addDescription,
            bool addReminder, bool addAttendees, bool attendeesToDescription,
            IDictionary<string, object> calendarSpecificData)
        {
            if (!calendarAppointments.Any())
            {
                return true;
            }
            CheckCalendarSpecificData(calendarSpecificData);

            bool result = await
                Task<bool>.Factory.StartNew(
                    () =>
                        AddEvents(calendarAppointments, addDescription, addReminder, addAttendees,
                            attendeesToDescription));

            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="calendarAppointments"></param>
        /// <param name="calendarSpecificData"></param>
        /// <returns></returns>
        public async Task<bool> DeleteCalendarEvent(List<Appointment> calendarAppointments,
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
            CalendarAppointments appointments =
                await GetCalendarEventsInRangeAsync(10 * 365, 10 * 365, calendarSpecificData);
            if (appointments != null)
            {
                bool success = await DeleteCalendarEvent(appointments, calendarSpecificData);
                return success;
            }
            return false;
        }

        #endregion

        private bool AddEvents(List<Appointment> calenderAppointments, bool addDescription,
            bool addReminder,
            bool addAttendees, bool attendeesToDescroption)
        {
            AppointmentListWrapper wrapper = AddEventsToOutlook(calenderAppointments, addDescription, addReminder,
                addAttendees, attendeesToDescroption);

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
        /// <param name="calenderAppointments"></param>
        /// <param name="addDescription"></param>
        /// <param name="addReminder"></param>
        /// <param name="addAttendees"></param>
        /// <param name="attendeesToDescription"></param>
        /// <returns></returns>
        private AppointmentListWrapper AddEventsToOutlook(List<Appointment> calenderAppointments, bool addDescription,
            bool addReminder, bool addAttendees, bool attendeesToDescription)
        {
            bool disposeOutlookInstances = false;
            Application application = null;
            NameSpace nameSpace = null;
            MAPIFolder defaultOutlookCalender = null;
            Items outlookItems = null;

            try
            {
                // Get Application and Namespace
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);

                // Get Default Calender
                defaultOutlookCalender = OutlookCalendar != null
                    ? nameSpace.GetFolderFromID(OutlookCalendar.EntryId, OutlookCalendar.StoreId)
                    : nameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
                outlookItems = defaultOutlookCalender.Items;

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

                foreach (Appointment calendarAppointment in calenderAppointments)
                {
                    var appItem = outlookItems.Add(OlItemType.olAppointmentItem) as AppointmentItem;
                    if (appItem == null)
                    {
                        continue;
                    }
                    AddAppointment(addDescription, addReminder, addAttendees, attendeesToDescription, appItem,
                        calendarAppointment);
                }
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.ToString());
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

                Marshal.FinalReleaseComObject(defaultOutlookCalender);
                defaultOutlookCalender = null;

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

        private void AddAppointment(bool addDescription, bool addReminder, bool addAttendees,
            bool attendeesToDescription, AppointmentItem appItem,
            Appointment calendarAppointment)
        {
            Recipients recipients = null;
            UserProperties userProperties = null;
            try
            {
                appItem.Subject = calendarAppointment.Subject;
                appItem.MeetingStatus = OlMeetingStatus.olMeeting;
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
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.Message);
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
                    dynamic appointmentItem = nameSpace.GetItemFromID(calendarAppointment.AppointmentId);
                    if (appointmentItem is AppointmentItem)
                    {
                        appointmentItem.Delete();
                    }
                }
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.ToString());
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
    }
}