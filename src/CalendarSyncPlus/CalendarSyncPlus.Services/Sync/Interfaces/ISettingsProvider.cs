using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;

namespace CalendarSyncPlus.Services.Interfaces
{
    public interface ISettingsProvider
    {
        Settings GetSettings();
    }
}