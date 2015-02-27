using System.ComponentModel.Composition;
using System.Windows.Controls;
using OutlookGoogleSyncRefresh.Application.Views;

namespace OutlookGoogleSyncRefresh.Presentation.Views
{
    /// <summary>
    ///     Interaction logic for SettingsView.xaml
    /// </summary>
    [Export(typeof (ISettingsView))]
    public partial class SettingsView : UserControl, ISettingsView
    {
        public SettingsView()
        {
            InitializeComponent();
        }
    }
}