using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Domain.Helpers
{
    public static class AppointmentHelper
    {
        public const string LineBreak = "==============================================";

        public static string GetDescription(this Attendee attendee)
        {
            return $"{attendee.Name} <{attendee.Email}>";
        }

        public static string GetDescriptionData(this Appointment calendarAppointment, bool addDescription,
            bool addAttendees)
        {
            if (!addAttendees)
            {
                return calendarAppointment.Description;
            }

            var additionDescription = new StringBuilder(string.Empty);
            if (addDescription)
            {
                var description = TrimDescription(calendarAppointment.Description);
                additionDescription.Append(description);
            }
            
            //Start Header
            var attendeesDescription = calendarAppointment.GetAttendeesData();
            if (!String.IsNullOrEmpty(attendeesDescription))
            {
                additionDescription.AppendLine(attendeesDescription);
            }
            return additionDescription.ToString();
        }

        private static string TrimDescription(string description)
        {
            try
            {
                if (!string.IsNullOrEmpty(description) && description.Contains(LineBreak))
                {
                    return description.Remove(description.IndexOf(LineBreak, StringComparison.InvariantCultureIgnoreCase)).Trim();
                }
            }
            catch
            {
                return description;
            }
            return description;
        }

        public static string GetAttendeesData(this Appointment calendarAppointment)
        {
            var hasData = false;
            var attendeesDescription = new StringBuilder();
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

                foreach (var requiredAttendee in calendarAppointment.RequiredAttendees)
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
                foreach (var requiredAttendee in calendarAppointment.OptionalAttendees)
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
        /// <returns>
        /// </returns>
        public static bool CompareDescription(this Appointment appointment, Appointment otherAppointment,
            bool addAttendeesToDescription)
        {
            if (String.IsNullOrEmpty(appointment.Description) && String.IsNullOrEmpty(otherAppointment.Description))
            {
                return true;
            }

            var description = ParseDescription(appointment);
            var otherDescription = ParseDescription(otherAppointment);
            if (description.Equals(otherDescription))
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

            var description = appointment.Description;
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

        public static string ParseDescription(this Appointment appointment)
        {
            if (appointment.Description == null)
            {
                return String.Empty;
            }

            var description = appointment.Description;
            if (appointment.Description.Contains(LineBreak))
            {
                if (appointment.Description.IndexOf(LineBreak, StringComparison.Ordinal) > 1)
                {
                    description =
                        appointment.Description.Split(new[] {LineBreak}, StringSplitOptions.RemoveEmptyEntries).First();
                }
                else
                {
                    description = String.Empty;
                }
            }
            return description.Trim();
        }

        public static bool CompareSourceId(this Appointment calendarAppointment, Appointment otherAppointment)
        {
            if (otherAppointment.SourceId == null)
            {
                return false;
            }
            return calendarAppointment.AppointmentId.Equals(otherAppointment.SourceId);
        }

        public static void LoadSourceId(this Appointment calendarAppointment, string sourceCalendarId)
        {
            var key = GetSourceEntryKey(sourceCalendarId);
            string value;
            if (calendarAppointment.ExtendedProperties.TryGetValue(key, out value))
            {
                calendarAppointment.SourceId = value;
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

        public static void LoadChildId(this Appointment calendarAppointment, string sourceCalendarId)
        {
            var key = GetChildEntryKey(sourceCalendarId);
            string value;
            if (calendarAppointment.ExtendedProperties.TryGetValue(key, out value))
            {
                calendarAppointment.ChildId = value;
            }
        }

        public static string GetChildEntryKey(this Appointment calendarAppointment)
        {
            return GetChildEntryKey(calendarAppointment.CalendarId);
        }

        public static string GetChildEntryKey(string sourceCalendarId)
        {
            return $"Child-{GetMD5Hash(sourceCalendarId)}";
        }

        private static string GetMD5Hash(string stringToHash)
        {
            try
            {
                var sb = new StringBuilder();
                using (var md5 = MD5.Create())
                {
                    var retVal = md5.ComputeHash(Encoding.Unicode.GetBytes(stringToHash));

                    foreach (var byteval in retVal)
                    {
                        sb.Append(byteval.ToString("x2"));
                    }
                }
                return sb.ToString();
            }
            catch
            {
                return "CalendarSyncPlusCalendar";
            }
        }

        public static string GetGoogleResponseStatus(MeetingResponseStatusEnum responseStatus)
        {
            //
            // Summary:
            //     The attendee's response status. Possible values are: - "needsAction" - The
            //     attendee has not responded to the invitation. - "declined" - The attendee
            //     has declined the invitation. - "tentative" - The attendee has tentatively
            //     accepted the invitation. - "accepted" - The attendee has accepted the invitation.
            switch (responseStatus)
            {
                    case MeetingResponseStatusEnum.Accepted:
                    case MeetingResponseStatusEnum.Organizer:
                    return "accepted";
                    case MeetingResponseStatusEnum.None:
                    return "";
                    case MeetingResponseStatusEnum.Tentative:
                    return "tentative";
                    case MeetingResponseStatusEnum.Declined:
                    return "declined";
            }
            return "needsAction";
        }


        public static MeetingResponseStatusEnum GetGoogleResponseStatus(string responseStatus)
        {
            //
            // Summary:
            //     The attendee's response status. Possible values are: - "needsAction" - The
            //     attendee has not responded to the invitation. - "declined" - The attendee
            //     has declined the invitation. - "tentative" - The attendee has tentatively
            //     accepted the invitation. - "accepted" - The attendee has accepted the invitation.
            switch (responseStatus)
            {
                case "accepted":
                    return MeetingResponseStatusEnum.Accepted;
                case "tentative":
                    return MeetingResponseStatusEnum.Tentative;
                case "declined":
                    return MeetingResponseStatusEnum.Declined;
            }
            return MeetingResponseStatusEnum.NotResponded;
        }

        public static string GetSensitivity(SensitivityEnum sensitivity)
        {
            // Visibility of the event. Optional. Possible values are: - "default" - Uses the default visibility
            //             for events on the calendar. This is the default value. - "public" - The event is public and event details
            //             are visible to all readers of the calendar. - "private" - The event is private and only event attendees may
            //             view event details. - "confidential" - The event is private. This value is provided for compatibility
            //             reasons.
            switch (sensitivity)
            {
                    case SensitivityEnum.Confidential:
                        return "confidential";
                    case SensitivityEnum.Public:
                        return "public";
                    case SensitivityEnum.Private:
                        return "private";
            }
            return "default";
        }

        public static SensitivityEnum GetSensitivityEnum(string sensitivity)
        {
            switch (sensitivity)
            {
                case "confidential":
                    return SensitivityEnum.Confidential;
                case "public":
                    return SensitivityEnum.Public;
                case "private":
                    return SensitivityEnum.Private;
            }
            return SensitivityEnum.Normal;
        }
        
    }
}