using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models
{
    public class Announcement : Model
    {
        private string _title;
        private DateTime _createdDate;
        private List<string> _tags;
        private string _content;
        private AnnouncementStateEnum _readState;

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public DateTime CreatedDate
        {
            get { return _createdDate; }
            set { SetProperty(ref _createdDate, value); }
        }

        public List<string> Tags
        {
            get { return _tags; }
            set { SetProperty(ref _tags, value); }
        }

        public string Content
        {
            get { return _content; }
            set { SetProperty(ref _content, value); }
        }

        public AnnouncementStateEnum ReadState
        {
            get { return _readState; }
            set { SetProperty(ref _readState, value); }
        }
    }
}
