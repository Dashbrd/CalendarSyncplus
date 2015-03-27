using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Office.Interop.Outlook;

namespace OutlookGoogleSyncRefresh.Application.Wrappers
{
    public class Category
    {
        public Color Color { get; set; }

        public string HexValue { get; set; }

        public string CategoryName { get; set; }

        public OlCategoryColor OutlookColor { get; set; }
    }
}
