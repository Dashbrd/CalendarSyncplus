using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Services.Interfaces
{
    public interface ISettingsProvider
    {
        Settings GetSettings();
    }
}