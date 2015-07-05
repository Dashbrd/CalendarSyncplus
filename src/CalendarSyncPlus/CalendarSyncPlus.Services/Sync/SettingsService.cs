using System.ComponentModel.Composition;
using System.Waf.Foundation;
using CalendarSyncPlus.Services.Interfaces;
using CalendarSyncPlus.Services.Sync.Interfaces;

namespace CalendarSyncPlus.Services.Sync
{
     [Export, Export(typeof(ISettingsService))]
    public class SettingsService : Model,  ISettingsService
    {
         public object CalendarView { get; set; }

         public object TaskView { get; set; }
         
         public object ContactsView { get; set; }

         public object ManageProfilesView { get; set; }

         public object AppSettingsView { get; set; }
    }
}