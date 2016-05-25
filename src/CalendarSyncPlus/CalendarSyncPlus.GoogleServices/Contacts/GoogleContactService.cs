using System.ComponentModel.Composition;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Services.Contacts.Interfaces;
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
            var contactEntry = new ContactEntry();
            contactEntry.Name = new Name();
        }
    }
}