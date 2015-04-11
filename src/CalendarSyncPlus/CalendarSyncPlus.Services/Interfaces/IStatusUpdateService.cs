using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookGoogleSyncRefresh.Application.Services
{
    public interface IStatusUpdateService
    {
        int GoogleEntriesCount { get; set; }

        int OutlookEntriesCount { get; set; }


    }
}
