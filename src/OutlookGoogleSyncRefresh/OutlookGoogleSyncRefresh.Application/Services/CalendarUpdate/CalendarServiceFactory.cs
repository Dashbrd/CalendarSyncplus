using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using OutlookGoogleSyncRefresh.Common.MetaData;

namespace OutlookGoogleSyncRefresh.Application.Services.CalendarUpdate
{
    [Export(typeof (ICalendarServiceFactory))]
    public class CalendarServiceFactory : ICalendarServiceFactory
    {
        [ImportingConstructor]
        public CalendarServiceFactory()
        {
        }

        [ImportMany(typeof (ICalendarService))]
        public IEnumerable<Lazy<ICalendarService, ICalendarServiceMetaData>> CalendarServicesFactoryLazy { get; set; }

        #region ICalendarServiceFactory Members

        public ICalendarService GetCalendarService(CalendarServiceType serviceType)
        {
            Lazy<ICalendarService, ICalendarServiceMetaData> serviceInstance =
                CalendarServicesFactoryLazy.FirstOrDefault(list => list.Metadata.ServiceType == serviceType);

            if (serviceInstance != null)
            {
                return serviceInstance.Value;
            }
            throw new ArgumentException("Calendar Service Type is not Available/Registered", "serviceType");
        }

        #endregion
    }
}