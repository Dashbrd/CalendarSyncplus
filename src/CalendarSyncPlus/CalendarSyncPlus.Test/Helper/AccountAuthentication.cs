using System;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace OutlookGoogleSyncRefresh.Helper
{
    public class AccountAuthentication
    {
        /// <summary>
        /// Authenticate to Google Using Oauth2
        /// Documentation https://developers.google.com/accounts/docs/OAuth2
        /// </summary>
        /// <param name="clientId">From Google Developer console https://console.developers.google.com</param>
        /// <param name="clientSecret">From Google Developer console https://console.developers.google.com</param>
        /// <param name="userName">A string used to identify a user (locally).</param>
        /// <param name="fileDataStorePath">Name/Path where the Auth Token and refresh token are stored (usually in %APPDATA%)</param>
        /// <param name="applicationName">Applicaiton Name</param>
        /// <returns></returns>
        public CalendarService AuthenticateCalenderOauth(string clientId, string clientSecret, string userName,string fileDataStorePath,string applicationName)
        {

            var scopes = new[]
            {
                CalendarService.Scope.Calendar, // Manage your calendars
                CalendarService.Scope.CalendarReadonly // View your Calendars
            };


            // here is where we Request the user to give us access, or use the Refresh Token that was previously stored in %AppData%
            UserCredential credential =
                GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets {ClientId = clientId, ClientSecret = clientSecret}
                    , scopes
                    , userName
                    , CancellationToken.None
                    , new FileDataStore(fileDataStorePath)).Result;



            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });
            return service;
        }
    }
}