using System.ComponentModel.Composition;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Services.Contacts.Interfaces;

namespace CalendarSyncPlus.ExchangeWebServices.Contact
{
    [Export(typeof(IExchangeWebContactService)), Export(typeof(IContactService))]
    [ExportMetadata("ServiceType", ServiceType.EWS)]
    public class ExchangeWebContactService : IExchangeWebContactService
    {
    }
}