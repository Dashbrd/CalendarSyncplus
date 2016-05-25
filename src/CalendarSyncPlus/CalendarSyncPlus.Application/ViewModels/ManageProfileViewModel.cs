using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
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
        private ObservableCollection<CalendarSyncProfile> _calendarSyncProfiles;
        private ObservableCollection<ContactSyncProfile> _contactsSyncProfiles;
        private DelegateCommand _createProfileCommand;
        private DelegateCommand _deleteProfileCommand;
        private DelegateCommand _moveDownCommand;
        private DelegateCommand _moveUpCommand;
        private ObservableCollection<TaskSyncProfile> _taskSyncProfiles;

        [ImportingConstructor]
        public ManageProfileViewModel(IManageProfileView view, IMessageService messageService) : base(view)
        {
            MessageService = messageService;
        }

        public IMessageService MessageService { get; set; }

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

        public ObservableCollection<ContactSyncProfile> ContactsSyncProfiles
        {
            get { return _contactsSyncProfiles; }
            set { SetProperty(ref _contactsSyncProfiles, value); }
        }


        /// <summary>
        /// </summary>
        private async void CreateProfile(object parameter)
        {
            if (parameter == null)
            {
                return;
            }

            if (parameter.ToString().Equals("Calendars"))
            {
                await AddNewProfile(CalendarSyncProfiles, CalendarSyncProfile.GetDefaultSyncProfile());
            }
            else if (parameter.ToString().Equals("Tasks"))
            {
                await AddNewProfile(TaskSyncProfiles, TaskSyncProfile.GetDefaultSyncProfile());
            }
            else if (parameter.ToString().Equals("Contacts"))
            {
                await AddNewProfile(ContactsSyncProfiles, ContactSyncProfile.GetDefaultSyncProfile());
            }
        }

        private async Task AddNewProfile<T>(ObservableCollection<T> profileList, T profile) where T : SyncProfile
        {
            if (profileList.Count > 4)
            {
                MessageService.ShowMessageAsync("You have reached the maximum number of profiles.");
                return;
            }

            var result = await MessageService.ShowInput("Please enter profile name.");

            if (!string.IsNullOrEmpty(result))
            {
                if (profileList.Any(t => !string.IsNullOrEmpty(t.Name) && t.Name.Equals(result)))
                {
                    MessageService.ShowMessageAsync(
                        $"A Profile with name '{result}' already exists. Please try again.");
                    return;
                }

                profile.Name = result;
                profile.IsDefault = false;
                profileList.Add(profile);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="parameter"></param>
        private async void DeleteProfile(object parameter)
        {
            await RemoveProfile(CalendarSyncProfiles, parameter as CalendarSyncProfile);

            await RemoveProfile(TaskSyncProfiles, parameter as TaskSyncProfile);

            await RemoveProfile(ContactsSyncProfiles, parameter as ContactSyncProfile);
        }

        private async Task RemoveProfile<T>(ObservableCollection<T> profileList, T profile) where T : SyncProfile
        {
            if (profile != null)
            {
                var task =
                    await MessageService.ShowConfirmMessage("Are you sure you want to delete the profile?");
                if (task == MessageDialogResult.Affirmative)
                {
                    profileList.Remove(profile);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="parameter"></param>
        private void MoveProfileUp(object parameter)
        {
            MoveUp(CalendarSyncProfiles, parameter as CalendarSyncProfile);
            MoveUp(TaskSyncProfiles, parameter as TaskSyncProfile);
            MoveUp(ContactsSyncProfiles, parameter as ContactSyncProfile);
        }

        private void MoveUp<T>(ObservableCollection<T> profileList, T profile) where T : SyncProfile
        {
            if (profile != null)
            {
                var index = profileList.IndexOf(profile);
                if (index > 0)
                {
                    profileList.Move(index, index - 1);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="parameter"></param>
        private void MoveProfileDown(object parameter)
        {
            MoveDown(CalendarSyncProfiles, parameter as CalendarSyncProfile);
            MoveDown(TaskSyncProfiles, parameter as TaskSyncProfile);
            MoveDown(ContactsSyncProfiles, parameter as ContactSyncProfile);
        }

        private void MoveDown<T>(ObservableCollection<T> profileList, T profile) where T : SyncProfile
        {
            if (profile != null)
            {
                var index = profileList.IndexOf(profile);
                if (index < profileList.Count - 1)
                {
                    profileList.Move(index, index + 1);
                }
            }
        }

        public void UpdateProfiles(ObservableCollection<CalendarSyncProfile> calendarSyncProfiles,
            ObservableCollection<TaskSyncProfile> taskSyncProfiles,
            ObservableCollection<ContactSyncProfile> contactsSyncProfiles)
        {
            CalendarSyncProfiles = calendarSyncProfiles;
            TaskSyncProfiles = taskSyncProfiles;
            ContactsSyncProfiles = contactsSyncProfiles;
        }
    }
}