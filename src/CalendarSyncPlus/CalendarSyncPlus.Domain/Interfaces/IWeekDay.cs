using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSyncPlus.Domain.Interfaces
{
    public interface IWeekDay : IEncodableDataType, IComparable
    {
        int Offset { get; set; }
        DayOfWeek DayOfWeek { get; set; }
    }
}
