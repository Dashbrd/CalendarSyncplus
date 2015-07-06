using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Documents;
using CalendarSyncPlus.Application.Controllers.Interfaces;
using CalendarSyncPlus.Application.ViewModels;
using CalendarSyncPlus.Services.Sync.Interfaces;

namespace CalendarSyncPlus.Application.Controllers
{
    [Export(typeof(ISettingsController))]
    public class SettingsController : ISettingsController
    {
        private readonly ICalendarController _calendarController;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly CalendarViewModel _calendarViewModel;
        private ManageProfileViewModel _manageProfileViewModel;
        private AppSettingsViewModel _appSettingsViewModel;

        [ImportingConstructor]
        public SettingsController(ISettingsService settingsService,
            ICalendarController calendarController,
            SettingsViewModel settingsViewModel,
            Lazy<CalendarViewModel> calendarViewModelLazy, Lazy<ManageProfileViewModel> manageProfileViewModelLazy,
            Lazy<AppSettingsViewModel> appSettingsViewModelLazy)
        {
            _calendarController = calendarController;
            _settingsViewModel = settingsViewModel;
            _calendarViewModel = calendarViewModelLazy.Value;
            _manageProfileViewModel = manageProfileViewModelLazy.Value;
            _appSettingsViewModel = appSettingsViewModelLazy.Value;
            settingsService.CalendarView = _calendarViewModel.View;
            settingsService.ManageProfilesView = _manageProfileViewModel.View;
            settingsService.AppSettingsView = _appSettingsViewModel.View;
        }
        public void Initialize()
        {
            _calendarController.Initialize();
            _calendarViewModel.AddGoogleAccountCommand = _settingsViewModel.AddNewGoogleAccount;
            _calendarViewModel.DisconnectAccountCommand = _settingsViewModel.DisconnectGoogleCommand;
            _calendarViewModel.Initialize();
            PropertyChangedEventManager.AddHandler(_settingsViewModel,SettingsPropertyChangedHandler, "");
        }

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
                        _appSettingsViewModel.AppSettings = _settingsViewModel.Settings.AppSettings;
                        _settingsViewModel.Init = false;
                    }
                    break;
            }
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
    }
}