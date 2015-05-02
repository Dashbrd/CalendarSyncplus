using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CalendarSyncPlus.Domain.Helpers;
using CalendarSyncPlus.Domain.Models;
using Recipient = CalendarSyncPlus.Domain.Models.Recipient;

namespace CalendarSyncPlus.Services.Utilities
{
    public static class AppointmentHelper
    {
        public static void LoadSourceId(this Appointment calendarAppointment, string sourceCalendarId)
        {
            string key = GetSourceEntryKey(sourceCalendarId);
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