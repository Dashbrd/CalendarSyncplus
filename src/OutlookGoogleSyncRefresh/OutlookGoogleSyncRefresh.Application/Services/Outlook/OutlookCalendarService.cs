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
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Win32;
using OutlookGoogleSyncRefresh.Application.Utilities;
using OutlookGoogleSyncRefresh.Application.Wrappers;
using OutlookGoogleSyncRefresh.Common.Log;
using OutlookGoogleSyncRefresh.Common.MetaData;
using OutlookGoogleSyncRefresh.Domain.Models;
using Exception = System.Exception;
using Recipient = Microsoft.Office.Interop.Outlook.Recipient;
using AppRecipient = OutlookGoogleSyncRefresh.Domain.Models.Recipient;
using Category = OutlookGoogleSyncRefresh.Application.Wrappers.Category;

#endregion

namespace OutlookGoogleSyncRefresh.Application.Services.Outlook
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
        public string CalendarServiceName
        {
            get { return "Outlook"; }
        }

        private OutlookCalendar OutlookCalendar { get; set; }

        private string ProfileName { get; set; }

        #endregion

        #region Private Methods

        private void GetOutlookApplication(out bool disposeOutlookInstances,
            out Microsoft.Office.Interop.Outlook.Application application,
            out NameSpace nameSpace, string profileName)
        {
            // Check whether there is an Outlook process running.
            if (Process.GetProcessesByName("OUTLOOK").Any())
            {
                // If so, use the GetActiveObject method to obtain the process and cast it to an Application object.
                application =
                    Marshal.GetActiveObject("Outlook.Application") as Microsoft.Office.Interop.Outlook.Application;
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
                application = new Microsoft.Office.Interop.Outlook.Application();
                nameSpace = application.GetNamespace("MAPI");
                nameSpace.Logon(profileName, "", false, true);
                disposeOutlookInstances = true;
            }
        }

        private AppointmentListWrapper GetOutlookEntriesForSelectedTimeRange(int daysInPast, int daysInFuture)
        {
            var disposeOutlookInstances = false;
            Microsoft.Office.Interop.Outlook.Application application = null;
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
                    OutlookCalendar = new OutlookCalendar()
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

                    var min = DateTime.Now.Date.AddDays(-daysInPast);
                    var max = DateTime.Now.Date.AddDays(+(daysInFuture + 1));

                    // create Final filter as string
                    var filter = "[End] >= '" + min.ToString("g") + "' AND [Start] < '" + max.ToString("g") + "'";

                    //Set filter on outlookItems and Loop through to create appointment List
                    var outlookEntries = outlookItems.Restrict(filter);
                    if (outlookEntries != null)
                    {
                        var appts = outlookEntries.Cast<AppointmentItem>();
                        var appointmentItems = appts as AppointmentItem[] ?? appts.ToArray();
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
                    ((_Application)application).Quit();
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
                foreach (var appointmentItem in appointmentItems)
                {
                    var app = new Appointment(appointmentItem.Body, appointmentItem.Location,
                        appointmentItem.Subject, appointmentItem.End, appointmentItem.Start)
                    {
                        AllDayEvent = appointmentItem.AllDayEvent,
                        ReminderMinutesBeforeStart = appointmentItem.ReminderMinutesBeforeStart,
                        ReminderSet = appointmentItem.ReminderSet,
                        AppointmentId = appointmentItem.IsRecurring
                            ? string.Format("{0}_{1}", appointmentItem.EntryID,
                              appointmentItem.Start.ToString("yy-MM-dd")) :
                            appointmentItem.EntryID,
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

                    var userProperties = appointmentItem.UserProperties;
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
            var list = GetOutlookEntriesForSelectedTimeRange(daysInPast, daysInFuture);
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
                    var mailBoxName = rootFolder.Name;

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

            var pa = recip.PropertyAccessor;
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

        private void SetColor(Category background)
        {
            var list = SetColorForSelectedCalendar(background);
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
            var disposeOutlookInstances = false;
            Microsoft.Office.Interop.Outlook.Application application = null;
            NameSpace nameSpace = null;

            //Close  and Shutdown
            try
            {
                // Get Application and Namespace
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);

                if (nameSpace.Categories[Constants.CategoryName] == null)
                {
                    nameSpace.Categories.Add(Constants.CategoryName, background.OutlookColor,
                        OlCategoryShortcutKey.olCategoryShortcutKeyNone);
                }
                else
                {
                    nameSpace.Categories[Constants.CategoryName].Color = background.OutlookColor;
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
                    ((_Application)application).Quit();
                    Marshal.FinalReleaseComObject(application);
                }
                application = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            return new AppointmentListWrapper()
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
            var disposeOutlookInstances = false;
            Microsoft.Office.Interop.Outlook.Application application = null;
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

                Marshal.FinalReleaseComObject(nameSpace);

                if (disposeOutlookInstances)
                {
                    // Casting Removes a warninig for Ambigous Call
                    ((_Application)application).Quit();
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
            var appointmentList =
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

            var result = await
                    Task<bool>.Factory.StartNew(() => AddEvents(calendarAppointments, addDescription, addReminder, addAttendees, attendeesToDescription));

            return result;
        }

        bool AddEvents(List<Appointment> calenderAppointments, bool addDescription,
            bool addReminder,
            bool addAttendees, bool attendeesToDescroption)
        {
            var wrapper = AddEventsToOutlook(calenderAppointments, addDescription, addReminder, addAttendees, attendeesToDescroption);

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
        /// 
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
            var disposeOutlookInstances = false;
            Microsoft.Office.Interop.Outlook.Application application = null;
            NameSpace nameSpace = null;
            MAPIFolder defaultOutlookCalender = null;
            Items outlookItems = null;

            try
            {

                // Get Application and Namespace
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);

                // Get Default Calender
                defaultOutlookCalender = OutlookCalendar != null ? nameSpace.GetFolderFromID(OutlookCalendar.EntryId, OutlookCalendar.StoreId) : nameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
                outlookItems = defaultOutlookCalender.Items;
                if (nameSpace.Categories[Constants.CategoryName] == null)
                {
                    nameSpace.Categories.Add(Constants.CategoryName, OlCategoryColor.olCategoryColorNone,
                        OlCategoryShortcutKey.olCategoryShortcutKeyNone);
                }

                foreach (var calendarAppointment in calenderAppointments)
                {
                    var appItem = outlookItems.Add(OlItemType.olAppointmentItem) as AppointmentItem;
                    if (appItem == null)
                    {
                        continue;
                    }
                    AddAppointment(addDescription, addReminder, addAttendees, attendeesToDescription, appItem, calendarAppointment);
                }
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.ToString());
                return new AppointmentListWrapper()
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
            return new AppointmentListWrapper()
            {
                WaitForApplicationQuit = disposeOutlookInstances,
                Success = true
            };
        }

        private void AddAppointment(bool addDescription, bool addReminder, bool addAttendees, bool attendeesToDescription, AppointmentItem appItem,
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
                appItem.Categories = Constants.CategoryName;

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
                            var recipient = appItem.Recipients.Add(string.Format("{0}<{1}>", rcptName.Name, rcptName.Email));
                            recipient.Type = (int)OlMeetingRecipientType.olRequired;
                            recipient.Resolve();
                        });
                    }

                    if (calendarAppointment.OptionalAttendees != null)
                    {
                        calendarAppointment.OptionalAttendees.ForEach(rcptName =>
                        {
                            var recipient = appItem.Recipients.Add(string.Format("{0}<{1}>", rcptName.Name, rcptName.Email));
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
                    var sourceProperty = userProperties.Add(calendarAppointment.GetSourceEntryKey(),
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


        /// <summary>
        /// 
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
            var result = await Task<bool>.Factory.StartNew(() => DeleteEvents(calendarAppointments));

            return result;
        }

        private bool DeleteEvents(List<Appointment> calendarAppointments)
        {
            var wrapper = DeleteEventsFromOutlook(calendarAppointments);

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
            var disposeOutlookInstances = false;
            Microsoft.Office.Interop.Outlook.Application application = null;
            NameSpace nameSpace = null;

            try
            {
                // Get Application and Namespace
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);


                foreach (var calendarAppointment in calendarAppointments)
                {
                    var appointmentItem = nameSpace.GetItemFromID(calendarAppointment.AppointmentId);
                    if (appointmentItem is AppointmentItem)
                    {
                        appointmentItem.Delete();
                    }
                }
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.ToString());
                return new AppointmentListWrapper()
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
            return new AppointmentListWrapper()
            {
                WaitForApplicationQuit = disposeOutlookInstances,
                Success = true
            };
        }

        public async Task<bool> ResetCalendar(IDictionary<string, object> calendarSpecificData)
        {
            var appointments = await GetCalendarEventsInRangeAsync(10 * 365, 10 * 365, calendarSpecificData);
            if (appointments != null)
            {
                var success = await DeleteCalendarEvent(appointments, calendarSpecificData);
                return success;
            }
            return false;
        }
        #endregion
    }
}