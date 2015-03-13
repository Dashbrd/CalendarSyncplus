using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;
using OutlookGoogleSyncRefresh.Common.Log;
using OutlookGoogleSyncRefresh.Common.MetaData;
using OutlookGoogleSyncRefresh.Domain.Models;
using AppAppointment = OutlookGoogleSyncRefresh.Domain.Models.Appointment;
using Appointment = Microsoft.Exchange.WebServices.Data.Appointment;

namespace OutlookGoogleSyncRefresh.Application.Services.ExchangeWeb
{
    [Export(typeof(ICalendarService)), Export(typeof(IExchangeWebCalendarService))]
    [ExportMetadata("ServiceType", CalendarServiceType.EWS)]
    public class ExchangeWebCalendarService : IExchangeWebCalendarService
    {
        public ApplicationLogger ApplicationLogger { get; set; }

        [ImportingConstructor]
        public ExchangeWebCalendarService(ApplicationLogger applicationLogger)
        {
            ApplicationLogger = applicationLogger;
        }

        #region IExchangeWebCalendarService Members

        public List<AppAppointment> GetAppointmentsAsync(int daysInPast, int daysInFuture,
            string profileName, OutlookCalendar outlookCalendar)
        {
            ExchangeService service = GetExchangeService(ExchangeVersion.Exchange2010_SP2);
            DateTime startDate = DateTime.Now.AddDays(-daysInPast);
            DateTime endDate = DateTime.Now.AddDays(+(daysInFuture + 1));
            var calendarview = new CalendarView(startDate, endDate);

            // Get Default Calendar
            var outlookAppointments = new List<AppAppointment>();
            FindItemsResults<Appointment> exchangeAppointments = service.FindAppointments(outlookCalendar.EntryId, calendarview);



            service.LoadPropertiesForItems(
                from Item item in exchangeAppointments select item,
                new PropertySet(BasePropertySet.FirstClassProperties) { RequestedBodyType = BodyType.Text });

            if (exchangeAppointments != null)
            {
                foreach (Appointment exchangeAppointment in exchangeAppointments)
                {
                    var appointment = new AppAppointment(exchangeAppointment.Body, exchangeAppointment.Location,
                        exchangeAppointment.Subject, exchangeAppointment.Start, exchangeAppointment.Start)
                    {
                        AppointmentId = exchangeAppointment.Id.UniqueId,
                        AllDayEvent = exchangeAppointment.IsAllDayEvent,
                        OptionalAttendees = GetAttendees(exchangeAppointment.OptionalAttendees),
                        ReminderMinutesBeforeStart = exchangeAppointment.ReminderMinutesBeforeStart,
                        Organizer =  new Recipient(){Name = exchangeAppointment.Organizer.Name, Email = exchangeAppointment.Organizer.Address},
                        ReminderSet = exchangeAppointment.IsReminderSet,
                        RequiredAttendees = GetAttendees(exchangeAppointment.RequiredAttendees),
                    };
                    outlookAppointments.Add(appointment);
                }
            }
            return outlookAppointments;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="attendeeCollection"></param>
        /// <returns></returns>
        private List<Recipient> GetAttendees(IEnumerable<Attendee> attendeeCollection)
        {
            var attendees = new List<Recipient>();

            foreach (Attendee attendee in attendeeCollection)
            {
                attendees.Add(new Recipient()
                {
                    Name = attendee.Name,Email = attendee.Address
                });
            }

            return attendees;
        }

        public List<OutlookCalendar> GetCalendarsAsync()
        {
            ExchangeService service = GetExchangeService(ExchangeVersion.Exchange2010_SP2);

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

        public ExchangeService GetExchangeService(ExchangeVersion exchangeVersion)
        {
            var service = new ExchangeService(exchangeVersion)
            {
                UseDefaultCredentials = true,
                EnableScpLookup = false,
                Url = new Uri(@"https://cas.etn.com/ews/exchange.asmx"),
            };
            service.AutodiscoverUrl("ankeshdave@outlook.com");
            //service.Credentials = new WebCredentials("user1@contoso.com", "password");
            return service;
        }

        public ExchangeVersion GetBestSuitedExchangeVersion()
        {
            var enumList = Enum.GetValues(typeof(ExchangeVersion)).Cast<ExchangeVersion>().Reverse();

            IWebProxy proxy = WebRequest.DefaultWebProxy;
            proxy.Credentials = CredentialCache.DefaultNetworkCredentials;


            var exchangeVersions = enumList as ExchangeVersion[] ?? enumList.ToArray();
            foreach (ExchangeVersion exchangeVersion in exchangeVersions)
            {
                var service = new ExchangeService(exchangeVersion)
                {
                    UseDefaultCredentials = true,
                    WebProxy = proxy
                };

                try
                {
                    service.TraceEnabled = true;
                    service.AutodiscoverUrl("ankeshdave@outlook.com");
                    CalendarFolder calendarFolder = CalendarFolder.Bind(service, WellKnownFolderName.Calendar);

                    var result = calendarFolder.FindAppointments(new CalendarView(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1)));

                    return exchangeVersion;
                }
                catch (Exception exception)
                {
                    ApplicationLogger.LogError(exception.ToString());
                    continue;
                }
            }
            return exchangeVersions.ElementAtOrDefault((exchangeVersions.Count() - 1));
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
                    EntryId = searchFolder.Id.UniqueId,
                    StoreId = searchFolder.ParentFolderId.UniqueId
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



        public string CalendarServiceName
        {
            get { return "Exchange Server"; }
        }

        public Task<bool> AddCalendarEvent(AppAppointment calendarAppointment, bool addDescription, bool addReminder, bool addAttendees, IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteCalendarEvent(AppAppointment calendarAppointment, IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteCalendarEvent(List<AppAppointment> calendarAppointments, IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<List<AppAppointment>> GetCalendarEventsInRangeAsync(int daysInPast, int daysInFuture, IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddCalendarEvent(List<AppAppointment> calenderAppointments, bool addDescription, bool addReminder, bool addAttendees, IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public void CheckCalendarSpecificData(IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }
    }
}