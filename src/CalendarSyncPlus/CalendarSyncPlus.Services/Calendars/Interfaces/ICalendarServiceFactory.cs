using CalendarSyncPlus.Common.MetaData;

namespace CalendarSyncPlus.Services.Calendars.Interfaces
{
    public interface ICalendarServiceFactory
    {
        ICalendarService GetCalendarService(ServiceType serviceType);
    }
}