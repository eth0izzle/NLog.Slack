using System;
using System.Collections.Generic;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Slack.Models;
using NLog.Targets;

namespace NLog.Slack
{
    [Target("Slack")]
    public class SlackTarget : TargetWithContext
    {
        //// ----------------------------------------------------------------------------------------------------------

        [RequiredParameter]
        public string WebHookUrl { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        public SimpleLayout Channel { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        public SimpleLayout Username { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        public string Icon { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        public bool Compact { get; set; }

        //// ----------------------------------------------------------------------------------------------------------
        public override IList<TargetPropertyWithContext> ContextProperties { get; } = new List<TargetPropertyWithContext>();

        //// ----------------------------------------------------------------------------------------------------------

        [ArrayParameter(typeof(TargetPropertyWithContext), "field")]
        public IList<TargetPropertyWithContext> Fields => ContextProperties;

        //// ----------------------------------------------------------------------------------------------------------

        protected override void InitializeTarget()
        {
            if (String.IsNullOrWhiteSpace(this.WebHookUrl))
                throw new ArgumentOutOfRangeException("WebHookUrl", "Webhook URL cannot be empty.");

            Uri uriResult;
            if (!Uri.TryCreate(this.WebHookUrl, UriKind.Absolute, out uriResult))
                throw new ArgumentOutOfRangeException("WebHookUrl", "Webhook URL is an invalid URL.");

            if (!String.IsNullOrWhiteSpace(this.Channel.Text)
                && (!this.Channel.Text.StartsWith("#") && !this.Channel.Text.StartsWith("@") && !this.Channel.Text.StartsWith("${")))
                throw new ArgumentOutOfRangeException("Channel", "The Channel name is invalid. It must start with either a # or a @ symbol or use a variable.");

            if (!this.Compact && this.ContextProperties.Count == 0)
            {
                this.ContextProperties.Add(new TargetPropertyWithContext("Process Name", Layout = "${machinename}\\${processname}"));
                this.ContextProperties.Add(new TargetPropertyWithContext("Process PID", Layout = "${processid}"));
            }

            base.InitializeTarget();
        }

        //// ----------------------------------------------------------------------------------------------------------

        protected override void Write(AsyncLogEventInfo info)
        {
            try
            {
                this.SendToSlack(info);
                info.Continuation(null);
            }
            catch (Exception e)
            {
                info.Continuation(e);
            }
        }

        //// ----------------------------------------------------------------------------------------------------------

        private void SendToSlack(AsyncLogEventInfo info)
        {
            var message = Layout.Render(info.LogEvent);
            var slack = SlackMessageBuilder
                .Build(this.WebHookUrl)
                .OnError(e => info.Continuation(e))
                .WithMessage(message);

            var channelValue = this.Channel.Render(info.LogEvent);
            if (!String.IsNullOrWhiteSpace(channelValue))
                slack.ToChannel(channelValue);

            var iconValue = this.Channel.Render(info.LogEvent);
            if (!String.IsNullOrWhiteSpace(iconValue))
                slack.WithIcon(iconValue);

            var usernameValue = this.Username.Render(info.LogEvent);
            if (!String.IsNullOrWhiteSpace(usernameValue))
                slack.AsUser(usernameValue);

            if (this.ShouldIncludeProperties(info.LogEvent))
            {
                var color = this.GetSlackColorFromLogLevel(info.LogEvent.Level);
                Attachment attachment = new Attachment(message) { Color = color };
                var allProperties = this.GetAllProperties(info.LogEvent);
                foreach (var property in allProperties)
                {
                    if (string.IsNullOrEmpty(property.Key))
                        continue;

                    var propertyValue = property.Value?.ToString();
                    if (string.IsNullOrEmpty(propertyValue))
                        continue;

                    attachment.Fields.Add(new Field(property.Key) { Value = propertyValue, Short = true });
                }
                if (attachment.Fields.Count > 0)
                    slack.AddAttachment(attachment);
            }
            else if (this.ContextProperties.Count > 0)
            {
                var color = this.GetSlackColorFromLogLevel(info.LogEvent.Level);
                Attachment attachment = new Attachment(message) { Color = color };
                foreach (var property in this.ContextProperties)
                {
                    if (string.IsNullOrEmpty(property.Name))
                        continue;

                    var propertyValue = property.Layout?.Render(info.LogEvent);
                    if (string.IsNullOrEmpty(propertyValue))
                        continue;

                    attachment.Fields.Add(new Field(property.Name) { Value = propertyValue, Short = true });
                }
                if (attachment.Fields.Count > 0)
                    slack.AddAttachment(attachment);
            }

            var exception = info.LogEvent.Exception;
            if (!this.Compact && exception != null)
            {
                var color = this.GetSlackColorFromLogLevel(info.LogEvent.Level);
                var exceptionAttachment = new Attachment(exception.Message) { Color = color };
                exceptionAttachment.Fields.Add(new Field("Type") { Value = exception.GetType().ToString(), Short = true });

                string stackTrace = exception.StackTrace;
                if (!String.IsNullOrWhiteSpace(stackTrace))
                    exceptionAttachment.Text = stackTrace;

                slack.AddAttachment(exceptionAttachment);
            }

            slack.Send();
        }

        //// ----------------------------------------------------------------------------------------------------------

        private string GetSlackColorFromLogLevel(LogLevel level)
        {
            if (LogLevelSlackColorMap.TryGetValue(level, out var color))
                return color;
            else
                return "#cccccc";
        }

        //// ----------------------------------------------------------------------------------------------------------

        private static readonly Dictionary<LogLevel, string> LogLevelSlackColorMap = new Dictionary<LogLevel, string>()
        {
            { LogLevel.Warn, "warning" },
            { LogLevel.Error, "danger" },
            { LogLevel.Fatal, "danger" },
            { LogLevel.Info, "#2a80b9" },
        };

        //// ----------------------------------------------------------------------------------------------------------
    }
}