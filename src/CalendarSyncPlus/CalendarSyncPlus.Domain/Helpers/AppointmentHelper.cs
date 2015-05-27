using System;
using System.Linq;
using System.Text;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Domain.Helpers
{
    public static class AppointmentHelper
    {
        public const string LineBreak = "==============================================";

        public static string GetDescription(this Recipient recipient)
        {
            return String.Format("{0} <{1}>", recipient.Name, recipient.Email);
        }

        public static string GetDescriptionData(this Appointment calendarAppointment, bool addDescription,
            bool addAttendees)
        {
            var additionDescription = new StringBuilder(string.Empty);
            if (addDescription)
            {
                additionDescription.Append(calendarAppointment.Description);
            }

            if (!addAttendees)
            {
                return additionDescription.ToString();
            }
            bool hasData = false;
            //Start Header
            var attendeesDescription = calendarAppointment.GetAttendeesData();
            if (!String.IsNullOrEmpty(attendeesDescription))
            {
                additionDescription.AppendLine(attendeesDescription);
            }
            return additionDescription.ToString();
        }

        public static string GetAttendeesData(this Appointment calendarAppointment)
        {
            bool hasData = false;
            StringBuilder attendeesDescription = new StringBuilder();
            //Start Header
            attendeesDescription.AppendLine(LineBreak);
            attendeesDescription.AppendLine(string.Empty);
            if (calendarAppointment.Organizer != null)
            {
                //Add Organiser
                attendeesDescription.AppendLine("Organizer");

                attendeesDescription.AppendLine(calendarAppointment.Organizer.GetDescription());
                attendeesDescription.AppendLine(string.Empty);
                hasData = true;
            }
            //Add Required Attendees
            if (calendarAppointment.RequiredAttendees.Any())
            {
                attendeesDescription.AppendLine("Required Attendees:");

                foreach (Recipient requiredAttendee in calendarAppointment.RequiredAttendees)
                {
                    attendeesDescription.AppendLine(requiredAttendee.GetDescription());
                }

                attendeesDescription.AppendLine(string.Empty);
                hasData = true;
            }
            //Add Optional Attendees

            if (calendarAppointment.OptionalAttendees.Any())
            {
                attendeesDescription.AppendLine("Optional Attendees:");
                foreach (Recipient requiredAttendee in calendarAppointment.OptionalAttendees)
                {
                    attendeesDescription.AppendLine(requiredAttendee.GetDescription());
                }
                attendeesDescription.AppendLine(string.Empty);
                hasData = true;
            }

            //Close Header
            return hasData ? attendeesDescription.ToString() : string.Empty;
        }


        /// <summary>
        /// </summary>
        /// <param name="appointment"></param>
        /// <param name="otherAppointment"></param>
        /// <param name="addAttendeesToDescription"></param>
        /// <returns></returns>
        public static bool CompareDescription(this Appointment appointment, Appointment otherAppointment, bool addAttendeesToDescription)
        {
            if (String.IsNullOrEmpty(appointment.Description) && String.IsNullOrEmpty(otherAppointment.Description))
            {
                return true;
            }

            string description = ParseDescription(appointment);
            string otherDescription = ParseDescription(otherAppointment);
            if (description.Trim().Equals(otherDescription.Trim()))
            {
                return true;
            }

            if (description.Length > 8000 && description.Length > otherDescription.Length)
            {
                if (description.Contains(otherDescription))
                {
                    return true;
                }
            }

            if (otherDescription.Length > 8000 && otherDescription.Length > description.Length)
            {
                if (otherDescription.Contains(description))
                {
                    return true;
                }
            }

            return false;
        }
        public static string ParseAttendees(this Appointment appointment)
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

        public static string ParseDescription(this Appointment appointment)
        {
            if (appointment.Description == null)
            {
                return String.Empty;
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
                    description = String.Empty;
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
    }
}
