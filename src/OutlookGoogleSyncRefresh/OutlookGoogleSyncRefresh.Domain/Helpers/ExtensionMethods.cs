using System;

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
    }
}