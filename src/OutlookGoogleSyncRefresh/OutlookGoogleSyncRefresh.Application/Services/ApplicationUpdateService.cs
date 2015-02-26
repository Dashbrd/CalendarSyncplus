using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Waf.Applications;
using Newtonsoft.Json;

namespace OutlookGoogleSyncRefresh.Application.Services
{
    /// <summary>
    /// 
    /// </summary>
    [Export(typeof(IApplicationUpdateService))]
    public class ApplicationUpdateService : IApplicationUpdateService
    {
        /// <summary>
        /// 
        /// </summary>
        private string _version;
        /// <summary>
        /// 
        /// </summary>
        private string _downloadLink;

        private DateTime _lastCheckedDateTime;
        /// <summary>
        /// 
        /// </summary>
        void GetLatestReleaseFromServer()
        {
            _version = null;
            _downloadLink = null;
            try
            {
                
                HttpWebRequest request = WebRequest.Create(new Uri("https://api.github.com/repos/ankeshdave/calendarsyncplus/releases/latest")) as HttpWebRequest;
                request.Method = "GET";
                request.ProtocolVersion = HttpVersion.Version11;
                request.ContentType = "application/json";
                request.ServicePoint.Expect100Continue = false;
                request.UnsafeAuthenticatedConnectionSharing = true;
                request.UserAgent = ApplicationInfo.ProductName;
                request.KeepAlive = false;
                string result;
                using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader =
                        new StreamReader(resp.GetResponseStream());
                    result = reader.ReadToEnd();
                }
                dynamic obj = JsonConvert.DeserializeObject(result);
                _version = obj.tag_name;
                
                _downloadLink = obj.assets[0].browser_download_url;
                _lastCheckedDateTime = DateTime.Now;

            }
            catch (Exception)
            {
                
            }
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsNewVersionAvailable()
        {
            GetLatestReleaseFromServer();
            Version version = new Version(_version.Substring(1));
            if (version > new Version(ApplicationInfo.Version))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetNewAvailableVersion()
        {
            return _version;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Uri GetDownloadUri()
        {
            if (_downloadLink == null)
                return null;
           return new Uri(_downloadLink);
        }

    }
}
