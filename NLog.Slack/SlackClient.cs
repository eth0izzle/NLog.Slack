using System;
using ServiceStack;

namespace NLog.Slack
{
    public class SlackClient
    {
        //// ----------------------------------------------------------------------------------------------------------

        public event Action<Exception> Error;

        //// ----------------------------------------------------------------------------------------------------------

        public void Send(string url, string data)
        {
            try
            {
                url.PostJsonToUrl(data);
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        //// ----------------------------------------------------------------------------------------------------------

        private void OnError(Exception obj)
        {
            if (this.Error != null)
                this.Error(obj);
        }

        //// ----------------------------------------------------------------------------------------------------------
    }
}