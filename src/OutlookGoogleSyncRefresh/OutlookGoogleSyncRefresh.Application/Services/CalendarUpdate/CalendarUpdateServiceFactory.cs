﻿using System;
using System.ComponentModel.Composition;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Services.CalendarUpdate
{
    [Export(typeof(ICalendarUpdateServiceFactory))]
    public class CalendarUpdateServiceFactory : ICalendarUpdateServiceFactory
    {
        public Lazy<ICalendarUpdateService> LazyOutlookGoogleService { get; set; }

        [ImportingConstructor]
        public CalendarUpdateServiceFactory(Lazy<ICalendarUpdateService> lazyOutlookGoogleService)
        {
            LazyOutlookGoogleService = lazyOutlookGoogleService;
        }

        public ICalendarUpdateService GetCalendarUpdateService(Settings settings)
        {
            if (settings.CalendarSyncMode == CalendarSyncModeEnum.OutlookGoogleOneWay)
            {
                return LazyOutlookGoogleService.Value;
            }
            return null;
        }
    }
}