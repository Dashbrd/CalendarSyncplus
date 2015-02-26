using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Waf.Applications;
using OutlookGoogleSyncRefresh.Application.Views;

namespace OutlookGoogleSyncRefresh.Application.ViewModels
{
    [Export]
    public class AboutViewModel : Utilities.ViewModel<IAboutView>
    {
        private string _productVersion;
        private DelegateCommand _uriCommand;
        [ImportingConstructor]
        public AboutViewModel(IAboutView aboutView)
            : base(aboutView)
        {
            ProductVersion = ApplicationInfo.Version;
        }

        public string ProductVersion
        {
            get { return _productVersion; }
            set { SetProperty(ref _productVersion, value); }
        }

        public DelegateCommand UriCommand
        {
            get { return _uriCommand = _uriCommand ?? new DelegateCommand(RequestNavigation); }
        }

        private void RequestNavigation(object parameter)
        {
            if (parameter != null)
            {
                Process.Start(new ProcessStartInfo(new Uri(parameter.ToString()).AbsoluteUri));
            }
        }
    }
}
