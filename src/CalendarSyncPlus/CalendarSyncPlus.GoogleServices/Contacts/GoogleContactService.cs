using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Services.Contacts.Interfaces;
using Google.Apis.Drive.v2;
using Google.GData.Contacts;
using Google.GData.Extensions;

namespace CalendarSyncPlus.GoogleServices.Contacts
{
    [Export(typeof(IContactService)), Export(typeof(IGoogleContactService))]
    [ExportMetadata("ServiceType", ServiceType.Google)]
    public class GoogleContactService : IGoogleContactService
    {
        public void CreateContactEntry(Contact contact)
        {
            ContactEntry contactEntry = new ContactEntry();
            contactEntry.Name = new Name();
        }

    }
}
