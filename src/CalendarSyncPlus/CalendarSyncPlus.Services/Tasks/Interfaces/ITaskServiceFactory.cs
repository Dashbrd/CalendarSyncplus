using CalendarSyncPlus.Common.MetaData;

namespace CalendarSyncPlus.Services.Tasks.Interfaces
{
    public interface ITaskServiceFactory
    {
        ITaskService GetTaskService(ServiceType serviceType);
    }
}
