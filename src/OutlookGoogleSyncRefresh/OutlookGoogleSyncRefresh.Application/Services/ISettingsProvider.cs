using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Services
{
    public interface ISettingsProvider
    {
        Settings GetSettings();
    }
}