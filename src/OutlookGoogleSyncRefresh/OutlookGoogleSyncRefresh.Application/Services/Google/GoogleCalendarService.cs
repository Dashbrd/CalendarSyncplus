#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        OutlookGoogleSyncRefresh
//  *      SubProject:     OutlookGoogleSyncRefresh.Application
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
using System.Text;
using System.Threading.Tasks;
using Google;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Requests;
using OutlookGoogleSyncRefresh.Common.Log;
using OutlookGoogleSyncRefresh.Domain.Models;
using Calendar = OutlookGoogleSyncRefresh.Domain.Models.Calendar;

#endregion

namespace OutlookGoogleSyncRefresh.Application.Services.Google
{
    [Export(typeof (IGoogleCalendarService))]
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly ApplicationLogger _applicationLogger;

        #region Constructors

        [ImportingConstructor]
        public GoogleCalendarService(IAccountAuthenticationService accountAuthenticationService,
            ApplicationLogger applicationLogger)
        {
            _applicationLogger = applicationLogger;
            AccountAuthenticationService = accountAuthenticationService;
        }

        #endregion

        #region Properties

        public IAccountAuthenticationService AccountAuthenticationService { get; set; }

        #endregion

        #region Private Methods

        private Event CreateGoogleCalendarEvent(Appointment calenderAppointment, bool addDescription, bool addReminder,
            bool addAttendees)
        {
            //Create Event
            var googleEvent = new Event
            {
                Start = new EventDateTime(),
                End = new EventDateTime(),
                Summary = calenderAppointment.Subject,
                Description =
                    addDescription
                        ? calenderAppointment.Description +
                          GetAdditionalDescriptionData(addAttendees, calenderAppointment)
                        : String.Empty,
                Location = calenderAppointment.Location
            };

            //Add Start/End Time
            if (calenderAppointment.AllDayEvent)
            {
                if (calenderAppointment.StartTime.HasValue)
                {
                    googleEvent.Start.Date = calenderAppointment.StartTime.Value.ToString("yyyy-MM-dd");
                }
                if (calenderAppointment.EndTime.HasValue)
                {
                    googleEvent.End.Date = calenderAppointment.EndTime.Value.ToString("yyyy-MM-dd");
                }
            }
            else
            {
                googleEvent.Start.DateTimeRaw = calenderAppointment.Rfc339FormatStartTime;
                googleEvent.End.DateTimeRaw = calenderAppointment.Rfc339FormatEndTime;
            }

            //Add Reminder
            if (addReminder && calenderAppointment.ReminderSet)
            {
                googleEvent.Reminders = new Event.RemindersData
                {
                    UseDefault = false,
                    Overrides = new List<EventReminder>
                    {
                        new EventReminder
                        {
                            Method = "popup",
                            Minutes = calenderAppointment.ReminderMinutesBeforeStart
                        }
                    }
                };
            }

            return googleEvent;
        }

        private string GetAdditionalDescriptionData(bool addAttendees, Appointment calenderAppointment)
        {
            var additionDescription = new StringBuilder(string.Empty);

            if (!addAttendees)
            {
                return additionDescription.ToString();
            }
            //Start Header
            additionDescription.AppendLine("==============================================");
            additionDescription.AppendLine(string.Empty);
            //Add Organiser
            additionDescription.AppendLine("Organizer");
            additionDescription.AppendLine(calenderAppointment.Organizer);
            additionDescription.AppendLine(string.Empty);

            //Add Required Attendees
            additionDescription.AppendLine("Required Attendees:");
            additionDescription.AppendLine(SplitAttendees(calenderAppointment.RequiredAttendees));
            additionDescription.AppendLine(string.Empty);

            //Add Optional Attendees
            additionDescription.AppendLine("Optional Attendees:");
            additionDescription.AppendLine(SplitAttendees(calenderAppointment.OptionalAttendees));
            additionDescription.AppendLine(string.Empty);
            //Close Header
            additionDescription.AppendLine("==============================================");

            return additionDescription.ToString();
        }

        private string SplitAttendees(string attendees)
        {
            var attendeesBuilder = new StringBuilder(string.Empty);
            if (string.IsNullOrEmpty(attendees))
            {
                return attendees;
            }

            foreach (string attendeeName in attendees.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries))
            {
                attendeesBuilder.AppendLine(attendeeName.Trim());
            }
            return attendees;
        }

        private CalendarService GetCalendarService()
        {
            return AccountAuthenticationService.AuthenticateCalenderOauth();
        }

        #endregion

        #region IGoogleCalendarService Members

        public async Task<List<Calendar>> GetAvailableCalendars()
        {
            //Get Calendar Service
            CalendarService calendarService = GetCalendarService();

            CalendarList calenderList = await calendarService.CalendarList.List().ExecuteAsync();

            List<Calendar> locaCalendarList =
                calenderList.Items.Select(
                    calendarListEntry => new Calendar {Id = calendarListEntry.Id, Name = calendarListEntry.Summary})
                    .ToList();
            return locaCalendarList;
        }

        public async Task<bool> AddCalendarEvent(Appointment calenderAppointment, string calenderId, bool addDescription,
            bool addReminder, bool addAttendees)
        {
            //Get Calendar Service
            CalendarService calendarService = GetCalendarService();

            if (calenderAppointment == null || string.IsNullOrEmpty(calenderId))
            {
                return false;
            }

            Event calendarEvent = CreateGoogleCalendarEvent(calenderAppointment, addDescription, addReminder,
                addAttendees);
            Event returnEvent = await calendarService.Events.Insert(calendarEvent, calenderId).ExecuteAsync();

            return returnEvent != null;
        }

        public async Task<bool> AddCalendarEvent(List<Appointment> appointments, string calenderId, bool addDescription,
            bool addReminder, bool addAttendees)
        {
            var eventIndexList = new Dictionary<KeyValuePair<int, Appointment>, HttpResponseMessage>();
            //Get Calendar Service
            CalendarService calendarService = GetCalendarService();

            if (appointments == null || string.IsNullOrEmpty(calenderId))
            {
                return false;
            }
            if (appointments.Any())
            {
                //Create a Batch Request
                var batchRequest = new BatchRequest(calendarService);

                //Split the list of appointments by 1000 per list

                //Iterate over each appointment to create a event and batch it 
                for (int i = 0; i < appointments.Count; i++)
                {
                    if (i != 0 && i%999 == 0)
                    {
                        await batchRequest.ExecuteAsync();
                        batchRequest = new BatchRequest(calendarService);
                    }

                    Appointment appointment = appointments[i];
                    Event calendarEvent = CreateGoogleCalendarEvent(appointment, addDescription, addReminder,
                        addAttendees);
                    EventsResource.InsertRequest insertRequest = calendarService.Events.Insert(calendarEvent, calenderId);
                    batchRequest.Queue<Event>(insertRequest,
                        (content, error, index, message) =>
                            InsertEventErrorMessage(content, error, index, message, eventIndexList));
                }

                await batchRequest.ExecuteAsync();
            }
            return true;
        }

        public async Task<bool> DeleteCalendarEvent(Appointment calendarAppointment, string calenderId)
        {
            //Get Calendar Service
            CalendarService calendarService = GetCalendarService();

            if (calendarAppointment == null || string.IsNullOrEmpty(calenderId))
            {
                return false;
            }

            //TODO: Manage return value for Event delete
            string returnValue =
                await calendarService.Events.Delete(calenderId, calendarAppointment.AppointmentId).ExecuteAsync();
            return true;
        }


        public async Task<bool> DeleteCalendarEvent(List<Appointment> calendarAppointment, string calenderId)
        {
            var eventIndexList = new Dictionary<KeyValuePair<int, Appointment>, HttpResponseMessage>();
            //Get Calendar Service
            CalendarService calendarService = GetCalendarService();

            if (calendarAppointment == null || string.IsNullOrEmpty(calenderId))
            {
                return false;
            }
            if (calendarAppointment.Any())
            {
                //Create a Batch Request
                var batchRequest = new BatchRequest(calendarService);

                //Split the list of appointments by 1000 per list

                //Iterate over each appointment to create a event and batch it 
                for (int i = 0; i < calendarAppointment.Count; i++)
                {
                    if (i != 0 && i%999 == 0)
                    {
                        await batchRequest.ExecuteAsync();
                        batchRequest = new BatchRequest(calendarService);
                    }

                    Appointment appointment = calendarAppointment[i];
                    EventsResource.DeleteRequest deleteRequest = calendarService.Events.Delete(calenderId,
                        appointment.AppointmentId);
                    batchRequest.Queue<Event>(deleteRequest,
                        (content, error, index, message) =>
                            InsertEventErrorMessage(content, error, index, message, eventIndexList));
                }

                await batchRequest.ExecuteAsync();
            }
            return true;
        }

        public async Task<List<Appointment>> GetCalendarEventsInRangeAsync(int daysInPast, int daysInFuture,
            string calenderId)
        {
            //Get Calendar Service
            CalendarService calendarService = GetCalendarService();

            var finalEventList = new List<Appointment>();

            Events result = null;

            EventsResource.ListRequest eventListRequest = calendarService.Events.List(calenderId);

            // Add Filters to event List Request
            eventListRequest.TimeMin = DateTime.Now.AddDays(-(daysInPast));
            eventListRequest.TimeMax = DateTime.Now.AddDays((daysInFuture + 1));

            try
            {
                result = eventListRequest.Execute();
            }
            catch (GoogleApiException exception)
            {
                _applicationLogger.LogError(exception.ToString());
            }
            while (result.Items != null)
            {
                // Add events to list
                finalEventList.AddRange(
                    result.Items.Select(CreateAppointment));

                //If all pages are over break
                if (result.NextPageToken == null)
                {
                    break;
                }

                //Set the next page to pull from request
                eventListRequest.PageToken = result.NextPageToken;

                result = await eventListRequest.ExecuteAsync();
            }
            return finalEventList;
        }

        #endregion

        private void InsertEventErrorMessage(Event content, RequestError error, int index, HttpResponseMessage message,
            Dictionary<KeyValuePair<int, Appointment>, HttpResponseMessage> eventListIndex)
        {
            //var getKey = eventListIndex.FirstOrDefault(pair => pair.Key.Key == index);
            // eventListIndex[getKey.Key] = message;
        }

        private Appointment CreateAppointment(Event googleEvent)
        {
            Appointment appointment;
            if (googleEvent.Start.DateTime == null && googleEvent.End.DateTime == null)
            {
                appointment = new Appointment(googleEvent.Description, googleEvent.Location, googleEvent.Summary,
                    DateTime.Parse(googleEvent.End.Date),
                    DateTime.Parse(googleEvent.Start.Date), googleEvent.Id);
                appointment.AllDayEvent = true;
            }
            else
            {
                appointment = new Appointment(googleEvent.Description, googleEvent.Location, googleEvent.Summary,
                    googleEvent.End.DateTime,
                    googleEvent.Start.DateTime, googleEvent.Id);
            }
            return appointment;
        }
    }
}