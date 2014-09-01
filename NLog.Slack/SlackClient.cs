using System;
using System.Net;
using System.Threading.Tasks;
using NLog.Slack.Models;
using ServiceStack.Text;

namespace NLog.Slack
{
    public class SlackClient
    {
        //// ----------------------------------------------------------------------------------------------------------

        private readonly WebClient _webClient;

        //// ----------------------------------------------------------------------------------------------------------

        public event Action<Exception> Error;

        //// ----------------------------------------------------------------------------------------------------------

        public SlackClient()
        {
            this._webClient = new WebClient();

        }

        //// ----------------------------------------------------------------------------------------------------------

        public void Send(string url, string data)
        {
            Task.Run(() =>
                {
                    this._webClient.UploadString(url, "POST", data);
                });
        }

        //// ----------------------------------------------------------------------------------------------------------

        protected virtual void OnError(Exception obj)
        {
            var handler = Error;

            if (handler != null)
                handler(obj);
        }

        //// ----------------------------------------------------------------------------------------------------------

        private WebClient SetupWebClient()
        {
            var webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=UTF-8");
            webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");

            UploadStringCompletedEventHandler cb = null;

            cb = (s, e) =>
            {
                if (cb != null)
                {
                    webClient.UploadStringCompleted -= cb;
                }

                if (!(e.Error is WebException))
                {
                    OnError(e.Error);

                    return;
                }

                try
                {
                    var we = e.Error as WebException;
                    var response = JsonSerializer.DeserializeFromStream<Response>(we.Response.GetResponseStream());

                    if (response.HasError)
                    {
                        OnError(new Exception(response.Error, e.Error));

                        return;
                    }
                }
                catch (Exception)
                {
                    OnError(new Exception("Failed to send log event to Slack", e.Error));
                }

                OnError(e.Error);
            };

            webClient.UploadStringCompleted += cb;

            return webClient;
        }

        //// ----------------------------------------------------------------------------------------------------------
    }
}