using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Waf.Foundation;
using CalendarSyncPlus.Analytics.Interfaces;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Metrics;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.Services.Calendars;
using CalendarSyncPlus.Services.Tasks.Interfaces;
using CalendarSyncPlus.Services.Utilities;
using CalendarSyncPlus.SyncEngine;
using CalendarSyncPlus.SyncEngine.Interfaces;
using log4net;

namespace CalendarSyncPlus.Services.Tasks
{
    [Export(typeof(ITaskUpdateService))]
    public class TaskUpdateService : Model, ITaskUpdateService
    {
        private ITaskSyncEngine _taskSyncEngine;
        private ITaskServiceFactory _taskServiceFactory;
        private ISyncAnalyticsService _analyticsService;
        private TasksWrapper _destinationTasks;
        private TasksWrapper _sourceTasks;
        private string _taskSyncStatus;
        private ITaskService _sourceTaskService;
        private ITaskService _destinationTaskService;

        #region Constructors

        [ImportingConstructor]
        public TaskUpdateService(ITaskServiceFactory calendarServiceFactory,
            ITaskSyncEngine calendarSyncEngine,
            ISyncAnalyticsService analyticsService,
            ApplicationLogger applicationLogger)
        {
            Logger = applicationLogger.GetLogger(GetType());
            TaskServiceFactory = calendarServiceFactory;
            TaskSyncEngine = calendarSyncEngine;
            AnalyticsService = analyticsService;
        }

        #endregion

        public ITaskSyncEngine TaskSyncEngine
        {
            get { return _taskSyncEngine; }
            set { SetProperty(ref _taskSyncEngine, value); }
        }

        public ITaskServiceFactory TaskServiceFactory
        {
            get { return _taskServiceFactory; }
            set { SetProperty(ref _taskServiceFactory, value); }
        }

        public ISyncAnalyticsService AnalyticsService
        {
            get { return _analyticsService; }
            set { SetProperty(ref _analyticsService, value); }
        }

        public ILog Logger { get; set; }

        public TasksWrapper DestinationTasks
        {
            get { return _destinationTasks; }
            set { SetProperty(ref _destinationTasks, value); }
        }

        public TasksWrapper SourceTasks
        {
            get { return _sourceTasks; }
            set { SetProperty(ref _sourceTasks, value); }
        }

        public string TaskSyncStatus
        {
            get { return _taskSyncStatus; }
            set { SetProperty(ref _taskSyncStatus, value); }
        }

        public ITaskService SourceTaskService
        {
            get { return _sourceTaskService; }
            set { SetProperty(ref _sourceTaskService, value); }
        }

        public ITaskService DestinationTaskService
        {
            get { return _destinationTaskService; }
            set { SetProperty(ref _destinationTaskService, value); }
        }

        void InitiatePreSyncSetup(TaskSyncProfile syncProfile)
        {
            SourceTaskService = TaskServiceFactory.GetTaskService(syncProfile.Source);
            DestinationTaskService = TaskServiceFactory.GetTaskService(syncProfile.Destination);
        }
        private IDictionary<string, object> GetCalendarSpecificData(ServiceType serviceType,
          TaskSyncProfile syncProfile)
        {
            IDictionary<string, object> calendarSpecificData = null;
            switch (serviceType)
            {
                case ServiceType.Google:
                    calendarSpecificData = new Dictionary<string, object>
                    {
                        {"CalendarId", syncProfile.GoogleSettings.GoogleCalendar.Id},
                        {"AccountName", syncProfile.GoogleSettings.GoogleAccount.Name}
                    };
                    break;
                case ServiceType.OutlookDesktop:
                    calendarSpecificData = new Dictionary<string, object>
                    {
                        {
                            "ProfileName",
                            !syncProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultProfile)
                                ? syncProfile.OutlookSettings.OutlookProfileName
                                : null
                        },
                        {
                            "OutlookCalendar",
                            !syncProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultMailBoxCalendar)
                                ? syncProfile.OutlookSettings.OutlookFolder
                                : null
                        }
                    };
                    break;
                case ServiceType.EWS:
                    return null;
            }
           
            return calendarSpecificData;
        }
        public bool SyncTask(TaskSyncProfile syncProfile, SyncMetric syncMetric, SyncCallback syncCallback)
        {
            InitiatePreSyncSetup(syncProfile);
            var isSucess = false;
            if (syncProfile != null)
            {
                
            }
            throw new NotImplementedException();
        }

    }
}
