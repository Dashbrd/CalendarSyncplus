using System.ComponentModel.Composition;
using System.Windows.Controls;
using CalendarSyncPlus.Application.Views;

namespace CalendarSyncPlus.Presentation.Views
{
    /// <summary>
    ///     Interaction logic for ProfileSettingsView.xaml
    /// </summary>
    [Export(typeof(ICalendarView))]
    public partial class CalendarsView : UserControl, ICalendarView
    {
        public CalendarsView()
        {
            InitializeComponent();
        }
    }
}