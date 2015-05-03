using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSyncPlus.BootstrapperUI
{
    public class MainViewModel
    {
        public ManagedBootstrapperApplication Bootstrapper { get; set; }

        public MainViewModel(ManagedBootstrapperApplication bootstrapper)
        {
            Bootstrapper = bootstrapper;
        }
    }
}
