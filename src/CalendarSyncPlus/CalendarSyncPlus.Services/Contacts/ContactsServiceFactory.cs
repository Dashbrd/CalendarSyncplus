using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Services.Contacts.Interfaces;

namespace CalendarSyncPlus.Services.Contacts
{
    [Export(typeof(IContactsServiceFactory))]
    public class ContactsServiceFactory : IContactsServiceFactory
    {
         [ImportingConstructor]
        public ContactsServiceFactory()
        {
        }

        [ImportMany(typeof (IContactService))]
         public IEnumerable<Lazy<IContactService, IServiceMetaData>> CalendarServicesFactoryLazy { get; set; }

        #region ICalendarServiceFactory Members

        public IContactService GetCalendarService(ServiceType serviceType)
        {
            var serviceInstance =
                CalendarServicesFactoryLazy.FirstOrDefault(list => list.Metadata.ServiceType == serviceType);

            if (serviceInstance != null)
            {
                return serviceInstance.Value;
            }
            throw new ArgumentException("Contacts Service Type is not Available/Registered", "serviceType");
        }

        #endregion
    }
}
