using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog.Common;
using NLog.Config;
using NLog.Slack.Models;
using NLog.Targets;

namespace NLog.Slack
{
    [Target("Slack")]
    public class SlackTarget : TargetWithContext
    {
        [RequiredParameter]
        public string WebHookUrl { get; set; }

        public bool Compact { get; set; }

        public override IList<TargetPropertyWithContext> ContextProperties { get; } = new List<TargetPropertyWithContext>();

        [ArrayParameter(typeof(TargetPropertyWithContext), "field")]
        public IList<TargetPropertyWithContext> Fields => ContextProperties;

        private const int stackTraceChunk = 1990;

        protected override void InitializeTarget()
        {
            if (String.IsNullOrWhiteSpace(this.WebHookUrl))
                throw new ArgumentOutOfRangeException("WebHookUrl", "Webhook URL cannot be empty.");

            Uri uriResult;
            if (!Uri.TryCreate(this.WebHookUrl, UriKind.Absolute, out uriResult))
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

            var slack = SlackMessageBuilder
                .Build(this.WebHookUrl)
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
                var attachment = new Attachment(exception.Message) { Color = color };

                attachment.Fields.Add(new Field("Exception Message") { Value = exception.Message, Short = false });
                attachment.Fields.Add(new Field("Exception Type") { Value = exception.GetType().Name, Short = true });

                if (!string.IsNullOrWhiteSpace(exception.StackTrace))
                {
                    var parts = exception.StackTrace.SplitOn(stackTraceChunk).ToArray(); // Split call stack into consecutive fields of ~2k characters
                    for (int idx = 0; idx < parts.Length; idx++)
                    {
                        var name = "StackTrace" + (idx > 0 ? $" {idx + 1}" : null);
                        attachment.Fields.Add(new Field(name) { Value = "```" + parts[idx].Replace("```", "'''") + "```" });
                    }
                }

                slack.AddAttachment(attachment);
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
    }

    // source: https://github.com/jonfreeland/Log4Slack
    internal static class Extensions
    {
        public static IEnumerable<string> SplitOn(this string text, int numChars)
        {
            var splitOnPattern = new Regex($@"(?<line>.{{1,{numChars}}})([\r\n]|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return splitOnPattern.Matches(text).OfType<Match>().Select(m => m.Groups["line"].Value);
        }
    }
}