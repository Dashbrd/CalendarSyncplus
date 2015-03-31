using CalendarSyncPlus.Common.MetaData;

namespace CalendarSyncPlus.Application.Services.CalendarUpdate
{
    public interface ICalendarServiceFactory
    {
        ICalendarService GetCalendarService(CalendarServiceType serviceType);
    }
}