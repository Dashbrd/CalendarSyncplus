using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using OutlookGoogleSyncRefresh.Common.MetaData;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Services.CalendarUpdate
{
    [Export(typeof(ICalendarServiceFactory))]
    public class CalendarServiceFactory : ICalendarServiceFactory
    {
        [ImportMany(typeof(ICalendarService))]
        public IEnumerable<Lazy<ICalendarService, ICalendarServiceMetaData>> CalendarServicesFactoryLazy { get; set; }


        [ImportingConstructor]
        public CalendarServiceFactory()
        {
        }

        public ICalendarService GetCalendarService(CalendarServiceType serviceType)
        {
            var serviceInstance = CalendarServicesFactoryLazy.FirstOrDefault(list => list.Metadata.ServiceType == serviceType);

            if (serviceInstance != null)
            {
                return serviceInstance.Value;
            }
            throw new ArgumentException("Calendar Service Type is not Available/Registered", "serviceType");
        }
    }
}