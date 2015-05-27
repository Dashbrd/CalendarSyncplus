using System.Linq;
using CalendarSyncPlus.Domain.Helpers;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.SyncEngine.Helpers
{
    public static class ExtensionMethods
    {
        public static bool CopyDetail(this Appointment appointment, Appointment otherAppointment, CalendarEntryOptionsEnum calendarEntryOptions)
        {
            appointment.OldStartTime = appointment.StartTime;
            appointment.StartTime = otherAppointment.StartTime;
            appointment.EndTime = otherAppointment.EndTime;
            appointment.Subject = otherAppointment.Subject;
            appointment.AllDayEvent = otherAppointment.AllDayEvent;

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
