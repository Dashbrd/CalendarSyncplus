using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Waf.Applications;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.GoogleServices.Google;
using CalendarSyncPlus.Services.Interfaces;
using CalendarSyncPlus.Services.Utilities;
using Google.Apis.Analytics.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Util.Store;
using log4net;

namespace CalendarSyncPlus.Authentication.Google
{
    [Export(typeof (IAccountAuthenticationService))]
    public class AccountAuthenticationService : IAccountAuthenticationService
    {
        private readonly ILog ApplicationLogger;

        [ImportingConstructor]
        public AccountAuthenticationService(ApplicationLogger applicationLogger, IMessageService messageService)
        {
            MessageService = messageService;
            ApplicationLogger = applicationLogger.GetLogger(GetType());
        }

        public IMessageService MessageService { get; set; }

        #region IAccountAuthenticationService Members

        /// <summary>
        ///     <see cref="Authenticate" /> to Google Using Oauth2 Documentation
        ///     https://developers.google.com/accounts/docs/OAuth2
        /// </summary>
        /// <param name="clientId">
        ///     From Google Developer console https://console.developers.google.com
        /// </param>
        /// <param name="clientSecret">
        ///     From Google Developer console https://console.developers.google.com
        /// </param>
        /// <param name="userName">
        ///     A string used to identify a user (locally).
        /// </param>
        /// <param name="fileDataStorePath">
        ///     Name/Path where the Auth Token and refresh token are stored (usually
        ///     in %APPDATA%)
        /// </param>
        /// <param name="applicationName">Applicaiton Name</param>
        /// <param name="isFullPath">
        ///     <paramref name="fileDataStorePath" /> is completePath or Directory
        ///     Name
        /// </param>
        /// <returns>
        /// </returns>
        public CalendarService AuthenticateCalendarOauth(string clientId, string clientSecret, string userName,
            string fileDataStorePath, string applicationName, bool isFullPath = false)
        {
            try
            {
                var authTask = Authenticate(clientId, clientSecret, userName, fileDataStorePath, isFullPath);

                authTask.Wait(30000);

                if (authTask.Status == TaskStatus.WaitingForActivation)
                {
                    return null;
                }

                var service = new CalendarService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = authTask.Result,
                    ApplicationName = applicationName
                });

                return service;
            }
            catch (AggregateException exception)
            {
                ApplicationLogger.Error(exception);
                return null;
            }
            catch (Exception exception)
            {
                ApplicationLogger.Error(exception);
                return null;
            }
        }


        /// <summary>
        ///     <see cref="Authenticate" /> to Google Using Oauth2 Documentation
        ///     https://developers.google.com/accounts/docs/OAuth2
        /// </summary>
        /// <param name="clientId">
        ///     From Google Developer console https://console.developers.google.com
        /// </param>
        /// <param name="clientSecret">
        ///     From Google Developer console https://console.developers.google.com
        /// </param>
        /// <param name="userName">
        ///     A string used to identify a user (locally).
        /// </param>
        /// <param name="fileDataStorePath">
        ///     Name/Path where the Auth Token and refresh token are stored (usually
        ///     in %APPDATA%)
        /// </param>
        /// <param name="applicationName">Applicaiton Name</param>
        /// <param name="isFullPath">
        ///     <paramref name="fileDataStorePath" /> is completePath or Directory
        ///     Name
        /// </param>
        /// <returns>
        /// </returns>
        public AnalyticsService AuthenticateAnalyticsOauth(string clientId, string clientSecret, string userName,
            string fileDataStorePath, string applicationName, bool isFullPath = false)
        {
            try
            {
                var authTask = Authenticate(clientId, clientSecret, userName, fileDataStorePath, isFullPath);

                authTask.Wait(30000);

                if (authTask.Status == TaskStatus.WaitingForActivation)
                {
                    return null;
                }

                var service = new AnalyticsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = authTask.Result,
                    ApplicationName = applicationName
                });

                return service;
            }
            catch (AggregateException exception)
            {
                ApplicationLogger.Error(exception);
                return null;
            }
            catch (Exception exception)
            {
                ApplicationLogger.Error(exception);
                return null;
            }
        }

        /// <summary>
        ///     <see cref="Authenticate" /> to Google Using Oauth2 Documentation
        ///     https://developers.google.com/accounts/docs/OAuth2
        /// </summary>
        /// <param name="clientId">
        ///     From Google Developer console https://console.developers.google.com
        /// </param>
        /// <param name="clientSecret">
        ///     From Google Developer console https://console.developers.google.com
        /// </param>
        /// <param name="userName">
        ///     A string used to identify a user (locally).
        /// </param>
        /// <param name="fileDataStorePath">
        ///     Name/Path where the Auth Token and refresh token are stored (usually
        ///     in %APPDATA%)
        /// </param>
        /// <param name="applicationName">Applicaiton Name</param>
        /// <param name="isFullPath">
        ///     <paramref name="fileDataStorePath" /> is completePath or Directory
        ///     Name
        /// </param>
        /// <returns>
        /// </returns>
        public TasksService AuthenticateTasksOauth(string clientId, string clientSecret, string userName,
            string fileDataStorePath, string applicationName, bool isFullPath = false)
        {
            try
            {
                var authTask = Authenticate(clientId, clientSecret, userName, fileDataStorePath, isFullPath);

                authTask.Wait(30000);

                if (authTask.Status == TaskStatus.WaitingForActivation)
                {
                    return null;
                }

                var service = new TasksService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = authTask.Result,
                    ApplicationName = applicationName
                });

                return service;
            }
            catch (AggregateException exception)
            {
                ApplicationLogger.Error(exception);
                return null;
            }
            catch (Exception exception)
            {
                ApplicationLogger.Error(exception);
                return null;
            }
        }


        private static Task<UserCredential> Authenticate(string clientId, string clientSecret, string userName,
            string fileDataStorePath,
            bool isFullPath)
        {
            var scopes = GetScopes();

            var fileDataStore = new FileDataStore(fileDataStorePath, isFullPath);

            var cancellationToken = new CancellationTokenSource().Token;
            // here is where we Request the user to give us access, or use the Refresh Token that was previously stored in %AppData%
            return GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets {ClientId = clientId, ClientSecret = clientSecret}
                , scopes
                , String.Format("-{0}-googletoken", userName)
                , cancellationToken
                , fileDataStore);
        }

        public AnalyticsService AuthenticateAnalyticsOauth(string accountName)
        {
            var applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,
                Environment.SpecialFolderOption.None);
            var fullPath = applicationDataPath + @"\CalendarSyncPlus\" + Constants.AuthFolderPath;

            return AuthenticateAnalyticsOauth(Constants.ClientId, Constants.ClientSecret,
                accountName, fullPath, ApplicationInfo.ProductName, true);
        }

        public CalendarService AuthenticateCalendarOauth(string accountName)
        {
            var applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,
                Environment.SpecialFolderOption.None);
            var fullPath = applicationDataPath + @"\CalendarSyncPlus\" + Constants.AuthFolderPath;

            return AuthenticateCalendarOauth(Constants.ClientId, Constants.ClientSecret,
                accountName, fullPath, ApplicationInfo.ProductName, true);
        }

        public TasksService AuthenticateTasksOauth(string accountName)
        {
            var applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,
                Environment.SpecialFolderOption.None);
            var fullPath = applicationDataPath + @"\CalendarSyncPlus\" + Constants.AuthFolderPath;

            return AuthenticateTasksOauth(Constants.ClientId, Constants.ClientSecret,
                accountName, fullPath, ApplicationInfo.ProductName, true);
        }

        public async Task<bool> AuthorizeGoogleAccount(string accountName, CancellationToken cancellationToken)
        {
            var applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,
                Environment.SpecialFolderOption.None);
            var fullPath = applicationDataPath + @"\CalendarSyncPlus\" + Constants.AuthFolderPath;
            var fileDataStore = new FileDataStore(fullPath, true);

            var scopes = GetScopes();

            var auth =
                await
                    GoogleWebAuthorizationBroker.AuthorizeAsync(
                        new ClientSecrets {ClientId = Constants.ClientId, ClientSecret = Constants.ClientSecret}
                        , scopes
                        , String.Format("-{0}-googletoken", accountName)
                        , cancellationToken
                        , fileDataStore);
            return true;
        }

        public bool DisconnectGoogle(string name)
        {
            try
            {
                var applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,
                    Environment.SpecialFolderOption.None);
                var fullPath = applicationDataPath + @"\CalendarSyncPlus\" + Constants.AuthFolderPath;

                if (Directory.Exists(fullPath))
                {
                    var files = Directory.EnumerateFiles(fullPath, "*.*", SearchOption.AllDirectories);

                    IEnumerable<string> filePaths = files as string[] ?? files.ToArray();

                    if (!filePaths.Any()) return true;

                    foreach (var filePath in filePaths.Where(s => s.Contains(name)))
                    {
                        File.Delete(filePath);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception exception)
            {
                ApplicationLogger.Error(exception);
                return false;
            }
        }

        public async Task<bool> ManualAccountAuthetication(string accountName, CancellationToken cancellationToken,
            Func<Task<string>> getCodeDeledateFunc)
        {
            try
            {
                var applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,
                    Environment.SpecialFolderOption.None);
                var fullPath = applicationDataPath + @"\CalendarSyncPlus\" + Constants.AuthFolderPath;


                var initializer = new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets =
                        new ClientSecrets
                        {
                            ClientId = Constants.ClientId,
                            ClientSecret = Constants.ClientSecret
                        },
                    Scopes = GetScopes(),
                    DataStore = new FileDataStore(fullPath, true)
                };

                var authTask = await new AuthorizationCodeInstalledApp(
                    new GoogleAuthorizationCodeFlow(initializer),
                    new CustomCodeReceiver(getCodeDeledateFunc))
                    .AuthorizeAsync(String.Format("-{0}-googletoken", accountName), CancellationToken.None);

                var service = new CalendarService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = authTask,
                    ApplicationName = ApplicationInfo.ProductName
                });
                return true;
            }
            catch (AggregateException exception)
            {
                ApplicationLogger.Error(exception);
            }
            catch (Exception exception)
            {
                ApplicationLogger.Error(exception);
            }
            return false;
        }

        #endregion

        private static string[] GetScopes()
        {
            var scopes = new[]
            {
                CalendarService.Scope.Calendar, // Manage your calendars
                CalendarService.Scope.CalendarReadonly, // View your Calendars
                TasksService.Scope.Tasks, // Manage your tasks
                TasksService.Scope.TasksReadonly // View your tasks
                //AnalyticsService.Scope.Analytics, // view and manage your analytics data
                //AnalyticsService.Scope.AnalyticsEdit, // edit management actives
                //AnalyticsService.Scope.AnalyticsManageUsers, // manage users
                //AnalyticsService.Scope.AnalyticsReadonly
            };
            return scopes;
        }
    }
}