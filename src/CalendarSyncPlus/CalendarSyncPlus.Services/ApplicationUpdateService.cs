using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Waf.Applications;
using CalendarSyncPlus.Common;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Services.Interfaces;
using Newtonsoft.Json;

namespace CalendarSyncPlus.Services
{
    /// <summary>
    /// </summary>
    [Export(typeof(IApplicationUpdateService))]
    public class ApplicationUpdateService : IApplicationUpdateService
    {
        /// <summary>
        /// </summary>
        private string _downloadLink;
        /// <summary>
        /// 
        /// </summary>
        private bool _isAlpha;

        /// <summary>
        /// </summary>
        private string _version;

        [ImportingConstructor]
        public ApplicationUpdateService(ApplicationLogger applicationLogger)
        {
            ApplicationLogger = applicationLogger;
        }

        public ApplicationLogger ApplicationLogger { get; set; }

        #region IApplicationUpdateService Members

        /// <summary>
        /// </summary>
        public string GetLatestReleaseFromServer(bool includeAlpha)
        {
            _version = null;
            _downloadLink = null;
            _isAlpha = false;
            try
            {
                var obj = GetLatestReleaseTag();
                if (includeAlpha)
                {
                    string version1 = obj.tag_name;
                    obj = GetLatestTag();
                    string version2 = obj.tag_name;
                    _isAlpha = IsAlpha(version1,version2);
                    _version = version2;
                }

                string body = obj.body;
                if (body.Contains("[link]"))
                {
                    body = body.Split(new[] { "[link]" }, StringSplitOptions.RemoveEmptyEntries).Last();
                    if (body.Contains("(") && body.Contains(")"))
                    {
                        var arrayValues = body.Split(new string[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries);
                        _downloadLink = arrayValues.FirstOrDefault(t => !string.IsNullOrEmpty(t.Trim()));
                    }
                }

                if (_downloadLink == null || string.IsNullOrEmpty(_downloadLink.Trim()))
                {
                    _downloadLink = obj.assets[0].browser_download_url;
                }
            }
            catch (Exception exception)
            {
                ApplicationLogger.LogError(exception.ToString());
                return exception.Message;
            }
            return null;
        }

        bool IsAlpha(string version1, string version2)
        {
            version1 = version1.Contains("-") ? version1.Remove(version1.IndexOf("-",StringComparison.InvariantCultureIgnoreCase)) : version1;
            version2 = version2.Contains("-") ? version2.Remove(version2.IndexOf("-", StringComparison.InvariantCultureIgnoreCase)) : version2;
            var version = new Version(version1.Substring(1));
            if (version > new Version(version2.Substring(1)))
            {
                return true;
            }
            return false;
        }
        private dynamic GetLatestReleaseTag()
        {
            HttpWebRequest request =
                WebRequest.Create(new Uri("https://api.github.com/repos/ankeshdave/calendarsyncplus/releases/latest"))
                    as HttpWebRequest;
            request.Method = "GET";
            request.ProtocolVersion = HttpVersion.Version11;
            request.ContentType = "application/json";
            request.ServicePoint.Expect100Continue = false;
            request.UnsafeAuthenticatedConnectionSharing = true;
            request.UserAgent = ApplicationInfo.ProductName;
            request.KeepAlive = false;
            string result = null;
            using (var resp = request.GetResponse() as HttpWebResponse)
            {
                if (resp != null)
                {
                    var reader =
                        new StreamReader(resp.GetResponseStream());
                    result = reader.ReadToEnd();
                }
            }
            dynamic obj = JsonConvert.DeserializeObject(result);
            return obj;
        }

        private dynamic GetLatestTag()
        {
            HttpWebRequest request =
                WebRequest.Create(new Uri("https://api.github.com/repos/ankeshdave/calendarsyncplus/releases"))
                    as HttpWebRequest;
            request.Method = "GET";
            request.ProtocolVersion = HttpVersion.Version11;
            request.ContentType = "application/json";
            request.ServicePoint.Expect100Continue = false;
            request.UnsafeAuthenticatedConnectionSharing = true;
            request.UserAgent = ApplicationInfo.ProductName;
            request.KeepAlive = false;
            string result = null;
            using (var resp = request.GetResponse() as HttpWebResponse)
            {
                if (resp != null)
                {
                    var reader =
                        new StreamReader(resp.GetResponseStream());
                    result = reader.ReadToEnd();
                }
            }
            dynamic obj = JsonConvert.DeserializeObject(result);
            return obj[0];
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
            if (_isAlpha)
                return string.Format("{0}-alpha", _version);
            return _version;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public Uri GetDownloadUri()
        {
            //Avoid for a while
            if (_downloadLink == null)
            {
                return null;
            }
            return new Uri(_downloadLink);
        }

        #endregion
    }
}