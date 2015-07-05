using System;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models
{
    [Serializable]
    public class Category : Model
    {
        public string HexValue { get; set; }
        public string CategoryName { get; set; }
        public string ColorNumber { get; set; }
    }
}