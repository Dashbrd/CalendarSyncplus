using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Services.Calendars.Interfaces;

namespace CalendarSyncPlus.Services.Calendars
{
    [Export(typeof (ICalendarServiceFactory))]    
    public class CalendarServiceFactory : ICalendarServiceFactory
    {
        
        public CalendarServiceFactory()
        {
        }

        [ImportMany(typeof (ICalendarService))]
        public IEnumerable<Lazy<ICalendarService, IServiceMetaData>> CalendarServicesFactoryLazy { get; set; }

        #region ICalendarServiceFactory Members

        public ICalendarService GetCalendarService(ServiceType serviceType)
        {
            var serviceInstance =
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