using System.ComponentModel;

namespace CalendarSyncPlus.Services.Interfaces
{
    public interface IShellService : INotifyPropertyChanged
    {
        object ShellView { get; set; }
        object SettingsView { get; set; }

        object AboutView { get; set; }

        object HelpView { get; set; }

        object LogView { get; set; }

    }
}