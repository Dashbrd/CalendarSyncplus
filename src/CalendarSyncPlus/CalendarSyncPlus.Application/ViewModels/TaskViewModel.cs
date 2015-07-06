using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using CalendarSyncPlus.Application.Views;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Domain.Models.Preferences;
using log4net;

namespace CalendarSyncPlus.Application.ViewModels
{
    [Export]
    public class TaskViewModel : ViewModel<ITaskView>
    {
        private ObservableCollection<TaskSyncProfile> _taskSyncProfiles;
        private TaskSyncProfile _selectedProfile;
        private ObservableCollection<GoogleAccount> _googleAccounts;
        [ImportingConstructor]
        public TaskViewModel(ITaskView view, ApplicationLogger applicationLogger) : base(view)
        {
            Logger = applicationLogger.GetLogger(this.GetType());
        }

        public ILog Logger { get; set; }

        public ObservableCollection<GoogleAccount> GoogleAccounts
        {
            get { return _googleAccounts; }
            set { SetProperty(ref _googleAccounts, value); }
        }

        public ObservableCollection<TaskSyncProfile> TaskSyncProfiles
        {
            get { return _taskSyncProfiles; }
            set { SetProperty(ref _taskSyncProfiles, value); }
        }


        public TaskSyncProfile SelectedProfile
        {
            get { return _selectedProfile; }
            set { SetProperty(ref _selectedProfile, value); }
        }
    }
}