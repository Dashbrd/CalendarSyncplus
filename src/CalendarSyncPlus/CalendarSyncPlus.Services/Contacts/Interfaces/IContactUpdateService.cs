using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models.Metrics;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.Services.Utilities;

namespace CalendarSyncPlus.Services.Contacts.Interfaces
{
    public interface IContactUpdateService : INotifyPropertyChanged
    {
        #region Public Methods

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="syncMetric"></param>
        /// <param name="syncCallback"></param>
        /// <returns>
        /// </returns>
        bool SyncContact(ContactSyncProfile syncProfile, SyncMetric syncMetric, SyncCallback syncCallback);

        #endregion

        #region Properties

        ContactsWrapper DestinationAppointments { get; set; }
        ContactsWrapper SourceAppointments { get; set; }

        string ContactSyncStatus { get; set; }
        IContactService SourceCalendarService { get; set; }
        IContactService DestinationCalendarService { get; set; }

        #endregion
    }
}
