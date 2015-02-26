using System;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Domain.Helpers
{
    public static class ExtensionMethods
    {
        public static string Rfc339FFormat(this DateTime dateTime)
        {
            string timezone = TimeZoneInfo.Local.GetUtcOffset(dateTime).ToString();
            if (timezone[0] != '-') timezone = '+' + timezone;
            timezone = timezone.Substring(0, 6);

            var result = dateTime.GetDateTimeFormats('s')[0] + timezone;
            return result;
        }


        public static bool ValidateOutlookOptions(this Settings settings)
        {
            if (!settings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultProfile) &&
                string.IsNullOrEmpty(settings.OutlookProfileName))
            {
                return false;
            }

            if (!settings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultCalendar) &&
                (settings.OutlookCalendar == null || settings.OutlookMailBox == null))
            {
                return false;
            }

            return true;
        }

        public static void UpdateEntryOptions(this Settings settings, bool addDescription, bool addReminders, bool addAttendees)
        {
            settings.CalendarEntryOptions = CalendarEntryOptionsEnum.None;
            if (addDescription)
            {
                settings.CalendarEntryOptions = settings.CalendarEntryOptions | CalendarEntryOptionsEnum.Description;
            }

            if (addReminders)
            {
                settings.CalendarEntryOptions = settings.CalendarEntryOptions | CalendarEntryOptionsEnum.Reminders;
            }

            if (addAttendees)
            {
                settings.CalendarEntryOptions = settings.CalendarEntryOptions | CalendarEntryOptionsEnum.Attendees;
            }
        }


        public static void UpdateOutlookOptions(this Settings settings, bool isDefaultProfile, bool isDefaultMailBox, bool isExchangeWebServices)
        {
            settings.OutlookOptions = OutlookOptionsEnum.None;
            if (isDefaultProfile)
            {
                settings.OutlookOptions = settings.OutlookOptions | OutlookOptionsEnum.AlternateProfile;
            }

            if (isDefaultMailBox)
            {
                settings.OutlookOptions = settings.OutlookOptions | OutlookOptionsEnum.AlternateCalendar;
            }

            if (isExchangeWebServices)
            {
                settings.OutlookOptions = settings.OutlookOptions | OutlookOptionsEnum.ExchangeWebServices;
            }
        }

    }
}