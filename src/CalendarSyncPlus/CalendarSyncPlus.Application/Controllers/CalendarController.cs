using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using CalendarSyncPlus.Application.ViewModels;

namespace CalendarSyncPlus.Application.Controllers
{
    [Export(typeof(ICalendarController))]
    public class CalendarController : ICalendarController
    {
        public SettingsViewModel SettingsViewModel { get; private set; }
        public CalendarViewModel CalendarViewModel { get; private set; }

        [ImportingConstructor]
        public CalendarController(SettingsViewModel settingsViewModel, CalendarViewModel calendarViewModel)
        {
            SettingsViewModel = settingsViewModel;
            CalendarViewModel = calendarViewModel;
        }

        public void Initialize()
        {
            PropertyChangedEventManager.AddHandler(CalendarViewModel,CalendarPropertyChangedHandler, "");
        }

        private void CalendarPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsLoading":
                    SettingsViewModel.IsLoading = CalendarViewModel.IsLoading;
                    break;
            }
        }

        public void Run(bool startMinimized)
        {

        }

        public void Shutdown()
        {
            PropertyChangedEventManager.RemoveHandler(CalendarViewModel, CalendarPropertyChangedHandler, "");
        }
    }
}