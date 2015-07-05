using System;
using System.Collections.Generic;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [Serializable]
    public class GoogleAccount : Model
    {
        private string _name;
       
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
    }

  
}