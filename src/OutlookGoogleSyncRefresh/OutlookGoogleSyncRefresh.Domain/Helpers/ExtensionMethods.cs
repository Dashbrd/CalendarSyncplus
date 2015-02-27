using System;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Domain.Helpers
{
    public static class ExtensionMethods
    {
        public static string Rfc339FFormat(this DateTime dateTime)
        {
            string timezone = TimeZoneInfo.Local.GetUtcOffset(dateTime).ToString();
            if (timezone[0] != '-')
            {
                timezone = '+' + timezone;
            }
            timezone = timezone.Substring(0, 6);

            string result = dateTime.GetDateTimeFormats('s')[0] + timezone;
            return result;
        }


        public static bool ValidateOutlookSettings(this Settings settings)
        {
            if (!settings.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultProfile) &&
                string.IsNullOrEmpty(settings.OutlookSettings.OutlookProfileName))
            {
                return false;
            }

            if (!settings.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultCalendar) &&
                (settings.OutlookSettings.OutlookCalendar == null || settings.OutlookSettings.OutlookMailBox == null))
            {
                return false;
            }

            return true;
        }

        public static void UpdateEntryOptions(this Settings settings, bool addDescription, bool addReminders,
            bool addAttendees, bool addAttachments)
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

            if (addAttachments)
            {
                settings.CalendarEntryOptions = settings.CalendarEntryOptions | CalendarEntryOptionsEnum.Attachments;
            }
        }


        public static void UpdateOutlookOptions(this OutlookSettings settings, bool isDefaultProfile,
            bool isDefaultMailBox, bool isExchangeWebServices)
        {
            settings.OutlookOptions = OutlookOptionsEnum.None;

            if (isExchangeWebServices)
            {
                settings.OutlookOptions = settings.OutlookOptions | OutlookOptionsEnum.ExchangeWebServices;
            }
            else
            {
                settings.OutlookOptions = !isDefaultProfile
                    ? settings.OutlookOptions | OutlookOptionsEnum.AlternateProfile
                    : settings.OutlookOptions | OutlookOptionsEnum.DefaultProfile;

                settings.OutlookOptions = !isDefaultMailBox
                    ? settings.OutlookOptions | OutlookOptionsEnum.AlternateCalendar
                    : settings.OutlookOptions | OutlookOptionsEnum.DefaultCalendar;
            }
        }

        public static bool IsTimeValid(this DateTime dateTime, DateTime timeOfDay)
        {
            if (dateTime.ToString("HH:mm").Equals(timeOfDay.ToString("HH:mm")))
            {
                return true;
            }
            return false;
        }
    }
}