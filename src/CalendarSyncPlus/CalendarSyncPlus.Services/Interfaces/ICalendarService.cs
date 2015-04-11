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
//  *      Created On:     05-03-2015 3:09 PM
//  *      Modified On:    05-03-2015 3:09 PM
//  *      FileName:       ICalendarService.cs
//  * 
//  *****************************************************************************/

#endregion

using System.Collections.Generic;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Services.Wrappers;

namespace CalendarSyncPlus.Services.Interfaces
{
    public interface ICalendarService
    {
        string CalendarServiceName { get; }

        Task<bool> DeleteCalendarEvent(List<Appointment> calendarAppointments,
            IDictionary<string, object> calendarSpecificData);

        Task<CalendarAppointments> GetCalendarEventsInRangeAsync(int daysInPast, int daysInFuture,
            IDictionary<string, object> calendarSpecificData);

        Task<bool> AddCalendarEvent(List<Appointment> calendarAppointments, bool addDescription,
            bool addReminder, bool addAttendees, bool attendeesToDescription,
            IDictionary<string, object> calendarSpecificData);

        void CheckCalendarSpecificData(IDictionary<string, object> calendarSpecificData);

        Task<bool> ResetCalendar(IDictionary<string, object> calendarSpecificData);

        void SetCalendarColor(Category background, IDictionary<string, object> calendarSpecificData);
    }
}