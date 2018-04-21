using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Waf.Applications;
using CalendarSyncPlus.Application.Views;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Domain.Models.Preferences;
using log4net;

namespace CalendarSyncPlus.Application.ViewModels
{
    [Export]
    public class AppSettingsViewModel : ViewModel<IAppSettingsView>
    {
        private AppSettings _appSettings;
        private AsyncDelegateCommand _uriCommand;

        [ImportingConstructor]
        public AppSettingsViewModel(IAppSettingsView view, ApplicationLogger applicationLogger) : base(view)
        {
            Logger = applicationLogger.GetLogger(GetType());
        }

        public ILog Logger { get; set; }

        public AppSettings AppSettings
        {
            get { return _appSettings; }
            set { SetProperty(ref _appSettings, value); }
        }

        public AsyncDelegateCommand UriCommand => _uriCommand ?? (_uriCommand = new AsyncDelegateCommand(UriHandler));

        private async Task UriHandler(object parameter)
        {
            try
            {
                await Task.Run(() => { Process.Start(parameter.ToString()); });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}