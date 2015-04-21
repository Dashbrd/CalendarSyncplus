#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        CalendarSyncPlus
//  *      SubProject:     CalendarSyncPlus.Application
//  *      Author:         Dave, Ankesh
//  *      Created On:     02-02-2015 5:56 PM
//  *      Modified On:    04-02-2015 12:39 PM
//  *      FileName:       GoogleCalendarService.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Services;
using CalendarSyncPlus.Services.Interfaces;
using CalendarSyncPlus.Services.Utilities;
using CalendarSyncPlus.Services.Wrappers;
using Google;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Requests;
using Calendar = CalendarSyncPlus.Domain.Models.Calendar;

#endregion

namespace CalendarSyncPlus.GoogleServices.Google
{
    [Export(typeof(ICalendarService)), Export(typeof(IGoogleCalendarService))]
    [ExportMetadata("ServiceType", CalendarServiceType.Google)]
    public class GoogleCalendarService : IGoogleCalendarService
    {
        #region Static and Constants

        private const string dictionaryKey_CalendarId = "CalendarId";
        private const string dictionaryKey_AccountName = "AccountName";

        #endregion

        #region Fields

        private string calendarId;
        private string accountName;
        private ApplicationLogger ApplicationLogger { get; set; }

        #endregion

        #region Constructors

        [ImportingConstructor]
        public GoogleCalendarService(IAccountAuthenticationService accountAuthenticationService,
            ApplicationLogger applicationLogger)
        {
            ApplicationLogger = applicationLogger;
            AccountAuthenticationService = accountAuthenticationService;
        }

        #endregion

        #region Properties

        public IAccountAuthenticationService AccountAuthenticationService { get; set; }

        private string CalendarId
        {
            get { return calendarId; }
            set { calendarId = value; }
        }

        public Category EventCategory { get; set; }

        public string CalendarServiceName
        {
            get { return "Google"; }
        }

        private string AccountName
        {
            get { return accountName; }
            set { accountName = value; }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="calendarAppointment"></param>
        /// <param name="addDescription"></param>
        /// <param name="addReminder"></param>
        /// <param name="addAttendees"></param>
        /// <param name="attendeesToDescription"></param>
        /// <returns></returns>
        private Event CreateUpdatedGoogleCalendarEvent(Appointment calendarAppointment, bool addDescription, bool addReminder,
           bool addAttendees, bool attendeesToDescription)
        {
            //Create Event
            var googleEvent = new Event
            {
                Id = calendarAppointment.AppointmentId,
                Start = new EventDateTime(),
                End = new EventDateTime(),
                Summary = calendarAppointment.Subject,
                Description = calendarAppointment.GetDescriptionData(addDescription, attendeesToDescription),
                Location = calendarAppointment.Location,
                Visibility = calendarAppointment.Privacy,
                Transparency = (calendarAppointment.BusyStatus == BusyStatusEnum.Free) ? "transparent" : "opaque",
            };

            if (EventCategory != null && !string.IsNullOrEmpty(EventCategory.ColorNumber))
            {
                googleEvent.ColorId = EventCategory.ColorNumber;
            }

            //Need to make recurring appointment IDs unique - append the item's date   
            googleEvent.ExtendedProperties = new Event.ExtendedPropertiesData { Private = new Dictionary<string, string>() };

            foreach (var extendedProperty in calendarAppointment.ExtendedProperties)
            {
                googleEvent.ExtendedProperties.Private.Add(extendedProperty.Key,extendedProperty.Value);
            }

            //Add Start/End Time
            if (calendarAppointment.AllDayEvent)
            {
                if (calendarAppointment.StartTime.HasValue)
                {
                    googleEvent.Start.Date = calendarAppointment.StartTime.Value.ToString("yyyy-MM-dd");
                }
                if (calendarAppointment.EndTime.HasValue)
                {
                    googleEvent.End.Date = calendarAppointment.EndTime.Value.ToString("yyyy-MM-dd");
                }
            }
            else
            {
                googleEvent.Start.DateTimeRaw = calendarAppointment.Rfc339FormatStartTime;
                googleEvent.End.DateTimeRaw = calendarAppointment.Rfc339FormatEndTime;
            }

            //Add Reminder
            if (addReminder && calendarAppointment.ReminderSet)
            {
                googleEvent.Reminders = new Event.RemindersData
                {
                    UseDefault = false,
                    Overrides = new List<EventReminder>
                    {
                        new EventReminder
                        {
                            Method = "popup",
                            Minutes = calendarAppointment.ReminderMinutesBeforeStart
                        }
                    }
                };
            }

            if (googleEvent.Attendees == null)
            {
                googleEvent.Attendees = new List<EventAttendee>();
            }

            if (addAttendees && !attendeesToDescription)
            {
                //Add Required Attendees
                AddEventAttendees(calendarAppointment.RequiredAttendees, googleEvent, false);

                //Add optional Attendees
                AddEventAttendees(calendarAppointment.OptionalAttendees, googleEvent, true);
            }
            //Add Organizer
            if (calendarAppointment.Organizer != null && calendarAppointment.Organizer.Email.IsValidEmailAddress())
            {
                googleEvent.Organizer = new Event.OrganizerData
                {
                    DisplayName = calendarAppointment.Organizer.Name,
                    Email = calendarAppointment.Organizer.Email
                };
            }

            return googleEvent;
        }


        private Event CreateGoogleCalendarEvent(Appointment calendarAppointment, bool addDescription, bool addReminder,
            bool addAttendees, bool attendeesToDescription)
        {
            //Create Event
            var googleEvent = new Event
            {
                Start = new EventDateTime(),
                End = new EventDateTime(),
                Summary = calendarAppointment.Subject,
                Description = calendarAppointment.GetDescriptionData(addDescription, attendeesToDescription),
                Location = calendarAppointment.Location,
                Visibility = calendarAppointment.Privacy,
                Transparency = (calendarAppointment.BusyStatus == BusyStatusEnum.Free) ? "transparent" : "opaque",
                //Need to make recurring appointment IDs unique - append the item's date   
                ExtendedProperties =
                    new Event.ExtendedPropertiesData
                    {
                        Private =
                            new Dictionary<string, string>
                            {
                                {calendarAppointment.GetSourceEntryKey(), calendarAppointment.AppointmentId}
                            }
                    }
            };

            if (EventCategory != null && !string.IsNullOrEmpty(EventCategory.ColorNumber))
            {
                googleEvent.ColorId = EventCategory.ColorNumber;
            }
            //Add Start/End Time
            if (calendarAppointment.AllDayEvent)
            {
                if (calendarAppointment.StartTime.HasValue)
                {
                    googleEvent.Start.Date = calendarAppointment.StartTime.Value.ToString("yyyy-MM-dd");
                }
                if (calendarAppointment.EndTime.HasValue)
                {
                    googleEvent.End.Date = calendarAppointment.EndTime.Value.ToString("yyyy-MM-dd");
                }
            }
            else
            {
                googleEvent.Start.DateTimeRaw = calendarAppointment.Rfc339FormatStartTime;
                googleEvent.End.DateTimeRaw = calendarAppointment.Rfc339FormatEndTime;
            }

            //Add Reminder
            if (addReminder && calendarAppointment.ReminderSet)
            {
                googleEvent.Reminders = new Event.RemindersData
                {
                    UseDefault = false,
                    Overrides = new List<EventReminder>
                    {
                        new EventReminder
                        {
                            Method = "popup",
                            Minutes = calendarAppointment.ReminderMinutesBeforeStart
                        }
                    }
                };
            }

            if (googleEvent.Attendees == null)
            {
                googleEvent.Attendees = new List<EventAttendee>();
            }

            if (addAttendees && !attendeesToDescription)
            {
                //Add Required Attendees
                AddEventAttendees(calendarAppointment.RequiredAttendees, googleEvent, false);

                //Add optional Attendees
                AddEventAttendees(calendarAppointment.OptionalAttendees, googleEvent, true);
            }
            //Add Organizer
            if (calendarAppointment.Organizer != null && calendarAppointment.Organizer.Email.IsValidEmailAddress())
            {
                googleEvent.Organizer = new Event.OrganizerData
                {
                    DisplayName = calendarAppointment.Organizer.Name,
                    Email = calendarAppointment.Organizer.Email
                };
            }

            return googleEvent;
        }

        private void AddEventAttendees(IEnumerable<Recipient> recipients, Event googleEvent, bool optional)
        {
            IEnumerable<Recipient> recipeintList = recipients as IList<Recipient> ?? recipients.ToList();
            if (!recipeintList.Any() && googleEvent == null)
            {
                return;
            }

            foreach (Recipient recipient in recipeintList)
            {
                //Ignore recipients with invalid Email
                if (!recipient.Email.IsValidEmailAddress())
                {
                    continue;
                }
                var eventAttendee = new EventAttendee
                {
                    DisplayName = recipient.Name,
                    Email = recipient.Email,
                    Optional = optional
                };
                googleEvent.Attendees.Add(eventAttendee);
            }
        }

        private CalendarService GetCalendarService(string accountName)
        {
            return AccountAuthenticationService.AuthenticateCalendarOauth(accountName);
        }

        private void InsertEventErrorMessage(Event content, RequestError error, int index, HttpResponseMessage message,
            List<Appointment> eventList,Dictionary<int,Appointment> errorAppointments)
        {
            if (!message.IsSuccessStatusCode)
            {
                var googleEvent = eventList[index];
                errorAppointments.Add(index,googleEvent);
                ApplicationLogger.LogError(googleEvent.ToString());
            }
        }

        private Appointment CreateAppointment(Event googleEvent)
        {
            Appointment appointment;

            if (googleEvent.Start.DateTime == null && googleEvent.End.DateTime == null)
            {
                appointment = new Appointment(googleEvent.Description, googleEvent.Location, googleEvent.Summary,
                    DateTime.Parse(googleEvent.End.Date),
                    DateTime.Parse(googleEvent.Start.Date), googleEvent.Id) { AllDayEvent = true };
            }
            else
            {
                appointment = new Appointment(googleEvent.Description, googleEvent.Location, googleEvent.Summary,
                    googleEvent.End.DateTime,
                    googleEvent.Start.DateTime, googleEvent.Id);
            }

            if(googleEvent.Reminders != null)
            {
                if (!googleEvent.Reminders.UseDefault.GetValueOrDefault() && googleEvent.Reminders.Overrides != null)
                {
                    appointment.ReminderSet = true;
                    appointment.ReminderMinutesBeforeStart = 
                        googleEvent.Reminders.Overrides.First().Minutes.GetValueOrDefault();
                }
            }
            
            
            appointment.CalendarId = CalendarId;
            if (googleEvent.ExtendedProperties != null && googleEvent.ExtendedProperties.Private != null)
            {
                foreach (var property in googleEvent.ExtendedProperties.Private)
                {
                    appointment.ExtendedProperties.Add(property.Key, property.Value);
                }
            }

            appointment.Created = googleEvent.Created;
            appointment.LastModified = googleEvent.Updated;

            if (googleEvent.Organizer != null)
            {
                //Add Organizer
                appointment.Organizer = new Recipient
                {
                    Name = googleEvent.Organizer.DisplayName,
                    Email = googleEvent.Organizer.Email
                };
            }
            
            //Add Required Attendee
            GetAttendees(googleEvent, appointment.RequiredAttendees, false);

            //Add optional Attendee
            GetAttendees(googleEvent, appointment.OptionalAttendees, true);


            return appointment;
        }

        private void GetAttendees(Event googleEvent, List<Recipient> recipients, bool isOptional)
        {
            if (googleEvent != null && googleEvent.Attendees != null)
            {
                IEnumerable<EventAttendee> attendees =
                    googleEvent.Attendees.Where(attendee => attendee.Optional.GetValueOrDefault() == isOptional);

                foreach (EventAttendee eventAttendee in attendees)
                {
                    recipients.Add(new Recipient { Name = eventAttendee.DisplayName, Email = eventAttendee.Email });
                }
            }
        }

        #endregion

        #region IGoogleCalendarService Members

        public async void SetCalendarColor(Category background, IDictionary<string, object> calendarSpecificData)
        {
            CheckCalendarSpecificData(calendarSpecificData);

            CalendarService calendarService = GetCalendarService(AccountName);

            CalendarListEntry calendarListEntry = await calendarService.CalendarList.Get(CalendarId).ExecuteAsync();

            calendarListEntry.BackgroundColor = background.HexValue;

            await calendarService.CalendarList.Update(calendarListEntry, calendarId).ExecuteAsync();
        }

        /// <exception cref="ArgumentNullException"><paramref name="calendarSpecificData"/> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">If AccountName, CalendarId are do not have valid values.</exception>
        public void CheckCalendarSpecificData(IDictionary<string, object> calendarSpecificData)
        {
            if (calendarSpecificData == null)
            {
                throw new ArgumentNullException("calendarSpecificData", "Calendar Specific Data cannot be null");
            }

            object calendarIdValue;
            if (!calendarSpecificData.TryGetValue(dictionaryKey_CalendarId, out calendarIdValue))
            {
                throw new InvalidOperationException(string.Format("{0} is a required.", dictionaryKey_CalendarId));
            }

            CalendarId = calendarIdValue as string;

            if (string.IsNullOrEmpty(calendarId))
            {
                throw new InvalidOperationException(string.Format("{0} cannot be null or empty.",
                    dictionaryKey_CalendarId));
            }

            object accountNameValue;
            if (!calendarSpecificData.TryGetValue(dictionaryKey_AccountName, out accountNameValue))
            {
                throw new InvalidOperationException(string.Format("{0} is a required.", dictionaryKey_AccountName));
            }

            AccountName = accountNameValue as string;

            if (string.IsNullOrEmpty(accountName))
            {
                throw new InvalidOperationException(string.Format("{0} cannot be null or empty.",
                    dictionaryKey_CalendarId));
            }

            object eventCategory;
            if (calendarSpecificData.TryGetValue("EventCategory", out eventCategory))
            {
                EventCategory = eventCategory as Category;
            }
            else
            {
                EventCategory = null;
            }
        }

        public async Task<List<GoogleCalendar>> GetAvailableCalendars(string accountName)
        {
            //Get Calendar Service
            CalendarService calendarService = GetCalendarService(accountName);

            CalendarList calendarList = await calendarService.CalendarList.List().ExecuteAsync();

            List<GoogleCalendar> localCalendarList =
                calendarList.Items.Select(
                    calendarListEntry =>
                        new GoogleCalendar {Id = calendarListEntry.Id, Name = calendarListEntry.Summary})
                    .ToList();
            return localCalendarList;
        }

        public async Task<bool> AddCalendarEvents(List<Appointment> calendarAppointments, bool addDescription,
            bool addReminder, bool addAttendees, bool attendeesToDescription,
            IDictionary<string, object> calendarSpecificData)
        {
            if (!calendarAppointments.Any())
            {
                return true;
            }

            CheckCalendarSpecificData(calendarSpecificData);

            var errorList = new Dictionary<int,Appointment>();
            //Get Calendar Service
            CalendarService calendarService = GetCalendarService(AccountName);

            if (calendarAppointments == null || string.IsNullOrEmpty(CalendarId))
            {
                return false;
            }
            try
            {

                if (calendarAppointments.Any())
                {
                    //Create a Batch Request
                    var batchRequest = new BatchRequest(calendarService);

                    //Split the list of calendarAppointments by 1000 per list

                    //Iterate over each appointment to create a event and batch it 
                    for (int i = 0; i < calendarAppointments.Count; i++)
                    {
                        if (i != 0 && i % 999 == 0)
                        {
                            await batchRequest.ExecuteAsync();
                            batchRequest = new BatchRequest(calendarService);
                        }

                        Appointment appointment = calendarAppointments[i];
                        Event calendarEvent = CreateGoogleCalendarEvent(appointment, addDescription, addReminder,
                            attendeesToDescription,
                            addAttendees);
                        EventsResource.InsertRequest insertRequest = calendarService.Events.Insert(calendarEvent,
                            CalendarId);
                        insertRequest.SendNotifications = false;
                        
                        batchRequest.Queue<Event>(insertRequest,
                            (content, error, index, message) =>
                                InsertEventErrorMessage(content, error, index, message, calendarAppointments, errorList));
                    }

                    await batchRequest.ExecuteAsync();
                }
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.ToString());
                return false;
            }
            return true;
        }


        public async Task<bool> DeleteCalendarEvents(List<Appointment> calendarAppointments,
            IDictionary<string, object> calendarSpecificData)
        {
            if (!calendarAppointments.Any())
            {
                return true;
            }

            CheckCalendarSpecificData(calendarSpecificData);

            var errorList = new Dictionary<int, Appointment>();
            //Get Calendar Service
            CalendarService calendarService = GetCalendarService(AccountName);

            if (calendarAppointments == null || string.IsNullOrEmpty(CalendarId))
            {
                return false;
            }

            try
            {
                if (calendarAppointments.Any())
                {
                    //Create a Batch Request
                    var batchRequest = new BatchRequest(calendarService);

                    //Split the list of calendarAppointments by 1000 per list

                    //Iterate over each appointment to create a event and batch it 
                    for (int i = 0; i < calendarAppointments.Count; i++)
                    {
                        if (i != 0 && i % 999 == 0)
                        {
                            await batchRequest.ExecuteAsync();
                            batchRequest = new BatchRequest(calendarService);
                        }

                        Appointment appointment = calendarAppointments[i];
                        EventsResource.DeleteRequest deleteRequest = calendarService.Events.Delete(CalendarId,
                            appointment.AppointmentId);
                        batchRequest.Queue<Event>(deleteRequest,
                            (content, error, index, message) =>
                                InsertEventErrorMessage(content, error, index, message, calendarAppointments, errorList));
                    }
                    await batchRequest.ExecuteAsync();
                }
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.ToString());
                return false;
            }
            return true;
        }

        public async Task<CalendarAppointments> GetCalendarEventsInRangeAsync(DateTime startDate, DateTime endDate,
            IDictionary<string, object> calendarSpecificData)
        {
            CheckCalendarSpecificData(calendarSpecificData);

            //Get Calendar Service
            CalendarService calendarService = GetCalendarService(AccountName);

            var finalEventList = new List<Appointment>();

            Events result = null;

            EventsResource.ListRequest eventListRequest = calendarService.Events.List(CalendarId);

            // Add Filters to event List Request
            eventListRequest.TimeMin = startDate;
            eventListRequest.TimeMax = endDate;
            eventListRequest.MaxAttendees = 1000;
            
            try
            {
                result = eventListRequest.Execute();
                if (result != null)
                {
                    while (result.Items != null)
                    {
                        // Add events to list, Split recurring appointments
                        foreach (Event eventItem in result.Items)
                        {
                            if (eventItem.Status == "cancelled")
                            {
                                continue;
                            }

                            Appointment appointment = CreateAppointment(eventItem);
                            if (eventItem.Recurrence != null && eventItem.Recurrence.Count > 0)
                            {
                                EventsResource.InstancesRequest instancesRequest  = calendarService.Events.Instances(CalendarId,
                                    appointment.AppointmentId);

                                // Add Filters to event List Request
                                instancesRequest.TimeMin = startDate;
                                instancesRequest.TimeMax = endDate;
                                instancesRequest.MaxAttendees = 1000;

                                var instanceResult = instancesRequest.Execute();
                                while (instanceResult.Items != null)
                                {

                                    foreach (var instance in instanceResult.Items)
                                    {
                                        finalEventList.Add(CreateAppointment(instance));
                                    }

                                    //If all pages are over break
                                    if (instanceResult.NextPageToken == null)
                                    {
                                        break;
                                    }

                                    instancesRequest.PageToken = instanceResult.NextPageToken;
                                    instanceResult = await instancesRequest.ExecuteAsync();
                                }
                            }
                            else
                            {
                                finalEventList.Add(appointment);
                            }
                        }

                        //If all pages are over break
                        if (result.NextPageToken == null)
                        {
                            break;
                        }

                        //Set the next page to pull from request
                        eventListRequest.PageToken = result.NextPageToken;

                        result = await eventListRequest.ExecuteAsync();
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (GoogleApiException exception)
            {
                ApplicationLogger.LogError(exception.ToString());
                return null;
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.ToString());
                return null;
            }

            var calendarAppointments = new CalendarAppointments { CalendarId = CalendarId };
            calendarAppointments.AddRange(finalEventList);
            return calendarAppointments;
        }

        public async Task<bool> ResetCalendar(IDictionary<string, object> calendarSpecificData)
        {
            DateTime startDate = DateTime.Today.AddDays(-(10 * 365));
            DateTime endDate = DateTime.Today.AddDays(10 * 365);
            
            CalendarAppointments appointments =
                await GetCalendarEventsInRangeAsync(startDate,endDate, calendarSpecificData);
            if (appointments != null)
            {
                bool success = await DeleteCalendarEvents(appointments, calendarSpecificData);
                return success;
            }
            return false;
        }

        #endregion


        public async Task<bool> UpdateCalendarEvents(List<Appointment> calendarAppointments, bool addDescription, bool addReminder, bool addAttendees, bool attendeesToDescription, IDictionary<string, object> calendarSpecificData)
        {
            if (!calendarAppointments.Any())
            {
                return true;
            }

            CheckCalendarSpecificData(calendarSpecificData);

            Dictionary<int, Appointment> errorList = new Dictionary<int, Appointment>();
            //Get Calendar Service
            CalendarService calendarService = GetCalendarService(AccountName);

            if (calendarAppointments == null || string.IsNullOrEmpty(CalendarId))
            {
                return false;
            }

            try
            {
                if (calendarAppointments.Any())
                {
                    //Create a Batch Request
                    var batchRequest = new BatchRequest(calendarService);

                    //Split the list of calendarAppointments by 1000 per list

                    //Iterate over each appointment to create a event and batch it 
                    for (int i = 0; i < calendarAppointments.Count; i++)
                    {
                        if (i != 0 && i % 999 == 0)
                        {
                            await batchRequest.ExecuteAsync();
                            batchRequest = new BatchRequest(calendarService);
                        }

                        Appointment appointment = calendarAppointments[i];
                        Event calendarEvent = CreateUpdatedGoogleCalendarEvent(appointment, addDescription, addReminder,
                            attendeesToDescription,
                            addAttendees);
                        EventsResource.UpdateRequest updateRequest = calendarService.Events.Update(calendarEvent,
                            CalendarId, calendarEvent.Id);
                        updateRequest.SendNotifications = false;
                        batchRequest.Queue<Event>(updateRequest,
                            (content, error, index, message) =>
                                InsertEventErrorMessage(content, error, index, message, calendarAppointments, errorList));
                    }

                    await batchRequest.ExecuteAsync();
                }
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.ToString());
                return false;
            }
            return true;
        }
    }
}