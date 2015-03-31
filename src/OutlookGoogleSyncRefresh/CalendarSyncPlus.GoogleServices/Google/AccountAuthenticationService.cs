using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Waf.Applications;
using CalendarSyncPlus.Application.Utilities;
using CalendarSyncPlus.Common.Log;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace CalendarSyncPlus.Application.Services.Google
{
    [Export(typeof (IAccountAuthenticationService))]
    public class AccountAuthenticationService : IAccountAuthenticationService
    {
        private readonly ApplicationLogger _applicationLogger;

        [ImportingConstructor]
        public AccountAuthenticationService(ApplicationLogger applicationLogger)
        {
            _applicationLogger = applicationLogger;
        }

        #region IAccountAuthenticationService Members

        /// <summary>
        ///     Authenticate to Google Using Oauth2
        ///     Documentation https://developers.google.com/accounts/docs/OAuth2
        /// </summary>
        /// <param name="clientId">From Google Developer console https://console.developers.google.com</param>
        /// <param name="clientSecret">From Google Developer console https://console.developers.google.com</param>
        /// <param name="userName">A string used to identify a user (locally).</param>
        /// <param name="fileDataStorePath">Name/Path where the Auth Token and refresh token are stored (usually in %APPDATA%)</param>
        /// <param name="applicationName">Applicaiton Name</param>
        /// <param name="isFullPath">
        ///     <paramref name="fileDataStorePath" /> is completePath or Directory Name
        /// </param>
        /// <returns></returns>
        public CalendarService AuthenticateCalenderOauth(string clientId, string clientSecret, string userName,
            string fileDataStorePath, string applicationName, bool isFullPath = false)
        {
            try
            {
                var scopes = new[]
                {
                    CalendarService.Scope.Calendar, // Manage your calendars
                    CalendarService.Scope.CalendarReadonly // View your Calendars
                };

                var fileDataStore = new FileDataStore(fileDataStorePath, isFullPath);

                CancellationToken cancellationToken = new CancellationTokenSource().Token;
                // here is where we Request the user to give us access, or use the Refresh Token that was previously stored in %AppData%
                Task<UserCredential> authTask = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets {ClientId = clientId, ClientSecret = clientSecret}
                    , scopes
                    , userName
                    , cancellationToken
                    , fileDataStore);
                authTask.Wait(30000);

                if (authTask.Status == TaskStatus.WaitingForActivation)
                {
                    return null;
                }

                var service = new CalendarService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = authTask.Result,
                    ApplicationName = applicationName,
                });

                return service;
            }
            catch (AggregateException exception)
            {
                _applicationLogger.LogError(exception.ToString());
                return null;
            }
            catch (Exception exception)
            {
                _applicationLogger.LogError(exception.ToString());
                return null;
            }
        }


        public CalendarService AuthenticateCalenderOauth()
        {
            string applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,
                Environment.SpecialFolderOption.None);
            string fullPath = applicationDataPath + @"\CalendarSyncPlus\" + Constants.AuthFolderPath;

            return AuthenticateCalenderOauth(Constants.ClientId, Constants.ClientSecret,
                Constants.User, fullPath, ApplicationInfo.ProductName, true);
        }

        #endregion
    }
}