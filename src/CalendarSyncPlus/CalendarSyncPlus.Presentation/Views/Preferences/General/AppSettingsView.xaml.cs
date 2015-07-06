using System.ComponentModel.Composition;
using System.Windows.Controls;
using CalendarSyncPlus.Application.Views;

namespace CalendarSyncPlus.Presentation.Views
{
    /// <summary>
    ///     Interaction logic for AdvancedSettingsView.xaml
    /// </summary>
    [Export(typeof(IAppSettingsView))]
    public partial class AppSettingsView : UserControl, IAppSettingsView
    {
        public AppSettingsView()
        {
            InitializeComponent();
        }
    }
}