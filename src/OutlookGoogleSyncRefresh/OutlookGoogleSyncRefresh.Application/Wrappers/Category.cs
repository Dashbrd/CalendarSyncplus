using System.Windows.Media;
using Microsoft.Office.Interop.Outlook;

namespace CalendarSyncPlus.Application.Wrappers
{
    public class Category
    {
        public Color Color { get; set; }

        public string HexValue { get; set; }

        public string CategoryName { get; set; }

        public OlCategoryColor OutlookColor { get; set; }
    }
}