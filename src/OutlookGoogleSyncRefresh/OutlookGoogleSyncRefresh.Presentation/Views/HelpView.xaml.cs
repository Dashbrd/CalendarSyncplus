using System.ComponentModel.Composition;
using OutlookGoogleSyncRefresh.Application.Views;

namespace OutlookGoogleSyncRefresh.Presentation.Views
{
    /// <summary>
    ///     Interaction logic for HelpView.xaml
    /// </summary>
    [Export(typeof (IHelpView))]
    public partial class HelpView : IHelpView
    {
        public HelpView()
        {
            InitializeComponent();
        }
    }
}