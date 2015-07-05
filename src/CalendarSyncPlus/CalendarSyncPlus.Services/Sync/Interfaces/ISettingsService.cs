using System.ComponentModel;

namespace CalendarSyncPlus.Services.Sync.Interfaces
{
    public interface ISettingsService : INotifyPropertyChanged
    {
        object CalendarView { get; set; }

        object TaskView { get; set; }

        object ContactsView { get; set; }

        object ManageProfilesView { get; set; }

        object AppSettingsView { get; set; }
    }
}