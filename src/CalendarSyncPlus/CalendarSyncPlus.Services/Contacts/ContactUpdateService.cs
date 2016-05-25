using System;
using System.ComponentModel.Composition;
using System.Waf.Foundation;
using CalendarSyncPlus.Domain.Models.Metrics;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.Services.Contacts.Interfaces;
using CalendarSyncPlus.Services.Utilities;

namespace CalendarSyncPlus.Services.Contacts
{
    [Export(typeof(IContactUpdateService))]
    public class ContactUpdateService : Model, IContactUpdateService
    {
        #region IContactUpdateService Members

        public bool SyncContact(ContactSyncProfile syncProfile, SyncMetric syncMetric, SyncCallback syncCallback)
        {
            throw new NotImplementedException();
        }

        public ContactsWrapper DestinationAppointments
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public ContactsWrapper SourceAppointments
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string ContactSyncStatus
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IContactService SourceCalendarService
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IContactService DestinationCalendarService
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion
    }
}