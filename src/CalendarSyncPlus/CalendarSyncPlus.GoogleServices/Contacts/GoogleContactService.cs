using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Services.Contacts.Interfaces;

namespace CalendarSyncPlus.GoogleServices.Contacts
{
    [Export(typeof(IContactService)), Export(typeof(IGoogleContactService))]
    [ExportMetadata("ServiceType", ServiceType.Google)]
    public class GoogleContactService : IGoogleContactService
    {
    }
}
