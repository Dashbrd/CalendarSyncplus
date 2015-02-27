using System.Threading.Tasks;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Services
{
    public interface ISettingsSerializationService
    {
        Task<bool> SerializeSettingsAsync(Settings settings);

        Task<Settings> DeserializeSettingsAsync();

        bool SerializeSettings(Settings settings);

        Settings DeserializeSettings();
    }
}