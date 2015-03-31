using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Application.Services
{
    public interface ISettingsProvider
    {
        Settings GetSettings();
    }
}