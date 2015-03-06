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

using Microsoft.Office.Interop.Outlook;
using Microsoft.Win32;

using OutlookGoogleSyncRefresh.Common.Attributes;
using OutlookGoogleSyncRefresh.Common.MetaData;
using OutlookGoogleSyncRefresh.Domain.Models;

#endregion

namespace OutlookGoogleSyncRefresh.Application.Services.Outlook
{
    [Export(typeof(ICalendarService)),Export(typeof(IOutlookCalendarService))]
    [ExportMetadata("ServiceType", CalendarServiceType.OutlookDesktop)]
    public class OutlookCalendarService : IOutlookCalendarService
    {
        #region Properties

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

        private AppointmentListWrapper GetOutlookEntriesForSelectedTimeRange(int daysInPast, int daysInFuture,
            string profileName,
            OutlookCalendar outlookCalendar)
        {
            bool disposeOutlookInstances;
            Microsoft.Office.Interop.Outlook.Application application = null;
            NameSpace nameSpace = null;
            MAPIFolder defaultOutlookCalender = null;
            Items outlookItems = null;
            var outlookAppointments = new List<Appointment>();
            // Get Application and Namespace
            GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, profileName);

            // Get Default Calender
            if (outlookCalendar != null)
            {
                defaultOutlookCalender = nameSpace.GetFolderFromID(outlookCalendar.EntryId, outlookCalendar.StoreId);
            }
            else
            {
                defaultOutlookCalender = nameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
            }

            // Get outlook Items
            outlookItems = defaultOutlookCalender.Items;

            if (outlookItems != null)
            {
                //Add Filter to outlookItems to limit and Avoid endless loop(appointments without end date)
                outlookItems.Sort("[Start]", Type.Missing);
                outlookItems.IncludeRecurrences = true;

                DateTime min = DateTime.Now.AddDays(-daysInPast);
                DateTime max = DateTime.Now.AddDays(+(daysInFuture + 1));

                // create Final filter as string
                string filter = "[End] >= '" + min.ToString("g") + "' AND [Start] < '" + max.ToString("g") + "'";

                //Set filter on outlookItems and Loop through to create appointment List

                outlookAppointments.AddRange(
                    outlookItems.Restrict(filter)
                        .Cast<AppointmentItem>()
                        .Select(
                            appointmentItem =>
                                new Appointment(appointmentItem.Body, appointmentItem.Location,
                                    appointmentItem.Subject, appointmentItem.End, appointmentItem.Start)
                                {
                                    AllDayEvent = appointmentItem.AllDayEvent,
                                    OptionalAttendees = appointmentItem.OptionalAttendees,
                                    ReminderMinutesBeforeStart = appointmentItem.ReminderMinutesBeforeStart,
                                    Organizer = appointmentItem.Organizer,
                                    ReminderSet = appointmentItem.ReminderSet,
                                    RequiredAttendees = appointmentItem.RequiredAttendees,
                                    AppointmentId = appointmentItem.IsRecurring
                                        ? string.Format("{0}_{1}", appointmentItem.EntryID, appointmentItem.Start.ToString("d"))
                                        : appointmentItem.EntryID,
                                    Visibility = (appointmentItem.Sensitivity == OlSensitivity.olNormal) ? "default" : "private",
                                    Transparency = (appointmentItem.BusyStatus == OlBusyStatus.olFree) ? "transparent" : "opaque"
                                }));
            }

            //Close  and Cleanup

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
            return new AppointmentListWrapper
            {
                Appointments = outlookAppointments,
                WaitForApplicationQuit = disposeOutlookInstances
            };
        }

        private List<Appointment> GetAppointments(int daysInPast, int daysInFuture, string profileName,
            OutlookCalendar outlookCalendar)
        {
            AppointmentListWrapper list = GetOutlookEntriesForSelectedTimeRange(daysInPast, daysInFuture, profileName,
                outlookCalendar);
            if (!list.WaitForApplicationQuit)
            {
                return list.Appointments;
            }
            while (Process.GetProcessesByName("OUTLOOK").Any())
            {
                TaskEx.Delay(5000);
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

        #endregion

        #region IOutlookCalendarService Members

        public List<OutlookMailBox> GetAllMailBoxes(string profileName = "")
        {
            ProfileName = profileName;
            bool disposeOutlookInstances;
            Microsoft.Office.Interop.Outlook.Application application;
            NameSpace nameSpace;
            GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);
            Folders rootFolders = nameSpace.Folders;
            List<OutlookMailBox> mailBoxes = GetOutlookMailBoxes(rootFolders);
            //Close  and Cleanup
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
                TaskEx.Delay(5000);
            }

            return mailBoxes;
        }

        public async Task<List<string>> GetOutLookProfieListAsync()
        {
            return await Task<List<string>>.Factory.StartNew(GetOutlookProfileList);
        }


        public async Task<List<Appointment>> GetCalendarEventsInRangeAsync(int daysInPast, int daysInFuture,
            IDictionary<string, object> calendarSpecificData)
        {
            CheckCalendarSpecificData(calendarSpecificData);
            //Get Outlook Entries
            List<Appointment> appointmentList =
                await
                    Task<List<Appointment>>.Factory.StartNew(
                        () => GetAppointments(daysInPast, daysInFuture, ProfileName, OutlookCalendar));
            return appointmentList;
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



        public async Task<bool> AddCalendarEvent(List<Appointment> calenderAppointments, bool addDescription, bool addReminder,
            bool addAttendees,
            IDictionary<string, object> calendarSpecificData)
        {
            if (calendarSpecificData == null)
            {
                return false;
            }
            object data;
            if (!calendarSpecificData.TryGetValue("OutlookCalendar", out data))
            {
                return false;
            }
            OutlookCalendar outlookCalendar = data as OutlookCalendar;
            if (!calendarSpecificData.TryGetValue("ProfileName", out data))
            {
                return false;
            }
            string profileName = data as string;

            bool disposeOutlookInstances;
            Microsoft.Office.Interop.Outlook.Application application = null;
            NameSpace nameSpace = null;
            MAPIFolder defaultOutlookCalender = null;
            Items outlookItems = null;
            var outlookAppointments = new List<Appointment>();
            // Get Application and Namespace
            GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, profileName);

            // Get Default Calender
            if (outlookCalendar != null)
            {
                defaultOutlookCalender = nameSpace.GetFolderFromID(outlookCalendar.EntryId, outlookCalendar.StoreId);
            }
            else
            {
                defaultOutlookCalender = nameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
            }

            foreach (var calenderAppointment in calenderAppointments)
            {
                AppointmentItem appt = application.CreateItem(OlItemType.olAppointmentItem) as AppointmentItem;
                appt.Subject = calenderAppointment.Subject;
                appt.MeetingStatus = OlMeetingStatus.olMeeting;
                appt.Location = calenderAppointment.Location;
                if (calenderAppointment.AllDayEvent)
                {
                    appt.AllDayEvent = true;
                }
                else
                {
                    appt.Start = calenderAppointment.StartTime.GetValueOrDefault();
                    appt.End = calenderAppointment.EndTime.GetValueOrDefault();
                }
                Recipient recipRequired =
                    appt.Recipients.Add("Ryan Gregg");
                recipRequired.Type =
                    (int)OlMeetingRecipientType.olRequired;
                Recipient recipOptional =
                    appt.Recipients.Add("Peter Allenspach");
                recipOptional.Type =
                    (int)OlMeetingRecipientType.olOptional;
                Recipient recipConf =
                   appt.Recipients.Add("Conf Room 36/2021 (14) AV");
                recipConf.Type =
                    (int)OlMeetingRecipientType.olResource;
                appt.Recipients.ResolveAll();
                appt.Display(false);
                defaultOutlookCalender.Items.Add(appt);
            }
            
            //Close  and Cleanup

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
            return true;
        }

        //private AppointmentItem CreateOutlookAppointmentItem(Appointment calendarAppointment)
        //{
        //    AppointmentItem appointmentItem = new AppointmentItemClass()
        //}


        public Task<List<Calendar>> GetAvailableCalendars(IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddCalendarEvent(Appointment calenderAppointment, bool addDescription, bool addReminder,
            bool addAttendees,
            IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteCalendarEvent(Appointment calendarAppointment, IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteCalendarEvent(List<Appointment> calendarAppointments,
            IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}