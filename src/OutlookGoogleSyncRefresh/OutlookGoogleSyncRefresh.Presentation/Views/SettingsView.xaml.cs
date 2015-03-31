using System.ComponentModel.Composition;
using System.Windows.Controls;
using CalendarSyncPlus.Application.Views;

namespace CalendarSyncPlus.Presentation.Views
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