using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;
using OutlookGoogleSyncRefresh.Domain.Models;
using AppAppointment = OutlookGoogleSyncRefresh.Domain.Models.Appointment;
using Appointment = Microsoft.Exchange.WebServices.Data.Appointment;

namespace OutlookGoogleSyncRefresh.Application.Services.ExchangeWeb
{
    [Export(typeof (IExchangeWebCalendarService))]
    public class ExchangeWebCalendarService : IExchangeWebCalendarService
    {
        #region IExchangeWebCalendarService Members

        public List<AppAppointment> GetAppointmentsAsync(int daysInPast, int daysInFuture,
            string profileName, OutlookCalendar outlookCalendar)
        {
            ExchangeService service = GetExchangeService();
            DateTime startDate = DateTime.Now.AddDays(-daysInPast);
            DateTime endDate = DateTime.Now.AddDays(+(daysInFuture + 1));
            var calendarview = new CalendarView(startDate, endDate);

            // Get Default Calendar
            var outlookAppointments = new List<AppAppointment>();
            FindItemsResults<Appointment> exchangeAppointments = service.FindAppointments(outlookCalendar.EntryId, calendarview);

            if (exchangeAppointments != null)
            {
                foreach (Appointment exchangeAppointment in exchangeAppointments)
                {
                    exchangeAppointment.Load(new PropertySet(BasePropertySet.FirstClassProperties) { RequestedBodyType = BodyType.Text });

                    var appointment = new AppAppointment(exchangeAppointment.Body, exchangeAppointment.Location,
                        exchangeAppointment.Subject, exchangeAppointment.Start, exchangeAppointment.Start)
                    {
                        AllDayEvent = exchangeAppointment.IsAllDayEvent,
                        OptionalAttendees = GetAttendees(exchangeAppointment.OptionalAttendees),
                        ReminderMinutesBeforeStart = exchangeAppointment.ReminderMinutesBeforeStart,
                        Organizer = exchangeAppointment.Organizer.Name,
                        ReminderSet = exchangeAppointment.IsReminderSet,
                        RequiredAttendees = GetAttendees(exchangeAppointment.RequiredAttendees),
                    };
                    outlookAppointments.Add(appointment);
                }
            }

            return outlookAppointments;
        }

        private string GetAttendees(IEnumerable<Attendee> attendeeCollection)
        {
            var attendees = new StringBuilder(string.Empty);

            foreach (Attendee attendee in attendeeCollection)
            {
                attendees.Append(attendee.Name + ";");
            }

            return attendees.ToString();
        }

        public List<OutlookCalendar> GetCalendarsAsync()
        {
            ExchangeService service = GetExchangeService();

            // Create a new folder view, and pass in the maximum number of folders to return.
            var view = new FolderView(1000);

            // Create an extended property definition for the PR_ATTR_HIDDEN property,
            // so that your results will indicate whether the folder is a hidden folder.
            var isHiddenProp = new ExtendedPropertyDefinition(0x10f4, MapiPropertyType.Boolean);

            // As a best practice, limit the properties returned to only those required.
            // In this case, return the folder ID, DisplayName, and the value of the isHiddenProp
            // extended property.
            view.PropertySet = new PropertySet(BasePropertySet.FirstClassProperties, FolderSchema.DisplayName, isHiddenProp);

            // Indicate a Traversal value of Deep, so that all subfolders are retrieved.
            view.Traversal = FolderTraversal.Deep;

            // Call FindFolders to retrieve the folder hierarchy, starting with the MsgFolderRoot folder.
            // This method call results in a FindFolder call to EWS.
            FindFoldersResults findFolderResults = service.FindFolders(WellKnownFolderName.MsgFolderRoot, view);

            var outlookCalendars = new List<OutlookCalendar>();
            foreach (Folder searchFolder in findFolderResults.Folders)
            {
                GetCalendars(searchFolder, outlookCalendars, view);
            }
            return outlookCalendars;
        }

        #endregion

        public ExchangeService GetExchangeService()
        {
            var service = new ExchangeService(ExchangeVersion.Exchange2010_SP2)
            {
                UseDefaultCredentials = true,
                EnableScpLookup=false,
            };

            service.AutodiscoverUrl("ankeshdave@eaton.com");
            //service.Credentials = new WebCredentials("user1@contoso.com", "password");
            return service;
        }

        private void GetCalendars(Folder searchFolder, List<OutlookCalendar> outlookCalendars, FolderView view)
        {
            if (searchFolder == null)
            {
                return;
            }

            if (searchFolder.FolderClass == "IPF.Appointment")
            {
                //Add Calendar MAPIFolder to List
                outlookCalendars.Add(new OutlookCalendar
                {
                    Name = searchFolder.DisplayName,
                    EntryId = searchFolder.Id.ToString(),
                    StoreId = searchFolder.ParentFolderId.ToString()
                });
                return;
            }

            //Walk through all subFolders in MAPIFolder
            foreach (Folder subFolder in searchFolder.FindFolders(view))
            {
                //Get Calendar MAPIFolders
                GetCalendars(subFolder, outlookCalendars, view);
            }
        }

     
    }
}