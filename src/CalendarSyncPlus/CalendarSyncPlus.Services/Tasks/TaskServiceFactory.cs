using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Services.Tasks.Interfaces;

namespace CalendarSyncPlus.Services.Tasks
{
    [Export(typeof(ITaskServiceFactory))]
    public class TaskServiceFactory : ITaskServiceFactory
    {
         [ImportingConstructor]
        public TaskServiceFactory()
        {
        }

        [ImportMany(typeof (ITaskService))]
        public IEnumerable<Lazy<ITaskService, IServiceMetaData>> TaskServicesFactoryLazy { get; set; }

        #region ICalendarServiceFactory Members

        public ITaskService GetTaskService(ServiceType serviceType)
        {
            var serviceInstance =
                TaskServicesFactoryLazy.FirstOrDefault(list => list.Metadata.ServiceType == serviceType);

            if (serviceInstance != null)
            {
                return serviceInstance.Value;
            }
            throw new ArgumentException("Task Service Type is not Available/Registered", "serviceType");
        }

        #endregion
    }
}
