﻿using System;
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

        public static void UpdateEntryOptions(this Settings settings, bool addDescription, bool addReminders, bool addAttendees, bool addAttachments)
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


        public static void UpdateOutlookOptions(this OutlookSettings settings, bool isDefaultProfile, bool isDefaultMailBox, bool isExchangeWebServices)
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