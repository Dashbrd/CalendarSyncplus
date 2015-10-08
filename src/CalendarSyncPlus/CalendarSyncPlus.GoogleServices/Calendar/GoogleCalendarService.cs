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
using CalendarSyncPlus.Authentication.Google;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Helpers;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.Services.Calendars.Interfaces;
using CalendarSyncPlus.Services.Utilities;
using Google;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Requests;
using log4net;

#endregion

namespace CalendarSyncPlus.GoogleServices.Calendar
{
    /// <summary>
    /// 
    /// </summary>
    [Export(typeof(ICalendarService)), Export(typeof(IGoogleCalendarService))]
    [ExportMetadata("ServiceType", ServiceType.Google)]
    public class GoogleCalendarService : IGoogleCalendarService
    {
        #region Constructors

        [ImportingConstructor]
        public GoogleCalendarService(IAccountAuthenticationService accountAuthenticationService,
            ApplicationLogger applicationLogger)
        {
            Logger = applicationLogger.GetLogger(GetType());
            AccountAuthenticationService = accountAuthenticationService;
        }

        #endregion

        #region Fields

        private ILog Logger { get; set; }

        #endregion

        public async Task<AppointmentsWrapper> UpdateCalendarEvents(List<Appointment> calendarAppointments, bool addDescription,
            bool addReminder, bool addAttendees, bool attendeesToDescription,
            IDictionary<string, object> calendarSpecificData)
        {
            var updatedAppointments = new AppointmentsWrapper();
            if (!calendarAppointments.Any())
            {
                updatedAppointments.IsSuccess = true;
                return updatedAppointments;
            }

            CheckCalendarSpecificData(calendarSpecificData);

            var errorList = new Dictionary<int, Appointment>();
            //Get Calendar Service
            var calendarService = GetCalendarService(AccountName);

            if (calendarAppointments == null || string.IsNullOrEmpty(CalendarId))
            {
                updatedAppointments.IsSuccess = false;
                return updatedAppointments;
            }

            try
            {
                if (calendarAppointments.Any())
                {
                    //Create a Batch Request
                    var batchRequest = new BatchRequest(calendarService);

                    //Split the list of calendarAppointments by 1000 per list

                    //Iterate over each appointment to create a event and batch it 
                    for (var i = 0; i < calendarAppointments.Count; i++)
                    {
                        if (i != 0 && i % 999 == 0)
                        {
                            await batchRequest.ExecuteAsync();
                            batchRequest = new BatchRequest(calendarService);
                        }

                        var appointment = calendarAppointments[i];
                        var calendarEvent = CreateUpdatedGoogleCalendarEvent(appointment, addDescription, addReminder,
                            addAttendees, attendeesToDescription);
                        var updateRequest = calendarService.Events.Update(calendarEvent,
                            CalendarId, calendarEvent.Id);
                        updateRequest.MaxAttendees = 10000;

                        batchRequest.Queue<Event>(updateRequest,
                            (content, error, index, message) =>
                                CallbackEventErrorMessage(content, error, index, message, calendarAppointments, "Error in updating event",errorList,updatedAppointments));
                    }

                    await batchRequest.ExecuteAsync();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                updatedAppointments.IsSuccess = false;
                return updatedAppointments;
            }
            updatedAppointments.IsSuccess = true;
            return updatedAppointments;
        }

        #region Static and Constants

        private const string dictionaryKey_CalendarId = "CalendarId";
        private const string dictionaryKey_AccountName = "AccountName";
        private const int maxAttendees = 200;

        #endregion

        #region Properties

        public IAccountAuthenticationService AccountAuthenticationService { get; set; }

        private string CalendarId { get; set; }

        public Category EventCategory { get; set; }

        public string CalendarServiceName
        {
            get { return "Google"; }
        }

        private string AccountName { get; set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// </summary>
        /// <param name="calendarAppointment"></param>
        /// <param name="addDescription"></param>
        /// <param name="addReminder"></param>
        /// <param name="addAttendees"></param>
        /// <param name="attendeesToDescription"></param>
        /// <returns>
        /// </returns>
        private Event CreateUpdatedGoogleCalendarEvent(Appointment calendarAppointment, bool addDescription,
            bool addReminder,
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
                Visibility = AppointmentHelper.GetVisibility(calendarAppointment.Privacy),
                Transparency = (calendarAppointment.BusyStatus == BusyStatusEnum.Free) ? "transparent" : "opaque"
            };

            if (EventCategory != null && !string.IsNullOrEmpty(EventCategory.ColorNumber))
            {
                googleEvent.ColorId = EventCategory.ColorNumber;
            }

            //Need to make recurring appointment IDs unique - append the item's date   
            googleEvent.ExtendedProperties = new Event.ExtendedPropertiesData
            {
                Private__ = new Dictionary<string, string>()
            };

            foreach (var extendedProperty in calendarAppointment.ExtendedProperties)
            {
                googleEvent.ExtendedProperties.Private__.Add(extendedProperty.Key, extendedProperty.Value);
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
                Visibility = AppointmentHelper.GetVisibility(calendarAppointment.Privacy),
                Transparency = (calendarAppointment.BusyStatus == BusyStatusEnum.Free) ? "transparent" : "opaque",
                //Need to make recurring appointment IDs unique - append the item's date   
                ExtendedProperties =
                    new Event.ExtendedPropertiesData
                    {
                        Private__ = 
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

        private void AddEventAttendees(IEnumerable<Attendee> recipients, Event googleEvent, bool optional)
        {
            IEnumerable<Attendee> recipeintList = recipients as IList<Attendee> ??
                                                   recipients.Take(maxAttendees).ToList();
            if (!recipeintList.Any() && googleEvent == null)
            {
                return;
            }
            
            foreach (var recipient in recipeintList)
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
                    Optional = optional,
                    ResponseStatus = AppointmentHelper.GetGoogleResponseStatus(recipient.MeetingResponseStatus)
                };
                googleEvent.Attendees.Add(eventAttendee);
            }
        }

        private CalendarService GetCalendarService(string accountName)
        {
            return AccountAuthenticationService.AuthenticateCalendarOauth(accountName);
        }

        private void CallbackEventErrorMessage(Event content, RequestError error, int index, HttpResponseMessage message,
            List<Appointment> eventList,string errorMessage,
            Dictionary<int, Appointment> errorAppointments, List<Appointment> modifiedEvents)
        {
            var phrase = message.ReasonPhrase;
            if (!message.IsSuccessStatusCode)
            {
                var googleEvent = eventList[index];
                errorAppointments.Add(index, googleEvent);
                Logger.ErrorFormat("{0} : {1}{2} - {3}", errorMessage,Environment.NewLine, phrase, googleEvent);
            }
            else
            {
                if (content != null)
                {
                    modifiedEvents.Add(CreateAppointment(content));
                }
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

            if (googleEvent.Reminders != null)
            {
                if (!googleEvent.Reminders.UseDefault.GetValueOrDefault() && googleEvent.Reminders.Overrides != null)
                {
                    appointment.ReminderSet = true;
                    appointment.ReminderMinutesBeforeStart =
                        googleEvent.Reminders.Overrides.First().Minutes.GetValueOrDefault();
                }
            }
            
            //Getting Additional Data
            appointment.BusyStatus = googleEvent.Transparency !=null && googleEvent.Transparency.Equals("transparent") ? 
                BusyStatusEnum.Free : BusyStatusEnum.Busy;
            appointment.Privacy = AppointmentHelper.GetSensitivityEnum(googleEvent.Visibility);
            appointment.CalendarId = CalendarId;

            if (googleEvent.ExtendedProperties != null && googleEvent.ExtendedProperties.Private__ != null)
            {
                foreach (var property in googleEvent.ExtendedProperties.Private__)
                {
                    appointment.ExtendedProperties.Add(property.Key, property.Value);
                }
            }
            
            appointment.Created = googleEvent.Created;
            appointment.LastModified = googleEvent.Updated;

            if (googleEvent.Organizer != null)
            {
                //Add Organizer
                appointment.Organizer = new Attendee
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

        private void GetAttendees(Event googleEvent, List<Attendee> recipients, bool isOptional)
        {
            if (googleEvent != null && googleEvent.Attendees != null)
            {
                var attendees =
                    googleEvent.Attendees.Where(attendee => attendee.Optional.GetValueOrDefault() == isOptional);

                foreach (var eventAttendee in attendees)
                {
                    recipients.Add(new Attendee
                    {
                        Name = eventAttendee.DisplayName, Email = eventAttendee.Email,
                        MeetingResponseStatus = AppointmentHelper.GetGoogleResponseStatus(eventAttendee.ResponseStatus)
                    });
                }
            }
        }

        #endregion

        #region IGoogleCalendarService Members

        public async void SetCalendarColor(Category background, IDictionary<string, object> calendarSpecificData)
        {
            CheckCalendarSpecificData(calendarSpecificData);

            var calendarService = GetCalendarService(AccountName);

            var calendarListEntry = await calendarService.CalendarList.Get(CalendarId).ExecuteAsync();

            calendarListEntry.BackgroundColor = background.HexValue;

            await calendarService.CalendarList.Update(calendarListEntry, CalendarId).ExecuteAsync();
        }

        /// <exception cref="ArgumentNullException">
        ///     <paramref name="calendarSpecificData" /> is <see langword="null" />
        ///     .
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     If AccountName, <see cref="CalendarId" /> are do not have valid
        ///     values.
        /// </exception>
        public void CheckCalendarSpecificData(IDictionary<string, object> calendarSpecificData)
        {
            if (calendarSpecificData == null)
            {
                throw new ArgumentNullException("calendarSpecificData", "Calendar Specific Data cannot be null");
            }

            object calendarIdValue;
            if (!calendarSpecificData.TryGetValue(dictionaryKey_CalendarId, out calendarIdValue))
            {
                throw new InvalidOperationException($"{dictionaryKey_CalendarId} is a required.");
            }

            CalendarId = calendarIdValue as string;

            if (string.IsNullOrEmpty(CalendarId))
            {
                throw new InvalidOperationException($"{dictionaryKey_CalendarId} cannot be null or empty.");
            }

            object accountNameValue;
            if (!calendarSpecificData.TryGetValue(dictionaryKey_AccountName, out accountNameValue))
            {
                throw new InvalidOperationException($"{dictionaryKey_AccountName} is a required.");
            }

            AccountName = accountNameValue as string;

            if (string.IsNullOrEmpty(AccountName))
            {
                throw new InvalidOperationException($"{dictionaryKey_CalendarId} cannot be null or empty.");
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
            var calendarService = GetCalendarService(accountName);

            var calendarList = await calendarService.CalendarList.List().ExecuteAsync();

            var localCalendarList =
                calendarList.Items.Select(
                    calendarListEntry =>
                        new GoogleCalendar { Id = calendarListEntry.Id, Name = calendarListEntry.Summary })
                    .ToList();
            return localCalendarList;
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

            var errorList = new Dictionary<int, Appointment>();
            //Get Calendar Service
            var calendarService = GetCalendarService(AccountName);

            if (calendarAppointments == null || string.IsNullOrEmpty(CalendarId))
            {
                addedAppointments.IsSuccess = false;
                return addedAppointments;
            }

            try
            {
                if (calendarAppointments.Any())
                {
                    //Split the list of calendarAppointments by 1000 per list
                    var appts =
                        await AddCalendarEventsInternal(calendarAppointments, addDescription, addReminder, addAttendees,
                            attendeesToDescription, calendarService, errorList);
                    addedAppointments.AddRange(appts);
                    if (errorList.Count > 0)
                    {
                        var remainingList = errorList.Select(CreateAppointmentWithoutAttendees).ToList();
                        errorList.Clear();

                        appts = await AddCalendarEventsInternal(remainingList, addDescription, addReminder, addAttendees,
                            attendeesToDescription, calendarService, errorList);
                        addedAppointments.AddRange(appts);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                addedAppointments.IsSuccess = false;
                return addedAppointments;
            }
            addedAppointments.IsSuccess = true;
            return addedAppointments;
        }

        private Appointment CreateAppointmentWithoutAttendees(KeyValuePair<int, Appointment> arg)
        {
            var appointment = arg.Value;
            appointment.RequiredAttendees.Clear();
            appointment.OptionalAttendees.Clear();
            return appointment;
        }

        private async Task<List<Appointment>> AddCalendarEventsInternal(List<Appointment> calendarAppointments,
            bool addDescription, bool addReminder,
            bool addAttendees,
            bool attendeesToDescription, CalendarService calendarService,
            Dictionary<int, Appointment> errorList)
        {
            var addedEvents = new List<Appointment>();
            //Create a Batch Request
            var batchRequest = new BatchRequest(calendarService);

            for (var i = 0; i < calendarAppointments.Count; i++)
            {
                if (i != 0 && i % 999 == 0)
                {
                    await batchRequest.ExecuteAsync();
                    batchRequest = new BatchRequest(calendarService);
                }

                var appointment = calendarAppointments[i];
                var calendarEvent = CreateGoogleCalendarEvent(appointment, addDescription, addReminder,
                    addAttendees,
                    attendeesToDescription);
                var insertRequest = calendarService.Events.Insert(calendarEvent,
                    CalendarId);
                insertRequest.MaxAttendees = 10000;
                batchRequest.Queue<Event>(insertRequest,
                    (content, error, index, message) =>
                        CallbackEventErrorMessage(content, error, index, message, 
                        calendarAppointments, "Error in adding events",errorList,
                            addedEvents));
            }

            await batchRequest.ExecuteAsync();

            return addedEvents;
        }

        public async Task<AppointmentsWrapper> DeleteCalendarEvents(List<Appointment> calendarAppointments,
            IDictionary<string, object> calendarSpecificData)
        {
            var deletedAppointments = new AppointmentsWrapper();
            if (!calendarAppointments.Any())
            {
                deletedAppointments.IsSuccess = true;
                return deletedAppointments;
            }

            CheckCalendarSpecificData(calendarSpecificData);

            var errorList = new Dictionary<int, Appointment>();
            //Get Calendar Service
            var calendarService = GetCalendarService(AccountName);

            if (calendarAppointments == null || string.IsNullOrEmpty(CalendarId))
            {
                deletedAppointments.IsSuccess = false;
                return deletedAppointments;
            }

            try
            {
                if (calendarAppointments.Any())
                {
                    //Create a Batch Request
                    var batchRequest = new BatchRequest(calendarService);
                    
                    //Split the list of calendarAppointments by 1000 per list
                    //Iterate over each appointment to create a event and batch it 
                    for (var i = 0; i < calendarAppointments.Count; i++)
                    {
                        if (i != 0 && i % 999 == 0)
                        {
                            await batchRequest.ExecuteAsync();
                            batchRequest = new BatchRequest(calendarService);
                        }

                        var appointment = calendarAppointments[i];
                        var deleteRequest = calendarService.Events.Delete(CalendarId,
                            appointment.AppointmentId);
                        
                        batchRequest.Queue<Event>(deleteRequest,
                            (content, error, index, message) =>
                                CallbackEventErrorMessage(content, error, index, message, calendarAppointments, 
                                "Error in deleting events",errorList,deletedAppointments));
                    }
                    await batchRequest.ExecuteAsync();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                deletedAppointments.IsSuccess = false;
                return deletedAppointments;
            }
            deletedAppointments.IsSuccess = true;
            return deletedAppointments;
        }

        public async Task<AppointmentsWrapper> GetCalendarEventsInRangeAsync(DateTime startDate, DateTime endDate,bool skipPrivateEntries,
            IDictionary<string, object> calendarSpecificData)
        {
            CheckCalendarSpecificData(calendarSpecificData);

            //Get Calendar Service
            var calendarService = GetCalendarService(AccountName);

            var finalEventList = new List<Appointment>();

            Events result = null;

            var eventListRequest = calendarService.Events.List(CalendarId);

            // Add Filters to event List Request
            eventListRequest.TimeMin = startDate;
            eventListRequest.TimeMax = endDate;
            eventListRequest.MaxAttendees = 1000;
            eventListRequest.SingleEvents = true;
            eventListRequest.ShowHiddenInvitations = !skipPrivateEntries;
            try
            {
                result = eventListRequest.Execute();
                if (result != null)
                {
                    while (result.Items != null)
                    {
                        // Add events to list, Split recurring appointments
                        foreach (var eventItem in result.Items)
                        {
                            if (eventItem.Status == "cancelled")
                            {
                                continue;
                            }

                            var appointment = CreateAppointment(eventItem);
                            finalEventList.Add(appointment);
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
                Logger.Error(exception);
                return null;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return null;
            }

            var calendarAppointments = new AppointmentsWrapper { CalendarId = CalendarId };
            calendarAppointments.AddRange(finalEventList);
            return calendarAppointments;
        }

        public async Task<bool> ClearCalendar(IDictionary<string, object> calendarSpecificData)
        {
            var startDate = DateTime.Today.AddDays(-(10 * 365));
            var endDate = DateTime.Today.AddDays(10 * 365);

            var appointments =
                await GetCalendarEventsInRangeAsync(startDate, endDate,true, calendarSpecificData);
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
                var success = await UpdateCalendarEvents(appointments, true, true,true, false, calendarSpecificData);
                return success.IsSuccess;
            }
            return false;
        }
        #endregion
    }
}