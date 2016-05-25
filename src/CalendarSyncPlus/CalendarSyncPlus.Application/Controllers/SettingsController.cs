using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using CalendarSyncPlus.Application.Controllers.Interfaces;
using CalendarSyncPlus.Application.ViewModels;
using CalendarSyncPlus.Services.Sync.Interfaces;

namespace CalendarSyncPlus.Application.Controllers
{
    /// <summary>
    /// </summary>
    [Export(typeof(ISettingsController))]
    public class SettingsController : ISettingsController
    {
        private readonly ICalendarController _calendarController;
        private readonly CalendarViewModel _calendarViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly AppSettingsViewModel _appSettingsViewModel;
        private readonly ManageProfileViewModel _manageProfileViewModel;
        private readonly TaskViewModel _taskViewModel;

        /// <summary>
        /// </summary>
        /// <param name="settingsService"></param>
        /// <param name="calendarController"></param>
        /// <param name="settingsViewModel"></param>
        /// <param name="calendarViewModelLazy"></param>
        /// <param name="manageProfileViewModelLazy"></param>
        /// <param name="taskViewModelLazy"></param>
        /// <param name="appSettingsViewModelLazy"></param>
        [ImportingConstructor]
        public SettingsController(ISettingsService settingsService,
            ICalendarController calendarController,
            SettingsViewModel settingsViewModel,
            Lazy<CalendarViewModel> calendarViewModelLazy,
            Lazy<ManageProfileViewModel> manageProfileViewModelLazy,
            Lazy<TaskViewModel> taskViewModelLazy,
            Lazy<AppSettingsViewModel> appSettingsViewModelLazy)
        {
            _calendarController = calendarController;
            _settingsViewModel = settingsViewModel;
            _calendarViewModel = calendarViewModelLazy.Value;
            _taskViewModel = taskViewModelLazy.Value;
            _manageProfileViewModel = manageProfileViewModelLazy.Value;
            _appSettingsViewModel = appSettingsViewModelLazy.Value;

            settingsService.TaskView = _taskViewModel.View;
            settingsService.CalendarView = _calendarViewModel.View;
            settingsService.ManageProfilesView = _manageProfileViewModel.View;
            settingsService.AppSettingsView = _appSettingsViewModel.View;
        }

        #region ISettingsController Members

        public void Initialize()
        {
            _calendarController.Initialize();

            _calendarViewModel.AddGoogleAccountCommand = _settingsViewModel.AddNewGoogleAccount;
            _calendarViewModel.DisconnectAccountCommand = _settingsViewModel.DisconnectGoogleCommand;
            _calendarViewModel.Initialize();

            _taskViewModel.AddGoogleAccountCommand = _settingsViewModel.AddNewGoogleAccount;
            _taskViewModel.DisconnectAccountCommand = _settingsViewModel.DisconnectGoogleCommand;
            _taskViewModel.Initialize();
            PropertyChangedEventManager.AddHandler(_settingsViewModel, SettingsPropertyChangedHandler, "");
        }

        public void Run(bool startMinimized)
        {
            _calendarController.Run(startMinimized);
        }

        public void Shutdown()
        {
            _calendarController.Shutdown();
            PropertyChangedEventManager.RemoveHandler(_settingsViewModel, SettingsPropertyChangedHandler, "");
        }

        #endregion

        private void SettingsPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Init":
                    if (_settingsViewModel.Init)
                    {
                        _calendarViewModel.CalendarSyncProfiles = _settingsViewModel.Settings.CalendarSyncProfiles;
                        _calendarViewModel.GoogleAccounts = _settingsViewModel.Settings.GoogleAccounts;
                        _calendarViewModel.SelectedProfile = _calendarViewModel.CalendarSyncProfiles.FirstOrDefault();

                        _manageProfileViewModel.CalendarSyncProfiles = _settingsViewModel.Settings.CalendarSyncProfiles;
                        _manageProfileViewModel.TaskSyncProfiles = _settingsViewModel.Settings.TaskSyncProfiles;
                        _manageProfileViewModel.ContactsSyncProfiles = _settingsViewModel.Settings.ContactSyncProfiles;

                        _appSettingsViewModel.AppSettings = _settingsViewModel.Settings.AppSettings;

                        _taskViewModel.TaskSyncProfiles = _settingsViewModel.Settings.TaskSyncProfiles;
                        _taskViewModel.GoogleAccounts = _settingsViewModel.Settings.GoogleAccounts;
                        _taskViewModel.SelectedProfile = _taskViewModel.TaskSyncProfiles.FirstOrDefault();

                        _settingsViewModel.Init = false;
                    }
                    break;
            }
        }
    }
}