using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Foundation;

using OutlookGoogleSyncRefresh.Domain.Models;

using Model = OutlookGoogleSyncRefresh.Domain.Models.Model;

namespace OutlookGoogleSyncRefresh.Application.ViewModels
{
    public abstract class SyncFrequencyViewModel : Model
    {

        public virtual SyncFrequency GetFrequency()
        {
            return null;
        }
    }
}
