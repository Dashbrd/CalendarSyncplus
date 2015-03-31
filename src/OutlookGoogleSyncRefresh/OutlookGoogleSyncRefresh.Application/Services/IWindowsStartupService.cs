namespace CalendarSyncPlus.Application.Services
{
    public interface IWindowsStartupService
    {
        void RunAtWindowsStartup();

        void RemoveFromWindowsStartup();
    }
}