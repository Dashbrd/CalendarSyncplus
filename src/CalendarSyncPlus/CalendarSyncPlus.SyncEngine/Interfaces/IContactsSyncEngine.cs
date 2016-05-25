using System.Collections.Generic;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;

namespace CalendarSyncPlus.SyncEngine.Interfaces
{
    public interface IContactsSyncEngine
    {
        /// <summary>
        /// </summary>
        List<Contact> SourceContactsToUpdate { get; set; }

        /// <summary>
        /// </summary>
        List<Contact> SourceOrphanEntries { get; set; }

        /// <summary>
        /// </summary>
        List<Contact> SourceContactsToDelete { get; set; }

        /// <summary>
        /// </summary>
        List<Contact> SourceContactsToAdd { get; set; }

        /// <summary>
        /// </summary>
        List<Contact> DestContactsToUpdate { get; set; }

        /// <summary>
        /// </summary>
        List<Contact> DestContactsToDelete { get; set; }

        /// <summary>
        /// </summary>
        List<Contact> DestContactsToAdd { get; set; }

        /// <summary>
        /// </summary>
        List<Contact> DestOrphanEntries { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns>
        /// </returns>
        bool GetSourceEntriesToDelete(ContactSyncProfile syncProfile, ContactsWrapper sourceList,
            ContactsWrapper destinationList);

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns>
        /// </returns>
        bool GetSourceEntriesToAdd(ContactSyncProfile syncProfile, ContactsWrapper sourceList,
            ContactsWrapper destinationList);

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns>
        /// </returns>
        bool GetDestEntriesToDelete(ContactSyncProfile syncProfile, ContactsWrapper sourceList,
            ContactsWrapper destinationList);

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns>
        /// </returns>
        bool GetDestEntriesToAdd(ContactSyncProfile syncProfile, ContactsWrapper sourceList,
            ContactsWrapper destinationList);

        /// <summary>
        /// </summary>
        void Clear();
    }
}