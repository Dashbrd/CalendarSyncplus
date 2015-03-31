using Google.Apis.Calendar.v3;

namespace CalendarSyncPlus.Application.Services.Google
{
    public interface IAccountAuthenticationService
    {
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
        CalendarService AuthenticateCalenderOauth(string clientId, string clientSecret, string userName,
            string fileDataStorePath, string applicationName, bool isFullPath);

        /// <summary>
        ///     Default Authentication Method
        /// </summary>
        /// <returns></returns>
        CalendarService AuthenticateCalenderOauth();
    }
}