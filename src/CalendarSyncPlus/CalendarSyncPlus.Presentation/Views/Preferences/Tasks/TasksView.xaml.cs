using System.ComponentModel.Composition;
using System.Windows.Controls;
using CalendarSyncPlus.Application.Views;

namespace CalendarSyncPlus.Presentation.Views.Preferences.Tasks
{
    /// <summary>
    ///     Interaction logic for TasksView.xaml
    /// </summary>
    [Export(typeof(ITaskView))]
    public partial class TasksView : UserControl, ITaskView
    {
        public TasksView()
        {
            InitializeComponent();
        }
    }
}