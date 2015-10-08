using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CalendarSyncPlus.Common.Log;
using Microsoft.Experimental.IdentityModel.Clients.ActiveDirectory;
using System.Waf.Applications;
using log4net;

namespace CalendarSyncPlus.Authentication.Live
{
    [Export(typeof(ILiveAuthenticationService))]
    public class LiveAuthenticationService : ILiveAuthenticationService
    {
        public ILog Logger { get; set; }

        [ImportingConstructor]
        public LiveAuthenticationService(ApplicationLogger applicationLogger)
        {
            Logger = applicationLogger.GetLogger(this.GetType());
        }

        //// Properties of the native client app. Get the ClientId from the resources section of the App.xaml file.
        //public const string ClientID = "";
        //// Get the _returnUri from app settings.
        //public Uri _returnUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri();

        //// Properties used to communicate with a Windows Azure AD tenant.  
        //public const string CommonAuthority = "https://login.windows.net/Common";
        //public const string DiscoveryResourceId = "https://api.office.com/discovery/";

        //private string LastAuthority { get; set; }

        //public AuthenticationContext _authenticationContext { get; set; }
        //private  async Task<string> GetTokenHelperAsync(AuthenticationContext context)
        //{
        //    string accessToken = null;
        //    AuthenticationResult result = null;

        //    result = await context.AcquireTokenAsync(GetScopes(),null, ClientID, _returnUri,null);

        //    accessToken = result.Token;
        //    //Store authority in application data.
        //    LastAuthority = context.Authority;

        //    return accessToken;
        //}

        //public async Task<OutlookServicesClient> CreateOutlookClientAsync(string capability)
        //{
        //    try
        //    {
        //        //First, look for the authority used during the last authentication.
        //        //If that value is not populated, use CommonAuthority.
        //        string authority = null;
        //        if (String.IsNullOrEmpty(LastAuthority))
        //        {
        //            authority = CommonAuthority;
        //        }
        //        else
        //        {
        //            authority = LastAuthority;
        //        }
        //        // Create an AuthenticationContext using this authority.
        //        _authenticationContext = new AuthenticationContext(authority);

        //        //See the Discovery Service Sample (https://github.com/OfficeDev/Office365-Discovery-Service-Sample)
        //        //for an approach that improves performance by storing the discovery service information in a cache.
        //        DiscoveryClient discoveryClient = new DiscoveryClient(
        //            async () => await GetTokenHelperAsync(_authenticationContext, DiscoveryResourceId));

        //        // Get the specified capability ("Calendar").
        //        CapabilityDiscoveryResult result =
        //            await discoveryClient.DiscoverCapabilityAsync(capability);
        //        var client = new OutlookServicesClient(
        //            result.ServiceEndpointUri,
        //            async () =>
        //                await GetTokenHelperAsync(_authenticationContext, result.ServiceResourceId));
        //        return client;
        //    }
        //    catch (Exception)
        //    {
        //        if (_authenticationContext != null && _authenticationContext.TokenCache != null)
        //            _authenticationContext.TokenCache.Clear();
        //        return null;
        //    }
        //}


        //public string[] GetScopes()
        //{
        //    return new string[] { "http://outlook.office.com/Calendars.ReadWrite" };
        //}
    }
}
