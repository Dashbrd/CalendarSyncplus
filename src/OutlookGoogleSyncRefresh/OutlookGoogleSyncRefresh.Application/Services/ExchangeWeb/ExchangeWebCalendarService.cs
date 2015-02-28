using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
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

        public async Task<List<AppAppointment>> GetAppointmentsAsync(int daysInPast, int daysInFuture,
            string profileName, OutlookCalendar outlookCalendar)
        {
            ExchangeService service = GetExchangeService("","");
            FindItemsResults<Appointment> outlookItems;
            DateTime startDate = DateTime.Now.AddDays(-daysInPast);
            DateTime endDate = DateTime.Now.AddDays(+(daysInFuture + 1));
            var calendarview = new CalendarView(startDate, endDate);

            // Get Default Calendar
            var outlookAppointments = new List<AppAppointment>();
            outlookItems = service.FindAppointments(outlookCalendar.StoreId, calendarview);


            if (outlookItems != null)
            {
                outlookAppointments.AddRange(
                    outlookAppointments.Select(
                        appointmentItem =>
                            new AppAppointment(appointmentItem.Description, appointmentItem.Location,
                                appointmentItem.Subject, appointmentItem.EndTime, appointmentItem.StartTime)
                            {
                                AllDayEvent = appointmentItem.AllDayEvent,
                                OptionalAttendees = appointmentItem.OptionalAttendees,
                                ReminderMinutesBeforeStart = appointmentItem.ReminderMinutesBeforeStart,
                                Organizer = appointmentItem.Organizer,
                                ReminderSet = appointmentItem.ReminderSet,
                                RequiredAttendees = appointmentItem.RequiredAttendees,
                            }));
            }


            return outlookAppointments;
        }

        public async Task<List<OutlookCalendar>> GetCalendarsAsync()
        {
            ExchangeService service = GetExchangeService("","");

            // Create a new folder view, and pass in the maximum number of folders to return.
            var view = new FolderView(1000);

            // Create an extended property definition for the PR_ATTR_HIDDEN property,
            // so that your results will indicate whether the folder is a hidden folder.
            var isHiddenProp = new ExtendedPropertyDefinition(0x10f4, MapiPropertyType.Boolean);

            // As a best practice, limit the properties returned to only those required.
            // In this case, return the folder ID, DisplayName, and the value of the isHiddenProp
            // extended property.
            view.PropertySet = new PropertySet(BasePropertySet.IdOnly, FolderSchema.DisplayName, isHiddenProp);

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

        public ExchangeService GetExchangeService(string username,string password = null)
        {
            var service = new ExchangeService(ExchangeVersion.Exchange2013_SP1);
            service.Credentials = new NetworkCredential(username,password);
            service.AutodiscoverUrl(username);
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