using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Test.Model;
using Calendar = Test.Model.Calendar;

namespace Test.Services
{
    public class GoogleCalendarService
    {
        public CalendarService CalendarService { get; private set; }

        public GoogleCalendarService(CalendarService calendarService)
        {
            CalendarService = calendarService;
        }

        public async Task<List<Calendar>> GetAvailableCalendars()
        {
            CalendarList calenderList = await CalendarService.CalendarList.List().ExecuteAsync();

            var locaCalendarList =
                calenderList.Items.Select(
                    calendarListEntry => new Calendar() {Id = calendarListEntry.Id, Name = calendarListEntry.Summary})
                    .ToList();
            return locaCalendarList;
        }

        public async Task<bool> AddCalendarEvent(Event calendarEvent,string calenderId)
        {
            if (calendarEvent == null || string.IsNullOrEmpty(calenderId)) return false;

            Event returnEvent = await CalendarService.Events.Insert(calendarEvent, calenderId).ExecuteAsync();

            return returnEvent.Id == calendarEvent.Id;
        }

        public async Task<bool> AddCalendarEvent(Appointment calenderAppointment, string calenderId, bool addDescription,
            bool addReminder, bool addAttendees)
        {
            if (calenderAppointment == null || string.IsNullOrEmpty(calenderId)) return false;

            var calendarEvent = CreateGoogleCalendarEvent(calenderAppointment, addDescription, addReminder, addAttendees);
            var isSame = false;
            Event returnEvent = await CalendarService.Events.Insert(calendarEvent, calenderId).ExecuteAsync();
            isSame = returnEvent.Id == calenderAppointment.AppointmentId;

            return isSame;
        }

        private Event CreateGoogleCalendarEvent(Appointment calenderAppointment,bool addDescription,bool addReminder,bool addAttendees)
        {
            //Create Event
            var googleEvent = new Event
            {
                Start = new EventDateTime(),
                End = new EventDateTime(),
                Summary = calenderAppointment.Subject,
                Description = addDescription ? calenderAppointment.Description + GetAdditionalDescriptionData(addAttendees,calenderAppointment) : String.Empty,
                Location = calenderAppointment.Location
            };

            //Add Start/End Time
            if (calenderAppointment.AllDayEvent)
            {
                if (calenderAppointment.StartTime.HasValue)
                    googleEvent.Start.Date = calenderAppointment.StartTime.Value.ToString("yyyy-MM-dd");
                if (calenderAppointment.EndTime.HasValue)
                    googleEvent.End.Date = calenderAppointment.EndTime.Value.ToString("yyyy-MM-dd");
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

            if (!addAttendees) return additionDescription.ToString();
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
            if (string.IsNullOrEmpty(attendees)) return attendees;

            foreach (string attendeeName in attendees.Split(new []{";"},StringSplitOptions.RemoveEmptyEntries))
            {
                attendeesBuilder.AppendLine(attendeeName.Trim());
            }
            return attendees;
        }

        public async Task<bool> DeleteCalendarEvent(Appointment calendarAppointment, string calenderId)
        {
            if (calendarAppointment == null || string.IsNullOrEmpty(calenderId)) return false;

            //TODO: Manage return value for Event delete
            string returnValue = await CalendarService.Events.Delete(calenderId, calendarAppointment.AppointmentId).ExecuteAsync();
            return true;

        }
        public async Task<List<Appointment>> GetCalendarEventsInRangeAsync(int daysInPast, int daysInFuture, string calenderId)
        {
            var finalEventList = new List<Appointment>();

            Events result = null;

            var eventListRequest = CalendarService.Events.List(calenderId);

            // Add Filters to event List Request
            eventListRequest.TimeMin = DateTime.Now.AddDays(-(daysInPast));
            eventListRequest.TimeMax = DateTime.Now.AddDays((daysInFuture+1));

            result = await eventListRequest.ExecuteAsync();

            while (result.Items != null)
            {
                // Add events to list
                finalEventList.AddRange(
                    result.Items.Select(
                        item =>
                            new Appointment(item.Description, item.Location, item.Summary, item.End.DateTime,
                                item.Start.DateTime, item.Id)));

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
    }
}