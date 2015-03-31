using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Application.Services
{
    public interface ISettingsSerializationService
    {
        Task<bool> SerializeSettingsAsync(Settings syncProfile);

        Task<Settings> DeserializeSettingsAsync();

        bool SerializeSettings(Settings syncProfile);

        Settings DeserializeSettings();
    }
}