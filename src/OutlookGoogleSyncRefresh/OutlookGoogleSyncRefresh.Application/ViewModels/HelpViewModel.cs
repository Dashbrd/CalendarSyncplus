using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Applications;
using OutlookGoogleSyncRefresh.Application.Views;

namespace OutlookGoogleSyncRefresh.Application.ViewModels
{
    [Export]
    public class HelpViewModel : Utilities.ViewModel<IHelpView>
    {
        [ImportingConstructor]
        public HelpViewModel(IHelpView helpView)
            : base(helpView)
        {
            
        }
    }
}
