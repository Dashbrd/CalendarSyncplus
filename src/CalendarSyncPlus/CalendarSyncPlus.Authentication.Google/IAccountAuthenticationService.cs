using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Analytics.v3;
using Google.Apis.Calendar.v3;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;

namespace CalendarSyncPlus.Authentication.Google
{
    /// <summary>
    /// </summary>
    public interface IAccountAuthenticationService
    {
        /// <summary>
        ///     Authenticate to Google Using Oauth2 Documentation
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
        CalendarService AuthenticateCalendarOauth(string clientId, string clientSecret, string userName,
            string fileDataStorePath, string applicationName, bool isFullPath);

        /// <summary>
        ///     Authenticate to Google Using Oauth2 Documentation
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
        AnalyticsService AuthenticateAnalyticsOauth(string clientId, string clientSecret, string userName,
            string fileDataStorePath, string applicationName, bool isFullPath);
        /// <summary>
        ///     Authenticate to Google Using Oauth2 Documentation
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
        TasksService AuthenticateTasksOauth(string clientId, string clientSecret, string userName,
            string fileDataStorePath, string applicationName, bool isFullPath);

        /// <summary>
        ///     Default Authentication Method
        /// </summary>
        /// <param name="accountName"></param>
        /// <returns>
        /// </returns>
        CalendarService AuthenticateCalendarOauth(string accountName);

        /// <summary>
        /// </summary>
        /// <param name="accountName"></param>
        /// <returns>
        /// </returns>
        AnalyticsService AuthenticateAnalyticsOauth(string accountName);

        TasksService AuthenticateTasksOauth(string accountName);

        Task<bool> AuthorizeGoogleAccount(string accountName, CancellationToken cancellationToken);

        /// <summary>
        ///     Disconnects google account by deleting the auth token
        /// </summary>
        /// <param name="name"></param>
        /// <returns>
        /// </returns>
        bool DisconnectGoogle(string name);

        /// <summary>
        ///     Allows user to authenticate Google by manually entering the
        ///     authorization code
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="getCodeDeledateFunc"></param>
        /// <returns>
        /// </returns>
        Task<bool> ManualAccountAuthetication(string accountName, CancellationToken cancellationToken,
            Func<Task<string>> getCodeDeledateFunc);
    }
}