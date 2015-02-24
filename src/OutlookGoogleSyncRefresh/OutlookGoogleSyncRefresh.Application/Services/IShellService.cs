using System.ComponentModel;

namespace OutlookGoogleSyncRefresh.Application.Services
{
    public interface IShellService : INotifyPropertyChanged
    {
        object ShellView { get; set; }
        object SettingsView { get; set; }

        object AboutView { get; set; }

        object HelpView { get; set; }
    }
}