using System.ComponentModel.Composition;
using System.Waf.Applications;
using CalendarSyncPlus.Application.Views;

namespace CalendarSyncPlus.Application.ViewModels
{
    [Export]
    public class AppSettingsViewModel : ViewModel<IAppSettingsView>
    {
        [ImportingConstructor]
        public AppSettingsViewModel(IAppSettingsView view) : base(view)
        {
        }
    }
}