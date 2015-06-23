using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Services.Interfaces;

namespace CalendarSyncPlus.Services
{
    [Export(typeof(ISyncSummaryProvider))]
    public class SyncSummaryProvider : ISyncSummaryProvider
    { 
        #region Constructors

        [ImportingConstructor]
        public SettingsProvider(ISummarySerializationService summarySerializationService)
        {
            SummarySerializationService = summarySerializationService;
        }

        #endregion

        #region Properties

        public ISummarySerializationService SummarySerializationService { get; set; }

        #endregion

        #region ISettingsProvider Members

        public SyncSummary GetSyncSummary()
        {
            return SummarySerializationService.DeserializeSyncSummary();
        }

        #endregion
    }
}
