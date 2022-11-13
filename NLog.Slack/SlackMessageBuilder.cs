using System;
using NLog.Slack.Models;

namespace NLog.Slack
{
    public class SlackMessageBuilder
    {
        private readonly string _webHookUrl;

        private readonly string _webProxyUrl;

        private readonly SlackClient _client;

        private readonly Payload _payload;

        public SlackMessageBuilder(string webHookUrl, string webProxyUrl = null)
        {
            this._webHookUrl = webHookUrl;
            this._webProxyUrl = webProxyUrl;
            this._client = new SlackClient();
            this._payload = new Payload();
        }

        public static SlackMessageBuilder Build(string webHookUrl, string webProxyUrl = null)
        {
            return new SlackMessageBuilder(webHookUrl, webProxyUrl);
        }

        public SlackMessageBuilder WithMessage(string message)
        {
            this._payload.Text = message;

            return this;
        }

        public SlackMessageBuilder AddAttachment(Attachment attachment)
        {
            this._payload.Attachments.Add(attachment);

            return this;
        }

        public SlackMessageBuilder OnError(Action<Exception> error)
        {
            this._client.Error += error;

            return this;
        }

        public void Send()
        {
            this._client.Send(this._webHookUrl, this._payload.ToJson(), this._webProxyUrl);
        }
    }
}