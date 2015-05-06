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

        public CustomCodeReceiver(Func<Task<string>> getCodeDeledateFunc)
        {
            GetCodeDeledateFunc = getCodeDeledateFunc;
            _redirectUri = "urn:ietf:wg:oauth:2.0:oob";
        }

        public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url,
            CancellationToken taskCancellationToken)
        {
            var completionSource =
                new TaskCompletionSource<AuthorizationCodeResponseUrl>();
            string fileName = ((object)url.Build()).ToString();

            Process.Start(fileName);
            string str;
            //for (str = string.Empty; string.IsNullOrEmpty(str); str = Console.ReadLine())
            //{
            //    Console.WriteLine("Please enter code: ");
            //}
            str = await GetCodeDeledateFunc();

            completionSource.SetResult(new AuthorizationCodeResponseUrl()
            {
                Code = str
            });
            return completionSource.Task.Result;
        }

        public string RedirectUri
        {
            get { return _redirectUri; }
        }
    }
}