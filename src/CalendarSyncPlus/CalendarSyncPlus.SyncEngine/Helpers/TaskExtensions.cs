using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.SyncEngine.Helpers
{
    public static class TaskExtensions
    {
        public static void CopyDetail(this ReminderTask appointment, ReminderTask otherAppointment)
        {
            appointment.IsDeleted = otherAppointment.IsDeleted;
            appointment.IsCompleted = otherAppointment.IsCompleted;
            appointment.Due = otherAppointment.Due;
            appointment.CompletedOn = otherAppointment.CompletedOn;
            appointment.Title = otherAppointment.Title;
            appointment.Notes = otherAppointment.Notes;
        }
    }
}