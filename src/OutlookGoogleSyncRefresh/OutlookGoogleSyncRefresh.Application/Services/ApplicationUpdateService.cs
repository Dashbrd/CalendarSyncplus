using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Waf.Applications;
using Newtonsoft.Json;
using OutlookGoogleSyncRefresh.Common.Log;

namespace OutlookGoogleSyncRefresh.Application.Services
{
    /// <summary>
    /// </summary>
    [Export(typeof (IApplicationUpdateService))]
    public class ApplicationUpdateService : IApplicationUpdateService
    {
        public ApplicationLogger ApplicationLogger { get; set; }

        /// <summary>
        /// </summary>
        private string _downloadLink;

        /// <summary>
        /// </summary>
        private string _version;
        [ImportingConstructor]
        public ApplicationUpdateService(ApplicationLogger applicationLogger)
        {
            ApplicationLogger = applicationLogger;
        }

        #region IApplicationUpdateService Members

        /// <summary>
        /// </summary>
        public string GetLatestReleaseFromServer()
        {
            _version = null;
            _downloadLink = null;
            try
            {
                var request =
                    WebRequest.Create(new Uri("https://api.github.com/repos/ankeshdave/calendarsyncplus/releases/latest"))
                        as HttpWebRequest;
                request.Method = "GET";
                request.ProtocolVersion = HttpVersion.Version11;
                request.ContentType = "application/json";
                request.ServicePoint.Expect100Continue = false;
                request.UnsafeAuthenticatedConnectionSharing = true;
                request.UserAgent = ApplicationInfo.ProductName;
                request.KeepAlive = false;
                string result;
                using (var resp = request.GetResponse() as HttpWebResponse)
                {
                    var reader =
                        new StreamReader(resp.GetResponseStream());
                    result = reader.ReadToEnd();
                }
                dynamic obj = JsonConvert.DeserializeObject(result);
                _version = obj.tag_name;

                _downloadLink = obj.assets[0].browser_download_url;
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.ToString());
                return exception.Message;
            }
            return null;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public bool IsNewVersionAvailable()
        {
            try
            {
                var version = new Version(_version.Substring(1));
                if (version > new Version(ApplicationInfo.Version))
                {
                    return true;
                }
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.ToString());
            }
            return false;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public string GetNewAvailableVersion()
        {
            return _version;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public Uri GetDownloadUri()
        {
            if (_downloadLink == null)
            {
                return null;
            }
            return new Uri(_downloadLink);
        }

        #endregion
    }
}