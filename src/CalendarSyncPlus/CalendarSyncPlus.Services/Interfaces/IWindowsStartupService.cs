namespace CalendarSyncPlus.Services.Interfaces
{
    public interface IWindowsStartupService
    {
        void RunAtWindowsStartup();
        void RemoveFromWindowsStartup();
    }
}