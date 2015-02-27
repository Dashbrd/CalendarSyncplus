using System.ComponentModel.Composition;
using OutlookGoogleSyncRefresh.Application.Utilities;
using OutlookGoogleSyncRefresh.Application.Views;

namespace OutlookGoogleSyncRefresh.Application.ViewModels
{
    [Export]
    public class HelpViewModel : ViewModel<IHelpView>
    {
        [ImportingConstructor]
        public HelpViewModel(IHelpView helpView)
            : base(helpView)
        {
        }
    }
}