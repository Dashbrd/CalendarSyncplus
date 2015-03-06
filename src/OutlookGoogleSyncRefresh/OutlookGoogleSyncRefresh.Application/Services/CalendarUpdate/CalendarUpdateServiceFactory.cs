using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using OutlookGoogleSyncRefresh.Common.MetaData;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Services.CalendarUpdate
{
    [Export(typeof(ICalendarUpdateServiceFactory))]
    public class CalendarUpdateServiceFactory : ICalendarUpdateServiceFactory
    {
        public Lazy<ICalendarUpdateService> LazyOutlookGoogleService { get; set; }

        [ImportMany(typeof(ICalendarService))]
        public IEnumerable<Lazy<ICalendarService, ICalendarServiceMetaData>> CalendarServicesFactoryLazy { get; set; }


        [ImportingConstructor]
        public CalendarUpdateServiceFactory(Lazy<ICalendarUpdateService> lazyOutlookGoogleService)
        {
            LazyOutlookGoogleService = lazyOutlookGoogleService;
        }

        public ICalendarUpdateService GetCalendarUpdateService(Settings settings)
        {
            if (settings.CalendarSyncMode == CalendarSyncModeEnum.OutlookGoogleOneWay)
            {
                return LazyOutlookGoogleService.Value;
            }
            return null;
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