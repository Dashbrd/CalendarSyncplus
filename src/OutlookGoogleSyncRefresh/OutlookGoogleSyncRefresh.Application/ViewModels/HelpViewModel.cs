using System.ComponentModel.Composition;
using System.Waf.Applications;
using CalendarSyncPlus.Application.Views;

namespace CalendarSyncPlus.Application.ViewModels
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