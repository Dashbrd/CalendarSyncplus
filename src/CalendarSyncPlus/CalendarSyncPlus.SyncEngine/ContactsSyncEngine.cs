using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.SyncEngine.Interfaces;

namespace CalendarSyncPlus.SyncEngine
{
    [Export(typeof(IContactsSyncEngine))]
    public class ContactsSyncEngine : IContactsSyncEngine
    {
        public List<Contact> SourceContactsToUpdate
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<Contact> SourceOrphanEntries
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<Contact> SourceContactsToDelete
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<Contact> SourceContactsToAdd
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<Contact> DestContactsToUpdate
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<Contact> DestContactsToDelete
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<Contact> DestContactsToAdd
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<Contact> DestOrphanEntries
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool GetSourceEntriesToDelete(ContactsSyncProfile syncProfile, ContactsWrapper sourceList, ContactsWrapper destinationList)
        {
            throw new NotImplementedException();
        }

        public bool GetSourceEntriesToAdd(ContactsSyncProfile syncProfile, ContactsWrapper sourceList, ContactsWrapper destinationList)
        {
            throw new NotImplementedException();
        }

        public bool GetDestEntriesToDelete(ContactsSyncProfile syncProfile, ContactsWrapper sourceList, ContactsWrapper destinationList)
        {
            throw new NotImplementedException();
        }

        public bool GetDestEntriesToAdd(ContactsSyncProfile syncProfile, ContactsWrapper sourceList, ContactsWrapper destinationList)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
