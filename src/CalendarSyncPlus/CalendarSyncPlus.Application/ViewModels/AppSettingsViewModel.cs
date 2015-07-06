using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
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
        private DelegateCommand _uriCommand;

        [ImportingConstructor]
        public AppSettingsViewModel(IAppSettingsView view, ApplicationLogger applicationLogger) : base(view)
        {
            Logger = applicationLogger.GetLogger(this.GetType());
        }

        public ILog Logger { get; set; }

        public AppSettings AppSettings
        {
            get { return _appSettings; }
            set { SetProperty(ref _appSettings, value); }
        }

        public DelegateCommand UriCommand
        {
            get { return _uriCommand ?? (_uriCommand = new DelegateCommand(UriHandler)); }
        }

        private void UriHandler(object parameter)
        {
            try
            {
                Process.Start(parameter.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }

}