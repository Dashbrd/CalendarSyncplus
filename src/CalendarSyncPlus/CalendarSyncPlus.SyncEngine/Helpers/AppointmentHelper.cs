using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.SyncEngine.Helpers
{
    public static class AppointmentHelper
    {
        public const string LineBreak = "==============================================";
        /// <summary>
        /// </summary>
        /// <param name="appointment"></param>
        /// <param name="otherAppointment"></param>
        /// <returns></returns>
        public static bool CompareDescription(this Appointment appointment, Appointment otherAppointment)
        {
            if (string.IsNullOrEmpty(appointment.Description) && string.IsNullOrEmpty(otherAppointment.Description))
            {
                return true;
            }

            string description = ParseDescription(appointment);
            string otherDescription = ParseDescription(otherAppointment);
            if (description.Equals(otherDescription))
            {
                return true;
            }
            return false;
        }

        private static string ParseDescription(Appointment appointment)
        {
            if (appointment.Description == null)
            {
                return string.Empty;
            }

            string description = appointment.Description;
            if (appointment.Description.Contains(LineBreak))
            {
                if (appointment.Description.IndexOf(LineBreak, StringComparison.Ordinal) > 1)
                {
                    description =
                        appointment.Description.Split(new[] { LineBreak }, StringSplitOptions.RemoveEmptyEntries).First();
                }
                else
                {
                    description = string.Empty;
                }
            }
            return description;
        }

        public static bool CompareSourceId(this Appointment calendarAppointment, Appointment otherAppointment)
        {
            if (otherAppointment.SourceId == null)
            {
                return false;
            }
            return calendarAppointment.AppointmentId.Equals(otherAppointment.SourceId);
        }

        public static bool CopyDetail(this Appointment appointment, Appointment otherAppointment, CalendarEntryOptionsEnum calendarEntryOptions)
        {
            if (calendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description))
            {

                if (calendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description))
                {

                }

            }

            if (calendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attendees))
            {

            }

            if (calendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description))
            {

            }
            return true;
        }
    }
}
