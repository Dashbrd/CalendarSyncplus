using System.ComponentModel.Composition;
using System.Windows.Controls;
using CalendarSyncPlus.Application.Views;

namespace CalendarSyncPlus.Presentation.Views
{
    /// <summary>
    ///     Interaction logic for ManageProfilesView.xaml
    /// </summary>
    [Export(typeof(IManageProfileView))]
    public partial class ManageProfilesView : UserControl, IManageProfileView
    {
        public ManageProfilesView()
        {
            InitializeComponent();
        }
    }
}