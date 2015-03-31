using CalendarSyncPlus.Common.MetaData;

namespace CalendarSyncPlus.Services.Interfaces
{
    public interface ICalendarServiceFactory
    {
        ICalendarService GetCalendarService(CalendarServiceType serviceType);
    }
}