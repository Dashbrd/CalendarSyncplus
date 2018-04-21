using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [DataContract]
    public class LogSettings : Model
    {
        private bool _logSyncInfo;
        private bool _createNewFileForEverySync;

        [DataMember]
        public bool LogSyncInfo { get => _logSyncInfo; set => SetProperty(ref _logSyncInfo, value); }
        [DataMember]
        public bool CreateNewFileForEverySync { get => _createNewFileForEverySync; set => SetProperty(ref _createNewFileForEverySync, value); }
    }
}