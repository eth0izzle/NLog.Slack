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
        [RequiredParameter]
        public Layout WebHookUrl { get; set; }

        public bool Compact { get; set; }

        public override IList<TargetPropertyWithContext> ContextProperties { get; } = new List<TargetPropertyWithContext>();

        [ArrayParameter(typeof(TargetPropertyWithContext), "field")]
        public IList<TargetPropertyWithContext> Fields => ContextProperties;

        protected override void InitializeTarget()
        {
            var webHookUrl = GetWebHookUrl();
            if (String.IsNullOrWhiteSpace(webHookUrl))
                throw new ArgumentOutOfRangeException("WebHookUrl", "Webhook URL cannot be empty.");

            Uri uriResult;
            if (!Uri.TryCreate(webHookUrl, UriKind.Absolute, out uriResult))
                throw new ArgumentOutOfRangeException("WebHookUrl", "Webhook URL is an invalid URL.");

            if (!this.Compact && this.ContextProperties.Count == 0)
            {
                this.ContextProperties.Add(new TargetPropertyWithContext("Process Name", Layout = "${machinename}\\${processname}"));
                this.ContextProperties.Add(new TargetPropertyWithContext("Process PID", Layout = "${processid}"));
            }

            base.InitializeTarget();
        }

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

        private void SendToSlack(AsyncLogEventInfo info)
        {
            var message = RenderLogEvent(Layout, info.LogEvent);

            var webHookUrl = GetWebHookUrl(info.LogEvent);
            var slack = SlackMessageBuilder
                .Build(webHookUrl)
                .OnError(e => info.Continuation(e))
                .WithMessage(message);

            if (this.ShouldIncludeProperties(info.LogEvent) || this.ContextProperties.Count > 0)
            {
                var color = this.GetSlackColorFromLogLevel(info.LogEvent.Level);
                Attachment attachment = new Attachment(info.LogEvent.Message) { Color = color };
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
      
            var exception = info.LogEvent.Exception;
            if (!this.Compact && exception != null)
            {
                var color = this.GetSlackColorFromLogLevel(info.LogEvent.Level);
                var exceptionAttachment = new Attachment(null)
                {
                    Color = color,
                    Text = $"*Exception*\n```{exception}```"
                };
                slack.AddAttachment(exceptionAttachment);
            }

            slack.Send();
        }

        private string GetSlackColorFromLogLevel(LogLevel level)
        {
            if (LogLevelSlackColorMap.TryGetValue(level, out var color))
                return color;
            else
                return "#cccccc";
        }

        private static readonly Dictionary<LogLevel, string> LogLevelSlackColorMap = new Dictionary<LogLevel, string>()
        {
            { LogLevel.Warn, "warning" },
            { LogLevel.Error, "danger" },
            { LogLevel.Fatal, "danger" },
            { LogLevel.Info, "#2a80b9" },
        };

        private string GetWebHookUrl(LogEventInfo eventInfo = null)
        {
            return WebHookUrl?.Render(eventInfo ?? LogEventInfo.CreateNullEvent());
        }
    }
}