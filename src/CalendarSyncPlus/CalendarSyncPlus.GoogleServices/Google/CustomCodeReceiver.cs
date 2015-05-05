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
        public Func<string> GetCodeDeledateFunc { get; set; }
        private string _redirectUri;

        public CustomCodeReceiver(Func<string> getCodeDeledateFunc)
        {
            GetCodeDeledateFunc = getCodeDeledateFunc;
            _redirectUri = "urn:ietf:wg:oauth:2.0:oob";
        }

        public Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url,
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
            str = GetCodeDeledateFunc();

            completionSource.SetResult(new AuthorizationCodeResponseUrl()
            {
                Code = str
            });
            return completionSource.Task;
        }

        public string RedirectUri
        {
            get { return _redirectUri; }
        }
    }
}