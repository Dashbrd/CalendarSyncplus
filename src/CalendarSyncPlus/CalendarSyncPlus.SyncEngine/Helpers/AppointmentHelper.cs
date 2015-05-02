using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Helpers;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.SyncEngine.Helpers
{
    public static class AppointmentHelper
    {
        public static bool CopyDetail(this Appointment appointment, Appointment otherAppointment, CalendarEntryOptionsEnum calendarEntryOptions)
        {
            if (calendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description))
            {
                appointment.Description = otherAppointment.ParseDescription();
            }

            if (calendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attendees)
                && !calendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.AttendeesToDescription))
            {
                appointment.RequiredAttendees = otherAppointment.RequiredAttendees.Select(t => t).ToList();
                appointment.OptionalAttendees = otherAppointment.OptionalAttendees.Select(t => t).ToList();
                appointment.Organizer = otherAppointment.Organizer;
            }

            if (calendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders))
            {
                appointment.ReminderSet = otherAppointment.ReminderSet;
                appointment.ReminderMinutesBeforeStart = otherAppointment.ReminderMinutesBeforeStart;
            }
            return true;
        }
    }
}
