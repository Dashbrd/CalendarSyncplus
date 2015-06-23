using System;
using System.Collections.Generic;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models
{
    public class Announcement : Model
    {
        private string _content;
        private DateTime _createdDate;
        private AnnouncementStateEnum _readState;
        private List<string> _tags;
        private string _title;

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