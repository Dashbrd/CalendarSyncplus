namespace OutlookGoogleSyncRefresh.Application.Services.CalendarUpdate
{
    public enum CalendarUpdateDirectionEnum
    {
        OutlookToGoogle,
        GoogleToOutlook
    }

    public class CalendarUpdateServiceFactory
    {
        public static ICalendarUpdateService GetCalendarUpdateService(
            CalendarUpdateDirectionEnum calendarUpdateDirectionEnum)
        {
            if (calendarUpdateDirectionEnum == CalendarUpdateDirectionEnum.OutlookToGoogle)
            {
            }
            return null;
        }
    }
}