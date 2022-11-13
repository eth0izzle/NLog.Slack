using System;
using System.Net;
using System.Text;

namespace NLog.Slack
{
    public class SlackClient
    {
        public event Action<Exception> Error;

        public void Send(string url, string data, string webProxyUrl = null)
        {
            try
            {
                using (var client = new WebClient())
                {
                    if (!string.IsNullOrWhiteSpace(webProxyUrl))
                        client.Proxy = new WebProxy(webProxyUrl);

                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Encoding = Encoding.UTF8;
                    client.UploadString(url, "POST", data);
                }
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        private void OnError(Exception obj)
        {
            if (this.Error != null)
                this.Error(obj);
        }
    }
}