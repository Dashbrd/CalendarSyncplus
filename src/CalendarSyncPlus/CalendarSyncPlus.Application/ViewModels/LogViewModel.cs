using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Applications;
using CalendarSyncPlus.Application.Views;
using log4net.Core;

namespace CalendarSyncPlus.Application.ViewModels
{
    [Export]
    public class LogViewModel : ViewModel<ILogView>
    {
        public LogViewModel(ILogView view)
            : base(view)
        {
        }

        public List<Level> LogLevels { get; set; }

        public Level SelectedLogLevel { get; set; }
    }
}
