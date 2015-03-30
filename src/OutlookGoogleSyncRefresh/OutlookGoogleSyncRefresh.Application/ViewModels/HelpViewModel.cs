using System.ComponentModel.Composition;
using System.Waf.Applications;
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