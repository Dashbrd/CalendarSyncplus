using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Services.CalendarUpdate
{
    public interface ICalendarUpdateServiceFactory
    {
        ICalendarUpdateService GetCalendarUpdateService(Settings settings);
    }
}
