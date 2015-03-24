using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Applications;
using OutlookGoogleSyncRefresh.Application.Services;
using OutlookGoogleSyncRefresh.Application.Services.Google;
using OutlookGoogleSyncRefresh.Application.Services.Outlook;
using OutlookGoogleSyncRefresh.Application.Views;
using OutlookGoogleSyncRefresh.Common.Log;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.ViewModels
{
    [Export]
    public class ProfilesViewModel : ViewModel<IProfilesView>
    {
        private ObservableCollection<Settings> _syncProfiles;

        [ImportingConstructor]
        public ProfilesViewModel(IProfilesView view,
            IMessageService messageService, ApplicationLogger applicationLogger)
            : base(view)
        {
            ApplicationLogger = applicationLogger;
            MessageService = messageService;
        }

        public ApplicationLogger ApplicationLogger { get; set; }

        public IMessageService MessageService { get; set; }

        public ObservableCollection<Settings> SyncProfiles
        {
            get { return _syncProfiles; }
            set { SetProperty(ref _syncProfiles, value); }
        }
    }
}
