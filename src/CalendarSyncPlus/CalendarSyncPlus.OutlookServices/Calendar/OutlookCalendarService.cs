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
using CalendarSyncPlus.Domain.Helpers;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.OutlookServices.Utilities;
using CalendarSyncPlus.OutlookServices.Wrappers;
using CalendarSyncPlus.Services.Calendars.Interfaces;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Win32;
using Category = CalendarSyncPlus.Domain.Models.Category;
using Exception = System.Exception;
using Recipient = Microsoft.Office.Interop.Outlook.Recipient;
using ThreadingTask = System.Threading.Tasks.Task;
#endregion

namespace CalendarSyncPlus.OutlookServices.Calendar
{
    [Export(typeof (ICalendarService)), Export(typeof (IOutlookCalendarService))]
    [ExportMetadata("ServiceType", ServiceType.OutlookDesktop)]
    public class OutlookCalendarService : IOutlookCalendarService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationLogger"></param>
        [ImportingConstructor]
        public OutlookCalendarService(ApplicationLogger applicationLogger)
        {
            Logger = applicationLogger.GetLogger(GetType());
        }
        #region Properties

        public ILog Logger { get; set; }

        private OutlookFolder OutlookCalendar { get; set; }

        public bool AddAsAppointments { get; set; }

        public bool SetOrganizer { get; set; }

        private string ProfileName { get; set; }

        private Category EventCategory { get; set; }

        public string CalendarServiceName
        {
            get { return "Outlook"; }
        }

        #endregion

        #region Private Methods
        private bool AddEvents(List<Appointment> calendarAppointments, bool addDescription,
            bool addReminder,
            bool addAttendees, bool attendeesToDescription, List<Appointment> addedAppointment)
        {
            var wrapper = AddEventsToOutlook(calendarAppointments, addDescription, addReminder,
                addAttendees, attendeesToDescription, addedAppointment);

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

        /// <summary>
        /// </summary>
        /// <param name="calendarAppointments"></param>
        /// <param name="addDescription"></param>
        /// <param name="addReminder"></param>
        /// <param name="addAttendees"></param>
        /// <param name="attendeesToDescription"></param>
        /// <param name="addedAppointment"></param>
        /// <returns>
        /// </returns>
        private OutlookAppointmentsWrapper AddEventsToOutlook(List<Appointment> calendarAppointments, bool addDescription,
            bool addReminder, bool addAttendees, bool attendeesToDescription, List<Appointment> addedAppointment)
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
                defaultOutlookCalendar = OutlookCalendar != null
                    ? nameSpace.GetFolderFromID(OutlookCalendar.EntryId, OutlookCalendar.StoreId)
                    : nameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
                outlookItems = defaultOutlookCalendar.Items;

                if (EventCategory != null)
                {
                    if (nameSpace.Categories[EventCategory.CategoryName] == null)
                    {
                        nameSpace.Categories.Add(EventCategory.CategoryName,
                            CategoryHelper.GetOutlookColor(EventCategory.HexValue),
                            OlCategoryShortcutKey.olCategoryShortcutKeyNone);
                    }
                    else
                    {
                        nameSpace.Categories[EventCategory.CategoryName].Color =
                            CategoryHelper.GetOutlookColor(EventCategory.HexValue);
                    }
                }
                string id = defaultOutlookCalendar.EntryID;
                foreach (var calendarAppointment in calendarAppointments)
                {
                    var appItem = outlookItems.Add(OlItemType.olAppointmentItem) as AppointmentItem;
                    if (appItem == null)
                    {
                        continue;
                    }
                    var newAppointment = AddAppointment(addDescription, addReminder, addAttendees,
                        attendeesToDescription, appItem,
                        calendarAppointment, id);
                    addedAppointment.Add(newAppointment);
                    Marshal.FinalReleaseComObject(appItem);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return new OutlookAppointmentsWrapper
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
            return new OutlookAppointmentsWrapper
            {
                WaitForApplicationQuit = disposeOutlookInstances,
                Success = true
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="addDescription"></param>
        /// <param name="addReminder"></param>
        /// <param name="addAttendees"></param>
        /// <param name="attendeesToDescription"></param>
        /// <param name="appItem"></param>
        /// <param name="calendarAppointment"></param>
        private Appointment AddAppointment(bool addDescription, bool addReminder, bool addAttendees,
            bool attendeesToDescription, AppointmentItem appItem,
            Appointment calendarAppointment, string id)
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
                appItem.Sensitivity = calendarAppointment.GetAppointmentSensitivity();
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
                
                Recipient organizer = null;
                if (addAttendees && !attendeesToDescription)
                {
                    calendarAppointment.RequiredAttendees?.ForEach(rcptName =>
                    {
                        var recipient =
                            appItem.Recipients.Add($"{rcptName.Name}<{rcptName.Email}>");
                        if (SetOrganizer && calendarAppointment.Organizer != null &&
                            rcptName.Name.Equals(calendarAppointment.Organizer.Name))
                        {
                            recipient.Type = (int) OlMeetingRecipientType.olOrganizer;
                            recipient.Resolve();
                            organizer = recipient;
                        }
                        else
                        {
                            recipient.Type = (int) OlMeetingRecipientType.olRequired;
                            recipient.Resolve();
                        }
                    });

                    calendarAppointment.OptionalAttendees?.ForEach(rcptName =>
                    {
                        var recipient =
                            appItem.Recipients.Add($"{rcptName.Name}<{rcptName.Email}>");
                        if (SetOrganizer && calendarAppointment.Organizer != null &&
                            rcptName.Name.Equals(calendarAppointment.Organizer.Name))
                        {
                            recipient.Type = (int) OlMeetingRecipientType.olOrganizer;
                            recipient.Resolve();
                            organizer = recipient;
                        }
                        else
                        {
                            recipient.Type = (int)OlMeetingRecipientType.olOptional;
                            recipient.Resolve();
                        }
                    });
                }
                else if (SetOrganizer && calendarAppointment.Organizer != null)
                {
                    var recipient =
                                appItem.Recipients.Add(
                                    $"{calendarAppointment.Organizer.Name}<{calendarAppointment.Organizer.Email}>");
                    recipient.Type = (int)OlMeetingRecipientType.olOrganizer;
                    recipient.Resolve();
                    organizer = recipient;
                }

                SetAppointmentOrganizer(appItem, organizer);

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

                createdAppointment = GetAppointmentFromItem(id, appItem);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
            finally
            {
                if (userProperties != null)
                {
                    Marshal.FinalReleaseComObject(userProperties);
                }
                if (recipients != null)
                {
                    Marshal.FinalReleaseComObject(recipients);
                }
                if (appItem != null)
                {
                    Marshal.FinalReleaseComObject(appItem);
                }
            }
            return createdAppointment;
        }

        private void SetAppointmentOrganizer(AppointmentItem appItem, Recipient organizer)
        {
            try
            {
                if (appItem == null || organizer == null || !SetOrganizer)
                    return;

                const string PR_SENT_REPRESENTING_NAME = "http://schemas.microsoft.com/mapi/proptag/0x0042001F";
                const string PR_SENT_REPRESENTING_ENTRY_ID = "http://schemas.microsoft.com/mapi/proptag/0x00410102";
                const string PR_SENDER_ENTRYID = "http://schemas.microsoft.com/mapi/proptag/0x0C190102";
                
                PropertyAccessor pa = appItem.PropertyAccessor;
                pa.SetProperty(PR_SENDER_ENTRYID, pa.StringToBinary(organizer.EntryID));
                pa.SetProperty(PR_SENT_REPRESENTING_NAME, organizer.Name);
                pa.SetProperty(PR_SENT_REPRESENTING_ENTRY_ID, pa.StringToBinary(organizer.EntryID));
                
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }


        private bool DeleteEvents(List<Appointment> calendarAppointments, List<Appointment> deletedAppointments)
        {
            var wrapper = DeleteEventsFromOutlook(calendarAppointments,deletedAppointments);

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

        private OutlookAppointmentsWrapper DeleteEventsFromOutlook(List<Appointment> calendarAppointments, List<Appointment> deletedAppointments)
        {
            var disposeOutlookInstances = false;
            Application application = null;
            NameSpace nameSpace = null;

            try
            {
                // Get Application and Namespace
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);


                foreach (var calendarAppointment in calendarAppointments)
                {
                    try
                    {
                        AppointmentItem appointmentItem = null;
                        if (calendarAppointment.IsRecurring)
                        {
                            var idArray = calendarAppointment.AppointmentId.Split(new[] {"_"},
                                StringSplitOptions.RemoveEmptyEntries);
                            var parentAppointment = nameSpace.GetItemFromID(idArray.FirstOrDefault()) as AppointmentItem;
                            if (parentAppointment != null)
                            {
                                var pattern = parentAppointment.GetRecurrencePattern();
                                appointmentItem =
                                    pattern.GetOccurrence(calendarAppointment.StartTime.GetValueOrDefault());
                            }
                        }
                        else
                        {
                            appointmentItem =
                                nameSpace.GetItemFromID(calendarAppointment.AppointmentId) as AppointmentItem;
                        }

                        if (appointmentItem != null)
                        {
                            appointmentItem.Delete();
                            Marshal.FinalReleaseComObject(appointmentItem);
                            deletedAppointments.Add(calendarAppointment);
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
                return new OutlookAppointmentsWrapper
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
            return new OutlookAppointmentsWrapper
            {
                WaitForApplicationQuit = disposeOutlookInstances,
                Success = true
            };
        }

        private bool UpdateEvents(List<Appointment> calendarAppointments, bool addDescription,
            bool addReminder,
            bool addAttendees, bool attendeesToDescription, List<Appointment> updatedAppointments)
        {
            var wrapper = UpdateEventsToOutlook(calendarAppointments, addDescription, addReminder,
                addAttendees, attendeesToDescription, updatedAppointments);

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

        /// <summary>
        /// </summary>
        /// <param name="calendarAppointments"></param>
        /// <param name="addDescription"></param>
        /// <param name="addReminder"></param>
        /// <param name="addAttendees"></param>
        /// <param name="attendeesToDescription"></param>
        /// <param name="updatedAppointments"></param>
        /// <returns>
        /// </returns>
        private OutlookAppointmentsWrapper UpdateEventsToOutlook(List<Appointment> calendarAppointments, bool addDescription,
            bool addReminder, bool addAttendees, bool attendeesToDescription, List<Appointment> updatedAppointments)
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
                defaultOutlookCalendar = OutlookCalendar != null
                    ? nameSpace.GetFolderFromID(OutlookCalendar.EntryId, OutlookCalendar.StoreId)
                    : nameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
                outlookItems = defaultOutlookCalendar.Items;


                foreach (var calendarAppointment in calendarAppointments)
                {
                    try
                    {
                        AppointmentItem appItem = null;
                        if (calendarAppointment.IsRecurring)
                        {
                            var idArray = calendarAppointment.AppointmentId.Split(new[] {"_"},
                                StringSplitOptions.RemoveEmptyEntries);
                            var parentAppointment = nameSpace.GetItemFromID(idArray.FirstOrDefault()) as AppointmentItem;
                            var pattern = parentAppointment.GetRecurrencePattern();
                            var startTime = calendarAppointment.OldStartTime == null
                                ? calendarAppointment.StartTime.GetValueOrDefault()
                                : calendarAppointment.OldStartTime.GetValueOrDefault();
                            appItem = pattern.GetOccurrence(startTime);
                        }
                        else
                        {
                            appItem = nameSpace.GetItemFromID(calendarAppointment.AppointmentId) as AppointmentItem;
                        }

                        if (appItem == null)
                        {
                            continue;
                        }
                        var success = UpdateAppointment(addDescription, addReminder, addAttendees, attendeesToDescription, appItem,
                            calendarAppointment);
                        if (success)
                        {
                            updatedAppointments.Add(calendarAppointment);
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
                return new OutlookAppointmentsWrapper
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
            return new OutlookAppointmentsWrapper
            {
                WaitForApplicationQuit = disposeOutlookInstances,
                Success = true
            };
        }

        private bool UpdateAppointment(bool addDescription, bool addReminder, bool addAttendees,
            bool attendeesToDescription, AppointmentItem appItem,
            Appointment calendarAppointment)
        {
            Recipients recipients = null;
            UserProperties userProperties = null;
            try
            {
                appItem.Subject = calendarAppointment.Subject;
                appItem.Location = calendarAppointment.Location;
                appItem.BusyStatus = calendarAppointment.GetOutlookBusyStatus();
                
                if (EventCategory != null)
                {
                    appItem.Categories = EventCategory.CategoryName;
                }

                if (calendarAppointment.AllDayEvent != appItem.AllDayEvent)
                {
                    appItem.AllDayEvent = calendarAppointment.AllDayEvent;
                }

                appItem.Sensitivity = calendarAppointment.GetAppointmentSensitivity();
                appItem.Start = calendarAppointment.StartTime.GetValueOrDefault();
                appItem.End = calendarAppointment.EndTime.GetValueOrDefault();
                if (addDescription)
                {
                    appItem.Body = calendarAppointment.Description;
                }

                if (addAttendees && !attendeesToDescription)
                {
                    recipients = appItem.Recipients;
                    if (calendarAppointment.RequiredAttendees != null)
                    {
                        calendarAppointment.RequiredAttendees.ForEach(rcptName =>
                        {
                            if (!CheckIfRecipientExists(recipients, rcptName))
                            {
                                var recipient =
                                    appItem.Recipients.Add($"{rcptName.Name}<{rcptName.Email}>");
                                recipient.Type = (int) OlMeetingRecipientType.olRequired;
                                recipient.Resolve();
                            }
                        });
                    }

                    if (calendarAppointment.OptionalAttendees != null)
                    {
                        calendarAppointment.OptionalAttendees.ForEach(rcptName =>
                        {
                            if (!CheckIfRecipientExists(recipients, rcptName))
                            {
                                var recipient =
                                    appItem.Recipients.Add($"{rcptName.Name}<{rcptName.Email}>");
                                recipient.Type = (int) OlMeetingRecipientType.olOptional;
                                recipient.Resolve();
                            }
                        });
                    }
                }
                

                if (addReminder)
                {
                    if (appItem.ReminderSet != calendarAppointment.ReminderSet)
                    {
                        appItem.ReminderMinutesBeforeStart = calendarAppointment.ReminderMinutesBeforeStart;
                        if (calendarAppointment.ReminderSet &&
                            appItem.ReminderMinutesBeforeStart != calendarAppointment.ReminderMinutesBeforeStart)
                        {
                            appItem.ReminderMinutesBeforeStart = calendarAppointment.ReminderMinutesBeforeStart;
                        }
                    }
                }

                userProperties = appItem.UserProperties;
                if (userProperties != null)
                {
                    for (var i = 0; i < userProperties.Count; i++)
                    {
                        userProperties.Remove(i + 1);
                    }

                    foreach (var extendedProperty in calendarAppointment.ExtendedProperties)
                    {
                        var sourceProperty = userProperties.Add(extendedProperty.Key,
                            OlUserPropertyType.olText);
                        sourceProperty.Value = extendedProperty.Value;
                    }
                }

                appItem.Save();
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return false;
            }
            finally
            {
                if (recipients != null)
                {
                    Marshal.FinalReleaseComObject(recipients);
                }
                if (userProperties != null)
                {
                    Marshal.FinalReleaseComObject(userProperties);
                }
                if (appItem != null)
                {
                    Marshal.FinalReleaseComObject(appItem);
                }
            }
            return true;
        }

        private bool CheckIfRecipientExists(Recipients recipients, Domain.Models.Attendee rcptName)
        {
            bool recipientFound = false;
            foreach (Recipient attendee in recipients)
            {
                string name, email;
                if (!attendee.GetEmailFromName(out name, out email))
                {
                    name = attendee.Name;
                    email = GetSMTPAddressForRecipients(attendee);
                }

                if (rcptName.Email.Equals(email))
                {
                    recipientFound = true;
                    break;
                }
            }
            return recipientFound;
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

        private OutlookAppointmentsWrapper GetOutlookEntriesForSelectedTimeRange(DateTime startDate, DateTime endDate, bool skipPrivateEntries)
        {
            var disposeOutlookInstances = false;
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
                    Logger.Info("Outlook filter string : "+filter);
                    //Set filter on outlookItems and Loop through to create appointment List
                    var outlookEntries = outlookItems.Restrict(filter);
                    if (outlookEntries != null)
                    {
                        var appts = outlookEntries.Cast<AppointmentItem>();
                        var appointmentItems = appts as AppointmentItem[] ?? appts.ToArray();
                        if (appointmentItems.Any())
                        {
                            var id = defaultOutlookCalendar.EntryID;
                            foreach (var appointmentItem in appointmentItems)
                            {
                                try
                                {
                                    if (skipPrivateEntries && (appointmentItem.Sensitivity == OlSensitivity.olPrivate ||
                                                               appointmentItem.Sensitivity ==
                                                               OlSensitivity.olConfidential))
                                    {
                                        continue;
                                    }

                                    var app = GetAppointmentFromItem(id, appointmentItem);
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
                    else
                    {
                        Logger.Warn("Outlook items null, check short date & time format in date time settings of your system.");
                    }
                }
                else
                {
                    Logger.Warn("Outlook items null, check short date & time format in date time settings of your system.");
                }
                
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return new OutlookAppointmentsWrapper
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

            return new OutlookAppointmentsWrapper
            {
                Appointments = outlookAppointments,
                WaitForApplicationQuit = disposeOutlookInstances
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="appointmentItem"></param>
        /// <returns>
        /// </returns>
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
                    ? $"{appointmentItem.EntryID}_{appointmentItem.Start.ToString("yy-MM-dd")}"
                    : appointmentItem.EntryID,
                Privacy = appointmentItem.GetAppointmentSensitivity(),
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
            var userProperties = appointmentItem.UserProperties;
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
                Logger.Error(exception);
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
        /// </summary>
        /// <param name="appointmentItem"></param>
        /// <param name="app"></param>
        private void GetRecipients(AppointmentItem appointmentItem, Appointment app)
        {
            foreach (Recipient attendee in appointmentItem.Recipients)
            {
                var recipient = new Attendee();
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

        private List<Appointment> GetAppointments(DateTime startDate, DateTime endDate, bool skipPrivateEntries)
        {
            var list = GetOutlookEntriesForSelectedTimeRange(startDate, endDate, skipPrivateEntries);
            if (!list.WaitForApplicationQuit)
            {
                return list.Appointments;
            }
            while (Process.GetProcessesByName("OUTLOOK").Any())
            {
                ThreadingTask.Delay(5000);
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
                    try
                    {
                        if (rootFolder == null)
                        {
                            continue;
                        }
                        
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
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception);
                    }
                    finally
                    {
                        Marshal.FinalReleaseComObject(rootFolder);
                    }
                }
            }
            return mailBoxes;
        }

        private string GetSMTPAddressForRecipients(Recipient recip)
        {
            const string PR_SMTP_ADDRESS =
                "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";

            var pa = recip.PropertyAccessor;
            var smtpAddress = "fake.email.generated@for.user";
            try
            {
                smtpAddress = pa.GetProperty(PR_SMTP_ADDRESS).ToString();
            }
            catch (Exception exception)
            {
                Logger.Warn(
                    $"Unable to retrieve Email for the User : {recip.Name}{Environment.NewLine}{exception.Message}");
            }
            return smtpAddress;
        }

        private void GetCalendars(MAPIFolder searchFolder, List<OutlookFolder> outlookCalendars)
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

        private List<string> GetOutlookProfileList()
        {
            var profileList = new List<string>();
            const string defaultProfilePath =
                @"Software\Microsoft\Windows NT\CurrentVersion\Windows Messaging Subsystem\Profiles";
            const string newProfilePath = @"Software\Microsoft\Office\{0}\Outlook\Profiles";

            string office2016Path = string.Format(newProfilePath, "16.0");
            string office2013Path = string.Format(newProfilePath, "15.0");

            var pathList = new List<string>() {office2013Path, office2016Path};


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

            foreach (string profilePath in pathList)
            {
                var newregKey = Registry.CurrentUser.OpenSubKey(profilePath, RegistryKeyPermissionCheck.Default);

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
                ThreadingTask.Delay(5000);
            }
        }

        private OutlookAppointmentsWrapper SetColorForSelectedCalendar(Category background)
        {
            var disposeOutlookInstances = false;
            Application application = null;
            NameSpace nameSpace = null;

            //Close  and Shutdown
            try
            {
                // Get Application and Namespace
                GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace, ProfileName);

                if (nameSpace.Categories[background.CategoryName] == null)
                {
                    nameSpace.Categories.Add(background.CategoryName,
                        CategoryHelper.GetOutlookColor(background.HexValue),
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
                Logger.Error(exception);
                return new OutlookAppointmentsWrapper
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

            return new OutlookAppointmentsWrapper
            {
                Appointments = null,
                WaitForApplicationQuit = disposeOutlookInstances
            };
        }

        #endregion

        #region IOutlookCalendarService Members
        public async Task<AppointmentsWrapper> UpdateCalendarEvents(List<Appointment> calendarAppointments, bool addDescription,
          bool addReminder, bool addAttendees, bool attendeesToDescription,
          IDictionary<string, object> calendarSpecificData)
        {
            var updateAppointments = new AppointmentsWrapper();
            if (!calendarAppointments.Any())
            {
                updateAppointments.IsSuccess = true;
                return updateAppointments;
            }
            CheckCalendarSpecificData(calendarSpecificData);

            var result = await
                Task<bool>.Factory.StartNew(
                    () =>
                        UpdateEvents(calendarAppointments, addDescription, addReminder, addAttendees,
                            attendeesToDescription, updateAppointments));
            updateAppointments.IsSuccess = result;
            return updateAppointments;
        }

        public async void SetCalendarColor(Category background, IDictionary<string, object> calendarSpecificData)
        {
            CheckCalendarSpecificData(calendarSpecificData);

            await ThreadingTask.Factory.StartNew(
                () => SetColor(background));
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
                if (rootFolders != null)
                {
                    mailBoxes = GetOutlookMailBoxes(rootFolders);
                }
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

        public async Task<List<string>> GetOutLookProfileListAsync()
        {
            return await Task<List<string>>.Factory.StartNew(GetOutlookProfileList);
        }


        public async Task<AppointmentsWrapper> GetCalendarEventsInRangeAsync(DateTime startDate, DateTime endDate,bool skipPrivateEntries,
            IDictionary<string, object> calendarSpecificData)
        {
            CheckCalendarSpecificData(calendarSpecificData);
            var calendarAppointments = new AppointmentsWrapper();

            var appointmentList =
                await
                    Task<List<Appointment>>.Factory.StartNew(
                        () => GetAppointments(startDate, endDate, skipPrivateEntries));

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
            OutlookCalendar = outlookCalendarValue as OutlookFolder;
            AddAsAppointments = (bool) addAsAppointments;
            object eventCategory;
            if (calendarSpecificData.TryGetValue("EventCategory", out eventCategory))
            {
                EventCategory = eventCategory as Category;
            }
            else
            {
                EventCategory = null;
            }

            object setOrganizer;
            if (calendarSpecificData.TryGetValue("SetOrganizer", out setOrganizer))
            {
                SetOrganizer = (bool)setOrganizer;
            }
            else
            {
                SetOrganizer = false;
            }
        }


        public async Task<AppointmentsWrapper> AddCalendarEvents(List<Appointment> calendarAppointments,
            bool addDescription,
            bool addReminder, bool addAttendees, bool attendeesToDescription,
            IDictionary<string, object> calendarSpecificData)
        {
            var addedAppointments = new AppointmentsWrapper();
            if (!calendarAppointments.Any())
            {
                addedAppointments.IsSuccess = true;
                return addedAppointments;
            }
            CheckCalendarSpecificData(calendarSpecificData);

            var result = await
                Task<bool>.Factory.StartNew(() => AddEvents(calendarAppointments, addDescription, addReminder, addAttendees,
                            attendeesToDescription, addedAppointments));

            addedAppointments.IsSuccess = result;
            return addedAppointments;
        }

        /// <summary>
        /// </summary>
        /// <param name="calendarAppointments"></param>
        /// <param name="calendarSpecificData"></param>
        /// <returns>
        /// </returns>
        public async Task<AppointmentsWrapper> DeleteCalendarEvents(List<Appointment> calendarAppointments,
            IDictionary<string, object> calendarSpecificData)
        {
            var deleteAppointments = new AppointmentsWrapper();
            if (!calendarAppointments.Any())
            {
                deleteAppointments.IsSuccess = true;
                return deleteAppointments;
            }
            CheckCalendarSpecificData(calendarSpecificData);
            var result = await 
                Task<bool>.Factory.StartNew(() =>
                    DeleteEvents(calendarAppointments, deleteAppointments));
            
            deleteAppointments.IsSuccess = result;
            return deleteAppointments;
        }

        public async Task<bool> ClearCalendar(IDictionary<string, object> calendarSpecificData)
        {
            var startDate = DateTime.Today.AddDays(-(10*365));
            var endDate = DateTime.Today.AddDays(10*365);
            var appointments =
                await GetCalendarEventsInRangeAsync(startDate, endDate, true,calendarSpecificData);
            if (appointments != null)
            {
                var success = await DeleteCalendarEvents(appointments, calendarSpecificData);
                return success.IsSuccess;
            }
            return false;
        }

        public async Task<bool> ResetCalendarEntries(IDictionary<string, object> calendarSpecificData)
        {
            var startDate = DateTime.Today.AddDays(-(10 * 365));
            var endDate = DateTime.Today.AddDays(10 * 365);
            var appointments =
                await GetCalendarEventsInRangeAsync(startDate, endDate,true, calendarSpecificData);
            if (appointments != null)
            {
                appointments.ForEach(t => t.ExtendedProperties = new Dictionary<string, string>());
                var success = await UpdateCalendarEvents(appointments, false, false, false, false, calendarSpecificData);
                return success.IsSuccess;
            }
            return false;
        }
        #endregion
    }
}