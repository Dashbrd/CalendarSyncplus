using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookGoogleSyncRefresh.Application.Services
{
    public interface IWindowsStartupService
    {
        void RunAtWindowsStartup();

        void RemoveFromWindowsStartup();
    }
}
