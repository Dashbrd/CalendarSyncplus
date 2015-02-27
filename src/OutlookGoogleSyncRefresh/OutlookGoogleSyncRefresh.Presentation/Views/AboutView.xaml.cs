using System.ComponentModel.Composition;
using OutlookGoogleSyncRefresh.Application.Views;

namespace OutlookGoogleSyncRefresh.Presentation.Views
{
    /// <summary>
    ///     Interaction logic for AboutView.xaml
    /// </summary>
    [Export(typeof (IAboutView))]
    public partial class AboutView : IAboutView
    {
        public AboutView()
        {
            InitializeComponent();
        }
    }
}