using OutlookGoogleSyncRefresh.Common.MetaData;

namespace OutlookGoogleSyncRefresh.Application.Services.CalendarUpdate
{
    public interface ICalendarServiceFactory
    {
        ICalendarService GetCalendarService(CalendarServiceType serviceType);
    }
}