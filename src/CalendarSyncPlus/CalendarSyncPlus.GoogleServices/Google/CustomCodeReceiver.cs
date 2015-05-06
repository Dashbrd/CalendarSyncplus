using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;

namespace CalendarSyncPlus.GoogleServices.Google
{
    public class CustomCodeReceiver : ICodeReceiver
    {
        public Func<Task<string>> GetCodeDeledateFunc { get; set; }
        private string _redirectUri;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="getCodeDeledateFunc"></param>
        public CustomCodeReceiver(Func<Task<string>> getCodeDeledateFunc)
        {
            GetCodeDeledateFunc = getCodeDeledateFunc;
            _redirectUri = "urn:ietf:wg:oauth:2.0:oob";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="taskCancellationToken"></param>
        /// <returns></returns>
        public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url,
            CancellationToken taskCancellationToken)
        {
            var completionSource =
                new TaskCompletionSource<AuthorizationCodeResponseUrl>();
            string fileName = ((object)url.Build()).ToString();

            Process.Start(fileName);

            var googleAuthCode = await GetCodeDeledateFunc();

            completionSource.SetResult(new AuthorizationCodeResponseUrl()
            {
                Code = googleAuthCode
            });
            return completionSource.Task.Result;
        }

        public string RedirectUri
        {
            get { return _redirectUri; }
        }
    }
}