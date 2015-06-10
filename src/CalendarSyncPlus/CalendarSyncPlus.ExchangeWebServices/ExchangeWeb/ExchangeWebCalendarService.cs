using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Wrappers;
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
        private const string EWSCALENDAR = "EWSCalendar";
        private const string EXCHANGESERVERSETTINGS = "ExchangeServerSettings";
        private ExchangeServerSettings _exchangeServerSettings;
        EWSCalendar _ewsCalendar;
        private bool _addAsAppointments;
        private Category _eventCategory;

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

            return null;
        }

        public List<EWSCalendar> GetCalendarsAsync(int maxFoldersToRetrive)
        {

            ExchangeService service = GetExchangeService(null);

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

        private ExchangeServerSettings ExchangeServerSettings
        {
            get { return _exchangeServerSettings; }
            set { _exchangeServerSettings = value; }
        }

        public Task<bool> DeleteCalendarEvents(List<AppAppointment> calendarAppointments,
            IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public async Task<CalendarAppointments> GetCalendarEventsInRangeAsync(DateTime startDate, DateTime endDate,
            IDictionary<string, object> calendarSpecificData)
        {
            CheckCalendarSpecificData(calendarSpecificData);


            ExchangeService service = GetExchangeService(ExchangeServerSettings);

            var calendarview = new CalendarView(startDate, endDate);

            // Get Default Calendar
            var outlookAppointments = new CalendarAppointments();
            FindItemsResults<Appointment> exchangeAppointments = service.FindAppointments(_ewsCalendar.EntryId,
                calendarview);


            service.LoadPropertiesForItems(
                from Item item in exchangeAppointments select item,
                new PropertySet(BasePropertySet.FirstClassProperties) { RequestedBodyType = BodyType.Text, });

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

        public Task<CalendarAppointments> AddCalendarEvents(List<AppAppointment> calendarAppointments, bool addDescription,
            bool addReminder, bool addAttendees,
            bool attendeesToDescription, IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public void CheckCalendarSpecificData(IDictionary<string, object> calendarSpecificData)
        {
            if (calendarSpecificData == null)
            {
                throw new ArgumentNullException("calendarSpecificData", "Calendar Specific Data cannot be null");
            }

            object ewsCalendar;
            object serverSettings;
            object addAsAppointments;
            if (!(calendarSpecificData.TryGetValue(EWSCALENDAR, out ewsCalendar) &&
                  calendarSpecificData.TryGetValue(EXCHANGESERVERSETTINGS, out serverSettings) &&
                  calendarSpecificData.TryGetValue("AddAsAppointments", out addAsAppointments)))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "{0} {1} and {2}  keys should be present, both of them can be null in case Default Profile and Default Calendar will be used. {0} is of 'string' type, {1} is of 'OutlookCalendar' type and {2} is of bool type.",
                        EWSCALENDAR, EXCHANGESERVERSETTINGS, "AddAsAppointments"));
            }
            _ewsCalendar = ewsCalendar as EWSCalendar;
            ExchangeServerSettings = serverSettings as ExchangeServerSettings;
            _addAsAppointments = (bool)addAsAppointments;
            object eventCategory;
            if (calendarSpecificData.TryGetValue("EventCategory", out eventCategory))
            {
                _eventCategory = eventCategory as Category;
            }
            else
            {
                _eventCategory = null;
            }
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
            return attendeeCollection.Select(attendee => new Recipient
            {
                Name = attendee.Name,
                Email = attendee.Address
            }).ToList();
        }

        public ExchangeService GetExchangeService(ExchangeServerSettings exchangeServerSettings)
        {
            ExchangeVersion exchangeVersion;
            var isValidVersion = Enum.TryParse(exchangeServerSettings.ExchangeVersion, true, out exchangeVersion);
            if (isValidVersion)
            {
                var service = new ExchangeService(exchangeVersion)
                    {
                        UseDefaultCredentials = exchangeServerSettings.UsingCorporateNetwork,
                        EnableScpLookup = false,
                        Url = new Uri(exchangeServerSettings.ExchangeServerUrl)
                    };
                if (string.IsNullOrEmpty(exchangeServerSettings.ExchangeServerUrl))
                {
                    service.AutodiscoverUrl(exchangeServerSettings.EmailId);

                }
                if (!exchangeServerSettings.UsingCorporateNetwork)
                {
                    service.Credentials = new WebCredentials(exchangeServerSettings.EmailId, exchangeServerSettings.Password,
                        exchangeServerSettings.Domain);
                }
                //service.Credentials = new WebCredentials("user1@contoso.com", "password");
                return service;
            }
            return null;
        }

        public ExchangeServerSettings GetBestSuitedExchangeServerData(string domain, string emailId, string password, bool usingCorporateNetwork = false)
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
                        EmailId = emailId,
                        Domain = domain,
                        UsingCorporateNetwork = usingCorporateNetwork

                    };
                }
                catch (Exception exception)
                {
                    ApplicationLogger.LogError(exception.ToString(), typeof(ExchangeWebCalendarService));
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


        public Task<bool> UpdateCalendarEvents(List<AppAppointment> calendarAppointments, bool addDescription, bool addReminder, bool addAttendees, bool attendeesToDescription, IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }
    }
}