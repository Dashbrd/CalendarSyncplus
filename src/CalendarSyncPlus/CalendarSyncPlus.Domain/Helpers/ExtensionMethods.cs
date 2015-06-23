using System;
using System.Linq;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Domain.Helpers
{
    public static class ExtensionMethods
    {
        public static string Rfc339FFormat(this DateTime dateTime)
        {
            var timezone = TimeZoneInfo.Local.GetUtcOffset(dateTime).ToString();
            if (timezone[0] != '-')
            {
                timezone = '+' + timezone;
            }
            timezone = timezone.Substring(0, 6);

            var result = dateTime.GetDateTimeFormats('s')[0] + timezone;
            return result;
        }

        public static bool ValidateSettings(this Settings settings)
        {
            if (settings == null || !settings.SyncProfiles.Any() ||
                settings.SyncProfiles.All(t => !t.ValidateOutlookSettings() || t.GoogleAccount == null))
            {
                return false;
            }
            return true;
        }

        public static bool ValidateOutlookSettings(this CalendarSyncProfile syncProfile)
        {
            if (!syncProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultProfile) &&
                string.IsNullOrEmpty(syncProfile.OutlookSettings.OutlookProfileName))
            {
                return false;
            }

            if (!syncProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultCalendar) &&
                (syncProfile.OutlookSettings.OutlookCalendar == null ||
                 syncProfile.OutlookSettings.OutlookMailBox == null))
            {
                return false;
            }

            return true;
        }

        public static void UpdateEntryOptions(this CalendarSyncProfile syncProfile, bool addDescription,
            bool addReminders,
            bool addAttendees, bool addAttendeesToDescription, bool addAttachments, bool asAppointments)
        {
            syncProfile.CalendarEntryOptions = CalendarEntryOptionsEnum.None;
            if (addDescription)
            {
                syncProfile.CalendarEntryOptions = syncProfile.CalendarEntryOptions |
                                                   CalendarEntryOptionsEnum.Description;
            }

            if (addReminders)
            {
                syncProfile.CalendarEntryOptions = syncProfile.CalendarEntryOptions | CalendarEntryOptionsEnum.Reminders;
            }

            if (addAttendees)
            {
                syncProfile.CalendarEntryOptions = syncProfile.CalendarEntryOptions | CalendarEntryOptionsEnum.Attendees;
                if (addAttendeesToDescription)
                {
                    syncProfile.CalendarEntryOptions = syncProfile.CalendarEntryOptions |
                                                       CalendarEntryOptionsEnum.AttendeesToDescription;
                }
            }

            if (addAttachments)
            {
                syncProfile.CalendarEntryOptions = syncProfile.CalendarEntryOptions |
                                                   CalendarEntryOptionsEnum.Attachments;
            }

            if (asAppointments)
            {
                syncProfile.CalendarEntryOptions = syncProfile.CalendarEntryOptions |
                                                   CalendarEntryOptionsEnum.AsAppointments;
            }
        }

        public static void UpdateOutlookOptions(this OutlookSettings settings, OutlookOptionsEnum isDefaultProfile,
            OutlookOptionsEnum isDefaultMailBox, bool isExchangeWebServices)
        {
            settings.OutlookOptions = OutlookOptionsEnum.None;

            if (isExchangeWebServices)
            {
                settings.OutlookOptions = settings.OutlookOptions | OutlookOptionsEnum.ExchangeWebServices;
            }
            else
            {
                settings.OutlookOptions = settings.OutlookOptions | isDefaultProfile;
                settings.OutlookOptions = settings.OutlookOptions | isDefaultMailBox;
            }
        }

        public static bool IsTimeValid(this DateTime dateTime, DateTime timeOfDay)
        {
            if (dateTime.ToString("HH:mm:ss").Equals(timeOfDay.ToString("HH:mm:ss")))
            {
                return true;
            }
            return false;
        }
    }
}