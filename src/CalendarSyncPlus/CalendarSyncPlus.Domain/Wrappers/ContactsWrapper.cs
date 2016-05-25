using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Wrappers
{
    public class ContactsWrapper : Model
    {
        public string ContactsListId { get; set; }
        public bool IsSuccess { get; set; }
    }
}