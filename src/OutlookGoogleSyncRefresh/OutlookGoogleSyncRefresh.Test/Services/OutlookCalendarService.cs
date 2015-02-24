using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Test.Model;

namespace Test.Services
{
    public class OutlookCalendarService
    {
        private void GetOutlookApplication(out bool disposeOutlookInstances, out Microsoft.Office.Interop.Outlook.Application application,
            out Microsoft.Office.Interop.Outlook.NameSpace nameSpace)
        {

            // Check whether there is an Outlook process running.
            if (Process.GetProcessesByName("OUTLOOK").Any())
            {

                // If so, use the GetActiveObject method to obtain the process and cast it to an Application object.
                application = Marshal.GetActiveObject("Outlook.Application") as Microsoft.Office.Interop.Outlook.Application;
                disposeOutlookInstances = false;
                nameSpace = null;
                if (application != null) nameSpace = application.GetNamespace("MAPI");

            }
            else
            {

                // If not, create a new instance of Outlook and log on to the default profile.
                application = new Microsoft.Office.Interop.Outlook.Application();
                nameSpace = application.GetNamespace("MAPI");
                nameSpace.Logon("", "", false, true);
                disposeOutlookInstances = true;
            }
        }

        private AppointmentListWrapper GetOutlookEntriesForSelectedTimeRange(int daysInPast, int daysInFuture)
        {
            bool disposeOutlookInstances;
            Microsoft.Office.Interop.Outlook.Application application = null;
            Microsoft.Office.Interop.Outlook.NameSpace nameSpace = null;
            Microsoft.Office.Interop.Outlook.MAPIFolder defaultOutlookCalender = null;
            Microsoft.Office.Interop.Outlook.Items outlookItems = null;
            var outlookAppointments = new List<Appointment>();
            // Get Application and Namespace
            GetOutlookApplication(out disposeOutlookInstances, out application, out nameSpace);

            // Get Default Calender
            defaultOutlookCalender = nameSpace.GetDefaultFolder(Microsoft.Office.Interop.Outlook.OlDefaultFolders.olFolderCalendar);

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
                        .Cast<Microsoft.Office.Interop.Outlook.AppointmentItem>()
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
                ((Microsoft.Office.Interop.Outlook._Application)application).Quit();
                Marshal.FinalReleaseComObject(application);
            }
            application = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return new AppointmentListWrapper()
            {
                Appointments = outlookAppointments,
                WaitForApplicationQuit = disposeOutlookInstances
            };
        }

        public async Task<List<Appointment>> GetOutlookAppointmentsAsync(int daysInPast, int daysInFuture)
        {
            //Get Outlook Entries
            List<Appointment> appointmentList =
                await Task.Run(() => GetAppointments(daysInPast, daysInFuture));
            return appointmentList;
        }

        private List<Appointment> GetAppointments(int daysInPast, int daysInFuture)
        {
            var list = GetOutlookEntriesForSelectedTimeRange(daysInPast, daysInFuture);
            if (!list.WaitForApplicationQuit) return list.Appointments;
            while (Process.GetProcessesByName("OUTLOOK").Any())
            {
                Task.Delay(5000);
            }
            return list.Appointments;
        }

        
    }
}