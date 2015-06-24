using System;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Threading.Tasks;
using System.Waf.Applications;
using CalendarSyncPlus.Analytics.Interfaces;
using CalendarSyncPlus.Authentication.Google;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Domain.Models;
using Google.Apis.Analytics.v3;
using Google.Apis.Analytics.v3.Data;
using Google.Apis.Requests;
using Google.Apis.Services;
using log4net;

namespace CalendarSyncPlus.Analytics
{
    /// <summary>
    /// </summary>
    [Export(typeof (ISyncAnalyticsService))]
    public class SyncAnalyticsService : ISyncAnalyticsService
    {
        /// <summary>
        /// </summary>
        /// <param name="accountAuthenticationService"></param>
        /// <param name="applicationLogger"></param>
        [ImportingConstructor]
        public SyncAnalyticsService(IAccountAuthenticationService accountAuthenticationService,
            ApplicationLogger applicationLogger)
        {
            AccountAuthenticationService = accountAuthenticationService;
            Logger = applicationLogger.GetLogger(GetType());
        }

        public IAccountAuthenticationService AccountAuthenticationService { get; set; }

        /// <summary>
        /// </summary>
        public ILog Logger { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="syncMetric"></param>
        /// <param name="accountName"></param>
        /// <returns>
        /// </returns>
        public async Task<bool> UploadSyncData(SyncMetric syncMetric, string accountName)
        {
            try
            {
                var analyticsService = new AnalyticsService(new BaseClientService.Initializer
                {
                    ApplicationName = ApplicationInfo.ProductName,
                    ApiKey = "AIzaSyBrpqcL6Nh1vVecfhIbxGVnyGHMZ8-aH6k"
                });
                var batchRequest = new BatchRequest(analyticsService);
                var metric = new CustomMetric
                {
                    Name = "SyncMetric",
                    Kind = "string"
                };

                var insertRequest = analyticsService.Management.CustomMetrics.Insert(metric, "", "");
                batchRequest.Queue<CustomMetric>(insertRequest, InsertMetricCallback);
                await batchRequest.ExecuteAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
            return true;
        }

        private void InsertMetricCallback(CustomMetric content, RequestError error, int index,
            HttpResponseMessage message)
        {
            if (!message.IsSuccessStatusCode)
            {
                Logger.Warn(message.StatusCode);
            }
        }
    }
}