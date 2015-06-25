using System.ComponentModel.Composition;
using CalendarSyncPlus.Application.Views;

namespace CalendarSyncPlus.Presentation.Views
{
    /// <summary>
    ///     Interaction logic for LogView.xaml
    /// </summary>
    [Export(typeof (ILogView))]
    public partial class LogView : ILogView
    {
        public LogView()
        {
            InitializeComponent();
        }
    }
}