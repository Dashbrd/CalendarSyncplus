using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.Services.Calendars.Interfaces;
using log4net;
using Microsoft.Exchange.WebServices.Data;
using Appointment = CalendarSyncPlus.Domain.Models.Appointment;

namespace CalendarSyncPlus.ExchangeWebServices.Calendar
{
    [Export(typeof (ICalendarService)), Export(typeof (IExchangeWebCalendarService))]
    [ExportMetadata("ServiceType", ServiceType.EWS)]
    public class ExchangeWebCalendarService : IExchangeWebCalendarService
    {
        private const string EWSCALENDAR = "EWSCalendar";
        private const string EXCHANGESERVERSETTINGS = "ExchangeServerSettings";
        private bool _addAsAppointments;
        private Category _eventCategory;
        private EWSCalendar _ewsCalendar;

        [ImportingConstructor]
        public ExchangeWebCalendarService(ApplicationLogger applicationLogger)
        {
            ApplicationLogger = applicationLogger.GetLogger(GetType());
        }

        public ILog ApplicationLogger { get; set; }

        public Task<AppointmentsWrapper> UpdateCalendarEvents(List<Appointment> calendarAppointments, bool addDescription,
            bool addReminder, bool addAttendees, bool attendeesToDescription,
            IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        /// <param name="attendeeCollection"></param>
        /// <returns>
        /// </returns>
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
                    service.Credentials = new WebCredentials(exchangeServerSettings.EmailId,
                        exchangeServerSettings.Password,
                        exchangeServerSettings.Domain);
                }
                //service.Credentials = new WebCredentials("user1@contoso.com", "password");
                return service;
            }
            return null;
        }

        public ExchangeServerSettings GetBestSuitedExchangeServerData(string domain, string emailId, string password,
            bool usingCorporateNetwork = false)
        {
            ExchangeServerSettings exchangeServerSettings = null;
            var enumList =
                Enum.GetValues(typeof (ExchangeVersion)).Cast<ExchangeVersion>().Reverse();

            var proxy = WebRequest.DefaultWebProxy;
            proxy.Credentials = CredentialCache.DefaultNetworkCredentials;


            var exchangeVersions = enumList as ExchangeVersion[] ?? enumList.ToArray();
            foreach (var exchangeVersion in exchangeVersions)
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
                    var calendarFolder = CalendarFolder.Bind(service, WellKnownFolderName.Calendar);
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
                    ApplicationLogger.Error(exception);
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
                ewsCalendars.Add(new EWSCalendar
                {
                    Name = searchFolder.DisplayName,
                    EntryId = searchFolder.Id.UniqueId,
                    StoreId = searchFolder.ParentFolderId.UniqueId
                });
                return;
            }

            //Walk through all subFolders in MAPIFolder
            foreach (var subFolder in searchFolder.FindFolders(view))
            {
                //Get Calendar MAPIFolders
                GetCalendars(subFolder, ewsCalendars, view);
            }
        }

        #region IExchangeWebCalendarService Members

        public List<Appointment> GetAppointmentsAsync(int daysInPast, int daysInFuture,
            string profileName, EWSCalendar outlookCalendar)
        {
            return null;
        }

        public List<EWSCalendar> GetCalendarsAsync(int maxFoldersToRetrive)
        {
            var service = GetExchangeService(null);

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
            var findFolderResults = service.FindFolders(WellKnownFolderName.MsgFolderRoot, view);

            var ewsCalendars = new List<EWSCalendar>();
            foreach (var searchFolder in findFolderResults.Folders)
            {
                GetCalendars(searchFolder, ewsCalendars, view);
            }
            return ewsCalendars;
        }


        public string CalendarServiceName
        {
            get { return "Exchange Server"; }
        }

        private ExchangeServerSettings ExchangeServerSettings { get; set; }

        public Task<AppointmentsWrapper> DeleteCalendarEvents(List<Appointment> calendarAppointments,
            IDictionary<string, object> calendarSpecificData)
        {
            throw new NotImplementedException();
        }

        public async Task<AppointmentsWrapper> GetCalendarEventsInRangeAsync(DateTime startDate, DateTime endDate,
            IDictionary<string, object> calendarSpecificData)
        {
            CheckCalendarSpecificData(calendarSpecificData);


            var service = GetExchangeService(ExchangeServerSettings);

            var calendarview = new CalendarView(startDate, endDate);

            // Get Default Calendar
            var outlookAppointments = new AppointmentsWrapper();
            var exchangeAppointments = service.FindAppointments(_ewsCalendar.EntryId,
                calendarview);


            service.LoadPropertiesForItems(
                from Item item in exchangeAppointments select item,
                new PropertySet(BasePropertySet.FirstClassProperties) {RequestedBodyType = BodyType.Text});

            if (exchangeAppointments != null)
            {
                foreach (var exchangeAppointment in exchangeAppointments)
                {
                    var appointment = new Appointment(exchangeAppointment.Body, exchangeAppointment.Location,
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
                        RequiredAttendees = GetAttendees(exchangeAppointment.RequiredAttendees)
                    };
                    outlookAppointments.Add(appointment);
                }
            }
            return outlookAppointments;
        }

        public Task<AppointmentsWrapper> AddCalendarEvents(List<Appointment> calendarAppointments,
            bool addDescription,
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
            _addAsAppointments = (bool) addAsAppointments;
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

        public async Task<bool> ClearCalendar(IDictionary<string, object> calendarSpecificData)
        {
            var startDate = DateTime.Today.AddDays(-(10 * 365));
            var endDate = DateTime.Today.AddDays(10 * 365);
            var appointments =
                await GetCalendarEventsInRangeAsync(startDate, endDate, calendarSpecificData);
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
                await GetCalendarEventsInRangeAsync(startDate, endDate, calendarSpecificData);
            if (appointments != null)
            {
                appointments.ForEach(t => t.ExtendedProperties = new Dictionary<string, string>());
                var success = await UpdateCalendarEvents(appointments, false,false,false,false, calendarSpecificData);
                return success.IsSuccess;
            }
            return false;
        }

        #endregion
    }
}