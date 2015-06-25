using System.ComponentModel.Composition;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Metrics;
using CalendarSyncPlus.Services.Interfaces;

namespace CalendarSyncPlus.Services
{
    [Export(typeof (ISyncSummaryProvider))]
    public class SyncSummaryProvider : ISyncSummaryProvider
    {
        #region Constructors

        [ImportingConstructor]
        public SyncSummaryProvider(ISummarySerializationService summarySerializationService)
        {
            SummarySerializationService = summarySerializationService;
        }

        #endregion

        #region Properties

        public ISummarySerializationService SummarySerializationService { get; set; }

        #endregion

        #region ISyncSummaryProvider Members

        public SyncSummary GetSyncSummary()
        {
            return SummarySerializationService.DeserializeSyncSummary();
        }

        #endregion
    }
}