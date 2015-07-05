using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Waf.Applications;
using CalendarSyncPlus.Application.Views;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Services.Interfaces;
using MahApps.Metro.Controls.Dialogs;

namespace CalendarSyncPlus.Application.ViewModels
{
    [Export]
    public class ManageProfileViewModel : ViewModel<IManageProfileView>
    {
        public IMessageService MessageService { get; set; }
        private DelegateCommand _createProfileCommand;
        private DelegateCommand _deleteProfileCommand;
        private DelegateCommand _moveDownCommand;
        private DelegateCommand _moveUpCommand;
        private ObservableCollection<CalendarSyncProfile> _calendarSyncProfiles;
        private ObservableCollection<TaskSyncProfile> _taskSyncProfiles;
        private ObservableCollection<ContactsSyncProfile> _contactsSyncProfiles;

        [ImportingConstructor]
        public ManageProfileViewModel(IManageProfileView view, IMessageService messageService) : base(view)
        {
            MessageService = messageService;
        }

        public DelegateCommand CreateProfileCommand
        {
            get { return _createProfileCommand ?? (_createProfileCommand = new DelegateCommand(CreateProfile)); }
        }

        public DelegateCommand DeleteProfileCommand
        {
            get { return _deleteProfileCommand ?? (_deleteProfileCommand = new DelegateCommand(DeleteProfile)); }
        }

        public DelegateCommand MoveUpCommand
        {
            get { return _moveUpCommand ?? (_moveUpCommand = new DelegateCommand(MoveProfileUp)); }
        }

        public DelegateCommand MoveDownCommand
        {
            get { return _moveDownCommand ?? (_moveDownCommand = new DelegateCommand(MoveProfileDown)); }
        }


        public ObservableCollection<CalendarSyncProfile> CalendarSyncProfiles
        {
            get { return _calendarSyncProfiles; }
            set { SetProperty(ref _calendarSyncProfiles, value); }
        }

        public ObservableCollection<TaskSyncProfile> TaskSyncProfiles
        {
            get { return _taskSyncProfiles; }
            set { SetProperty(ref _taskSyncProfiles, value); }
        }

        public ObservableCollection<ContactsSyncProfile> ContactsSyncProfiles
        {
            get { return _contactsSyncProfiles; }
            set { SetProperty(ref _contactsSyncProfiles, value); }
        }


        /// <summary>
        /// 
        /// </summary>
        private async void CreateProfile()
        {
            if (CalendarSyncProfiles.Count > 4)
            {
                MessageService.ShowMessageAsync("You have reached the maximum number of profiles.");
                return;
            }

            var result = await MessageService.ShowInput("Please enter profile name.");

            if (!string.IsNullOrEmpty(result))
            {
                if (CalendarSyncProfiles.Any(t => !string.IsNullOrEmpty(t.Name) && t.Name.Equals(result)))
                {
                    MessageService.ShowMessageAsync(
                        string.Format("A Profile with name '{0}' already exists. Please try again.", result));
                    return;
                }

                var syncProfile = CalendarSyncProfile.GetDefaultSyncProfile();
                syncProfile.Name = result;
                syncProfile.IsDefault = false;
                //var viewModel = new CalendarViewModel(syncProfile, GoogleCalendarService, OutlookCalendarService,
                //    MessageService,
                //    ExchangeWebCalendarService, ApplicationLogger, AccountAuthenticationService);
                //PropertyChangedEventManager.AddHandler(viewModel, ProfilePropertyChangedHandler, "IsLoading");
                //viewModel.Initialize(null);
                CalendarSyncProfiles.Add(syncProfile);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        private async void DeleteProfile(object parameter)
        {
            var profile = parameter as CalendarSyncProfile;
            if (profile != null)
            {
                var task =
                    await MessageService.ShowConfirmMessage("Are you sure you want to delete the profile?");
                if (task == MessageDialogResult.Affirmative)
                {
                    CalendarSyncProfiles.Remove(profile);
                    //PropertyChangedEventManager.RemoveHandler(profile, ProfilePropertyChangedHandler, "IsLoading");
                    //SelectedCalendarProfile = Settings.CalendarSyncProfiles.FirstOrDefault();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        private void MoveProfileUp(object parameter)
        {
            var profile = parameter as CalendarSyncProfile;
            if (profile != null)
            {
                var index = CalendarSyncProfiles.IndexOf(profile);
                if (index > 0)
                {
                    CalendarSyncProfiles.Move(index, index - 1);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        private void MoveProfileDown(object parameter)
        {
            var profile = parameter as CalendarSyncProfile;
            if (profile != null)
            {
                var index = CalendarSyncProfiles.IndexOf(profile);
                if (index < CalendarSyncProfiles.Count - 1)
                {
                    CalendarSyncProfiles.Move(index, index + 1);
                }
            }
        }

        public void UpdateProfiles(ObservableCollection<CalendarSyncProfile> calendarSyncProfiles,
            ObservableCollection<TaskSyncProfile> taskSyncProfiles,
            ObservableCollection<ContactsSyncProfile> contactsSyncProfiles)
        {
            CalendarSyncProfiles = calendarSyncProfiles;
            TaskSyncProfiles = taskSyncProfiles;
            ContactsSyncProfiles = contactsSyncProfiles;
        }


    }
}