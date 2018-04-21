using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using CalendarSyncPlus.Domain.File.Json;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using Newtonsoft.Json;

namespace CalendarSyncPlus.Domain.Helpers
{
    public static class ExtensionMethods
    {
        //Deep clone
        public static T DeepClone<T>(this T source)
        {
            if (source == null)
            {
                return default(T);
            }

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                NullValueHandling = NullValueHandling.Ignore
            };
            deserializeSettings.Converters.Add(new FrequencyConverter());
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }

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
            if (settings == null || !settings.CalendarSyncProfiles.Any() ||
                settings.CalendarSyncProfiles.All(t => !t.ValidateOutlookSettings() ||
                                                       t.GoogleSettings.GoogleAccount == null))
            {
                return false;
            }
            return true;
        }

        public static bool ValidateOutlookSettings(this SyncProfile syncProfile)
        {
            if (!syncProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultProfile) &&
                string.IsNullOrEmpty(syncProfile.OutlookSettings.OutlookProfileName))
            {
                return false;
            }

            if (!syncProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultMailBoxCalendar) &&
                (syncProfile.OutlookSettings.OutlookFolder == null ||
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
            settings.OutlookOptions = OutlookOptionsEnum.OutlookDesktop;

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