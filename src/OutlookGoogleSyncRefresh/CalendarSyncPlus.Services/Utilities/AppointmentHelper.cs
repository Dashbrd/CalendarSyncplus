using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CalendarSyncPlus.Domain.Models;
using Recipient = CalendarSyncPlus.Domain.Models.Recipient;

namespace CalendarSyncPlus.Services.Utilities
{
    public static class AppointmentHelper
    {
        public const string LineBreak = "==============================================";

        public static void LoadSourceId(this Appointment calendarAppointment, string sourceCalendarId)
        {
            string key = GetSourceEntryKey(sourceCalendarId);
            object value;
            if (calendarAppointment.ExtendedProperties.TryGetValue(key, out value))
            {
                calendarAppointment.SourceId = value.ToString();
            }
        }

        public static string GetSourceEntryKey(this Appointment calendarAppointment)
        {
            return GetSourceEntryKey(calendarAppointment.CalendarId);
        }

        public static string GetSourceEntryKey(string sourceCalendarId)
        {
            return GetMD5Hash(sourceCalendarId);
        }

        public static bool CompareSourceId(this Appointment calendarAppointment, Appointment otherAppointment)
        {
            if (otherAppointment.SourceId == null)
            {
                return false;
            }
            return calendarAppointment.AppointmentId.Equals(otherAppointment.SourceId);
        }

        public static string GetDescriptionData(this Appointment calenderAppointment, bool addDescription,
            bool addAttendees)
        {
            var additionDescription = new StringBuilder(string.Empty);
            if (addDescription)
            {
                additionDescription.Append(calenderAppointment.Description);
            }

            if (!addAttendees)
            {
                return additionDescription.ToString();
            }
            bool hasData = false;
            //Start Header
            additionDescription.AppendLine(LineBreak);
            additionDescription.AppendLine(string.Empty);
            if (calenderAppointment.Organizer != null)
            {
                //Add Organiser
                additionDescription.AppendLine("Organizer");

                additionDescription.AppendLine(calenderAppointment.Organizer.GetDescription());
                additionDescription.AppendLine(string.Empty);
                hasData = true;
            }
            //Add Required Attendees
            if (calenderAppointment.RequiredAttendees.Any())
            {
                additionDescription.AppendLine("Required Attendees:");

                foreach (Recipient requiredAttendee in calenderAppointment.RequiredAttendees)
                {
                    additionDescription.AppendLine(requiredAttendee.GetDescription());
                }

                additionDescription.AppendLine(string.Empty);
                hasData = true;
            }
            //Add Optional Attendees

            if (calenderAppointment.OptionalAttendees.Any())
            {
                additionDescription.AppendLine("Optional Attendees:");
                foreach (Recipient requiredAttendee in calenderAppointment.OptionalAttendees)
                {
                    additionDescription.AppendLine(requiredAttendee.GetDescription());
                }
                additionDescription.AppendLine(string.Empty);
                hasData = true;
            }

            //Close Header
            additionDescription.AppendLine(LineBreak);

            return hasData ? additionDescription.ToString() : string.Empty;
        }

        private static string GetDescription(this Recipient recipient)
        {
            return string.Format("{0} <{1}>", recipient.Name, recipient.Email);
        }

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
                        appointment.Description.Split(new[] {LineBreak}, StringSplitOptions.RemoveEmptyEntries).First();
                }
                else
                {
                    description = string.Empty;
                }
            }
            return description;
        }

        private static string GetMD5Hash(string stringToHash)
        {
            var sb = new StringBuilder();
            using (MD5 md5 = MD5.Create())
            {
                byte[] retVal = md5.ComputeHash(Encoding.Unicode.GetBytes(stringToHash));

                foreach (byte byteval in retVal)
                {
                    sb.Append(byteval.ToString("x2"));
                }
            }
            return sb.ToString();
        }
    }
}