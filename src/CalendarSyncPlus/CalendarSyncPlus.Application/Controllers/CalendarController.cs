using System.ComponentModel;
using System.ComponentModel.Composition;
using CalendarSyncPlus.Application.ViewModels;

namespace CalendarSyncPlus.Application.Controllers
{
    [Export(typeof(ICalendarController))]
    public class CalendarController : ICalendarController
    {
        [ImportingConstructor]
        public CalendarController(SettingsViewModel settingsViewModel, CalendarViewModel calendarViewModel)
        {
            SettingsViewModel = settingsViewModel;
            CalendarViewModel = calendarViewModel;
        }

        public SettingsViewModel SettingsViewModel { get; }
        public CalendarViewModel CalendarViewModel { get; }

        #region ICalendarController Members

        public void Initialize()
        {
            PropertyChangedEventManager.AddHandler(CalendarViewModel, CalendarPropertyChangedHandler, "");
        }

        public void Run(bool startMinimized)
        {
        }

        public void Shutdown()
        {
            PropertyChangedEventManager.RemoveHandler(CalendarViewModel, CalendarPropertyChangedHandler, "");
        }

        #endregion

        private void CalendarPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsLoading":
                    SettingsViewModel.IsLoading = CalendarViewModel.IsLoading;
                    break;
            }
        }
    }
}