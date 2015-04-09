using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Services;
using CalendarSyncPlus.Services.Interfaces;
using CalendarSyncPlus.Services.Wrappers;
using Microsoft.Exchange.WebServices.Data;
using AppAppointment = CalendarSyncPlus.Domain.Models.Appointment;
using Appointment = Microsoft.Exchange.WebServices.Data.Appointment;

namespace CalendarSyncPlus.ExchangeWebServices.ExchangeWeb
{
    [Export(typeof(ICalendarService)), Export(typeof(IExchangeWebCalendarService))]
    [ExportMetadata("ServiceType", CalendarServiceType.EWS)]
    public class ExchangeWebCalendarService : IExchangeWebCalendarService
    {
        [ImportingConstructor]
        public ExchangeWebCalendarService(ApplicationLogger applicationLogger)
        {
            ApplicationLogger = applicationLogger;
        }

        public ApplicationLogger ApplicationLogger { get; set; }

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
            FindItemsResults<Appointment> exchangeAppointments = service.FindAppointments(outlookCalendar.EntryId,
                calendarview);


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
                        Organizer =
                            new Recipient
                            {
                                Name = exchangeAppointment.Organizer.Name,
                                Email = exchangeAppointment.Organizer.Address
                            },
                        ReminderSet = exchangeAppointment.IsReminderSet,
                        RequiredAttendees = GetAttendees(exchangeAppointment.RequiredAttendees),
                    };
                    outlookAppointments.Add(appointment);
                }
            }
            return outlookAppointments;
        }

        public List<EWSCalendar> GetCalendarsAsync(int maxFoldersToRetrive)
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
            view.PropertySet = new PropertySet(BasePropertySet.FirstClassProperties, FolderSchema.DisplayName,
                isHiddenProp);

            // Indicate a Traversal value of Deep, so that all subfolders are retrieved.
            view.Traversal = FolderTraversal.Deep;

            // Call FindFolders to retrieve the folder hierarchy, starting with the MsgFolderRoot folder.
            // This method call results in a FindFolder call to EWS.
            FindFoldersResults findFolderResults = service.FindFolders(WellKnownFolderName.MsgFolderRoot, view);

            var ewsCalendars = new List<EWSCalendar>();
            foreach (Folder searchFolder in findFolderResults.Folders)
            {
                GetCalendars(searchFolder, ewsCalendars, view);
            }
            return ewsCalendars;
        }


        public string CalendarServiceName
        {
            get { return "Exchange Server"; }
        }

        public Task<bool> DeleteCalendarEvent(List<AppAppointment> calendarAppointments,
            IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public async Task<CalendarAppointments> GetCalendarEventsInRangeAsync(int daysInPast, int daysInFuture,
            IDictionary<string, object> calendarSpecificData)
        {
            //CheckCalendarSpecificData(calendarSpecificData);


            //ExchangeService service = GetExchangeService(ExchangeVersion.Exchange2010_SP2);
            //DateTime startDate = DateTime.Now.AddDays(-daysInPast);
            //DateTime endDate = DateTime.Now.AddDays(+(daysInFuture + 1));
            //var calendarview = new CalendarView(startDate, endDate);

            //// Get Default Calendar
            //var outlookAppointments = new List<AppAppointment>();
            //FindItemsResults<Appointment> exchangeAppointments = await service.FindAppointments(outlookCalendar.EntryId, calendarview);


            //service.LoadPropertiesForItems(
            //    from Item item in exchangeAppointments select item,
            //    new PropertySet(BasePropertySet.FirstClassProperties) { RequestedBodyType = BodyType.Text });

            //if (exchangeAppointments != null)
            //{
            //    foreach (Appointment exchangeAppointment in exchangeAppointments)
            //    {
            //        var appointment = new AppAppointment(exchangeAppointment.Body, exchangeAppointment.Location,
            //            exchangeAppointment.Subject, exchangeAppointment.Start, exchangeAppointment.Start)
            //        {
            //            AppointmentId = exchangeAppointment.Id.UniqueId,
            //            AllDayEvent = exchangeAppointment.IsAllDayEvent,
            //            OptionalAttendees = GetAttendees(exchangeAppointment.OptionalAttendees),
            //            ReminderMinutesBeforeStart = exchangeAppointment.ReminderMinutesBeforeStart,
            //            Organizer = new Recipient() { Name = exchangeAppointment.Organizer.Name, Email = exchangeAppointment.Organizer.Address },
            //            ReminderSet = exchangeAppointment.IsReminderSet,
            //            RequiredAttendees = GetAttendees(exchangeAppointment.RequiredAttendees),
            //        };
            //        outlookAppointments.Add(appointment);
            //    }
            //}
            //return outlookAppointments;

            throw new NotImplementedException();
        }

        public Task<bool> AddCalendarEvent(List<AppAppointment> calendarAppointments, bool addDescription,
            bool addReminder, bool addAttendees,
            bool attendeesToDescription, IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public void CheckCalendarSpecificData(IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ResetCalendar(IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }


        public void SetCalendarColor(Category background, IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// </summary>
        /// <param name="attendeeCollection"></param>
        /// <returns></returns>
        private List<Recipient> GetAttendees(IEnumerable<Attendee> attendeeCollection)
        {
            var attendees = new List<Recipient>();

            foreach (Attendee attendee in attendeeCollection)
            {
                attendees.Add(new Recipient
                {
                    Name = attendee.Name,
                    Email = attendee.Address
                });
            }

            return attendees;
        }

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

        public ExchangeServerSettings GetBestSuitedExchangeServerData(string domain,string emailId,string password,bool usingCorporateNetwork = false)
        {
            ExchangeServerSettings exchangeServerSettings = null;
            IEnumerable<ExchangeVersion> enumList =
                Enum.GetValues(typeof(ExchangeVersion)).Cast<ExchangeVersion>().Reverse();

            IWebProxy proxy = WebRequest.DefaultWebProxy;
            proxy.Credentials = CredentialCache.DefaultNetworkCredentials;


            ExchangeVersion[] exchangeVersions = enumList as ExchangeVersion[] ?? enumList.ToArray();
            foreach (ExchangeVersion exchangeVersion in exchangeVersions)
            {
                var service = new ExchangeService(exchangeVersion)
                {
                    UseDefaultCredentials = usingCorporateNetwork,
                    WebProxy = proxy
                };

                if (usingCorporateNetwork)
                {
                    service.Credentials = string.IsNullOrEmpty(domain)
                        ? new WebCredentials(emailId, password)
                        : new WebCredentials(emailId, password, domain);
                }

                try
                {
                    service.AutodiscoverUrl(emailId);
                    //Try to get value form Exchange Server
                    CalendarFolder calendarFolder = CalendarFolder.Bind(service, WellKnownFolderName.Calendar);
                    exchangeServerSettings = new ExchangeServerSettings
                    {
                        ExchangeServerUrl = service.Url.AbsoluteUri,
                        ExchangeVersion = exchangeVersion.ToString(),
                        Password = password,
                        Username = emailId,
                        Domain = domain,
                        UsingCorporateNetwork = usingCorporateNetwork

                    };
                }
                catch (Exception exception)
                {
                    ApplicationLogger.LogError(exception.ToString());
                }
            }
            return exchangeServerSettings;
        }

        private void GetCalendars(Folder searchFolder, List<EWSCalendar> ewsCalendars, FolderView view)
        {
            if (searchFolder == null)
            {
                return;
            }

            if (searchFolder.FolderClass == "IPF.Appointment")
            {
                //Add Calendar MAPIFolder to List
                ewsCalendars.Add(new EWSCalendar()
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
                GetCalendars(subFolder, ewsCalendars, view);
            }
        }
    }
}